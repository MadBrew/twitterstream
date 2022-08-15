using Moq;
using TwitterStream.Entities;
using TwitterStream.Interfaces;

namespace TwitterStream.Tests.Entities
{
    internal class LoggerTests
    {
        Mock<ILoggingService> _loggingService;

        [SetUp]
        public void Init()
        {
            _loggingService = new Mock<ILoggingService>();
        }

        [Test]
        public void TwitterConfig_WhenError_LogsError()
        {
            // Arrange
            const string message = "My error message.";
            var objectUnderTest = new Logger(_loggingService.Object);

            // Act.
            objectUnderTest.HandleMessage(message, LogMessageType.Error);

            // Assert.
            _loggingService.Verify(_ => _.LogError(message), Times.Once);
            _loggingService.Verify(_ => _.LogInfo(It.IsAny<string>()), Times.Never);
            _loggingService.Verify(_ => _.LogWarning(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void TwitterConfig_WhenInfo_LogsInfo()
        {
            // Arrange
            const string message = "My info message.";
            var objectUnderTest = new Logger(_loggingService.Object);

            // Act.
            objectUnderTest.HandleMessage(message, LogMessageType.Info);

            // Assert.
            _loggingService.Verify(_ => _.LogError(It.IsAny<string>()), Times.Never);
            _loggingService.Verify(_ => _.LogInfo(message), Times.Once);
            _loggingService.Verify(_ => _.LogWarning(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void TwitterConfig_WhenWarning_LogsWarning()
        {
            // Arrange
            const string message = "My warning message.";
            var objectUnderTest = new Logger(_loggingService.Object);

            // Act.
            objectUnderTest.HandleMessage(message, LogMessageType.Warning);

            // Assert.
            _loggingService.Verify(_ => _.LogError(It.IsAny<string>()), Times.Never);
            _loggingService.Verify(_ => _.LogInfo(It.IsAny<string>()), Times.Never);
            _loggingService.Verify(_ => _.LogWarning(message), Times.Once);
        }

        [Test]
        public void TwitterConfig_WhenNull_CallsNothing()
        {
            // Arrange
            const string message = "My error message.";
            var objectUnderTest = new Logger(null!);

            // Act.
            objectUnderTest.HandleMessage(message, LogMessageType.Error);

            // Assert.
            _loggingService.Verify(_ => _.LogError(It.IsAny<string>()), Times.Never);
            _loggingService.Verify(_ => _.LogInfo(It.IsAny<string>()), Times.Never);
            _loggingService.Verify(_ => _.LogWarning(It.IsAny<string>()), Times.Never);
        }
    }
}