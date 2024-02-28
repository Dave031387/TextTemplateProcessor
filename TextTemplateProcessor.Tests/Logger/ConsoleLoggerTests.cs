namespace TextTemplateProcessor.Logger
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
        private readonly Expression<Func<ILocater, string>> _locaterCurrentSegmentExpression = x => x.CurrentSegment;
        private readonly Expression<Func<ILocater, int>> _locaterLineNumberExpression = x => x.LineNumber;
        private readonly Mock<IMessageWriter> _messageWriter = new();

        public ConsoleLoggerTests()
        {
            _locater.Reset();
            _locater.Setup(_locaterCurrentSegmentExpression).Returns(SegmentName);
            _locater.Setup(_locaterLineNumberExpression).Returns(LineNumber);
            _messageWriter.Reset();
        }

        [Fact]
        internal void Clear_LoggerContainsMultipleLogEntries_ClearsAllLogEntries()
        {
            // Arrange
            ConsoleLogger consoleLogger = new(_messageWriter.Object, _locater.Object);
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
        internal void ConsoleLogger_DefaultConstructor_InitializesAllDependencies()
        {
            // Arrange
            Action action = () => { _ = new ConsoleLogger(); };

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
            ConsoleLogger consoleLogger = new(_messageWriter.Object, _locater.Object);
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
            ConsoleLogger consoleLogger = new(_messageWriter.Object, _locater.Object);
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
            ConsoleLogger consoleLogger = new(_messageWriter.Object, _locater.Object);
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
            ConsoleLogger consoleLogger = new(_messageWriter.Object, _locater.Object);
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
            ConsoleLogger consoleLogger = new(_messageWriter.Object, _locater.Object);
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
            ConsoleLogger consoleLogger = new(_messageWriter.Object, _locater.Object);
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
            ConsoleLogger consoleLogger = new(_messageWriter.Object, _locater.Object);
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
            List<string> writeBuffer = new();
            ConsoleLogger consoleLogger = new(_messageWriter.Object, _locater.Object);
            string expectedMessage1 = $"<{LogEntryType.Generating}> {SegmentName}[{LineNumber}] : {_expectedMessage}";
            string expectedMessage2 = $"<{LogEntryType.Writing}> {_expectedMessage}";
            string expectedMessage3 = $"<{LogEntryType.Parsing}> {SegmentName}[{LineNumber}] : {_expectedMessage}";
            string expectedMessage4 = $"<{LogEntryType.Reset}> {_expectedMessage}";
            string expectedMessage5 = $"<{LogEntryType.Loading}> {_expectedMessage}";
            string expectedMessage6 = $"<{LogEntryType.Setup}> {_expectedMessage}";
            List<string> expectedMessages = new()
            {
                expectedMessage1,
                expectedMessage2,
                expectedMessage3,
                expectedMessage4,
                expectedMessage5,
                expectedMessage6
            };
            WriteLogEntries(consoleLogger, true);
            _messageWriter.Setup(x => x.WriteLine(expectedMessage1))
                .Callback((string x) => writeBuffer.Add(x));
            _messageWriter.Setup(x => x.WriteLine(expectedMessage2))
                .Callback((string x) => writeBuffer.Add(x));
            _messageWriter.Setup(x => x.WriteLine(expectedMessage3))
                .Callback((string x) => writeBuffer.Add(x));
            _messageWriter.Setup(x => x.WriteLine(expectedMessage4))
                .Callback((string x) => writeBuffer.Add(x));
            _messageWriter.Setup(x => x.WriteLine(expectedMessage5))
                .Callback((string x) => writeBuffer.Add(x));
            _messageWriter.Setup(x => x.WriteLine(expectedMessage6))
                .Callback((string x) => writeBuffer.Add(x));

            // Act
            consoleLogger.WriteLogEntries();

            // Assert
            consoleLogger.LogEntries
                .Should()
                .BeEmpty();
            writeBuffer
                .Should()
                .ContainInConsecutiveOrder(expectedMessages);
            _messageWriter.Verify(x => x.WriteLine(expectedMessage1), Times.Once);
            _messageWriter.Verify(x => x.WriteLine(expectedMessage2), Times.Once);
            _messageWriter.Verify(x => x.WriteLine(expectedMessage3), Times.Once);
            _messageWriter.Verify(x => x.WriteLine(expectedMessage4), Times.Once);
            _messageWriter.Verify(x => x.WriteLine(expectedMessage5), Times.Once);
            _messageWriter.Verify(x => x.WriteLine(expectedMessage6), Times.Once);
            VerifyMocks(0);
        }

        private static void Log(ILogger logger, LogEntryType type, string message, string? arg1 = null, string? arg2 = null)
        {
            logger.SetLogEntryType(type);
            logger.Log(message, arg1, arg2);
        }

        private void VerifyMocks(int locaterCallCount)
        {
            if (locaterCallCount > 0)
            {
                _locater.Verify(_locaterCurrentSegmentExpression, Times.Exactly(locaterCallCount));
                _locater.Verify(_locaterLineNumberExpression, Times.Exactly(locaterCallCount));
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