using TwitterStream.Entities;

namespace TwitterStream.Interfaces
{
    public interface ILogger
    {
        void HandleMessage(string message, LogMessageType type);
    }
}