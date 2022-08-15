using TwitterStream.Entities;

namespace TwitterStream.Interfaces
{
    public interface ILoggingService
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}