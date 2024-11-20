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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(logEntryTypeSequence.GetNext)
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Exactly(2));
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.MessageWriter_WriteLine, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.MessageWriter_WriteLine, 2))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.ConsoleReader_ReadLine))
                .Returns("y")
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.ClearDirectory(_outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_ClearDirectory))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenClearingOutputDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.MessageWriter_WriteLine, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.MessageWriter_WriteLine, MethodCallID.Logger_WriteLogEntries, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_User, MethodCallID.MessageWriter_WriteLine, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.MessageWriter_WriteLine, MethodCallID.ConsoleReader_ReadLine, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.ConsoleReader_ReadLine, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.FileAndDirectoryService_ClearDirectory);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_ClearDirectory, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
            _logger
                .SetupSequence(logger => logger.GetLogEntryType())
                .Returns(LogEntryType.Generating)
                .Returns(LogEntryType.Reset)
                .Throws<ApplicationException>();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Exactly(2));
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.MessageWriter_WriteLine, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.MessageWriter_WriteLine, 2))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.ConsoleReader_ReadLine))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToGetUserResponse, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.MessageWriter_WriteLine, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.MessageWriter_WriteLine, MethodCallID.Logger_WriteLogEntries, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_User, MethodCallID.MessageWriter_WriteLine, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.MessageWriter_WriteLine, MethodCallID.ConsoleReader_ReadLine, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.ConsoleReader_ReadLine, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
            _logger
                .SetupSequence(logger => logger.GetLogEntryType())
                .Returns(LogEntryType.Generating)
                .Returns(LogEntryType.Reset)
                .Throws<ApplicationException>();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Exactly(2));
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.MessageWriter_WriteLine, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.MessageWriter_WriteLine, 2))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.ConsoleReader_ReadLine))
                .Returns(userResponse)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.MessageWriter_WriteLine, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.MessageWriter_WriteLine, MethodCallID.Logger_WriteLogEntries, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_User, MethodCallID.MessageWriter_WriteLine, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.MessageWriter_WriteLine, MethodCallID.ConsoleReader_ReadLine, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.ConsoleReader_ReadLine, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
            _logger
                .SetupSequence(logger => logger.GetLogEntryType())
                .Returns(LogEntryType.Generating)
                .Returns(LogEntryType.Reset)
                .Throws<ApplicationException>();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Exactly(2));
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.MessageWriter_WriteLine, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.MessageWriter_WriteLine, 2))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.ConsoleReader_ReadLine))
                .Returns(userResponse)
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.ClearDirectory(_outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_ClearDirectory))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgOutputDirectoryCleared, null, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.MessageWriter_WriteLine, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.MessageWriter_WriteLine, MethodCallID.Logger_WriteLogEntries, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_User, MethodCallID.MessageWriter_WriteLine, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.MessageWriter_WriteLine, MethodCallID.ConsoleReader_ReadLine, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.ConsoleReader_ReadLine, MethodCallID.FileAndDirectoryService_ClearDirectory);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_ClearDirectory, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.Logger_WriteLogEntries, 3);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.LoadTokenValues(tokenValues))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_LoadTokenValues))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine2Before))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(textLine2After)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.TokenProcessor_LoadTokenValues);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_LoadTokenValues, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.Logger_WriteLogEntries, 3);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(firstTimeIndent, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(12)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem4))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 3))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine4))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 4))
                .Returns(_textLine4)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 3, 4);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.Logger_WriteLogEntries, 4);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(1, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.IndentProcessor_SetTabSize);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.Logger_SetLogEntryType_Generating);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.Logger_WriteLogEntries, 3);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(3));
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SaveCurrentState())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SaveCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _padSegmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, padTextItem))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_padTextLine))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(_padTextLine)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.RestoreCurrentState())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_RestoreCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 3))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 4))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.IndentProcessor_SaveCurrentState);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SaveCurrentState, MethodCallID.Logger_Log_Message, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_RestoreCurrentState, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_RestoreCurrentState, MethodCallID.Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_GetLogEntryType, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.IndentProcessor_SetTabSize);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.Logger_SetLogEntryType_Generating);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.IndentProcessor_GetIndent, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 3, 4);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.Logger_WriteLogEntries, 4);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgSegmentHasNoTextLines, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.Logger_WriteLogEntries, 3);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(2));
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SaveCurrentState())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SaveCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _padSegmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, padTextItem))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_padTextLine))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(_padTextLine)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.RestoreCurrentState())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_RestoreCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 3))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 4))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.IndentProcessor_SaveCurrentState);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SaveCurrentState, MethodCallID.Logger_Log_Message, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_RestoreCurrentState, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_RestoreCurrentState, MethodCallID.Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetIndent, 2, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 3, 4);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.Logger_WriteLogEntries, 4);

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
            _locater
                .SetupProperty(locater => locater.CurrentSegment, _lastSegmentName);
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.IndentProcessor_SetTabSize);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.Logger_SetLogEntryType_Generating);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.Logger_WriteLogEntries, 3);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(_textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(_textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(_textLine3)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem4))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 3))
                .Returns(12)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(_textLine4))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 4))
                .Returns(_textLine4)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 3, 4);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.Logger_WriteLogEntries, 4);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgSegmentNameIsNullOrWhitespace, null, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgAttemptToGenerateSegmentBeforeItWasLoaded, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnknownSegmentName, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Exactly(2));
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetFullPath))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToLoadTemplateFile, _templateFilePath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_GetFullPath, MethodCallID.Logger_Log_Message, 0, 1);
            SetupResetAll(MethodCallID.Logger_Log_Message, null, 1, 0, true, _templateFileName);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Exactly(2));
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.PathValidater_ValidateFullPath))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToLoadTemplateFile, _templateFilePath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_GetFullPath, MethodCallID.PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.PathValidater_ValidateFullPath, MethodCallID.Logger_Log_Message, 0, 1);
            SetupResetAll(MethodCallID.Logger_Log_Message, null, 1, 0, true, _templateFileName);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath))
                .Returns(_emptyString)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_GetFullPath, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_SetFilePath);
            SetupResetAll(MethodCallID.TextReader_SetFilePath, null, 0, 0, true, "");

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.PathValidater_ValidateFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(fileNameSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath))
                .Returns(filePathSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgNextLoadRequestBeforeFirstIsWritten, _templateFileName, _lastTemplateFileName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_ReadTextFile))
                .Returns(SampleText)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgLoadingTemplateFile, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(templateLoader => templateLoader.LoadTemplate(SampleText, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_GetFullPath, MethodCallID.PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.PathValidater_ValidateFullPath, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_SetFilePath, MethodCallID.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.Logger_Log_Message, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.TextReader_ReadTextFile, 1);
            SetupResetAll(MethodCallID.TextReader_ReadTextFile, MethodCallID.Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.TemplateLoader_LoadTemplate, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TemplateLoader_LoadTemplate, MethodCallID.Logger_WriteLogEntries);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.PathValidater_ValidateFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(fileNameSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath))
                .Returns(filePathSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_ReadTextFile))
                .Returns(SampleText)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgLoadingTemplateFile, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(templateLoader => templateLoader.LoadTemplate(SampleText, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_GetFullPath, MethodCallID.PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.PathValidater_ValidateFullPath, MethodCallID.TextReader_GetFileName, 0, -1);
            _verifier.DefineExpectedCallOrder(MethodCallID.PathValidater_ValidateFullPath, MethodCallID.TextReader_GetFullFilePath, 0, -1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_SetFilePath, MethodCallID.TextReader_GetFullFilePath, 0, -2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_ReadTextFile, -2);
            SetupResetAll(MethodCallID.TextReader_ReadTextFile, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.TemplateLoader_LoadTemplate);
            _verifier.DefineExpectedCallOrder(MethodCallID.TemplateLoader_LoadTemplate, MethodCallID.Logger_WriteLogEntries);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.PathValidater_ValidateFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(fileNameSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath))
                .Returns(filePathSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_ReadTextFile))
                .Returns(returnedTemplateLines)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgLoadingTemplateFile, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(templateLoader => templateLoader.LoadTemplate(returnedTemplateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_GetFullPath, MethodCallID.PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.PathValidater_ValidateFullPath, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.PathValidater_ValidateFullPath, MethodCallID.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_SetFilePath, MethodCallID.TextReader_ReadTextFile);
            SetupResetAll(MethodCallID.TextReader_ReadTextFile, MethodCallID.TemplateLoader_LoadTemplate);
            _verifier.DefineExpectedCallOrder(MethodCallID.TemplateLoader_LoadTemplate, MethodCallID.Logger_WriteLogEntries);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.PathValidater_ValidateFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(_templateFileName)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath))
                .Returns(_templateFilePath)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgAttemptToLoadMoreThanOnce, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_GetFullPath, MethodCallID.PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.PathValidater_ValidateFullPath, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_SetFilePath, MethodCallID.TextReader_GetFullFilePath, 0, -2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.Logger_Log_Message, -2);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.PathValidater_ValidateFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(fileNameSequence.GetNext)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath))
                .Returns(filePathSequence.GetNext)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_ReadTextFile))
                .Returns(Array.Empty<string>())
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTemplateFileIsEmpty, _templateFilePath, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_GetFullPath, MethodCallID.PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(MethodCallID.PathValidater_ValidateFullPath, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_SetFilePath, -1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_SetFilePath, MethodCallID.TextReader_GetFullFilePath, 0, -2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_ReadTextFile, -2);
            SetupResetAll(MethodCallID.TextReader_ReadTextFile, MethodCallID.Logger_Log_Message, 0, 0, false, _emptyString, _templateFileName, -2);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgContinuationPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.MessageWriter_WriteLine))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.ConsoleReader_ReadLine))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToGetUserResponse, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_User, MethodCallID.MessageWriter_WriteLine);
            _verifier.DefineExpectedCallOrder(MethodCallID.MessageWriter_WriteLine, MethodCallID.ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(MethodCallID.ConsoleReader_ReadLine, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_SetLogEntryType_Loading);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.MessageWriter_WriteLine))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.ConsoleReader_ReadLine))
                .Returns(expected)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_User, MethodCallID.MessageWriter_WriteLine);
            _verifier.DefineExpectedCallOrder(MethodCallID.MessageWriter_WriteLine, MethodCallID.ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(MethodCallID.ConsoleReader_ReadLine, MethodCallID.Logger_SetLogEntryType_Loading);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgContinuationPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.MessageWriter_WriteLine))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.ConsoleReader_ReadLine))
                .Returns(expected)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_GetLogEntryType);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_User, MethodCallID.MessageWriter_WriteLine);
            _verifier.DefineExpectedCallOrder(MethodCallID.MessageWriter_WriteLine, MethodCallID.ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(MethodCallID.ConsoleReader_ReadLine, MethodCallID.Logger_SetLogEntryType_Loading);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Throws<ApplicationException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToResetAll, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Loading);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Verifiable(Times.Once);
            _defaultSegmentNameGenerator
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.DefaultSegmentNameGenerator_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ClearTokens))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ResetTokenDelimiters())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ResetTokenDelimiters))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.TokenProcessor_ResetTokenDelimiters);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.DefaultSegmentNameGenerator_Reset, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ClearTokens, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ResetTokenDelimiters, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Loading);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Verifiable(Times.Once);
            _defaultSegmentNameGenerator
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.DefaultSegmentNameGenerator_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ClearTokens))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ResetTokenDelimiters())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ResetTokenDelimiters))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(_templateFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTemplateHasBeenReset, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.TokenProcessor_ResetTokenDelimiters);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.DefaultSegmentNameGenerator_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ClearTokens, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ResetTokenDelimiters, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Loading);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Throws<ApplicationException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToResetGeneratedText, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(_templateFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextHasBeenReset, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = _segmentName1)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.LineNumber = 0)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_LineNumber_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Throws<ApplicationException>()
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgUnableToResetSegment, _segmentName1, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_LineNumber_Setter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Setter, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_LineNumber_Setter, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = segmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.LineNumber = 0)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_LineNumber_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgUnableToResetUnknownSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_LineNumber_Setter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Setter, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_LineNumber_Setter, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = _segmentName1)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.LineNumber = 0)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_LineNumber_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(_segmentName1)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgSegmentHasBeenReset, _segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_LineNumber_Setter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Setter, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_LineNumber_Setter, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ResetTokenDelimiters())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ResetTokenDelimiters))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.TokenProcessor_ResetTokenDelimiters);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ResetTokenDelimiters, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Writing)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidatePath(_outputDirectoryPath, false, false))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.PathValidater_ValidatePath))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CreateDirectory(_outputDirectoryPath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_CreateDirectory))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenCreatingOutputDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.PathValidater_ValidatePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.PathValidater_ValidatePath, MethodCallID.FileAndDirectoryService_CreateDirectory);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_CreateDirectory, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Writing);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Writing)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidatePath(_outputDirectoryPath, false, false))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.PathValidater_ValidatePath))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenCreatingOutputDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.PathValidater_ValidatePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.PathValidater_ValidatePath, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Writing);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Writing)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidatePath(_outputDirectoryPath, false, false))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.PathValidater_ValidatePath))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CreateDirectory(_outputDirectoryPath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_CreateDirectory))
                .Returns(_outputDirectoryPath)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.PathValidater_ValidatePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.PathValidater_ValidatePath, MethodCallID.FileAndDirectoryService_CreateDirectory);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_CreateDirectory, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Writing);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(tabSize))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.IndentProcessor_SetTabSize);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.SetTabSize(tabSize);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void SetTemplateFilePath_ExceptionIsThrown_LogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Throws<ApplicationException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToSetTemplateFilePath, _templateFilePath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_SetFilePath, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.SetTemplateFilePath(_templateFilePath);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void SetTemplateFilePath_WhenCalled_InvokesTextReaderSetFilePath()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_SetFilePath, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

            // Act
            consoleBase.SetTemplateFilePath(_templateFilePath);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.SetTokenDelimiters(tokenStart, tokenEnd, escapeChar))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_SetTokenDelimiters))
                .Returns(expected)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.TokenProcessor_SetTokenDelimiters);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_SetTokenDelimiters, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.SetTokenDelimiters(tokenStart, tokenEnd, escapeChar))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_SetTokenDelimiters))
                .Returns(expected)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.TokenProcessor_SetTokenDelimiters);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_SetTokenDelimiters, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetSolutionDirectory))
                .Returns(SolutionDirectory)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.FileAndDirectoryService_GetSolutionDirectory);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetSolutionDirectory))
                .Throws<ArgumentException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenLocatingSolutionDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.FileAndDirectoryService_GetSolutionDirectory);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_GetSolutionDirectory, MethodCallID.Logger_Log_Message, 0, 1);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetSolutionDirectory))
                .Returns(SolutionDirectory)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.FileAndDirectoryService_GetSolutionDirectory);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_GetSolutionDirectory))
                .Returns(SolutionDirectory)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.FileAndDirectoryService_GetSolutionDirectory);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombinePaths(_outputDirectoryPath, _outputFileName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhileConstructingFilePath, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Writing);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Writing, MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
            _logger
                .SetupSequence(logger => logger.GetLogEntryType())
                .Returns(LogEntryType.Generating)
                .Returns(LogEntryType.Writing)
                .Throws<ApplicationException>();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.AtLeastOnce);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombinePaths(_outputDirectoryPath, expectedFileName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName))
                .Returns(expectedFilePath)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(expectedFilePath, SampleText))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextWriter_WriteTextFile))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(expectedFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextHasBeenReset, expectedFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Writing, MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName, MethodCallID.TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextWriter_WriteTextFile, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_SetLogEntryType_Writing);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Writing, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.AtLeastOnce);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombinePaths(_outputDirectoryPath, _outputFileName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName))
                .Returns(_outputFilePath)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(_outputFilePath, SampleText))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextWriter_WriteTextFile))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.AtLeastOnce);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Writing, MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName, MethodCallID.TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextWriter_WriteTextFile, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
            _logger
                .SetupSequence(logger => logger.GetLogEntryType())
                .Returns(LogEntryType.Generating)
                .Returns(LogEntryType.Writing)
                .Throws<ApplicationException>();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.AtLeastOnce);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombinePaths(_outputDirectoryPath, _outputFileName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName))
                .Returns(_outputFilePath)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(_outputFilePath, SampleText))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextWriter_WriteTextFile))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(_outputFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextHasBeenReset, _outputFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Writing, MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.FileAndDirectoryService_CombineDirectoryAndFileName, MethodCallID.TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextWriter_WriteTextFile, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Generating)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgOutputDirectoryNotSet, null, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Writing);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Writing, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Generating);

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

        private void SetupResetAll(MethodCallID callBefore,
                                   MethodCallID? callAfter,
                                   int firstCallNumber = 0,
                                   int secondCallNumber = 0,
                                   bool shouldDisplayMessage = false,
                                   string firstFileName = "",
                                   string? secondFileName = null,
                                   int getFileNameCallNumber = 0)
        {
            _logger
                .Setup(logger => logger.GetLogEntryType())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_GetLogEntryType))
                .Returns(LogEntryType.Loading)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Verifiable(Times.Once);
            _defaultSegmentNameGenerator
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.DefaultSegmentNameGenerator_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ClearTokens))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ResetTokenDelimiters())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ResetTokenDelimiters))
                .Verifiable(Times.Once);

            if (shouldDisplayMessage)
            {
                if (secondFileName is null)
                {
                    _textReader
                                .Setup(textReader => textReader.FileName)
                                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                                .Returns(firstFileName)
                                .Verifiable(Times.AtLeastOnce);
                }
                else
                {
                    Sequence<string> fileNameSequence = new([firstFileName, secondFileName]);
                    _textReader
                        .Setup(textReader => textReader.FileName)
                        .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                        .Returns(fileNameSequence.GetNext)
                        .Verifiable(Times.AtLeast(2));
                }

                _logger
                    .Setup(logger => logger.Log(MsgTemplateHasBeenReset, firstFileName, null))
                    .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 99))
                    .Verifiable(Times.Once);
            }

            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_WriteLogEntries))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.AtLeastOnce);

            _verifier.DefineExpectedCallOrder(callBefore, MethodCallID.Logger_GetLogEntryType, firstCallNumber);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_GetLogEntryType, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.TokenProcessor_ResetTokenDelimiters);

            if (shouldDisplayMessage)
            {
                _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.TextReader_GetFileName, 0, getFileNameCallNumber);
                _verifier.DefineExpectedCallOrder(MethodCallID.DefaultSegmentNameGenerator_Reset, MethodCallID.TextReader_GetFileName, 0, getFileNameCallNumber);
                _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.TextReader_GetFileName, 0, getFileNameCallNumber);
                _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ClearTokens, MethodCallID.TextReader_GetFileName, 0, getFileNameCallNumber);
                _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ResetTokenDelimiters, MethodCallID.TextReader_GetFileName, 0, getFileNameCallNumber);
                _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.Logger_Log_Message, getFileNameCallNumber, 99);
                _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_WriteLogEntries, 99);
            }
            else
            {
                _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_WriteLogEntries);
                _verifier.DefineExpectedCallOrder(MethodCallID.DefaultSegmentNameGenerator_Reset, MethodCallID.Logger_WriteLogEntries);
                _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.Logger_WriteLogEntries);
                _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ClearTokens, MethodCallID.Logger_WriteLogEntries);
                _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ResetTokenDelimiters, MethodCallID.Logger_WriteLogEntries);
            }

            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_WriteLogEntries, MethodCallID.Logger_SetLogEntryType_Loading);

            if (callAfter is not null)
            {
                MethodCallID methodCallID = callAfter.Value;
                _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, methodCallID, 0, secondCallNumber);
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