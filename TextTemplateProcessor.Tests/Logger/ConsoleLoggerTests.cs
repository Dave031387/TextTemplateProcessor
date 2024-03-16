﻿namespace TextTemplateProcessor.Logger
{
    using System.Linq.Expressions;

    public class ConsoleLoggerTests
    {
        private const int LineNumber = 10;
        private const string SegmentName = "Segment1";
        private readonly string _expectedMessage = "This is a test";
        private readonly string _formatItem1 = "is a";
        private readonly string _formatItem2 = "test";
        private readonly string _formatStringNoArgs = "This is a test";
        private readonly string _formatStringOneArg = "This {0} test";
        private readonly string _formatStringTwoArgs = "This {0} {1}";
        private readonly Mock<ILocater> _locater = new();
        private readonly Mock<IMessageWriter> _messageWriter = new();
        private readonly MockHelper _mh = new();

        [Fact]
        internal void Clear_LoggerContainsMultipleLogEntries_ClearsAllLogEntries()
        {
            // Arrange
            InitializeMocks();
            ConsoleLogger consoleLogger = GetConsoleLogger();
            WriteLogEntries(consoleLogger, true);

            // Act
            consoleLogger.Clear();

            // Assert
            consoleLogger.LogEntries
                .Should()
                .BeEmpty();
            VerifyMocks(0);
        }

        [Fact]
        internal void ConsoleLogger_ConstructUsingNullLocaterObject_ThrowsException()
        {
            // Arrange
            Action action = () => { _ = new ConsoleLogger(_messageWriter.Object, null!); };
            string expected = GetNullDependencyMessage(ClassNames.ConsoleLoggerClass,
                                                       ServiceNames.LocaterService,
                                                       ServiceParameterNames.LocaterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        internal void ConsoleLogger_ConstructUsingNullMessageWriterObject_ThrowsException()
        {
            // Arrange
            Action action = () => { _ = new ConsoleLogger(null!, _locater.Object); };
            string expected = GetNullDependencyMessage(ClassNames.ConsoleLoggerClass,
                                                       ServiceNames.MessageWriterService,
                                                       ServiceParameterNames.MessageWriterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        internal void ConsoleLogger_ConstructUsingValidDependencies_InitializesAllDependencies()
        {
            // Arrange
            InitializeMocks();
            Action action = () => { GetConsoleLogger(); };

            // Act/Assert
            action
                .Should()
                .NotThrow();
            VerifyMocks(0);
        }

        [Fact]
        internal void Log_MultipleMessages_AllMessagesAreLogged()
        {
            // Arrange
            InitializeMocks();
            ConsoleLogger consoleLogger = GetConsoleLogger();
            List<LogEntry> expectedLogEntries = new()
            {
                new(LogEntryType.Generating, SegmentName, LineNumber, _expectedMessage),
                new(LogEntryType.Writing, string.Empty, 0, _expectedMessage),
                new(LogEntryType.Parsing, SegmentName, LineNumber, _expectedMessage),
                new(LogEntryType.Reset, string.Empty, 0, _expectedMessage),
                new(LogEntryType.Loading, string.Empty, 0, _expectedMessage),
                new(LogEntryType.Setup, string.Empty, 0, _expectedMessage)
            };

            // Act
            WriteLogEntries(consoleLogger);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .HaveCount(expectedLogEntries.Count);
            consoleLogger.LogEntries
                .Should()
                .ContainInConsecutiveOrder(expectedLogEntries);
            VerifyMocks(2);
        }

        [Theory]
        [InlineData(LogEntryType.Setup, "Setup Message")]
        [InlineData(LogEntryType.Writing, "Writing Message")]
        [InlineData(LogEntryType.Loading, "Loading Message")]
        [InlineData(LogEntryType.Reset, "Reset Message")]
        internal void Log_NoLocationAndNoFormatItems_LogsCorrectMessage(LogEntryType logEntryType, string message)
        {
            // Arrange
            InitializeMocks();
            ConsoleLogger consoleLogger = GetConsoleLogger();
            LogEntry expected = new(logEntryType, string.Empty, 0, message);

            // Act
            Log(consoleLogger, logEntryType, message);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            VerifyMocks(0);
        }

        [Theory]
        [InlineData(LogEntryType.Setup)]
        [InlineData(LogEntryType.Writing)]
        [InlineData(LogEntryType.Loading)]
        [InlineData(LogEntryType.Reset)]
        internal void Log_NoLocationAndOneFormatItem_LogsCorrectMessage(LogEntryType logEntryType)
        {
            // Arrange
            InitializeMocks();
            ConsoleLogger consoleLogger = GetConsoleLogger();
            string formatString = "This {0} test";
            string formatItem = "is a";
            string expectedMessage = "This is a test";
            LogEntry expected = new(logEntryType, string.Empty, 0, expectedMessage);

            // Act
            Log(consoleLogger, logEntryType, formatString, formatItem);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            VerifyMocks(0);
        }

        [Theory]
        [InlineData(LogEntryType.Setup)]
        [InlineData(LogEntryType.Writing)]
        [InlineData(LogEntryType.Loading)]
        [InlineData(LogEntryType.Reset)]
        internal void Log_NoLocationAndTwoFormatItems_LogsCorrectMessage(LogEntryType logEntryType)
        {
            // Arrange
            InitializeMocks();
            ConsoleLogger consoleLogger = GetConsoleLogger();
            string formatString = "This {0} {1}";
            string formatItem1 = "is a";
            string formatItem2 = "test";
            string expectedMessage = "This is a test";
            LogEntry expected = new(logEntryType, string.Empty, 0, expectedMessage);

            // Act
            Log(consoleLogger, logEntryType, formatString, formatItem1, formatItem2);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            VerifyMocks(0);
        }

        [Theory]
        [InlineData(LogEntryType.Generating, "Generating Message")]
        [InlineData(LogEntryType.Parsing, "Parsing Message")]
        internal void Log_WithLocationAndNoFormatItems_LogsCorrectMessage(LogEntryType logEntryType, string message)
        {
            // Arrange
            InitializeMocks();
            ConsoleLogger consoleLogger = GetConsoleLogger();
            LogEntry expected = new(logEntryType, SegmentName, LineNumber, message);

            // Act
            Log(consoleLogger, logEntryType, message);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            VerifyMocks(1);
        }

        [Theory]
        [InlineData(LogEntryType.Generating)]
        [InlineData(LogEntryType.Parsing)]
        internal void Log_WithLocationAndOneFormatItem_LogsCorrectMessage(LogEntryType logEntryType)
        {
            // Arrange
            InitializeMocks();
            ConsoleLogger consoleLogger = GetConsoleLogger();
            string formatString = "This {0} test";
            string formatItem = "is a";
            string expectedMessage = "This is a test";
            LogEntry expected = new(logEntryType, SegmentName, LineNumber, expectedMessage);

            // Act
            Log(consoleLogger, logEntryType, formatString, formatItem);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            VerifyMocks(1);
        }

        [Theory]
        [InlineData(LogEntryType.Generating)]
        [InlineData(LogEntryType.Parsing)]
        internal void Log_WithLocationAndTwoFormatItems_LogsCorrectMessage(LogEntryType logEntryType)
        {
            // Arrange
            InitializeMocks();
            ConsoleLogger consoleLogger = GetConsoleLogger();
            string formatString = "This {0} {1}";
            string formatItem1 = "is a";
            string formatItem2 = "test";
            string expectedMessage = "This is a test";
            LogEntry expected = new(logEntryType, SegmentName, LineNumber, expectedMessage);

            // Act
            Log(consoleLogger, logEntryType, formatString, formatItem1, formatItem2);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            VerifyMocks(1);
        }

        [Fact]
        internal void WriteLogEntries_LoggerContainsLogEntries_WritesAllLogEntriesAndClearsTheBuffer()
        {
            // Arrange
            InitializeMocks();
            ConsoleLogger consoleLogger = GetConsoleLogger();
            List<string> expectedMessages = new()
            {
                $"<{LogEntryType.Generating}> {SegmentName}[{LineNumber}] : {_expectedMessage}",
                $"<{LogEntryType.Writing}> {_expectedMessage}",
                $"<{LogEntryType.Parsing}> {SegmentName}[{LineNumber}] : {_expectedMessage}",
                $"<{LogEntryType.Reset}> {_expectedMessage}",
                $"<{LogEntryType.Loading}> {_expectedMessage}",
                $"<{LogEntryType.Setup}> {_expectedMessage}"
            };
            WriteLogEntries(consoleLogger, true);
            List<string> writeBuffer = new();
            void callback(string x) => writeBuffer.Add(x);
            List<Expression<Action<IMessageWriter>>> messageWriterExpressions = new();
            for (int i = 0; i < expectedMessages.Count; i++)
            {
                messageWriterExpressions.Add(MockHelper.SetupMessageWriter(_messageWriter, expectedMessages[i], callback));
            }

            // Act
            consoleLogger.WriteLogEntries();

            // Assert
            consoleLogger.LogEntries
                .Should()
                .BeEmpty();
            writeBuffer
                .Should()
                .ContainInConsecutiveOrder(expectedMessages);
            for (int i = 0; i < messageWriterExpressions.Count; i++)
            {
                _messageWriter.Verify(messageWriterExpressions[i], Times.Once);
            }
            VerifyMocks(0);
        }

        private static void Log(ILogger logger, LogEntryType type, string message, string? arg1 = null, string? arg2 = null)
        {
            logger.SetLogEntryType(type);
            logger.Log(message, arg1, arg2);
        }

        private ConsoleLogger GetConsoleLogger()
            => new(_messageWriter.Object, _locater.Object);

        private void InitializeMocks()
        {
            _locater.Reset();
            _mh.SetupLocater(_locater, SegmentName, LineNumber);
            _messageWriter.Reset();
        }

        private void VerifyMocks(int locaterCallCount)
        {
            if (locaterCallCount > 0)
            {
                _locater.Verify(_mh.CurrentSegmentExpression, Times.Exactly(locaterCallCount));
                _locater.Verify(_mh.LineNumberExpression, Times.Exactly(locaterCallCount));
            }

            _locater.VerifyNoOtherCalls();
            _messageWriter.VerifyNoOtherCalls();
        }

        private void WriteLogEntries(ILogger logger, bool resetLocater = false)
        {
            Log(logger, LogEntryType.Generating, _formatStringNoArgs);
            Log(logger, LogEntryType.Writing, _formatStringNoArgs);
            Log(logger, LogEntryType.Parsing, _formatStringOneArg, _formatItem1);
            Log(logger, LogEntryType.Reset, _formatStringOneArg, _formatItem1);
            Log(logger, LogEntryType.Loading, _formatStringTwoArgs, _formatItem1, _formatItem2);
            Log(logger, LogEntryType.Setup, _formatStringTwoArgs, _formatItem1, _formatItem2);

            if (resetLocater)
            {
                _locater.Reset();
            }
        }
    }
}