namespace TextTemplateProcessor.Console
{
    public class TextTemplateConsoleBaseTests
    {
        private static readonly string _emptyString = string.Empty;
        private static readonly string _lastOutputDirectoryPath = $"{VolumeRoot}{Sep}last";
        private static readonly string _lastSegmentName = "LastSegment";
        private static readonly string _lastTemplateFileName = "last-template.txt";
        private static readonly string _lastTemplateFilePath = $"{_templateDirectoryPath}{Sep}{_lastTemplateFileName}";
        private static readonly string _outputDirectoryPath = $"{VolumeRoot}{Sep}generated";
        private static readonly string _outputFileName = "output.txt";
        private static readonly string _outputFilePath = $"{_outputDirectoryPath}{Sep}{_outputFileName}";
        private static readonly string _padTextLine = "Pad line 1";
        private static readonly string _padSegmentName = "PadSegment";
        private static readonly string _segmentName1 = "Segment1";
        private static readonly string _segmentName2 = "Segment2";
        private static readonly string _segmentName3 = "Segment3";
        private static readonly string _templateDirectoryPath = $"{VolumeRoot}{Sep}test";
        private static readonly string _templateFileName = "template.txt";
        private static readonly string _templateFilePath = $"{_templateDirectoryPath}{Sep}{_templateFileName}";
        private static readonly string _textLine1 = "Text line 1";
        private static readonly string _textLine2 = "Text line 2";
        private static readonly string _textLine3 = "Text line 3";
        private static readonly string _textLine4 = "Text line 4";
        private readonly Mock<IConsoleReader> _consoleReader = new(MockBehavior.Strict);
        private readonly Mock<IDefaultSegmentNameGenerator> _defaultSegmentNameGenerator = new(MockBehavior.Strict);
        private readonly Mock<IFileAndDirectoryService> _fileService = new(MockBehavior.Strict);
        private readonly Mock<IIndentProcessor> _indentProcessor = new(MockBehavior.Strict);
        private readonly Mock<ILocater> _locater = new(MockBehavior.Strict);
        private readonly Mock<ILogger> _logger = new(MockBehavior.Strict);
        private readonly Mock<IMessageWriter> _messageWriter = new(MockBehavior.Strict);
        private readonly Mock<IPathValidater> _pathValidater = new(MockBehavior.Strict);
        private readonly Mock<ITemplateLoader> _templateLoader = new(MockBehavior.Strict);
        private readonly Mock<ITextReader> _textReader = new(MockBehavior.Strict);
        private readonly Mock<ITextWriter> _textWriter = new(MockBehavior.Strict);
        private readonly Mock<ITokenProcessor> _tokenProcessor = new(MockBehavior.Strict);
        private readonly MethodCallOrderVerifier _verifier = new();

