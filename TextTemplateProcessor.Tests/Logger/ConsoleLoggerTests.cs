namespace TextTemplateProcessor.Logger
{
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
        private readonly Mock<ILocater> _locater = new(MockBehavior.Strict);
        private readonly Mock<IMessageWriter> _messageWriter = new(MockBehavior.Strict);

        [Fact]
        internal void Clear_LoggerContainsMultipleLogEntries_ClearsAllLogEntries()
        {
            // Arrange
            InitializeMocks();
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Returns(SegmentName);
            _locater
                .Setup(locater => locater.LineNumber)
                .Returns(LineNumber);
            ConsoleLogger consoleLogger = GetConsoleLogger();
            WriteLogEntries(consoleLogger);
            InitializeMocks();

            // Act
            consoleLogger.Clear();

            // Assert
            consoleLogger.LogEntries
                .Should()
                .BeEmpty();
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        internal void ConsoleLogger_ConstructUsingNullLocaterObject_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () => { _ = new ConsoleLogger(null!, _messageWriter.Object); };
            string expected = GetNullDependencyMessage(ClassNames.ConsoleLoggerClass,
                                                       ServiceNames.LocaterService,
                                                       ServiceParameterNames.LocaterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        internal void ConsoleLogger_ConstructUsingNullMessageWriterObject_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () => { _ = new ConsoleLogger(_locater.Object, null!); };
            string expected = GetNullDependencyMessage(ClassNames.ConsoleLoggerClass,
                                                       ServiceNames.MessageWriterService,
                                                       ServiceParameterNames.MessageWriterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        internal void ConsoleLogger_ConstructUsingValidDependencies_InitializesAllDependencies()
        {
            // Arrange
            InitializeMocks();
            Action action = () => { _ = GetConsoleLogger(); };

            // Act/Assert
            action
                .Should()
                .NotThrow();
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        internal void Log_MultipleMessages_AllMessagesAreLogged()
        {
            // Arrange
            InitializeMocks();
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.Exactly(2));
            _locater
                .Setup(locater => locater.LineNumber)
                .Returns(LineNumber)
                .Verifiable(Times.Exactly(2));
            ConsoleLogger consoleLogger = GetConsoleLogger();
            List<LogEntry> expectedLogEntries =
            [
                new(LogEntryType.Generating, SegmentName, LineNumber, _expectedMessage),
                new(LogEntryType.Writing, string.Empty, 0, _expectedMessage),
                new(LogEntryType.Parsing, SegmentName, LineNumber, _expectedMessage),
                new(LogEntryType.Reset, string.Empty, 0, _expectedMessage),
                new(LogEntryType.Loading, string.Empty, 0, _expectedMessage),
                new(LogEntryType.Setup, string.Empty, 0, _expectedMessage),
                new(LogEntryType.User, string.Empty, 0, _expectedMessage)
            ];

            // Act
            WriteLogEntries(consoleLogger);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .HaveSameCount(expectedLogEntries);
            consoleLogger.LogEntries
                .Should()
                .ContainInConsecutiveOrder(expectedLogEntries);
            VerifyMocks();
        }

        [Theory]
        [InlineData(LogEntryType.Setup, "Setup Message")]
        [InlineData(LogEntryType.Writing, "Writing Message")]
        [InlineData(LogEntryType.Loading, "Loading Message")]
        [InlineData(LogEntryType.Reset, "Reset Message")]
        [InlineData(LogEntryType.User, "User Message")]
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
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(LogEntryType.Setup)]
        [InlineData(LogEntryType.Writing)]
        [InlineData(LogEntryType.Loading)]
        [InlineData(LogEntryType.Reset)]
        [InlineData(LogEntryType.User)]
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
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(LogEntryType.Setup)]
        [InlineData(LogEntryType.Writing)]
        [InlineData(LogEntryType.Loading)]
        [InlineData(LogEntryType.Reset)]
        [InlineData(LogEntryType.User)]
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
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(LogEntryType.Generating, "Generating Message")]
        [InlineData(LogEntryType.Parsing, "Parsing Message")]
        internal void Log_WithLocationAndNoFormatItems_LogsCorrectMessage(LogEntryType logEntryType, string message)
        {
            // Arrange
            InitializeMocks();
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.LineNumber)
                .Returns(LineNumber)
                .Verifiable(Times.Once);
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
            VerifyMocks();
        }

        [Theory]
        [InlineData(LogEntryType.Generating)]
        [InlineData(LogEntryType.Parsing)]
        internal void Log_WithLocationAndOneFormatItem_LogsCorrectMessage(LogEntryType logEntryType)
        {
            // Arrange
            InitializeMocks();
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.LineNumber)
                .Returns(LineNumber)
                .Verifiable(Times.Once);
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
            VerifyMocks();
        }

        [Theory]
        [InlineData(LogEntryType.Generating)]
        [InlineData(LogEntryType.Parsing)]
        internal void Log_WithLocationAndTwoFormatItems_LogsCorrectMessage(LogEntryType logEntryType)
        {
            // Arrange
            InitializeMocks();
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.LineNumber)
                .Returns(LineNumber)
                .Verifiable(Times.Once);
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
            VerifyMocks();
        }

        [Fact]
        internal void WriteLogEntries_LoggerContainsLogEntries_WritesAllLogEntriesAndClearsTheBuffer()
        {
            // Arrange
            List<string> expectedMessages =
            [
                $"<{LogEntryType.Generating}> {SegmentName}[{LineNumber}] : {_expectedMessage}",
                $"<{LogEntryType.Writing}> {_expectedMessage}",
                $"<{LogEntryType.Parsing}> {SegmentName}[{LineNumber}] : {_expectedMessage}",
                $"<{LogEntryType.Reset}> {_expectedMessage}",
                $"<{LogEntryType.Loading}> {_expectedMessage}",
                $"<{LogEntryType.Setup}> {_expectedMessage}",
                $"<{LogEntryType.User}> {_expectedMessage}"
            ];
            List<string> writeBuffer = [];

            _locater
                .Setup(locater => locater.CurrentSegment)
                .Returns(SegmentName);
            _locater
                .Setup(locater => locater.LineNumber)
                .Returns(LineNumber);
            ConsoleLogger consoleLogger = GetConsoleLogger();
            WriteLogEntries(consoleLogger);
            InitializeMocks();
            void callback(string x) => writeBuffer.Add(x);

            foreach (string message in expectedMessages)
            {
                _messageWriter
                    .Setup(messageWriter => messageWriter.WriteLine(message))
                    .Callback(callback)
                    .Verifiable(Times.Once);
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
            VerifyMocks();
        }

        private static void Log(ILogger logger, LogEntryType type, string message, string? arg1 = null, string? arg2 = null)
        {
            logger.SetLogEntryType(type);
            logger.Log(message, arg1, arg2);
        }

        private ConsoleLogger GetConsoleLogger()
            => new(_locater.Object, _messageWriter.Object);

        private void InitializeMocks()
        {
            _locater.Reset();
            _messageWriter.Reset();
        }

        private void MocksVerifyNoOtherCalls()
        {
            _locater.VerifyNoOtherCalls();
            _messageWriter.VerifyNoOtherCalls();
        }

        private void VerifyMocks()
        {
            if (_locater.Setups.Any())
            {
                _locater.Verify();
            }

            if (_messageWriter.Setups.Any())
            {
                _messageWriter.Verify();
            }

            MocksVerifyNoOtherCalls();
        }

        private void WriteLogEntries(ILogger logger)
        {
            Log(logger, LogEntryType.Generating, _formatStringNoArgs);
            Log(logger, LogEntryType.Writing, _formatStringNoArgs);
            Log(logger, LogEntryType.Parsing, _formatStringOneArg, _formatItem1);
            Log(logger, LogEntryType.Reset, _formatStringOneArg, _formatItem1);
            Log(logger, LogEntryType.Loading, _formatStringTwoArgs, _formatItem1, _formatItem2);
            Log(logger, LogEntryType.Setup, _formatStringTwoArgs, _formatItem1, _formatItem2);
            Log(logger, LogEntryType.User, _formatStringNoArgs);
        }
    }
}