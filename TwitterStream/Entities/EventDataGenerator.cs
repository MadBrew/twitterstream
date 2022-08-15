using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using Azure.Messaging.EventHubs;
using TwitterStream.Interfaces;

namespace TwitterStream.Entities
{
    // Gets a list of strings, accumulates it in EventData and calls send on eventDataOutputObserver.
    internal class EventDataGenerator : IObserver<string>
    {
        private readonly int maxSizePerMessageInBytes;
        private readonly IObserver<EventData> eventDataOutputObserver;
        private readonly Stopwatch waitIntervalStopWatch = Stopwatch.StartNew();
        private readonly TimeSpan maxTimeToBuffer;
        private MemoryStream memoryStream;
        private GZipStream gzipStream;
        private StreamWriter streamWriter;
        private long messagesCount = 0;
        private long tweetCount = 0;
        private readonly ILogger _logger;

        public EventDataGenerator(IObserver<EventData> eventDataOutputObserver, int maxSizePerMessageInBytes, int maxSecondsToBuffer, ILogger logger)
        {
            maxTimeToBuffer = TimeSpan.FromSeconds(maxSecondsToBuffer);
            this.maxSizePerMessageInBytes = maxSizePerMessageInBytes;
            this.eventDataOutputObserver = eventDataOutputObserver;
            memoryStream = new MemoryStream(this.maxSizePerMessageInBytes);
            gzipStream = new GZipStream(memoryStream, CompressionMode.Compress);
            streamWriter = new StreamWriter(gzipStream);
            _logger = logger;
        }

        public void OnCompleted()
        {
            SendEventData(isCompleted: true);
            _logger.HandleMessage($"Completed Sent TweetCount = {tweetCount} MessageCount = {messagesCount}", LogMessageType.Info);
            eventDataOutputObserver.OnCompleted();
        }

        public void OnError(Exception error)
        {
            eventDataOutputObserver.OnError(error);
        }

        public void OnNext(string value)
        {
            tweetCount++;
            streamWriter.WriteLine(value);
            streamWriter.Flush();

            if (waitIntervalStopWatch.Elapsed > maxTimeToBuffer || memoryStream.Length >= maxSizePerMessageInBytes)
            {
                SendEventData();
            }
        }

        private void SendEventData(bool isCompleted = false)
        {
            if (memoryStream.Length == 0)
            {
                return;
            }

            messagesCount++;
            gzipStream.Close();
            var eventData = new EventData(memoryStream.ToArray());
            eventDataOutputObserver.OnNext(eventData);
            gzipStream.Dispose();
            memoryStream.Dispose();

            if (!isCompleted)
            {
                memoryStream = new MemoryStream(maxSizePerMessageInBytes);
                gzipStream = new GZipStream(memoryStream, CompressionMode.Compress);
                streamWriter = new StreamWriter(gzipStream);
            }

            _logger.HandleMessage($"Time: {DateTime.UtcNow:o} Sent TweetCount = {tweetCount} MessageCount = {messagesCount}", LogMessageType.Info);
            waitIntervalStopWatch.Restart();
        }
    }
}