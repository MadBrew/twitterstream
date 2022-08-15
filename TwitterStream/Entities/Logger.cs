using System;
using TwitterStream.Interfaces;

namespace TwitterStream.Entities
{
    public class Logger: ILogger
    {
        private readonly ILoggingService _loggingService;

        public Logger(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public void HandleMessage(string message, LogMessageType type)
        {
            Console.WriteLine(string.Format("Log Level: {1} | Message: {0}", message, type));

            if (_loggingService != null)
            {
                switch (type)
                {
                    case LogMessageType.Info:
                        _loggingService.LogInfo(message);
                        break;
                    case LogMessageType.Warning:
                        _loggingService.LogWarning(message);
                        break;
                    default:
                        _loggingService.LogError(message);
                        break;
                }
            }
        }
    }
}