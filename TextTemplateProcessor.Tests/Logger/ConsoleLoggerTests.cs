namespace TextTemplateProcessor.Logger
{
    public class ConsoleLoggerTests
    {
        private const int LineNumber = 10;
        private const string SegmentName = "Segment1";

        [Fact]
        internal void Clear_LoggerContainsMultipleLogEntries_ClearsAllLogEntries()
        {
            // Arrange
            Mock<IMessageWriter> messageWriter = new();
            ConsoleLogger consoleLogger = new(messageWriter.Object);
            string formatString1 = "This {0} test";
            string formatString2 = "This {0} {1}";
            string formatItem1 = "is a";
            string formatItem2 = "test";
            string expectedMessage = "This is a test";
            consoleLogger.Log(LogEntryType.Generating, (SegmentName, LineNumber), expectedMessage);
            consoleLogger.Log(LogEntryType.Writing, expectedMessage);
            consoleLogger.Log(LogEntryType.Parsing, (SegmentName, LineNumber), formatString1, formatItem1);
            consoleLogger.Log(LogEntryType.Reset, formatString1, formatItem1);
            consoleLogger.Log(LogEntryType.Loading, (SegmentName, LineNumber), formatString2, formatItem1, formatItem2);
            consoleLogger.Log(LogEntryType.Setup, formatString2, formatItem1, formatItem2);

            // Act
            consoleLogger.Clear();

            // Assert
            consoleLogger.LogEntries
                .Should()
                .BeEmpty();
            messageWriter
                .VerifyNoOtherCalls();
        }

        [Fact]
        internal void ConsoleLogger_ConstructUsingNullMessageWriterObject_ThrowsException()
        {
            // Arrange
            Action action = () => { _ = new ConsoleLogger(null!); };
            string expected = GetNullDependencyMessage(
                ClassNames.ConsoleLoggerClass,
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
        }

        [Fact]
        internal void Log_MultipleMessages_AllMessagesAreLogged()
        {
            // Arrange
            Mock<IMessageWriter> messageWriter = new();
            ConsoleLogger consoleLogger = new(messageWriter.Object);
            string formatString1 = "This {0} test";
            string formatString2 = "This {0} {1}";
            string formatItem1 = "is a";
            string formatItem2 = "test";
            string expectedMessage = "This is a test";
            List<LogEntry> expectedLogEntries = new()
            {
                new(LogEntryType.Generating, SegmentName, LineNumber, expectedMessage),
                new(LogEntryType.Writing, string.Empty, 0, expectedMessage),
                new(LogEntryType.Parsing, SegmentName, LineNumber, expectedMessage),
                new(LogEntryType.Reset, string.Empty, 0, expectedMessage),
                new(LogEntryType.Loading, string.Empty, 0, expectedMessage),
                new(LogEntryType.Setup, string.Empty, 0, expectedMessage)
            };

            // Act
            consoleLogger.Log(LogEntryType.Generating, (SegmentName, LineNumber), expectedMessage);
            consoleLogger.Log(LogEntryType.Writing, expectedMessage);
            consoleLogger.Log(LogEntryType.Parsing, (SegmentName, LineNumber), formatString1, formatItem1);
            consoleLogger.Log(LogEntryType.Reset, formatString1, formatItem1);
            consoleLogger.Log(LogEntryType.Loading, (SegmentName, LineNumber), formatString2, formatItem1, formatItem2);
            consoleLogger.Log(LogEntryType.Setup, formatString2, formatItem1, formatItem2);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .HaveCount(expectedLogEntries.Count);
            consoleLogger.LogEntries
                .Should()
                .ContainInConsecutiveOrder(expectedLogEntries);
        }

        [Theory]
        [InlineData(LogEntryType.Setup, "Setup Message")]
        [InlineData(LogEntryType.Generating, "Generating Message")]
        [InlineData(LogEntryType.Writing, "Writing Message")]
        [InlineData(LogEntryType.Parsing, "Parsing Message")]
        [InlineData(LogEntryType.Loading, "Loading Message")]
        [InlineData(LogEntryType.Reset, "Reset Message")]
        internal void Log_NoLocationAndNoFormatItems_LogsCorrectMessage(LogEntryType logEntryType, string message)
        {
            // Arrange
            Mock<IMessageWriter> messageWriter = new();
            ConsoleLogger consoleLogger = new(messageWriter.Object);
            LogEntry expected = new(logEntryType, string.Empty, 0, message);

            // Act
            consoleLogger.Log(logEntryType, message);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            messageWriter
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(LogEntryType.Setup)]
        [InlineData(LogEntryType.Generating)]
        [InlineData(LogEntryType.Writing)]
        [InlineData(LogEntryType.Parsing)]
        [InlineData(LogEntryType.Loading)]
        [InlineData(LogEntryType.Reset)]
        internal void Log_NoLocationAndOneFormatItem_LogsCorrectMessage(LogEntryType logEntryType)
        {
            // Arrange
            Mock<IMessageWriter> messageWriter = new();
            ConsoleLogger consoleLogger = new(messageWriter.Object);
            string formatString = "This {0} test";
            string formatItem = "is a";
            string expectedMessage = "This is a test";
            LogEntry expected = new(logEntryType, string.Empty, 0, expectedMessage);

            // Act
            consoleLogger.Log(logEntryType, formatString, formatItem);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            messageWriter
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(LogEntryType.Setup)]
        [InlineData(LogEntryType.Generating)]
        [InlineData(LogEntryType.Writing)]
        [InlineData(LogEntryType.Parsing)]
        [InlineData(LogEntryType.Loading)]
        [InlineData(LogEntryType.Reset)]
        internal void Log_NoLocationAndTwoFormatItems_LogsCorrectMessage(LogEntryType logEntryType)
        {
            // Arrange
            Mock<IMessageWriter> messageWriter = new();
            ConsoleLogger consoleLogger = new(messageWriter.Object);
            string formatString = "This {0} {1}";
            string formatItem1 = "is a";
            string formatItem2 = "test";
            string expectedMessage = "This is a test";
            LogEntry expected = new(logEntryType, string.Empty, 0, expectedMessage);

            // Act
            consoleLogger.Log(logEntryType, formatString, formatItem1, formatItem2);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            messageWriter
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(LogEntryType.Setup, "", 0, "Setup Message")]
        [InlineData(LogEntryType.Generating, SegmentName, LineNumber, "Generating Message")]
        [InlineData(LogEntryType.Writing, "", 0, "Writing Message")]
        [InlineData(LogEntryType.Parsing, SegmentName, LineNumber, "Parsing Message")]
        [InlineData(LogEntryType.Loading, "", 0, "Loading Message")]
        [InlineData(LogEntryType.Reset, "", 0, "Reset Message")]
        internal void Log_WithLocationAndNoFormatItems_LogsCorrectMessage(
            LogEntryType logEntryType,
            string segmentName,
            int lineNumber,
            string message)
        {
            // Arrange
            Mock<IMessageWriter> messageWriter = new();
            ConsoleLogger consoleLogger = new(messageWriter.Object);
            LogEntry expected = new(logEntryType, segmentName, lineNumber, message);

            // Act
            consoleLogger.Log(logEntryType, (SegmentName, LineNumber), message);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            messageWriter
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(LogEntryType.Setup, "", 0)]
        [InlineData(LogEntryType.Generating, SegmentName, LineNumber)]
        [InlineData(LogEntryType.Writing, "", 0)]
        [InlineData(LogEntryType.Parsing, SegmentName, LineNumber)]
        [InlineData(LogEntryType.Loading, "", 0)]
        [InlineData(LogEntryType.Reset, "", 0)]
        internal void Log_WithLocationAndOneFormatItem_LogsCorrectMessage(
            LogEntryType logEntryType,
            string segmentName,
            int lineNumber)
        {
            // Arrange
            Mock<IMessageWriter> messageWriter = new();
            ConsoleLogger consoleLogger = new(messageWriter.Object);
            string formatString = "This {0} test";
            string formatItem = "is a";
            string expectedMessage = "This is a test";
            LogEntry expected = new(logEntryType, segmentName, lineNumber, expectedMessage);

            // Act
            consoleLogger.Log(logEntryType, (SegmentName, LineNumber), formatString, formatItem);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            messageWriter
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(LogEntryType.Setup, "", 0)]
        [InlineData(LogEntryType.Generating, SegmentName, LineNumber)]
        [InlineData(LogEntryType.Writing, "", 0)]
        [InlineData(LogEntryType.Parsing, SegmentName, LineNumber)]
        [InlineData(LogEntryType.Loading, "", 0)]
        [InlineData(LogEntryType.Reset, "", 0)]
        internal void Log_WithLocationAndTwoFormatItems_LogsCorrectMessage(
            LogEntryType logEntryType,
            string segmentName,
            int lineNumber)
        {
            // Arrange
            Mock<IMessageWriter> messageWriter = new();
            ConsoleLogger consoleLogger = new(messageWriter.Object);
            string formatString = "This {0} {1}";
            string formatItem1 = "is a";
            string formatItem2 = "test";
            string expectedMessage = "This is a test";
            LogEntry expected = new(logEntryType, segmentName, lineNumber, expectedMessage);

            // Act
            consoleLogger.Log(logEntryType, (SegmentName, LineNumber), formatString, formatItem1, formatItem2);

            // Assert
            consoleLogger.LogEntries
                .Should()
                .ContainSingle();
            consoleLogger.LogEntries
                .Should()
                .Contain(expected);
            messageWriter
                .VerifyNoOtherCalls();
        }

        [Fact]
        internal void WriteLogEntries_LoggerContainsLogEntries_WritesAllLogEntriesAndClearsTheBuffer()
        {
            // Arrange
            List<string> writeBuffer = new();
            Mock<IMessageWriter> messageWriter = new();
            ConsoleLogger consoleLogger = new(messageWriter.Object);
            string formatString1 = "This {0} test";
            string formatString2 = "This {0} {1}";
            string formatItem1 = "is a";
            string formatItem2 = "test";
            string expectedMessage = "This is a test";
            string expectedMessage1 = $"<{LogEntryType.Generating}> {SegmentName}[{LineNumber}] : {expectedMessage}";
            string expectedMessage2 = $"<{LogEntryType.Writing}> {expectedMessage}";
            string expectedMessage3 = $"<{LogEntryType.Parsing}> {SegmentName}[{LineNumber}] : {expectedMessage}";
            string expectedMessage4 = $"<{LogEntryType.Reset}> {expectedMessage}";
            string expectedMessage5 = $"<{LogEntryType.Loading}> {expectedMessage}";
            string expectedMessage6 = $"<{LogEntryType.Setup}> {expectedMessage}";
            List<string> expectedMessages = new()
            { expectedMessage1, expectedMessage2, expectedMessage3, expectedMessage4, expectedMessage5, expectedMessage6 };
            consoleLogger.Log(LogEntryType.Generating, (SegmentName, LineNumber), expectedMessage);
            consoleLogger.Log(LogEntryType.Writing, expectedMessage);
            consoleLogger.Log(LogEntryType.Parsing, (SegmentName, LineNumber), formatString1, formatItem1);
            consoleLogger.Log(LogEntryType.Reset, formatString1, formatItem1);
            consoleLogger.Log(LogEntryType.Loading, (SegmentName, LineNumber), formatString2, formatItem1, formatItem2);
            consoleLogger.Log(LogEntryType.Setup, formatString2, formatItem1, formatItem2);
            messageWriter.Setup(x => x.WriteLine(expectedMessage1))
                .Callback((string x) => writeBuffer.Add(x));
            messageWriter.Setup(x => x.WriteLine(expectedMessage2))
                .Callback((string x) => writeBuffer.Add(x));
            messageWriter.Setup(x => x.WriteLine(expectedMessage3))
                .Callback((string x) => writeBuffer.Add(x));
            messageWriter.Setup(x => x.WriteLine(expectedMessage4))
                .Callback((string x) => writeBuffer.Add(x));
            messageWriter.Setup(x => x.WriteLine(expectedMessage5))
                .Callback((string x) => writeBuffer.Add(x));
            messageWriter.Setup(x => x.WriteLine(expectedMessage6))
                .Callback((string x) => writeBuffer.Add(x));

            // Act
            consoleLogger.WriteLogEntries();

            // Assert
            consoleLogger.LogEntries
                .Should()
                .BeEmpty();
            messageWriter.Verify(x => x.WriteLine(expectedMessage1), Times.Once);
            messageWriter.Verify(x => x.WriteLine(expectedMessage2), Times.Once);
            messageWriter.Verify(x => x.WriteLine(expectedMessage3), Times.Once);
            messageWriter.Verify(x => x.WriteLine(expectedMessage4), Times.Once);
            messageWriter.Verify(x => x.WriteLine(expectedMessage5), Times.Once);
            messageWriter.Verify(x => x.WriteLine(expectedMessage6), Times.Once);
            writeBuffer
                .Should()
                .ContainInConsecutiveOrder(expectedMessages);
            messageWriter.VerifyNoOtherCalls();
        }
    }
}