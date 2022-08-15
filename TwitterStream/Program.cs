using System;
using System.Configuration;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.DependencyInjection;
using TwitterStream.Entities;
using TwitterStream.Interfaces;

namespace TwitterStream
{
    // TODO: Deploy as Azure Function under an App Service Plan with AlwaysOn enabled.
    static class Program
    {
        static async Task Main()
        {
            // Dependency Injection.
            var serviceProvider = new ServiceCollection()
                //.AddLogging() // If it were a real logger.
                .AddSingleton<ILogger, Logger>()
                .AddSingleton<ILoggingService, LoggingService>()
                .AddSingleton<ITwitterConfig, TwitterConfig>()
                .BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger>();
            // TODO: Move config settings to Azure Vault for security. Additional validation for settings (data types, not null, etc.).

            try
            {
                var apiKey = ConfigurationManager.AppSettings["apiKey"];
                var apiSecret = ConfigurationManager.AppSettings["apiSecret"];
                var apiUrl = ConfigurationManager.AppSettings["apiUrl"];
                var maxMessageSizeInBytes = Convert.ToInt32(ConfigurationManager.AppSettings["maxMessageSize"]) * 1024;
                var maxRequestsInProgress = Convert.ToInt32(ConfigurationManager.AppSettings["maxRequestsInProgress"]);
                var maxSecondsToBuffer = Convert.ToInt32(ConfigurationManager.AppSettings["maxSecondsToBuffer"]);
                var producer = new EventHubProducerClient(
                    ConfigurationManager.AppSettings["EventHubConnectionString"],
                    ConfigurationManager.AppSettings["EventHubName"],
                    new EventHubProducerClientOptions()
                );

                if (apiUrl == null ||
                    apiKey == null ||
                    apiSecret == null)
                {
                    throw new ArgumentNullException("Application configuration is incomplete.");
                }

                Console.WriteLine($"Sending data to Azure EventHub : {producer.EventHubName} PartitionCount = {(await producer.GetPartitionIdsAsync()).Length}");
                var twitterConfig = new TwitterConfig(apiUrl, apiKey, apiSecret);
                var twitterStream = TwitterStream.Entities.TwitterStream.StreamStatuses(twitterConfig).ToObservable();
                var eventDataObserver = Observable.Create<EventData>(
                    outputObserver => twitterStream.Subscribe(
                        new EventDataGenerator(outputObserver, maxMessageSizeInBytes, maxSecondsToBuffer, logger!)));
                var sendTasks = eventDataObserver.Select(e =>
                {
                    var batch = producer.CreateBatchAsync().Result;

                    if (!batch.TryAdd(e))
                    {
                        throw new ArgumentOutOfRangeException("Content too big to send in a single eventhub message");
                    }

                    return producer.SendAsync(batch);
                })
                .Buffer(TimeSpan.FromMinutes(1), maxRequestsInProgress)
                .Select(sendTaskList => Task.WhenAll(sendTaskList));
                sendTasks.Subscribe(
                    sendEventDatasTask => sendEventDatasTask.Wait(),
                    e => Console.WriteLine(e));
            }
            catch (Exception ex)
            {
                logger?.HandleMessage(ex.Message, LogMessageType.Error);
            }
        }
    }
}