        [Fact]
        public void ClearOutputDirectory_ErrorWhileClearingDirectory_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = _outputDirectoryPath;
            Sequence<LogEntryType> logEntryTypeSequence = new([LogEntryType.Generating, LogEntryType.Reset], 2);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(logEntryTypeSequence.GetNext)
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Exactly(2));
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine, 2))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(ConsoleReader_ReadLine))
                .Returns("y")
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.ClearDirectory(_outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_ClearDirectory))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenClearingOutputDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, MessageWriter_WriteLine, 0, 1);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, Logger_WriteLogEntries, 1);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_User, MessageWriter_WriteLine, 0, 2);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, ConsoleReader_ReadLine, 2);
            _verifier.DefineExpectedCallOrder(ConsoleReader_ReadLine, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, FileAndDirectoryService_ClearDirectory);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_ClearDirectory, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.ClearOutputDirectory();

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void ClearOutputDirectory_ErrorWhileReadingUserResponse_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = _outputDirectoryPath;
            Sequence<LogEntryType> logEntryTypeSequence = new([LogEntryType.Generating, LogEntryType.Reset], 2);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(logEntryTypeSequence.GetNext)
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Exactly(2));
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine, 2))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(ConsoleReader_ReadLine))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToGetUserResponse, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset, -1, -1);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, MessageWriter_WriteLine, -1, 1);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, Logger_GetLogEntryType, 1, -2);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_User, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_User, MessageWriter_WriteLine, 0, 2);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, ConsoleReader_ReadLine, 2);
            _verifier.DefineExpectedCallOrder(ConsoleReader_ReadLine, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_SetLogEntryType_Reset, 0, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Logger_WriteLogEntries, -2);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.ClearOutputDirectory();

            // Assert
            VerifyMocks();
        }

        [Theory]
        [InlineData("n")]
        [InlineData("N")]
        [InlineData("No")]
        [InlineData("1")]
        [InlineData("no")]
        [InlineData("!")]
        public void ClearOutputDirectory_NegativeUserResponse_DoesNothing(string userResponse)
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = _outputDirectoryPath;
            Sequence<LogEntryType> logEntryTypeSequence = new([LogEntryType.Generating, LogEntryType.Reset], 2);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(logEntryTypeSequence.GetNext)
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Exactly(2));
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine, 2))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(ConsoleReader_ReadLine))
                .Returns(userResponse)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset, -1, -1);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, MessageWriter_WriteLine, -1, 1);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, Logger_GetLogEntryType, 1, -2);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_User, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_User, MessageWriter_WriteLine, 0, 2);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, ConsoleReader_ReadLine, 2);
            _verifier.DefineExpectedCallOrder(ConsoleReader_ReadLine, Logger_SetLogEntryType_Reset, 0, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Logger_WriteLogEntries, -2);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.ClearOutputDirectory();

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void ClearOutputDirectory_OutputDirectoryNotSet_DoesNothing()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();

            // Act
            consoleBase.ClearOutputDirectory();

            // Assert
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("y")]
        [InlineData("Y")]
        [InlineData("yes")]
        [InlineData("Yes")]
        public void ClearOutputDirectory_PositiveUserResponse_ClearsDirectoryAndLogsMessage(string userResponse)
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = _outputDirectoryPath;
            Sequence<LogEntryType> logEntryTypeSequence = new([LogEntryType.Generating, LogEntryType.Reset], 2);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(logEntryTypeSequence.GetNext)
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Exactly(2));
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine, 2))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(ConsoleReader_ReadLine))
                .Returns(userResponse)
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.ClearDirectory(_outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_ClearDirectory))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgOutputDirectoryCleared, null, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset, -1, -1);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, MessageWriter_WriteLine, -1, 1);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, Logger_GetLogEntryType, 1, -2);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_User, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_User, MessageWriter_WriteLine, 0, 2);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, ConsoleReader_ReadLine, 2);
            _verifier.DefineExpectedCallOrder(ConsoleReader_ReadLine, Logger_SetLogEntryType_Reset, 0, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, FileAndDirectoryService_ClearDirectory, -2);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_ClearDirectory, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.ClearOutputDirectory();

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void CurrentIndent_WhenCalled_CallsIndentProcessorCurrentIndent()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            int expected = 4;
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.CurrentIndent)
                .Returns(expected)
                .Verifiable(Times.Once);

            // Act
            int actual = consoleBase.CurrentIndent;

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void CurrentSegment_Getter_CallsLocaterCurrentSegmentGetter()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            string expected = _segmentName1;
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Returns(_segmentName1)
                .Verifiable(Times.Once);

            // Act
            string actual = consoleBase.CurrentSegment;

            // Assert
            actual
                .Should()
                .Be(_segmentName1);
            VerifyMocks();
        }

        [Fact]
        public void GeneratedText_Getter_ReturnsCopyOfGeneratedTextBuffer()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase._generatedText.AddRange(SampleText);

            // Act
            List<string> actual = consoleBase.GeneratedText.ToList();

            // Assert
            actual
                .Should()
                .NotBeNullOrEmpty();
            actual
                .Should()
                .NotBeSameAs(consoleBase._generatedText);
            actual
                .Should()
                .HaveSameCount(SampleText);
            actual
                .Should()
                .ContainInConsecutiveOrder(SampleText);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void GenerateSegment_IsOutputFileWrittenFlagIsTrue_GeneratesTextAndSetsFlagToFalse()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            TextItem textItem1 = new(0, true, false, _textLine1);
            TextItem textItem2 = new(1, true, false, _textLine2);
            TextItem textItem3 = new(-1, true, false, _textLine3);
            ControlItem controlItem = new();
            List<string> expected = [_textLine1, $"    {_textLine2}", _textLine3];
            consoleBase._controlDictionary[_segmentName1] = controlItem;
            consoleBase._segmentDictionary[_segmentName1] = [textItem1, textItem2, textItem3];
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            _locater
                .SetupProperty(locater => locater.CurrentSegment, _lastSegmentName);
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.TabSize)
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetTabSize))
                .Returns(4)
                .Verifiable(Times.AtLeastOnce);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetFirstTimeIndent))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, IndentProcessor_GetTabSize);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetTabSize, IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetFirstTimeIndent, TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, Logger_WriteLogEntries, 3);

            // Act
            consoleBase.GenerateSegment(_segmentName1);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(_segmentName1);
            _locater.Object.LineNumber
                .Should()
                .Be(expected.Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_PassInTokenValueDictionary_CallsLoadTokenValuesOnTokenProcessor()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            string tokenName = "Token1";
            string tokenValue = "2";
            string textLine2Before = $"Text line <#{tokenName}#>";
            string textLine2After = $"Text line {tokenValue}";
            TextItem textItem1 = new(0, true, false, _textLine1);
            TextItem textItem2 = new(0, true, false, textLine2Before);
            TextItem textItem3 = new(0, true, false, _textLine3);
            Dictionary<string, string> tokenValues = new()
            {
                [tokenName] = tokenValue
            };
            ControlItem controlItem = new();
            List<string> expected = [_textLine1, textLine2After, _textLine3];
            consoleBase._controlDictionary[_segmentName1] = controlItem;
            consoleBase._segmentDictionary[_segmentName1] = [textItem1, textItem2, textItem3];
            consoleBase.IsTemplateLoaded = true;
            _locater
                .SetupProperty(locater => locater.CurrentSegment, _lastSegmentName);
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.LoadTokenValues(tokenValues))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_LoadTokenValues))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.TabSize)
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetTabSize))
                .Returns(4)
                .Verifiable(Times.AtLeastOnce);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetFirstTimeIndent))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 1))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine2Before))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 2))
                .Returns(textLine2After)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, TokenProcessor_LoadTokenValues);
            _verifier.DefineExpectedCallOrder(TokenProcessor_LoadTokenValues, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, IndentProcessor_GetTabSize);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetTabSize, IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetFirstTimeIndent, TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, Logger_WriteLogEntries, 3);

            // Act
            consoleBase.GenerateSegment(_segmentName1, tokenValues);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(_segmentName1);
            _locater.Object.LineNumber
                .Should()
                .Be(expected.Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasFirstTimeIndentOption_GeneratesTextWithCorrectFirstTimeIndent()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            int firstTimeIndent = 2;
            TextItem textItem1 = new(0, true, false, _textLine1);
            TextItem textItem2 = new(1, true, false, _textLine2);
            TextItem textItem3 = new(-1, true, false, _textLine3);
            TextItem textItem4 = new(-1, true, false, _textLine4);
            ControlItem controlItem = new() { FirstTimeIndent = firstTimeIndent };
            List<string> expected =
            [
                $"        {_textLine1}",
                $"            {_textLine2}",
                $"        {_textLine3}",
                $"    {_textLine4}"
            ];
            consoleBase._controlDictionary[_segmentName1] = controlItem;
            consoleBase._segmentDictionary[_segmentName1] = [textItem1, textItem2, textItem3, textItem4];
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = false;
            _locater
                .SetupProperty(locater => locater.CurrentSegment, _lastSegmentName);
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.TabSize)
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetTabSize))
                .Returns(4)
                .Verifiable(Times.AtLeastOnce);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(firstTimeIndent, textItem1))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetFirstTimeIndent))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 1))
                .Returns(12)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 2))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem4))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 3))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine4))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 4))
                .Returns(_textLine4)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, IndentProcessor_GetTabSize);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetTabSize, IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetFirstTimeIndent, TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 3, 4);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, Logger_WriteLogEntries, 4);

            // Act
            consoleBase.GenerateSegment(_segmentName1);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(_segmentName1);
            _locater.Object.LineNumber
                .Should()
                .Be(expected.Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasMultipleOptionsAndIsFirstTime_GeneratesTextAndHandlesOptionsCorrectly()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            int firstTimeIndent = 1;
            int tabSize = 2;
            TextItem textItem1 = new(0, true, false, _textLine1);
            TextItem textItem2 = new(1, true, false, _textLine2);
            TextItem textItem3 = new(-1, true, false, _textLine3);
            TextItem padTextItem = new(1, true, false, _padTextLine);
            ControlItem controlItem1 = new();
            ControlItem controlItem2 = new()
            {
                FirstTimeIndent = firstTimeIndent,
                PadSegment = _padSegmentName,
                TabSize = tabSize
            };
            List<string> expected =
            [
                $"  {_textLine1}",
                $"    {_textLine2}",
                $"  {_textLine3}"
            ];
            consoleBase._controlDictionary[_padSegmentName] = controlItem1;
            consoleBase._controlDictionary[_segmentName1] = controlItem2;
            consoleBase._segmentDictionary[_padSegmentName] = [padTextItem];
            consoleBase._segmentDictionary[_segmentName1] = [textItem1, textItem2, textItem3];
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = false;
            _locater
                .SetupProperty(locater => locater.CurrentSegment, _lastSegmentName);
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.TabSize)
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetTabSize))
                .Returns(4)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(1, textItem1))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetFirstTimeIndent))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 2))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, IndentProcessor_GetTabSize);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetTabSize, Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, IndentProcessor_SetTabSize);
            _verifier.DefineExpectedCallOrder(IndentProcessor_SetTabSize, Logger_SetLogEntryType_Generating);
            _verifier.DefineExpectedCallOrder(IndentProcessor_SetTabSize, IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetFirstTimeIndent, TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, Logger_WriteLogEntries, 3);

            // Act
            consoleBase.GenerateSegment(_segmentName1);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(_segmentName1);
            _locater.Object.LineNumber
                .Should()
                .Be(consoleBase._segmentDictionary[_segmentName1].Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasMultipleOptionsAndIsNotFirstTime_GeneratesTextAndHandlesOptionsCorrectly()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            int firstTimeIndent = 1;
            int tabSize = 2;
            TextItem textItem1 = new(0, true, false, _textLine1);
            TextItem textItem2 = new(1, true, false, _textLine2);
            TextItem textItem3 = new(-1, true, false, _textLine3);
            TextItem padTextItem = new(1, true, false, _padTextLine);
            ControlItem controlItem1 = new();
            ControlItem controlItem2 = new()
            {
                FirstTimeIndent = firstTimeIndent,
                PadSegment = _padSegmentName,
                TabSize = tabSize,
                IsFirstTime = false
            };
            List<string> expected =
            [
                $"    {_padTextLine}",
                _textLine1,
                $"  {_textLine2}",
                _textLine3
            ];
            consoleBase._controlDictionary[_padSegmentName] = controlItem1;
            consoleBase._controlDictionary[_segmentName1] = controlItem2;
            consoleBase._segmentDictionary[_padSegmentName] = [padTextItem];
            consoleBase._segmentDictionary[_segmentName1] = [textItem1, textItem2, textItem3];
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = false;
            _locater
                .SetupProperty(locater => locater.CurrentSegment, "OtherSegment");
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(3));
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SaveCurrentState())
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_SaveCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _padSegmentName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.TabSize)
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetTabSize))
                .Returns(4)
                .Verifiable(Times.AtLeastOnce);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, padTextItem))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetFirstTimeIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_padTextLine))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 1))
                .Returns(_padTextLine)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.RestoreCurrentState())
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_RestoreCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem1))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 1))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 2))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 3))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 4))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, IndentProcessor_SaveCurrentState);
            _verifier.DefineExpectedCallOrder(IndentProcessor_SaveCurrentState, Logger_Log_Message, 0, 1);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, IndentProcessor_GetTabSize, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetTabSize, IndentProcessor_GetFirstTimeIndent, 0, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetFirstTimeIndent, TokenProcessor_ReplaceTokens, 1, 1);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_RestoreCurrentState, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_RestoreCurrentState, Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_GetLogEntryType, 2);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, IndentProcessor_SetTabSize);
            _verifier.DefineExpectedCallOrder(IndentProcessor_SetTabSize, Logger_SetLogEntryType_Generating);
            _verifier.DefineExpectedCallOrder(IndentProcessor_SetTabSize, IndentProcessor_GetIndent, 0, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 3, 4);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, Logger_WriteLogEntries, 4);

            // Act
            consoleBase.GenerateSegment(_segmentName1);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(_segmentName1);
            _locater.Object.LineNumber
                .Should()
                .Be(consoleBase._segmentDictionary[_segmentName1].Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasNoTextLines_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.IsTemplateLoaded = true;
            consoleBase._controlDictionary[_segmentName1] = new();
            _locater
                .SetupProperty(locater => locater.CurrentSegment);
            _locater
                .SetupProperty(locater => locater.LineNumber);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgSegmentHasNoTextLines, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);

            // Act
            consoleBase.GenerateSegment(_segmentName1);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasPadOptionAndIsFirstTime_GeneratesTextWithoutPad()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            TextItem textItem1 = new(1, true, false, _textLine1);
            TextItem textItem2 = new(1, true, false, _textLine2);
            TextItem textItem3 = new(-2, true, false, _textLine3);
            TextItem padTextItem = new(2, true, false, _padTextLine);
            ControlItem controlItem1 = new();
            ControlItem controlItem2 = new() { PadSegment = _padSegmentName };
            List<string> expected =
            [
                $"    {_textLine1}",
                $"        {_textLine2}",
                _textLine3
            ];
            consoleBase._controlDictionary[_padSegmentName] = controlItem1;
            consoleBase._controlDictionary[_segmentName1] = controlItem2;
            consoleBase._segmentDictionary[_padSegmentName] = [padTextItem];
            consoleBase._segmentDictionary[_segmentName1] = [textItem1, textItem2, textItem3];
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = false;
            _locater
                .SetupProperty(locater => locater.CurrentSegment, _lastSegmentName);
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.TabSize)
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetTabSize))
                .Returns(4)
                .Verifiable(Times.AtLeastOnce);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetFirstTimeIndent))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 1))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, IndentProcessor_GetTabSize);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetTabSize, IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetFirstTimeIndent, TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, Logger_WriteLogEntries, 3);

            // Act
            consoleBase.GenerateSegment(_segmentName1);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(_segmentName1);
            _locater.Object.LineNumber
                .Should()
                .Be(consoleBase._segmentDictionary[_segmentName1].Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasPadOptionAndNotFirstTime_GeneratesTextWithCorrectPad()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            TextItem textItem1 = new(1, true, false, _textLine1);
            TextItem textItem2 = new(1, true, false, _textLine2);
            TextItem textItem3 = new(-2, true, false, _textLine3);
            TextItem padTextItem = new(2, true, false, _padTextLine);
            ControlItem controlItem1 = new();
            ControlItem controlItem2 = new() { PadSegment = _padSegmentName, IsFirstTime = false };
            List<string> expected =
            [
                $"        {_padTextLine}",
                $"    {_textLine1}",
                $"        {_textLine2}",
                _textLine3
            ];
            consoleBase._controlDictionary[_padSegmentName] = controlItem1;
            consoleBase._controlDictionary[_segmentName1] = controlItem2;
            consoleBase._segmentDictionary[_padSegmentName] = [padTextItem];
            consoleBase._segmentDictionary[_segmentName1] = [textItem1, textItem2, textItem3];
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = false;
            _locater
                .SetupProperty(locater => locater.CurrentSegment, _lastSegmentName);
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(2));
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SaveCurrentState())
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_SaveCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _padSegmentName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.TabSize)
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetTabSize))
                .Returns(4)
                .Verifiable(Times.AtLeastOnce);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, padTextItem))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetFirstTimeIndent))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_padTextLine))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 1))
                .Returns(_padTextLine)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.RestoreCurrentState())
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_RestoreCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem1))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 2))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 3))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 4))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, IndentProcessor_SaveCurrentState);
            _verifier.DefineExpectedCallOrder(IndentProcessor_SaveCurrentState, Logger_Log_Message, 0, 1);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, IndentProcessor_GetTabSize, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetTabSize, IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetFirstTimeIndent, TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_RestoreCurrentState, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_RestoreCurrentState, Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, IndentProcessor_GetTabSize, 2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetTabSize, IndentProcessor_GetIndent, 0, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 3, 4);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, Logger_WriteLogEntries, 4);

            // Act
            consoleBase.GenerateSegment(_segmentName1);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(_segmentName1);
            _locater.Object.LineNumber
                .Should()
                .Be(consoleBase._segmentDictionary[_segmentName1].Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasTabOption_GeneratesTextWithCorrectTabSetting()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            int tabSize = 2;
            TextItem textItem1 = new(1, true, false, _textLine1);
            TextItem textItem2 = new(1, true, false, _textLine2);
            TextItem textItem3 = new(-2, true, false, _textLine3);
            ControlItem controlItem = new() { TabSize = tabSize };
            List<string> expected =
            [
                $"  {_textLine1}",
                $"    {_textLine2}",
                _textLine3
            ];
            consoleBase._controlDictionary[_segmentName1] = controlItem;
            consoleBase._segmentDictionary[_segmentName1] = [textItem1, textItem2, textItem3];
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = false;
            Sequence<int> tabSizeSequence = new([4, 4, 2]);
            _locater
                .SetupProperty(locater => locater.CurrentSegment, _lastSegmentName);
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(3));
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.TabSize)
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetTabSize))
                .Returns(tabSizeSequence.GetNext)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Exactly(2));
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_SetTabSize, 1))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(4))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_SetTabSize, 2))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetFirstTimeIndent))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, Logger_Log_Message, -1);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, IndentProcessor_GetTabSize);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetTabSize, Logger_GetLogEntryType, 0, -1);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Setup, -1, -1);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, IndentProcessor_SetTabSize, -1, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_SetTabSize, Logger_SetLogEntryType_Generating, 1, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, IndentProcessor_GetFirstTimeIndent, -2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetFirstTimeIndent, TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, Logger_GetLogEntryType, 3, -2);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Setup, -2, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, IndentProcessor_SetTabSize, -2, 2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_SetTabSize, Logger_SetLogEntryType_Generating, 2, -3);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, Logger_WriteLogEntries, -3);

            // Act
            consoleBase.GenerateSegment(_segmentName1);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(_segmentName1);
            _locater.Object.LineNumber
                .Should()
                .Be(expected.Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasTextLinesAndDefaultSegmentOptions_GeneratesText()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            TextItem textItem1 = new(2, true, false, _textLine1);
            TextItem textItem2 = new(0, true, false, _textLine2);
            TextItem textItem3 = new(-1, true, false, _textLine3);
            TextItem textItem4 = new(2, true, false, _textLine4);
            ControlItem controlItem = new();
            List<string> expected =
            [
                $"        {_textLine1}",
                $"        {_textLine2}",
                $"    {_textLine3}",
                $"            {_textLine4}"
            ];
            consoleBase._controlDictionary[_segmentName1] = controlItem;
            consoleBase._segmentDictionary[_segmentName1] = [textItem1, textItem2, textItem3, textItem4];
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = false;
            _locater
                .SetupProperty(locater => locater.CurrentSegment, _lastSegmentName);
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.TabSize)
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetTabSize))
                .Returns(4)
                .Verifiable(Times.AtLeastOnce);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetFirstTimeIndent))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 1))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 2))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem4))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_GetIndent, 3))
                .Returns(12)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine4))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ReplaceTokens, 4))
                .Returns(_textLine4)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, IndentProcessor_GetTabSize);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetTabSize, IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetFirstTimeIndent, TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(IndentProcessor_GetIndent, TokenProcessor_ReplaceTokens, 3, 4);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ReplaceTokens, Logger_WriteLogEntries, 4);

            // Act
            consoleBase.GenerateSegment(_segmentName1);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(_segmentName1);
            _locater.Object.LineNumber
                .Should()
                .Be(expected.Count);
            VerifyMocks();
        }

        [Theory]
        [InlineData(Whitespace)]
        [InlineData(null)]
        public void GenerateSegment_SegmentNameIsNullOrWhitespace_LogsMessage(string? segmentName)
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            _locater
                .SetupProperty(locater => locater.LineNumber);
            _locater
                .SetupProperty(locater => locater.CurrentSegment);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgSegmentNameIsNullOrWhitespace, null, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);

            // Act
            consoleBase.GenerateSegment(segmentName!);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .BeEmpty();
            _locater.Object.CurrentSegment
                .Should()
                .BeEmpty();
            _locater.Object.LineNumber
                .Should()
                .Be(0);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_TemplateFileNotLoaded_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            _locater
                .SetupProperty(locater => locater.LineNumber);
            _locater
                .SetupProperty(locater => locater.CurrentSegment);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgAttemptToGenerateSegmentBeforeItWasLoaded, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);

            // Act
            consoleBase.GenerateSegment(_segmentName1);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .BeEmpty();
            _locater.Object.CurrentSegment
                .Should()
                .Be(_segmentName1);
            _locater.Object.LineNumber
                .Should()
                .Be(0);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_UnknownSegmentName_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.IsTemplateLoaded = true;
            _locater
                .SetupProperty(locater => locater.LineNumber);
            _locater
                .SetupProperty(locater => locater.CurrentSegment);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnknownSegmentName, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Generating, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);

            // Act
            consoleBase.GenerateSegment(_segmentName1);

            // Assert
            consoleBase.GeneratedText
                .Should()
                .BeEmpty();
            _locater.Object.CurrentSegment
                .Should()
                .Be(_segmentName1);
            _locater.Object.LineNumber
                .Should()
                .Be(0);
            VerifyMocks();
        }

        [Fact]
        public void LineNumber_Getter_CallsLocaterLineNumberGetter()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            int expected = 9;
            _locater
                .Setup(locater => locater.LineNumber)
                .Returns(expected)
                .Verifiable(Times.Once);

            // Act
            int actual = consoleBase.LineNumber;

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_ErrorWhenGettingFullTemplateFilePath_LogsMessageAndResetsState()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase._generatedText.Add(_textLine1);
            consoleBase._controlDictionary[_segmentName1] = new();
            consoleBase._segmentDictionary[_segmentName1] = [new(0, false, false, _textLine1)];
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Exactly(2));
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetFullPath))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToLoadTemplateFile, _templateFilePath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Loading, FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetFullPath, Logger_Log_Message, 0, 1);
            SetupResetAll(Logger_Log_Message, null, 1, 0, true, _templateFileName);

            // Act
            consoleBase.LoadTemplate(_templateFilePath);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase._controlDictionary
                .Should()
                .BeEmpty();
            consoleBase._segmentDictionary
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeFalse();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_ErrorWhenValidatingTemplateFilePath_LogsMessageAndResetsState()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase._generatedText.Add(_textLine1);
            consoleBase._controlDictionary[_segmentName1] = new();
            consoleBase._segmentDictionary[_segmentName1] = [new(0, false, false, _textLine1)];
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Exactly(2));
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidateFullPath))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToLoadTemplateFile, _templateFilePath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Loading, FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetFullPath, PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, Logger_Log_Message, 0, 1);
            SetupResetAll(Logger_Log_Message, null, 1, 0, true, _templateFileName);

            // Act
            consoleBase.LoadTemplate(_templateFilePath);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase._controlDictionary
                .Should()
                .BeEmpty();
            consoleBase._segmentDictionary
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeFalse();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_InvalidFilePath_LogsMessageAndResetsTemplateProcessor()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase.IsTemplateLoaded = true;
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFullFilePath))
                .Returns(_emptyString)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Loading, FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetFullPath, TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(TextReader_GetFileName, TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(TextReader_GetFullFilePath, TextReader_SetFilePath);
            SetupResetAll(TextReader_SetFilePath, null, 0, 0, true, "");

            // Act
            consoleBase.LoadTemplate(_templateFilePath);

            // Assert
            consoleBase.IsTemplateLoaded
                .Should()
                .BeFalse();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_LastGeneratedTextNotWritten_LogsMessageAndLoadsTemplate()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = false;
            Sequence<string> fileNameSequence = new([_lastTemplateFileName, _templateFileName]);
            Sequence<string> filePathSequence = new([_lastTemplateFilePath, _templateFilePath]);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidateFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFileName))
                .Returns(fileNameSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFullFilePath))
                .Returns(filePathSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgNextLoadRequestBeforeFirstIsWritten, _templateFileName, _lastTemplateFileName))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(TextReader_ReadTextFile))
                .Returns(SampleText)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgLoadingTemplateFile, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(templateLoader => templateLoader.LoadTemplate(SampleText, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Loading, FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetFullPath, PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(TextReader_GetFileName, TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(TextReader_GetFullFilePath, TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(TextReader_SetFilePath, TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(TextReader_GetFullFilePath, Logger_Log_Message, 0, 1);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, TextReader_ReadTextFile, 1);
            SetupResetAll(TextReader_ReadTextFile, Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, TemplateLoader_LoadTemplate, 2);
            _verifier.DefineExpectedCallOrder(TemplateLoader_LoadTemplate, Logger_WriteLogEntries);

            // Act
            consoleBase.LoadTemplate(_templateFilePath);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeTrue();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_LastGeneratedTextWasWritten_LoadsTemplate()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            Sequence<string> fileNameSequence = new([_lastTemplateFileName, _templateFileName]);
            Sequence<string> filePathSequence = new([_lastTemplateFilePath, _templateFilePath]);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidateFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFileName))
                .Returns(fileNameSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFullFilePath))
                .Returns(filePathSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(TextReader_ReadTextFile))
                .Returns(SampleText)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgLoadingTemplateFile, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(templateLoader => templateLoader.LoadTemplate(SampleText, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Loading, FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetFullPath, PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, TextReader_GetFileName, 0, -1);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, TextReader_GetFullFilePath, 0, -1);
            _verifier.DefineExpectedCallOrder(TextReader_GetFileName, TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(TextReader_GetFullFilePath, TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(TextReader_SetFilePath, TextReader_GetFullFilePath, 0, -2);
            _verifier.DefineExpectedCallOrder(TextReader_GetFullFilePath, TextReader_ReadTextFile, -2);
            SetupResetAll(TextReader_ReadTextFile, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, TemplateLoader_LoadTemplate);
            _verifier.DefineExpectedCallOrder(TemplateLoader_LoadTemplate, Logger_WriteLogEntries);

            // Act
            consoleBase.LoadTemplate(_templateFilePath);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeTrue();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_NoErrors_LoadsTemplateAndLogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            List<string> returnedTemplateLines = [_textLine1];
            consoleBase._generatedText.Add(_textLine1);
            consoleBase.IsTemplateLoaded = false;
            consoleBase.IsOutputFileWritten = true;
            Sequence<string> fileNameSequence = new([_emptyString, _templateFileName]);
            Sequence<string> filePathSequence = new([_emptyString, _templateFilePath]);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidateFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFileName))
                .Returns(fileNameSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFullFilePath))
                .Returns(filePathSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(TextReader_ReadTextFile))
                .Returns(returnedTemplateLines)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgLoadingTemplateFile, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(templateLoader => templateLoader.LoadTemplate(returnedTemplateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Loading, FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetFullPath, PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(TextReader_GetFileName, TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(TextReader_GetFullFilePath, TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(TextReader_SetFilePath, TextReader_ReadTextFile);
            SetupResetAll(TextReader_ReadTextFile, TemplateLoader_LoadTemplate);
            _verifier.DefineExpectedCallOrder(TemplateLoader_LoadTemplate, Logger_WriteLogEntries);

            // Act
            consoleBase.LoadTemplate(_templateFilePath);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeTrue();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_TemplateAlreadyLoaded_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase.IsTemplateLoaded = true;
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidateFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFileName))
                .Returns(_templateFileName)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFullFilePath))
                .Returns(_templateFilePath)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgAttemptToLoadMoreThanOnce, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Loading, FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetFullPath, PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(TextReader_GetFileName, TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(TextReader_GetFullFilePath, TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(TextReader_SetFilePath, TextReader_GetFullFilePath, 0, -2);
            _verifier.DefineExpectedCallOrder(TextReader_GetFullFilePath, Logger_Log_Message, -2);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);

            // Act
            consoleBase.LoadTemplate(_templateFilePath);

            // Assert
            consoleBase.IsTemplateLoaded
                .Should()
                .BeTrue();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase._generatedText
                .Should()
                .ContainInConsecutiveOrder(SampleText);
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_TemplateFileIsEmpty_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase.IsTemplateLoaded = false;
            consoleBase.IsOutputFileWritten = false;
            Sequence<string> fileNameSequence = new([_emptyString, _templateFileName]);
            Sequence<string> filePathSequence = new([_emptyString, _templateFilePath]);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidateFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFileName))
                .Returns(fileNameSequence.GetNext)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFullFilePath))
                .Returns(filePathSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(TextReader_ReadTextFile))
                .Returns(Array.Empty<string>())
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTemplateFileIsEmpty, _templateFilePath, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Loading, FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetFullPath, PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(TextReader_GetFileName, TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(TextReader_GetFullFilePath, TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(TextReader_SetFilePath, TextReader_GetFullFilePath, 0, -2);
            _verifier.DefineExpectedCallOrder(TextReader_GetFullFilePath, TextReader_ReadTextFile, -2);
            SetupResetAll(TextReader_ReadTextFile, Logger_Log_Message, 0, 0, false, _emptyString, _templateFileName, -2);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);

            // Act
            consoleBase.LoadTemplate(_templateFilePath);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeFalse();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void PromptUserForInput_ErrorDuringConsoleReadLine_LogsMessageAndReturnsEmptyString()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgContinuationPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(ConsoleReader_ReadLine))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToGetUserResponse, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_User, MessageWriter_WriteLine);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(ConsoleReader_ReadLine, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_SetLogEntryType_Loading);

            // Act
            string actual = consoleBase.PromptUserForInput();

            // Assert
            actual
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Fact]
        public void PromptUserForInput_UseCustomPrompt_DisplaysPromptAndReturnsUserResponse()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            string expected = "test";
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(ConsoleReader_ReadLine))
                .Returns(expected)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_User, MessageWriter_WriteLine);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(ConsoleReader_ReadLine, Logger_SetLogEntryType_Loading);

            // Act
            string actual = consoleBase.PromptUserForInput(MsgYesNoPrompt);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void PromptUserForInput_UseDefaultPrompt_DisplaysPromptAndReturnsUserResponse()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            string expected = "test";
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgContinuationPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(ConsoleReader_ReadLine))
                .Returns(expected)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_User, MessageWriter_WriteLine);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(ConsoleReader_ReadLine, Logger_SetLogEntryType_Loading);

            // Act
            string actual = consoleBase.PromptUserForInput();

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void PromptForUserInput_UserResponseIsNull_ReturnsEmptyString()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            string expected = _emptyString;
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgContinuationPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MessageWriter_WriteLine))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(ConsoleReader_ReadLine))
                .Returns((string)null!)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_User, MessageWriter_WriteLine);
            _verifier.DefineExpectedCallOrder(MessageWriter_WriteLine, ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(ConsoleReader_ReadLine, Logger_SetLogEntryType_Loading);

            // Act
            string actual = consoleBase.PromptUserForInput();

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ResetAll_ExceptionThrownDuringReset_LogsMessageAndDoesPartialReset()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase._controlDictionary[_segmentName1] = new();
            consoleBase._segmentDictionary[_segmentName1] =
            [
                new(0, true, false, _textLine1),
                new(0, true, false, _textLine2)
            ];
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(Locater_Reset))
                .Throws<ApplicationException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToResetAll, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_Reset);
            _verifier.DefineExpectedCallOrder(Locater_Reset, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Loading);

            // Act
            consoleBase.ResetAll(false);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase._controlDictionary
                .Should()
                .BeEmpty();
            consoleBase._segmentDictionary
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeFalse();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void ResetAll_ShouldDisplayMessageIsFalse_ResetsAllButDoesNotLogMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase._controlDictionary[_segmentName1] = new();
            consoleBase._segmentDictionary[_segmentName1] =
            [
                new(0, true, false, _textLine1),
                new(0, true, false, _textLine2)
            ];
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(Locater_Reset))
                .Verifiable(Times.Once);
            _defaultSegmentNameGenerator
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                .Callback(_verifier.GetCallOrderAction(DefaultSegmentNameGenerator_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ClearTokens))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ResetTokenDelimiters())
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ResetTokenDelimiters))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, TokenProcessor_ResetTokenDelimiters);
            _verifier.DefineExpectedCallOrder(Locater_Reset, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(DefaultSegmentNameGenerator_Reset, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(IndentProcessor_Reset, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ClearTokens, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ResetTokenDelimiters, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Loading);

            // Act
            consoleBase.ResetAll(false);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase._controlDictionary
                .Should()
                .BeEmpty();
            consoleBase._segmentDictionary
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeFalse();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void ResetAll_ShouldDisplayMessageIsTrue_ResetsAllAndLogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase._controlDictionary[_segmentName1] = new();
            consoleBase._segmentDictionary[_segmentName1] =
            [
                new(0, true, false, _textLine1),
                new(0, true, false, _textLine2)
            ];
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(Locater_Reset))
                .Verifiable(Times.Once);
            _defaultSegmentNameGenerator
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                .Callback(_verifier.GetCallOrderAction(DefaultSegmentNameGenerator_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ClearTokens))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ResetTokenDelimiters())
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ResetTokenDelimiters))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFileName))
                .Returns(_templateFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTemplateHasBeenReset, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, TokenProcessor_ResetTokenDelimiters);
            _verifier.DefineExpectedCallOrder(Locater_Reset, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(DefaultSegmentNameGenerator_Reset, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(IndentProcessor_Reset, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ClearTokens, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ResetTokenDelimiters, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(TextReader_GetFileName, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Loading);

            // Act
            consoleBase.ResetAll();

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase._controlDictionary
                .Should()
                .BeEmpty();
            consoleBase._segmentDictionary
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeFalse();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void ResetGeneratedText_ExceptionThrownDuringReset_LogsMessageAndDoesPartialReset()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            int firstTimeIndent = 1;
            int tabSize = 2;
            ControlItem controlItem1 = new()
            {
                IsFirstTime = false,
                PadSegment = _padSegmentName
            };
            ControlItem controlItem2 = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = firstTimeIndent
            };
            ControlItem controlItem3 = new()
            {
                IsFirstTime = false,
                TabSize = tabSize
            };
            Dictionary<string, ControlItem> expected = new()
            {
                [_segmentName1] = new ControlItem() { IsFirstTime = true, PadSegment = _padSegmentName },
                [_segmentName2] = new ControlItem() { IsFirstTime = true, FirstTimeIndent = firstTimeIndent },
                [_segmentName3] = new ControlItem() { IsFirstTime = true, TabSize = tabSize }
            };
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            consoleBase._controlDictionary[_segmentName1] = controlItem1;
            consoleBase._controlDictionary[_segmentName2] = controlItem2;
            consoleBase._controlDictionary[_segmentName3] = controlItem3;
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(Locater_Reset))
                .Throws<ApplicationException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToResetGeneratedText, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_Reset);
            _verifier.DefineExpectedCallOrder(Locater_Reset, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.ResetGeneratedText();

            // Assert
            consoleBase._controlDictionary
                .Should()
                .HaveSameCount(expected);
            consoleBase._controlDictionary
                .Should()
                .BeEquivalentTo(expected);
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeTrue();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void ResetGeneratedText_ShouldDisplayMessageIsFalse_ResetsGeneratedTextButDoesNotLogMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            int firstTimeIndent = 1;
            int tabSize = 2;
            ControlItem controlItem1 = new()
            {
                IsFirstTime = false,
                PadSegment = _padSegmentName
            };
            ControlItem controlItem2 = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = firstTimeIndent
            };
            ControlItem controlItem3 = new()
            {
                IsFirstTime = false,
                TabSize = tabSize
            };
            Dictionary<string, ControlItem> expected = new()
            {
                [_segmentName1] = new ControlItem() { IsFirstTime = true, PadSegment = _padSegmentName },
                [_segmentName2] = new ControlItem() { IsFirstTime = true, FirstTimeIndent = firstTimeIndent },
                [_segmentName3] = new ControlItem() { IsFirstTime = true, TabSize = tabSize }
            };
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            consoleBase._controlDictionary[_segmentName1] = controlItem1;
            consoleBase._controlDictionary[_segmentName2] = controlItem2;
            consoleBase._controlDictionary[_segmentName3] = controlItem3;
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(Locater_Reset, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(IndentProcessor_Reset, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.ResetGeneratedText(false);

            // Assert
            consoleBase._controlDictionary
                .Should()
                .HaveSameCount(expected);
            consoleBase._controlDictionary
                .Should()
                .BeEquivalentTo(expected);
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeTrue();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void ResetGeneratedText_ShouldDisplayMessageIsTrue_ResetsGeneratedTextButDoesNotLogMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            int firstTimeIndent = 1;
            int tabSize = 2;
            ControlItem controlItem1 = new()
            {
                IsFirstTime = false,
                PadSegment = _padSegmentName
            };
            ControlItem controlItem2 = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = firstTimeIndent
            };
            ControlItem controlItem3 = new()
            {
                IsFirstTime = false,
                TabSize = tabSize
            };
            Dictionary<string, ControlItem> expected = new()
            {
                [_segmentName1] = new ControlItem() { IsFirstTime = true, PadSegment = _padSegmentName },
                [_segmentName2] = new ControlItem() { IsFirstTime = true, FirstTimeIndent = firstTimeIndent },
                [_segmentName3] = new ControlItem() { IsFirstTime = true, TabSize = tabSize }
            };
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            consoleBase._controlDictionary[_segmentName1] = controlItem1;
            consoleBase._controlDictionary[_segmentName2] = controlItem2;
            consoleBase._controlDictionary[_segmentName3] = controlItem3;
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFileName))
                .Returns(_templateFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextHasBeenReset, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(Locater_Reset, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(IndentProcessor_Reset, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(TextReader_GetFileName, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.ResetGeneratedText();

            // Assert
            consoleBase._controlDictionary
                .Should()
                .HaveSameCount(expected);
            consoleBase._controlDictionary
                .Should()
                .BeEquivalentTo(expected);
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeTrue();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void ResetSegment_ExceptionThrownDuringReset_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = _segmentName1)
                .Callback(_verifier.GetCallOrderAction(Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.LineNumber = 0)
                .Callback(_verifier.GetCallOrderAction(Locater_LineNumber_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(Locater_CurrentSegment_Getter))
                .Throws<ApplicationException>()
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgUnableToResetSegment, _segmentName1, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_LineNumber_Setter);
            _verifier.DefineExpectedCallOrder(Locater_CurrentSegment_Setter, Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(Locater_LineNumber_Setter, Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(Locater_CurrentSegment_Getter, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.ResetSegment(_segmentName1);

            // Assert
            VerifyMocks();
        }

        [Theory]
        [InlineData("Segment1")]
        [InlineData("")]
        [InlineData(null)]
        public void ResetSegment_SegmentNotFound_LogsMessage(string? name)
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            string segmentName = name is null ? string.Empty : name;
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = segmentName)
                .Callback(_verifier.GetCallOrderAction(Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.LineNumber = 0)
                .Callback(_verifier.GetCallOrderAction(Locater_LineNumber_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgUnableToResetUnknownSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_LineNumber_Setter);
            _verifier.DefineExpectedCallOrder(Locater_CurrentSegment_Setter, Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(Locater_LineNumber_Setter, Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(Locater_CurrentSegment_Getter, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.ResetSegment(name!);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void ResetSegment_ValidSegmentName_ResetsSegment()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            ControlItem controlItem1 = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = 1,
                TabSize = 2,
                PadSegment = _padSegmentName
            };
            ControlItem expected1 = new()
            {
                IsFirstTime = true,
                FirstTimeIndent = 1,
                TabSize = 2,
                PadSegment = _padSegmentName
            };
            ControlItem controlItem2 = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = 0,
                TabSize = 0,
                PadSegment = ""
            };
            ControlItem expected2 = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = 0,
                TabSize = 0,
                PadSegment = ""
            };
            consoleBase._controlDictionary[_segmentName1] = controlItem1;
            consoleBase._controlDictionary[_segmentName2] = controlItem2;
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = _segmentName1)
                .Callback(_verifier.GetCallOrderAction(Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.LineNumber = 0)
                .Callback(_verifier.GetCallOrderAction(Locater_LineNumber_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(Locater_CurrentSegment_Getter))
                .Returns(_segmentName1)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgSegmentHasBeenReset, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_LineNumber_Setter);
            _verifier.DefineExpectedCallOrder(Locater_CurrentSegment_Setter, Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(Locater_LineNumber_Setter, Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(Locater_CurrentSegment_Getter, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.ResetSegment(_segmentName1);

            // Assert
            consoleBase._controlDictionary[_segmentName1]
                .Should()
                .Be(expected1);
            consoleBase._controlDictionary[_segmentName2]
                .Should()
                .Be(expected2);
            VerifyMocks();
        }

        [Fact]
        public void ResetTokenDelimiters_WhenCalled_InvokesTokenProcessorResetTokenDelimiters()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ResetTokenDelimiters())
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ResetTokenDelimiters))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, TokenProcessor_ResetTokenDelimiters);
            _verifier.DefineExpectedCallOrder(TokenProcessor_ResetTokenDelimiters, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.ResetTokenDelimiters();

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void SetOutputDirectory_ErrorWhenCreatingOutputDirectory_ClearsOutputDirectoryAndLogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = _lastOutputDirectoryPath;
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Writing)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidatePath(_outputDirectoryPath, false, false))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidatePath))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CreateDirectory(_outputDirectoryPath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_CreateDirectory))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenCreatingOutputDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, PathValidater_ValidatePath);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidatePath, FileAndDirectoryService_CreateDirectory);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_CreateDirectory, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Writing);

            // Act
            consoleBase.SetOutputDirectory(_outputDirectoryPath);

            // Assert
            consoleBase.OutputDirectory
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Fact]
        public void SetOutputDirectory_ErrorWhenValidatingDirectoryPath_ClearsOutputDirectoryAndLogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = _lastOutputDirectoryPath;
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Writing)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidatePath(_outputDirectoryPath, false, false))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidatePath))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenCreatingOutputDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, PathValidater_ValidatePath);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidatePath, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Writing);

            // Act
            consoleBase.SetOutputDirectory(_outputDirectoryPath);

            // Assert
            consoleBase.OutputDirectory
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Fact]
        public void SetOutputDirectory_NoErrors_CreatesAndSetsTheOutputDirectory()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = _lastOutputDirectoryPath;
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Writing)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidatePath(_outputDirectoryPath, false, false))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidatePath))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CreateDirectory(_outputDirectoryPath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_CreateDirectory))
                .Returns(_outputDirectoryPath)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, PathValidater_ValidatePath);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidatePath, FileAndDirectoryService_CreateDirectory);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_CreateDirectory, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Writing);

            // Act
            consoleBase.SetOutputDirectory(_outputDirectoryPath);

            // Assert
            consoleBase.OutputDirectory
                .Should()
                .Be(_outputDirectoryPath);
            VerifyMocks();
        }

        [Fact]
        public void SetTabSize_WhenCalled_InvokesIndentProcessorSetTabSize()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            int tabSize = 2;
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(tabSize))
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, IndentProcessor_SetTabSize);
            _verifier.DefineExpectedCallOrder(IndentProcessor_SetTabSize, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.SetTabSize(tabSize);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void SetTokenDelimiters_WhenSuccessful_ReturnsTrue()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            string tokenStart = "{{";
            string tokenEnd = "}}";
            char escapeChar = '!';
            bool expected = true;
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.SetTokenDelimiters(tokenStart, tokenEnd, escapeChar))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_SetTokenDelimiters))
                .Returns(expected)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, TokenProcessor_SetTokenDelimiters);
            _verifier.DefineExpectedCallOrder(TokenProcessor_SetTokenDelimiters, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            bool actual = consoleBase.SetTokenDelimiters(tokenStart, tokenEnd, escapeChar);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void SetTokenDelimiters_WhenUnsuccessful_ReturnsFalse()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            string tokenStart = "{{";
            string tokenEnd = "}}";
            char escapeChar = '!';
            bool expected = false;
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.SetTokenDelimiters(tokenStart, tokenEnd, escapeChar))
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_SetTokenDelimiters))
                .Returns(expected)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, TokenProcessor_SetTokenDelimiters);
            _verifier.DefineExpectedCallOrder(TokenProcessor_SetTokenDelimiters, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            bool actual = consoleBase.SetTokenDelimiters(tokenStart, tokenEnd, escapeChar);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void TabSize_Getter_CallsIndentProcessorTabSizeGetter()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            int expected = 3;
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.TabSize)
                .Returns(expected)
                .Verifiable(Times.Once);

            // Act
            int actual = consoleBase.TabSize;

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void TemplateFileName_Getter_CallsTextReaderFileNameGetter()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            _textReader
                .Setup(textReader => textReader.FileName)
                .Returns(_templateFileName)
                .Verifiable(Times.Once);

            // Act
            string actual = consoleBase.TemplateFileName;

            // Assert
            actual
                .Should()
                .Be(_templateFileName);
            VerifyMocks();
        }

        [Fact]
        public void TemplateFilePath_Getter_CallsTextReaderFullFilePathGetter()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);

            // Act
            string actual = consoleBase.TemplateFilePath;

            // Assert
            actual
                .Should()
                .Be(_templateFilePath);
            VerifyMocks();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructorUnableToGetFullTemplateFilePath_LogsMessageAndInitializesProperties()
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetSolutionDirectory))
                .Returns(SolutionDirectory)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, FileAndDirectoryService_GetSolutionDirectory);

            // Act
            MyConsoleBase consoleBase = GetMyConsoleBase(false);

            // Assert
            consoleBase._controlDictionary
                .Should()
                .NotBeNull();
            consoleBase._controlDictionary
                .Should()
                .BeEmpty();
            consoleBase._segmentDictionary
                .Should()
                .NotBeNull();
            consoleBase._segmentDictionary
                .Should()
                .BeEmpty();
            consoleBase._generatedText
                .Should()
                .NotBeNull();
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeFalse();
            consoleBase.SolutionDirectory
                .Should()
                .Be(SolutionDirectory);
            VerifyMocks();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructorUnableToGetSolutionDirectory_LogsMessageAndInitializesProperties()
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetSolutionDirectory))
                .Throws<ArgumentException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenLocatingSolutionDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, FileAndDirectoryService_GetSolutionDirectory);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetSolutionDirectory, Logger_Log_Message, 0, 1);

            // Act
            MyConsoleBase consoleBase = GetMyConsoleBase(false);

            // Assert
            consoleBase._controlDictionary
                .Should()
                .NotBeNull();
            consoleBase._controlDictionary
                .Should()
                .BeEmpty();
            consoleBase._segmentDictionary
                .Should()
                .NotBeNull();
            consoleBase._segmentDictionary
                .Should()
                .BeEmpty();
            consoleBase._generatedText
                .Should()
                .NotBeNull();
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeFalse();
            consoleBase.SolutionDirectory
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructorUnableToValidateFullTemplateFilePath_LogsMessageAndInitializesProperties()
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetSolutionDirectory))
                .Returns(SolutionDirectory)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, FileAndDirectoryService_GetSolutionDirectory);

            // Act
            MyConsoleBase consoleBase = GetMyConsoleBase(false);

            // Assert
            consoleBase._controlDictionary
                .Should()
                .NotBeNull();
            consoleBase._controlDictionary
                .Should()
                .BeEmpty();
            consoleBase._segmentDictionary
                .Should()
                .NotBeNull();
            consoleBase._segmentDictionary
                .Should()
                .BeEmpty();
            consoleBase._generatedText
                .Should()
                .NotBeNull();
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeFalse();
            consoleBase.SolutionDirectory
                .Should()
                .Be(SolutionDirectory);
            VerifyMocks();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullConsoleReader_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.ConsoleReaderService,
                                                       ServiceParameterNames.ConsoleReaderParameter);
            Action action = () =>
                {
                    consoleBase = new(null!,
                                      _defaultSegmentNameGenerator.Object,
                                      _fileService.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _logger.Object,
                                      _messageWriter.Object,
                                      _pathValidater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullDefaultSegmentNameGenerator_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.DefaultSegmentNameGeneratorService,
                                                       ServiceParameterNames.DefaultSegmentNameGeneratorParameter);
            Action action = () =>
                {
                    consoleBase = new(_consoleReader.Object,
                                      null!,
                                      _fileService.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _logger.Object,
                                      _messageWriter.Object,
                                      _pathValidater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullFileService_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.FileAndDirectoryService,
                                                       ServiceParameterNames.FileAndDirectoryServiceParameter);
            Action action = () =>
                {
                    consoleBase = new(_consoleReader.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      null!,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _logger.Object,
                                      _messageWriter.Object,
                                      _pathValidater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullIndentProcessor_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.IndentProcessorService,
                                                       ServiceParameterNames.IndentProcessorParameter);
            Action action = () =>
                {
                    consoleBase = new(_consoleReader.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _fileService.Object,
                                      null!,
                                      _locater.Object,
                                      _logger.Object,
                                      _messageWriter.Object,
                                      _pathValidater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullLocater_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.LocaterService,
                                                       ServiceParameterNames.LocaterParameter);
            Action action = () =>
                {
                    consoleBase = new(_consoleReader.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _fileService.Object,
                                      _indentProcessor.Object,
                                      null!,
                                      _logger.Object,
                                      _messageWriter.Object,
                                      _pathValidater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullLogger_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.LoggerService,
                                                       ServiceParameterNames.LoggerParameter);
            Action action = () =>
                {
                    consoleBase = new(_consoleReader.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _fileService.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      null!,
                                      _messageWriter.Object,
                                      _pathValidater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullMessageWriter_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.MessageWriterService,
                                                       ServiceParameterNames.MessageWriterParameter);
            Action action = () =>
                {
                    consoleBase = new(_consoleReader.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _fileService.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _logger.Object,
                                      null!,
                                      _pathValidater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullPathValidater_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.PathValidaterService,
                                                       ServiceParameterNames.PathValidaterParameter);
            Action action = () =>
                {
                    consoleBase = new(_consoleReader.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _fileService.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _logger.Object,
                                      _messageWriter.Object,
                                      null!,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullTemplateLoader_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.TemplateLoaderService,
                                                       ServiceParameterNames.TemplateLoaderParameter);
            Action action = () =>
                {
                    consoleBase = new(_consoleReader.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _fileService.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _logger.Object,
                                      _messageWriter.Object,
                                      _pathValidater.Object,
                                      null!,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullTextReader_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.TextReaderService,
                                                       ServiceParameterNames.TextReaderParameter);
            Action action = () =>
                {
                    consoleBase = new(_consoleReader.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _fileService.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _logger.Object,
                                      _messageWriter.Object,
                                      _pathValidater.Object,
                                      _templateLoader.Object,
                                      null!,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullTextWriter_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.TextWriterService,
                                                       ServiceParameterNames.TextWriterParameter);
            Action action = () =>
                {
                    consoleBase = new(_consoleReader.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _fileService.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _logger.Object,
                                      _messageWriter.Object,
                                      _pathValidater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      null!,
                                      _tokenProcessor.Object);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingNullTokenProcessor_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.TokenProcessorService,
                                                       ServiceParameterNames.TokenProcessorParameter);
            Action action = () =>
                {
                    consoleBase = new(_consoleReader.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _fileService.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _logger.Object,
                                      _messageWriter.Object,
                                      _pathValidater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      null!);
                };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextTemplateConsoleBase_ConstructUsingValidDependencies_InitializesProperties()
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetSolutionDirectory))
                .Returns(SolutionDirectory)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Setup, FileAndDirectoryService_GetSolutionDirectory);

            // Act
            MyConsoleBase consoleBase = GetMyConsoleBase(false);

            // Assert
            consoleBase._controlDictionary
                .Should()
                .NotBeNull();
            consoleBase._controlDictionary
                .Should()
                .BeEmpty();
            consoleBase._segmentDictionary
                .Should()
                .NotBeNull();
            consoleBase._segmentDictionary
                .Should()
                .BeEmpty();
            consoleBase._generatedText
                .Should()
                .NotBeNull();
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            consoleBase.IsTemplateLoaded
                .Should()
                .BeFalse();
            consoleBase.SolutionDirectory
                .Should()
                .Be(SolutionDirectory);
            VerifyMocks();
        }

        [Fact]
        public void WriteGeneratedTextToFile_ErrorWhileGeneratingOutputFilePath_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = _outputDirectoryPath;
            consoleBase._generatedText.AddRange(SampleText);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombinePaths(_outputDirectoryPath, _outputFileName))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_CombineDirectoryAndFileName))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhileConstructingFilePath, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Writing);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Writing, FileAndDirectoryService_CombineDirectoryAndFileName);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_CombineDirectoryAndFileName, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.WriteGeneratedTextToFile(_outputFileName);

            // Assert
            consoleBase._generatedText
                .Should()
                .NotBeEmpty();
            consoleBase._generatedText
                .Should()
                .ContainInConsecutiveOrder(SampleText);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void WriteGeneratedTextToFile_ExceptionThrownWhileWritingTextFile_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = _outputDirectoryPath;
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase._controlDictionary[_segmentName1] = new() { IsFirstTime = false };
            consoleBase._controlDictionary[_segmentName2] = new() { IsFirstTime = false };
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombinePaths(_outputDirectoryPath, _outputFileName))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_CombineDirectoryAndFileName))
                .Returns(_outputFilePath)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(_outputFilePath, SampleText))
                .Callback(_verifier.GetCallOrderAction(TextWriter_WriteTextFile))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToWriteGeneratedTextToFile, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Writing);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Writing, FileAndDirectoryService_CombineDirectoryAndFileName);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_CombineDirectoryAndFileName, TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(TextWriter_WriteTextFile, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.WriteGeneratedTextToFile(_outputFileName);

            // Assert
            consoleBase._generatedText
                .Should()
                .NotBeEmpty();
            consoleBase._generatedText
                .Should()
                .ContainInConsecutiveOrder(SampleText);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void WriteGeneratedTextToFile_FileNameIsEmptyAndResetGeneratedTextIsTrue_WritesGeneratedTextToDefaultFileAndResetsGeneratedTextBuffer()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            string expectedFileName = "File_0001.txt";
            string expectedFilePath = $"{_outputDirectoryPath}{Sep}{expectedFileName}";
            consoleBase.OutputDirectory = _outputDirectoryPath;
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase._controlDictionary[_segmentName1] = new() { IsFirstTime = false };
            consoleBase._controlDictionary[_segmentName2] = new() { IsFirstTime = false };
            Sequence<LogEntryType> logEntryTypeSequence = new([LogEntryType.Generating, LogEntryType.Writing], 2);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(logEntryTypeSequence.GetNext)
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Writing))
                .Verifiable(Times.AtLeastOnce);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombinePaths(_outputDirectoryPath, expectedFileName))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_CombineDirectoryAndFileName))
                .Returns(expectedFilePath)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(expectedFilePath, SampleText))
                .Callback(_verifier.GetCallOrderAction(TextWriter_WriteTextFile))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFileName))
                .Returns(expectedFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextHasBeenReset, expectedFileName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Writing, -1, -1);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Writing, FileAndDirectoryService_CombineDirectoryAndFileName, -1);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_CombineDirectoryAndFileName, TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(TextWriter_WriteTextFile, Logger_GetLogEntryType, 0, -2);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(Locater_Reset, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(IndentProcessor_Reset, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(TextReader_GetFileName, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_SetLogEntryType_Writing, 0, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Writing, Logger_WriteLogEntries, -2);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.WriteGeneratedTextToFile(_emptyString);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeTrue();
            consoleBase._controlDictionary[_segmentName2].IsFirstTime
                .Should()
                .BeTrue();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeTrue();
            VerifyMocks();
        }

        [Fact]
        public void WriteGeneratedTextToFile_NoErrorsAndResetGeneratedTextIsFalse_WritesGenerated()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = _outputDirectoryPath;
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase._controlDictionary[_segmentName1] = new() { IsFirstTime = false };
            consoleBase._controlDictionary[_segmentName2] = new() { IsFirstTime = false };
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Writing))
                .Verifiable(Times.AtLeastOnce);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombinePaths(_outputDirectoryPath, _outputFileName))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_CombineDirectoryAndFileName))
                .Returns(_outputFilePath)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(_outputFilePath, SampleText))
                .Callback(_verifier.GetCallOrderAction(TextWriter_WriteTextFile))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Writing, FileAndDirectoryService_CombineDirectoryAndFileName);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_CombineDirectoryAndFileName, TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(TextWriter_WriteTextFile, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.WriteGeneratedTextToFile(_outputFileName, false);

            // Assert
            consoleBase._generatedText
                .Should()
                .NotBeEmpty();
            consoleBase._generatedText
                .Should()
                .ContainInConsecutiveOrder(SampleText);
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeFalse();
            consoleBase._controlDictionary[_segmentName2].IsFirstTime
                .Should()
                .BeFalse();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeTrue();
            VerifyMocks();
        }

        [Fact]
        public void WriteGeneratedTextToFile_NoErrorsAndResetGeneratedTextIsTrue_WritesGeneratedTextAndResetsGeneratedTextBuffer()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = _outputDirectoryPath;
            consoleBase._generatedText.AddRange(SampleText);
            consoleBase._controlDictionary[_segmentName1] = new() { IsFirstTime = false };
            consoleBase._controlDictionary[_segmentName2] = new() { IsFirstTime = false };
            Sequence<LogEntryType> logEntryTypeSequence = new([LogEntryType.Generating, LogEntryType.Writing], 2);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(logEntryTypeSequence.GetNext)
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Writing))
                .Verifiable(Times.AtLeastOnce);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombinePaths(_outputDirectoryPath, _outputFileName))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_CombineDirectoryAndFileName))
                .Returns(_outputFilePath)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(_outputFilePath, SampleText))
                .Callback(_verifier.GetCallOrderAction(TextWriter_WriteTextFile))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(TextReader_GetFileName))
                .Returns(_outputFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextHasBeenReset, _outputFileName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Writing, -1, -1);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Writing, FileAndDirectoryService_CombineDirectoryAndFileName, -1);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_CombineDirectoryAndFileName, TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(TextWriter_WriteTextFile, Logger_GetLogEntryType, 0, -2);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(Locater_Reset, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(IndentProcessor_Reset, TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(TextReader_GetFileName, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_SetLogEntryType_Writing, 0, -2);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Writing, Logger_WriteLogEntries, -2);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.WriteGeneratedTextToFile(_outputFileName);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase._controlDictionary[_segmentName1].IsFirstTime
                .Should()
                .BeTrue();
            consoleBase._controlDictionary[_segmentName2].IsFirstTime
                .Should()
                .BeTrue();
            consoleBase.IsOutputFileWritten
                .Should()
                .BeTrue();
            VerifyMocks();
        }

        [Fact]
        public void WriteGeneratedTextToFile_OutputDirectoryNotSet_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase._generatedText.AddRange(SampleText);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgOutputDirectoryNotSet, null, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Writing);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Writing, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.WriteGeneratedTextToFile(_outputFileName);

            // Assert
            consoleBase._generatedText
                .Should()
                .NotBeEmpty();
            consoleBase._generatedText
                .Should()
                .ContainInConsecutiveOrder(SampleText);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        private MyConsoleBase GetMyConsoleBase(bool shouldInitializeMocks = true)
        {
            if (shouldInitializeMocks)
            {
                InitializeMocks();
                _logger
                    .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup));
                _fileService
                    .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                    .Returns(SolutionDirectory);
            }

            MyConsoleBase consoleBase = new(_consoleReader.Object,
                                            _defaultSegmentNameGenerator.Object,
                                            _fileService.Object,
                                            _indentProcessor.Object,
                                            _locater.Object,
                                            _logger.Object,
                                            _messageWriter.Object,
                                            _pathValidater.Object,
                                            _templateLoader.Object,
                                            _textReader.Object,
                                            _textWriter.Object,
                                            _tokenProcessor.Object);

            if (shouldInitializeMocks)
            {
                InitializeMocks();
            }

            return consoleBase;
        }

        private void InitializeMocks()
        {
            _consoleReader.Reset();
            _defaultSegmentNameGenerator.Reset();
            _fileService.Reset();
            _indentProcessor.Reset();
            _locater.Reset();
            _logger.Reset();
            _messageWriter.Reset();
            _pathValidater.Reset();
            _templateLoader.Reset();
            _textReader.Reset();
            _textWriter.Reset();
            _tokenProcessor.Reset();
            _verifier.Reset();
        }

        private void MocksVerifyNoOtherCalls()
        {
            _consoleReader.VerifyNoOtherCalls();
            _defaultSegmentNameGenerator.VerifyNoOtherCalls();
            _fileService.VerifyNoOtherCalls();
            _indentProcessor.VerifyNoOtherCalls();
            _locater.VerifyNoOtherCalls();
            _logger.VerifyNoOtherCalls();
            _messageWriter.VerifyNoOtherCalls();
            _pathValidater.VerifyNoOtherCalls();
            _templateLoader.VerifyNoOtherCalls();
            _textReader.VerifyNoOtherCalls();
            _textWriter.VerifyNoOtherCalls();
            _tokenProcessor.VerifyNoOtherCalls();
        }

        private void SetupResetAll(MethodCallToken callBefore,
                                   MethodCallToken? callAfter,
                                   int firstCallNumber = 0,
                                   int secondCallNumber = 0,
                                   bool shouldDisplayMessage = false,
                                   string firstFileName = "",
                                   string? secondFileName = null,
                                   int getFileNameCallNumber = 0)
        {
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(Locater_Reset))
                .Verifiable(Times.Once);
            _defaultSegmentNameGenerator
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                .Callback(_verifier.GetCallOrderAction(DefaultSegmentNameGenerator_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ClearTokens))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ResetTokenDelimiters())
                .Callback(_verifier.GetCallOrderAction(TokenProcessor_ResetTokenDelimiters))
                .Verifiable(Times.Once);

            if (shouldDisplayMessage)
            {
                if (secondFileName is null)
                {
                    _textReader
                                .Setup(textReader => textReader.FileName)
                                .Callback(_verifier.GetCallOrderAction(TextReader_GetFileName))
                                .Returns(firstFileName)
                                .Verifiable(Times.AtLeastOnce);
                }
                else
                {
                    Sequence<string> fileNameSequence = new([firstFileName, secondFileName]);
                    _textReader
                        .Setup(textReader => textReader.FileName)
                        .Callback(_verifier.GetCallOrderAction(TextReader_GetFileName))
                        .Returns(fileNameSequence.GetNext)
                        .Verifiable(Times.AtLeast(2));
                }

                _logger
                    .Setup(logger => logger.Log(MsgTemplateHasBeenReset, firstFileName, null))
                    .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 99))
                    .Verifiable(Times.Once);
            }

            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(Logger_SetLogEntryType_Loading))
                .Verifiable(Times.AtLeastOnce);

            _verifier.DefineExpectedCallOrder(callBefore, Logger_GetLogEntryType, firstCallNumber);
            _verifier.DefineExpectedCallOrder(Logger_GetLogEntryType, Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, Locater_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Reset, TokenProcessor_ResetTokenDelimiters);

            if (shouldDisplayMessage)
            {
                _verifier.DefineExpectedCallOrder(Locater_Reset, TextReader_GetFileName, 0, getFileNameCallNumber);
                _verifier.DefineExpectedCallOrder(DefaultSegmentNameGenerator_Reset, TextReader_GetFileName, 0, getFileNameCallNumber);
                _verifier.DefineExpectedCallOrder(IndentProcessor_Reset, TextReader_GetFileName, 0, getFileNameCallNumber);
                _verifier.DefineExpectedCallOrder(TokenProcessor_ClearTokens, TextReader_GetFileName, 0, getFileNameCallNumber);
                _verifier.DefineExpectedCallOrder(TokenProcessor_ResetTokenDelimiters, TextReader_GetFileName, 0, getFileNameCallNumber);
                _verifier.DefineExpectedCallOrder(TextReader_GetFileName, Logger_Log_Message, getFileNameCallNumber, 99);
                _verifier.DefineExpectedCallOrder(Logger_Log_Message, Logger_WriteLogEntries, 99);
            }
            else
            {
                _verifier.DefineExpectedCallOrder(Locater_Reset, Logger_WriteLogEntries);
                _verifier.DefineExpectedCallOrder(DefaultSegmentNameGenerator_Reset, Logger_WriteLogEntries);
                _verifier.DefineExpectedCallOrder(IndentProcessor_Reset, Logger_WriteLogEntries);
                _verifier.DefineExpectedCallOrder(TokenProcessor_ClearTokens, Logger_WriteLogEntries);
                _verifier.DefineExpectedCallOrder(TokenProcessor_ResetTokenDelimiters, Logger_WriteLogEntries);
            }

            _verifier.DefineExpectedCallOrder(Logger_WriteLogEntries, Logger_SetLogEntryType_Loading);

            if (callAfter is not null)
            {
                _verifier.DefineExpectedCallOrder(Logger_SetLogEntryType_Loading, callAfter, 0, secondCallNumber);
            }
        }

        private void VerifyMocks()
        {
            _consoleReader.VerifyAll();
            _defaultSegmentNameGenerator.VerifyAll();
            _fileService.VerifyAll();
            _indentProcessor.VerifyAll();
            _locater.VerifyAll();
            _logger.VerifyAll();
            _messageWriter.VerifyAll();
            _pathValidater.VerifyAll();
            _templateLoader.VerifyAll();
            _textReader.VerifyAll();
            _textWriter.VerifyAll();
            _tokenProcessor.VerifyAll();
            MocksVerifyNoOtherCalls();
            _verifier.Verify();
        }

        public class MyConsoleBase : TextTemplateConsoleBase
        {
            internal MyConsoleBase(IConsoleReader consoleReader,
                                   IDefaultSegmentNameGenerator defaultSegmentNameGenerator,
                                   IFileAndDirectoryService fileService,
                                   IIndentProcessor indentProcessor,
                                   ILocater locater,
                                   ILogger logger,
                                   IMessageWriter messageWriter,
                                   IPathValidater pathValidater,
                                   ITemplateLoader templateLoader,
                                   ITextReader textReader,
                                   ITextWriter textWriter,
                                   ITokenProcessor tokenProcessor) : base(consoleReader,
                                                                          defaultSegmentNameGenerator,
                                                                          fileService,
                                                                          indentProcessor,
                                                                          locater,
                                                                          logger,
                                                                          messageWriter,
                                                                          pathValidater,
                                                                          templateLoader,
                                                                          textReader,
                                                                          textWriter,
                                                                          tokenProcessor)
            {
            }
        }
    }
}