namespace TextTemplateProcessor.Console
{
    public class TextTemplateConsoleBaseTests
    {
        private static readonly string _outputDirectoryPath = $"{VolumeRoot}{Sep}generated";
        private static readonly string _outputFileName = "output.txt";
        private static readonly string _outputFilePath = $"{_outputDirectoryPath}{Sep}{_outputFileName}";
        private static readonly string _templateDirectoryPath = $"{VolumeRoot}{Sep}test";
        private static readonly string _templateFileName = "template.txt";
        private static readonly string _templateFilePath = $"{_templateDirectoryPath}{Sep}{_templateFileName}";
        private readonly Mock<IConsoleReader> _consoleReader = new();
        private readonly Mock<IDefaultSegmentNameGenerator> _defaultSegmentNameGenerator = new();
        private readonly Mock<IFileAndDirectoryService> _fileService = new();
        private readonly Mock<IIndentProcessor> _indentProcessor = new();
        private readonly Mock<ILocater> _locater = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<IMessageWriter> _messageWriter = new();
        private readonly Mock<IPathValidater> _pathValidater = new();
        private readonly Mock<ITemplateLoader> _templateLoader = new();
        private readonly Mock<ITextReader> _textReader = new();
        private readonly Mock<ITextWriter> _textWriter = new();
        private readonly Mock<ITokenProcessor> _tokenProcessor = new();
        private readonly MethodCallOrderVerifier _verifier = new();

        [Fact]
        public void ClearOutputDirectory_ErrorWhileClearingDirectory_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase = GetMyConsoleBase();
            InitializeOutputDirectoryPath(consoleBase);
            InitializeMocks();
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.MessageWriter_WriteLine_First))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCall.MessageWriter_WriteLine_Second))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCall.ConsoleReader_ReadLine))
                .Returns("y")
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.ClearDirectory(_outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_ClearDirectory))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenClearingOutputDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.MessageWriter_WriteLine_First, MethodCall.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_WriteLogEntries, MethodCall.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_User, MethodCall.MessageWriter_WriteLine_Second);
            _verifier.DefineExpectedCallOrder(MethodCall.MessageWriter_WriteLine_Second, MethodCall.ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(MethodCall.ConsoleReader_ReadLine, MethodCall.FileAndDirectoryService_ClearDirectory);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_ClearDirectory, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.Logger_WriteLogEntries);

            // Act
            consoleBase.ClearOutputDirectory();

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void ClearOutputDirectory_ErrorWhileReadingUserResponse_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            MyConsoleBase consoleBase = GetMyConsoleBase();
            InitializeOutputDirectoryPath(consoleBase);
            InitializeMocks();
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.MessageWriter_WriteLine_First))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCall.MessageWriter_WriteLine_Second))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCall.ConsoleReader_ReadLine))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhileReadingUserResponse, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.MessageWriter_WriteLine_First, MethodCall.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_WriteLogEntries, MethodCall.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_User, MethodCall.MessageWriter_WriteLine_Second);
            _verifier.DefineExpectedCallOrder(MethodCall.MessageWriter_WriteLine_Second, MethodCall.ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(MethodCall.ConsoleReader_ReadLine, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.Logger_WriteLogEntries);

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
            InitializeMocks();
            MyConsoleBase consoleBase = GetMyConsoleBase();
            InitializeOutputDirectoryPath(consoleBase);
            InitializeMocks();
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.MessageWriter_WriteLine_First))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCall.MessageWriter_WriteLine_Second))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCall.ConsoleReader_ReadLine))
                .Returns(userResponse)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.MessageWriter_WriteLine_First, MethodCall.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_WriteLogEntries, MethodCall.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_User, MethodCall.MessageWriter_WriteLine_Second);
            _verifier.DefineExpectedCallOrder(MethodCall.MessageWriter_WriteLine_Second, MethodCall.ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(MethodCall.ConsoleReader_ReadLine, MethodCall.Logger_WriteLogEntries);

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
            InitializeMocks();

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
            InitializeMocks();
            MyConsoleBase consoleBase = GetMyConsoleBase();
            InitializeOutputDirectoryPath(consoleBase);
            InitializeMocks();
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine(MsgClearTheOutputDirectory, _outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.MessageWriter_WriteLine_First))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCall.MessageWriter_WriteLine_Second))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCall.ConsoleReader_ReadLine))
                .Returns(userResponse)
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.ClearDirectory(_outputDirectoryPath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_ClearDirectory))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgOutputDirectoryCleared, null, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.MessageWriter_WriteLine_First, MethodCall.Logger_WriteLogEntries);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_WriteLogEntries, MethodCall.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_User, MethodCall.MessageWriter_WriteLine_Second);
            _verifier.DefineExpectedCallOrder(MethodCall.MessageWriter_WriteLine_Second, MethodCall.ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(MethodCall.ConsoleReader_ReadLine, MethodCall.FileAndDirectoryService_ClearDirectory);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_ClearDirectory, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.Logger_WriteLogEntries);

            // Act
            consoleBase.ClearOutputDirectory();

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_ErrorWhenGettingFullTemplateFilePath_LogsMessageAndResetsState()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            string segmentName = "Segment1";
            string text = "Line 1";
            consoleBase._generatedText.Add(text);
            consoleBase._controlDictionary[segmentName] = new();
            consoleBase._segmentDictionary[segmentName] = new() { new(0, false, false, text) };
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFullPath))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToLoadTemplateFile, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_FirstMessage))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                .Verifiable(Times.Once);
            _defaultSegmentNameGenerator
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.DefaultSegmentNameGenerator_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName))
                .Returns(_templateFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTemplateHasBeenReset, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_SecondMessage))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetFullPath, MethodCall.Logger_Log_FirstMessage);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.TextReader_FileName);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FileName, MethodCall.Logger_Log_SecondMessage);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_SecondMessage, MethodCall.Logger_WriteLogEntries);

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
            string segmentName = "Segment1";
            string text = "Line 1";
            consoleBase._generatedText.Add(text);
            consoleBase._controlDictionary[segmentName] = new();
            consoleBase._segmentDictionary[segmentName] = new() { new(0, false, false, text) };
            consoleBase.IsTemplateLoaded = true;
            consoleBase.IsOutputFileWritten = true;
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToLoadTemplateFile, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_FirstMessage))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                .Verifiable(Times.Once);
            _defaultSegmentNameGenerator
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.DefaultSegmentNameGenerator_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName))
                .Returns(_templateFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTemplateHasBeenReset, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_SecondMessage))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetFullPath, MethodCall.PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.Logger_Log_FirstMessage);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.TextReader_FileName);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FileName, MethodCall.Logger_Log_SecondMessage);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_SecondMessage, MethodCall.Logger_WriteLogEntries);

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
        public void LoadTemplate_NoErrors_LoadsTemplateAndLogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase._generatedText.Add("Line 1");
            consoleBase.IsTemplateLoaded = false;
            consoleBase.IsOutputFileWritten = true;
            string returnedFileName = string.Empty;
            string returnedFilePath = string.Empty;
            List<string> returnedTemplateLines = new() { "Line 1" };
            void firstFileNameAction() => returnedFileName = string.Empty;
            void firstFilePathAction() => returnedFilePath = string.Empty;
            void secondFileNameAction() => returnedFileName = _templateFileName;
            void secondFilePathAction() => returnedFilePath = _templateFilePath;
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.AtLeastOnce);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, It.IsAny<string>(), true))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName, secondFileNameAction, firstFileNameAction))
                .Returns(() => returnedFileName)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FullFilePath, secondFilePathAction, firstFilePathAction))
                .Returns(() => returnedFilePath)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_ReadTextFile))
                .Returns(returnedTemplateLines)
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                .Verifiable(Times.Once);
            _defaultSegmentNameGenerator
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.DefaultSegmentNameGenerator_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgLoadingTemplateFile, _templateFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(templateLoader => templateLoader.LoadTemplate(returnedTemplateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.FileAndDirectoryService_GetFullPath);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetFullPath, MethodCall.PathValidater_ValidateFullPath);
            _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.TextReader_FileName);
            _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.TextReader_FullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FileName, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FullFilePath, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_SetFilePath, MethodCall.TextReader_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.TextReader_FileName);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.TextReader_FileName);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.TextReader_FileName);
            _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.TextReader_FileName);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FileName, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.TemplateLoader_LoadTemplate);
            _verifier.DefineExpectedCallOrder(MethodCall.TemplateLoader_LoadTemplate, MethodCall.Logger_WriteLogEntries);

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
        public void PromptUserForInput_ErrorDuringConsoleReadLine_LogsMessageAndReturnsEmptyString()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            InitializeMocks();
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgContinuationPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCall.MessageWriter_WriteLine))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCall.ConsoleReader_ReadLine))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhileReadingUserResponse, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_WriteLogEntries, MethodCall.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_User, MethodCall.MessageWriter_WriteLine);
            _verifier.DefineExpectedCallOrder(MethodCall.MessageWriter_WriteLine, MethodCall.ConsoleReader_ReadLine);
            _verifier.DefineExpectedCallOrder(MethodCall.ConsoleReader_ReadLine, MethodCall.Logger_Log_Message);

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
            string expected = "test";
            MyConsoleBase consoleBase = GetMyConsoleBase();
            InitializeMocks();
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgYesNoPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCall.MessageWriter_WriteLine))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCall.ConsoleReader_ReadLine))
                .Returns(expected)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_WriteLogEntries, MethodCall.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_User, MethodCall.MessageWriter_WriteLine);
            _verifier.DefineExpectedCallOrder(MethodCall.MessageWriter_WriteLine, MethodCall.ConsoleReader_ReadLine);

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
            string expected = "test";
            MyConsoleBase consoleBase = GetMyConsoleBase();
            InitializeMocks();
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.User))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_User))
                .Verifiable(Times.Once);
            _messageWriter
                .Setup(messageWriter => messageWriter.WriteLine("\n" + MsgContinuationPrompt + "\n"))
                .Callback(_verifier.GetCallOrderAction(MethodCall.MessageWriter_WriteLine))
                .Verifiable(Times.Once);
            _consoleReader
                .Setup(consoleReader => consoleReader.ReadLine())
                .Callback(_verifier.GetCallOrderAction(MethodCall.ConsoleReader_ReadLine))
                .Returns(expected)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_WriteLogEntries, MethodCall.Logger_SetLogEntryType_User);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_User, MethodCall.MessageWriter_WriteLine);
            _verifier.DefineExpectedCallOrder(MethodCall.MessageWriter_WriteLine, MethodCall.ConsoleReader_ReadLine);

            // Act
            string actual = consoleBase.PromptUserForInput();

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void SetOutputDirectory_ErrorWhenCreatingOutputDirectory_ClearsOutputDirectoryAndLogsMessage()
        {
            // Arrange
            MyConsoleBase consoleBase = GetMyConsoleBase();
            consoleBase.OutputDirectory = @"C:\TestOutput";
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidatePath(_outputDirectoryPath, false, false))
                .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidatePath))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CreateDirectory(_outputDirectoryPath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_CreateDirectory))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenCreatingOutputDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.PathValidater_ValidatePath);
            _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidatePath, MethodCall.FileAndDirectoryService_CreateDirectory);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_CreateDirectory, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.Logger_WriteLogEntries);

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
            consoleBase.OutputDirectory = @"C:\TestOutput";
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidatePath(_outputDirectoryPath, false, false))
                .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidatePath))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenCreatingOutputDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.PathValidater_ValidatePath);
            _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidatePath, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.Logger_WriteLogEntries);

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
            consoleBase.OutputDirectory = @"C:\TestOutput";
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidatePath(_outputDirectoryPath, false, false))
                .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidatePath))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CreateDirectory(_outputDirectoryPath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_CreateDirectory))
                .Returns(_outputDirectoryPath)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.PathValidater_ValidatePath);
            _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidatePath, MethodCall.FileAndDirectoryService_CreateDirectory);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_CreateDirectory, MethodCall.Logger_WriteLogEntries);

            // Act
            consoleBase.SetOutputDirectory(_outputDirectoryPath);

            // Assert
            consoleBase.OutputDirectory
                .Should()
                .Be(_outputDirectoryPath);
            VerifyMocks();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructorUnableToGetFullTemplateFilePath_LogsMessageAndInitializesProperties(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetSolutionDirectory))
                .Returns(SolutionDirectory)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.FileAndDirectoryService_GetSolutionDirectory);

            if (useTemplateFilePath)
            {
                _logger
                    .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                    .Verifiable(Times.Once);
                _fileService
                    .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, SolutionDirectory, true))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFullPath))
                    .Throws<ArgumentException>()
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.Log(MsgUnableToLoadTemplateFile, It.IsAny<string>(), null))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_FirstMessage))
                    .Verifiable(Times.Once);
                _locater
                    .Setup(locater => locater.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                    .Verifiable(Times.Once);
                _defaultSegmentNameGenerator
                    .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.DefaultSegmentNameGenerator_Reset))
                    .Verifiable(Times.Once);
                _indentProcessor
                    .Setup(indentProcessor => indentProcessor.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                    .Verifiable(Times.Once);
                _tokenProcessor
                    .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(textReader => textReader.FileName)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName))
                    .Returns(_templateFileName)
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.Log(MsgTemplateHasBeenReset, _templateFileName, null))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_SecondMessage))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.WriteLogEntries())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                    .Verifiable(Times.Once);
                _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetSolutionDirectory, MethodCall.Logger_SetLogEntryType_Loading);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.FileAndDirectoryService_GetFullPath);
                _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetFullPath, MethodCall.Logger_Log_FirstMessage);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.Locater_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.DefaultSegmentNameGenerator_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.IndentProcessor_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.TokenProcessor_ClearTokens);
                _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_SetLogEntryType_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_SetLogEntryType_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_SetLogEntryType_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_SetLogEntryType_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.TextReader_FileName);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FileName, MethodCall.Logger_Log_SecondMessage);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_SecondMessage, MethodCall.Logger_WriteLogEntries);
            }

            // Act
            MyConsoleBase consoleBase = useTemplateFilePath ? GetMyConsoleBase(_templateFilePath) : GetMyConsoleBase();

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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructorUnableToGetSolutionDirectory_LogsMessageAndInitializesProperties(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            string templateFileName = string.Empty;
            string templateFilePath = string.Empty;
            void firstFileNameAction() => templateFileName = string.Empty;
            void firstFilePathAction() => templateFilePath = string.Empty;
            void fileNameAction() => templateFileName = _templateFileName;
            void filePathAction() => templateFilePath = _templateFilePath;
            List<string> templateLines = new() { "Line 1" };
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetSolutionDirectory))
                .Throws<ArgumentException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhenLocatingSolutionDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_FirstMessage))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.FileAndDirectoryService_GetSolutionDirectory);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetSolutionDirectory, MethodCall.Logger_Log_FirstMessage);

            if (useTemplateFilePath)
            {
                _logger
                    .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                    .Verifiable(Times.Exactly(2));
                _fileService
                    .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, string.Empty, true))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFullPath))
                    .Returns(_templateFilePath)
                    .Verifiable(Times.Once);
                _pathValidater
                    .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                    .Returns(_templateFilePath)
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(textReader => textReader.FileName)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName, fileNameAction, firstFileNameAction))
                    .Returns(() => templateFileName)
                    .Verifiable(Times.AtLeast(2));
                _textReader
                    .Setup(textReader => textReader.FullFilePath)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FullFilePath, filePathAction, firstFilePathAction))
                    .Returns(() => templateFilePath)
                    .Verifiable(Times.AtLeast(2));
                _textReader
                    .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(textReader => textReader.ReadTextFile())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_ReadTextFile))
                    .Returns(templateLines)
                    .Verifiable(Times.Once);
                _locater
                    .Setup(locater => locater.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                    .Verifiable(Times.Once);
                _defaultSegmentNameGenerator
                    .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.DefaultSegmentNameGenerator_Reset))
                    .Verifiable(Times.Once);
                _indentProcessor
                    .Setup(indentProcessor => indentProcessor.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                    .Verifiable(Times.Once);
                _tokenProcessor
                    .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.Log(MsgLoadingTemplateFile, _templateFileName, null))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_SecondMessage))
                    .Verifiable(Times.Once);
                _templateLoader
                    .Setup(templateLoader => templateLoader.LoadTemplate(templateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TemplateLoader_LoadTemplate))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.WriteLogEntries())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                    .Verifiable(Times.Once);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.Logger_SetLogEntryType_Loading);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.FileAndDirectoryService_GetFullPath);
                _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetFullPath, MethodCall.PathValidater_ValidateFullPath);
                _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.TextReader_FileName);
                _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.TextReader_FullFilePath);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FileName, MethodCall.TextReader_SetFilePath);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FullFilePath, MethodCall.TextReader_SetFilePath);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_SetFilePath, MethodCall.TextReader_ReadTextFile);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.Locater_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.DefaultSegmentNameGenerator_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.IndentProcessor_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.TokenProcessor_ClearTokens);
                _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_Log_SecondMessage);
                _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_Log_SecondMessage);
                _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_Log_SecondMessage);
                _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_Log_SecondMessage);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_SecondMessage, MethodCall.TemplateLoader_LoadTemplate);
                _verifier.DefineExpectedCallOrder(MethodCall.TemplateLoader_LoadTemplate, MethodCall.Logger_WriteLogEntries);
            }

            // Act
            MyConsoleBase consoleBase = useTemplateFilePath ? GetMyConsoleBase(_templateFilePath) : GetMyConsoleBase();

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
                .Be(useTemplateFilePath);
            consoleBase.SolutionDirectory
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructorUnableToValidateFullTemplateFilePath_LogsMessageAndInitializesProperties(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetSolutionDirectory))
                .Returns(SolutionDirectory)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.FileAndDirectoryService_GetSolutionDirectory);

            if (useTemplateFilePath)
            {
                _logger
                    .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                    .Verifiable(Times.Once);
                _fileService
                    .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, SolutionDirectory, true))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFullPath))
                    .Returns(_templateFilePath)
                    .Verifiable(Times.Once);
                _pathValidater
                    .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                    .Throws<ArgumentException>()
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.Log(MsgUnableToLoadTemplateFile, It.IsAny<string>(), null))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_FirstMessage))
                    .Verifiable(Times.Once);
                _locater
                    .Setup(locater => locater.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                    .Verifiable(Times.Once);
                _defaultSegmentNameGenerator
                    .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.DefaultSegmentNameGenerator_Reset))
                    .Verifiable(Times.Once);
                _indentProcessor
                    .Setup(indentProcessor => indentProcessor.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                    .Verifiable(Times.Once);
                _tokenProcessor
                    .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(textReader => textReader.FileName)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName))
                    .Returns(_templateFileName)
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.Log(MsgTemplateHasBeenReset, _templateFileName, null))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_SecondMessage))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.WriteLogEntries())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                    .Verifiable(Times.Once);
                _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetSolutionDirectory, MethodCall.Logger_SetLogEntryType_Loading);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.FileAndDirectoryService_GetFullPath);
                _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetFullPath, MethodCall.PathValidater_ValidateFullPath);
                _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.Logger_Log_FirstMessage);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.Locater_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.DefaultSegmentNameGenerator_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.IndentProcessor_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.TokenProcessor_ClearTokens);
                _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_SetLogEntryType_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_SetLogEntryType_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_SetLogEntryType_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_SetLogEntryType_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.TextReader_FileName);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FileName, MethodCall.Logger_Log_SecondMessage);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_SecondMessage, MethodCall.Logger_WriteLogEntries);
            }

            // Act
            MyConsoleBase consoleBase = useTemplateFilePath ? GetMyConsoleBase(_templateFilePath) : GetMyConsoleBase();

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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullConsoleReader_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.ConsoleReaderService,
                                                       ServiceParameterNames.ConsoleReaderParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      _logger.Object,
                                      null!,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                })
                : (() =>
                {
                    consoleBase = new(_logger.Object,
                                      null!,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullDefaultSegmentNameGenerator_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.DefaultSegmentNameGeneratorService,
                                                       ServiceParameterNames.DefaultSegmentNameGeneratorParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      _logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      null!,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                })
                : (() =>
                {
                    consoleBase = new(_logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      null!,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullFileService_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.FileAndDirectoryService,
                                                       ServiceParameterNames.FileAndDirectoryServiceParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      _logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      null!,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                })
                : (() =>
                {
                    consoleBase = new(_logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      null!,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullIndentProcessor_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.IndentProcessorService,
                                                       ServiceParameterNames.IndentProcessorParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      _logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      null!,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                })
                : (() =>
                {
                    consoleBase = new(_logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      null!,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullLocater_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.LocaterService,
                                                       ServiceParameterNames.LocaterParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      _logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      null!,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                })
                : (() =>
                {
                    consoleBase = new(_logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      null!,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullLogger_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.LoggerService,
                                                       ServiceParameterNames.LoggerParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      null!,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                })
                : (() =>
                {
                    consoleBase = new(null!,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullMessageWriter_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.MessageWriterService,
                                                       ServiceParameterNames.MessageWriterParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      _logger.Object,
                                      _consoleReader.Object,
                                      null!,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                })
                : (() =>
                {
                    consoleBase = new(_logger.Object,
                                      _consoleReader.Object,
                                      null!,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullPathValidater_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateConsoleBaseClass,
                                                       ServiceNames.PathValidaterService,
                                                       ServiceParameterNames.PathValidaterParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      _logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      null!,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                })
                : (() =>
                {
                    consoleBase = new(_logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      null!,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullTemplateLoader_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.TemplateLoaderService,
                                                       ServiceParameterNames.TemplateLoaderParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      _logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      null!,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                })
                : (() =>
                {
                    consoleBase = new(_logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      null!,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullTextReader_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.TextReaderService,
                                                       ServiceParameterNames.TextReaderParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      _logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      null!,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                })
                : (() =>
                {
                    consoleBase = new(_logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      null!,
                                      _textWriter.Object,
                                      _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullTextWriter_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.TextWriterService,
                                                       ServiceParameterNames.TextWriterParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      _logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      null!,
                                      _tokenProcessor.Object);
                })
                : (() =>
                {
                    consoleBase = new(_logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      null!,
                                      _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingNullTokenProcessor_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            Action action;
            MyConsoleBase consoleBase;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.TokenProcessorService,
                                                       ServiceParameterNames.TokenProcessorParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    consoleBase = new(_templateFilePath,
                                      _logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      null!);
                })
                : (() =>
                {
                    consoleBase = new(_logger.Object,
                                      _consoleReader.Object,
                                      _messageWriter.Object,
                                      _fileService.Object,
                                      _pathValidater.Object,
                                      _defaultSegmentNameGenerator.Object,
                                      _indentProcessor.Object,
                                      _locater.Object,
                                      _templateLoader.Object,
                                      _textReader.Object,
                                      _textWriter.Object,
                                      null!);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateConsoleBase_ConstructUsingValidDependencies_InitializesProperties(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            string templateFileName = string.Empty;
            string templateFilePath = string.Empty;
            void firstFileNameAction() => templateFileName = string.Empty;
            void firstFilePathAction() => templateFilePath = string.Empty;
            void fileNameAction() => templateFileName = _templateFileName;
            void filePathAction() => templateFilePath = _templateFilePath;
            List<string> templateLines = new() { "Line 1" };
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetSolutionDirectory))
                .Returns(SolutionDirectory)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.FileAndDirectoryService_GetSolutionDirectory);

            if (useTemplateFilePath)
            {
                _logger
                    .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                    .Verifiable(Times.Exactly(2));
                _fileService
                    .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFullPath(_templateFilePath, SolutionDirectory, true))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFullPath))
                    .Returns(_templateFilePath)
                    .Verifiable(Times.Once);
                _pathValidater
                    .Setup(pathValidater => pathValidater.ValidateFullPath(_templateFilePath, true, true))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                    .Returns(_templateFilePath)
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(textReader => textReader.FileName)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName, fileNameAction, firstFileNameAction))
                    .Returns(() => templateFileName)
                    .Verifiable(Times.AtLeast(2));
                _textReader
                    .Setup(textReader => textReader.FullFilePath)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FullFilePath, filePathAction, firstFilePathAction))
                    .Returns(() => templateFilePath)
                    .Verifiable(Times.AtLeast(2));
                _textReader
                    .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(textReader => textReader.ReadTextFile())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_ReadTextFile))
                    .Returns(templateLines)
                    .Verifiable(Times.Once);
                _locater
                    .Setup(locater => locater.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                    .Verifiable(Times.Once);
                _defaultSegmentNameGenerator
                    .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.DefaultSegmentNameGenerator_Reset))
                    .Verifiable(Times.Once);
                _indentProcessor
                    .Setup(indentProcessor => indentProcessor.Reset())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                    .Verifiable(Times.Once);
                _tokenProcessor
                    .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.Log(MsgLoadingTemplateFile, _templateFileName, null))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                    .Verifiable(Times.Once);
                _templateLoader
                    .Setup(templateLoader => templateLoader.LoadTemplate(templateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TemplateLoader_LoadTemplate))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.WriteLogEntries())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                    .Verifiable(Times.Once);
                _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetSolutionDirectory, MethodCall.Logger_SetLogEntryType_Loading);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.FileAndDirectoryService_GetFullPath);
                _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetFullPath, MethodCall.PathValidater_ValidateFullPath);
                _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.TextReader_FileName);
                _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.TextReader_FullFilePath);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FileName, MethodCall.TextReader_SetFilePath);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FullFilePath, MethodCall.TextReader_SetFilePath);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_SetFilePath, MethodCall.TextReader_ReadTextFile);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.Locater_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.DefaultSegmentNameGenerator_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.IndentProcessor_Reset);
                _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.TokenProcessor_ClearTokens);
                _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_Log_Message);
                _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_Log_Message);
                _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_Log_Message);
                _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_Log_Message);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.TemplateLoader_LoadTemplate);
                _verifier.DefineExpectedCallOrder(MethodCall.TemplateLoader_LoadTemplate, MethodCall.Logger_WriteLogEntries);
            }

            // Act
            MyConsoleBase consoleBase = useTemplateFilePath ? GetMyConsoleBase(_templateFilePath) : GetMyConsoleBase();

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
                .Be(useTemplateFilePath);
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
            List<string> generatedText = new()
            {
                "Line 1",
                "Line 2",
                "Line 3"
            };
            consoleBase._generatedText.AddRange(generatedText);
            InitializeOutputDirectoryPath(consoleBase);
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombineDirectoryAndFileName(_outputDirectoryPath, _outputFileName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName))
                .Throws<IOException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhileConstructingFilePath, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Writing, MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.Logger_WriteLogEntries);

            // Act
            consoleBase.WriteGeneratedTextToFile(_outputFileName);

            // Assert
            consoleBase._generatedText
                .Should()
                .NotBeEmpty();
            consoleBase._generatedText
                .Should()
                .ContainInConsecutiveOrder(generatedText);
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
            List<string> generatedText = new()
            {
                "Line 1",
                "Line 2",
                "Line 3"
            };
            consoleBase._generatedText.AddRange(generatedText);
            string segmentName1 = "Segment1";
            string segmentName2 = "Segment2";
            consoleBase._controlDictionary[segmentName1] = new() { IsFirstTime = false };
            consoleBase._controlDictionary[segmentName2] = new() { IsFirstTime = false };
            string expectedFileName = "File_0001.txt";
            string expectedFilePath = $"{_outputDirectoryPath}{Sep}{expectedFileName}";
            InitializeOutputDirectoryPath(consoleBase);
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.AtLeastOnce);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombineDirectoryAndFileName(_outputDirectoryPath, expectedFileName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName))
                .Returns(expectedFilePath)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(expectedFilePath, generatedText))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextWriter_WriteTextFile))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName))
                .Returns(expectedFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextHasBeenReset, expectedFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Writing, MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName, MethodCall.TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.TextWriter_WriteTextFile, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.TextReader_FileName);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.TextReader_FileName);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FileName, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.Logger_WriteLogEntries);

            // Act
            consoleBase.WriteGeneratedTextToFile(string.Empty);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase._controlDictionary[segmentName1].IsFirstTime
                .Should()
                .BeTrue();
            consoleBase._controlDictionary[segmentName2].IsFirstTime
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
            List<string> generatedText = new()
            {
                "Line 1",
                "Line 2",
                "Line 3"
            };
            consoleBase._generatedText.AddRange(generatedText);
            string segmentName1 = "Segment1";
            string segmentName2 = "Segment2";
            consoleBase._controlDictionary[segmentName1] = new() { IsFirstTime = false };
            consoleBase._controlDictionary[segmentName2] = new() { IsFirstTime = false };
            InitializeOutputDirectoryPath(consoleBase);
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.AtLeastOnce);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombineDirectoryAndFileName(_outputDirectoryPath, _outputFileName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName))
                .Returns(_outputFilePath)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(_outputFilePath, generatedText))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextWriter_WriteTextFile))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Writing, MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName, MethodCall.TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.TextWriter_WriteTextFile, MethodCall.Logger_WriteLogEntries);

            // Act
            consoleBase.WriteGeneratedTextToFile(_outputFileName, false);

            // Assert
            consoleBase._generatedText
                .Should()
                .NotBeEmpty();
            consoleBase._generatedText
                .Should()
                .ContainInConsecutiveOrder(generatedText);
            consoleBase._controlDictionary[segmentName1].IsFirstTime
                .Should()
                .BeFalse();
            consoleBase._controlDictionary[segmentName2].IsFirstTime
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
            List<string> generatedText = new()
            {
                "Line 1",
                "Line 2",
                "Line 3"
            };
            consoleBase._generatedText.AddRange(generatedText);
            string segmentName1 = "Segment1";
            string segmentName2 = "Segment2";
            consoleBase._controlDictionary[segmentName1] = new() { IsFirstTime = false };
            consoleBase._controlDictionary[segmentName2] = new() { IsFirstTime = false };
            InitializeOutputDirectoryPath(consoleBase);
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.AtLeastOnce);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CombineDirectoryAndFileName(_outputDirectoryPath, _outputFileName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName))
                .Returns(_outputFilePath)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(_outputFilePath, generatedText))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextWriter_WriteTextFile))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName))
                .Returns(_outputFileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextHasBeenReset, _outputFileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Writing, MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_CombineDirectoryAndFileName, MethodCall.TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.TextWriter_WriteTextFile, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.TextReader_FileName);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.TextReader_FileName);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_FileName, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.Logger_WriteLogEntries);

            // Act
            consoleBase.WriteGeneratedTextToFile(_outputFileName);

            // Assert
            consoleBase._generatedText
                .Should()
                .BeEmpty();
            consoleBase._controlDictionary[segmentName1].IsFirstTime
                .Should()
                .BeTrue();
            consoleBase._controlDictionary[segmentName2].IsFirstTime
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
            List<string> generatedText = new()
            {
                "Line 1",
                "Line 2",
                "Line 3"
            };
            consoleBase._generatedText.AddRange(generatedText);
            InitializeMocks();
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgOutputDirectoryNotSet, null, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.WriteLogEntries())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_WriteLogEntries))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Writing, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.Logger_WriteLogEntries);

            // Act
            consoleBase.WriteGeneratedTextToFile(_outputFileName);

            // Assert
            consoleBase._generatedText
                .Should()
                .NotBeEmpty();
            consoleBase._generatedText
                .Should()
                .ContainInConsecutiveOrder(generatedText);
            consoleBase.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        private MyConsoleBase GetMyConsoleBase(string? templateFilePath = null)
        {
            return templateFilePath is null
                ? new MyConsoleBase(_logger.Object,
                                    _consoleReader.Object,
                                    _messageWriter.Object,
                                    _fileService.Object,
                                    _pathValidater.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object)
                : new MyConsoleBase(templateFilePath,
                                    _logger.Object,
                                    _consoleReader.Object,
                                    _messageWriter.Object,
                                    _fileService.Object,
                                    _pathValidater.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
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

        private void InitializeOutputDirectoryPath(MyConsoleBase consoleBase)
        {
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CreateDirectory(_outputDirectoryPath, It.IsAny<string>()))
                .Returns(_outputDirectoryPath);
            consoleBase.SetOutputDirectory(_outputDirectoryPath);
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

        private void VerifyMocks()
        {
            if (_consoleReader.Setups.Any())
            {
                _consoleReader.Verify();
            }

            if (_defaultSegmentNameGenerator.Setups.Any())
            {
                _defaultSegmentNameGenerator.Verify();
            }

            if (_fileService.Setups.Any())
            {
                _fileService.Verify();
            }

            if (_indentProcessor.Setups.Any())
            {
                _indentProcessor.Verify();
            }

            if (_locater.Setups.Any())
            {
                _locater.Verify();
            }

            if (_logger.Setups.Any())
            {
                _logger.Verify();
            }

            if (_messageWriter.Setups.Any())
            {
                _messageWriter.Verify();
            }

            if (_pathValidater.Setups.Any())
            {
                _pathValidater.Verify();
            }

            if (_templateLoader.Setups.Any())
            {
                _templateLoader.Verify();
            }

            if (_textReader.Setups.Any())
            {
                _textReader.Verify();
            }

            if (_textWriter.Setups.Any())
            {
                _textWriter.Verify();
            }

            if (_tokenProcessor.Setups.Any())
            {
                _tokenProcessor.Verify();
            }

            MocksVerifyNoOtherCalls();
            _verifier.Verify();
        }

        public class MyConsoleBase : TextTemplateConsoleBase
        {
            internal MyConsoleBase(ILogger logger,
                                   IConsoleReader consoleReader,
                                   IMessageWriter messageWriter,
                                   IFileAndDirectoryService fileService,
                                   IPathValidater pathValidater,
                                   IDefaultSegmentNameGenerator defaultSegmentNameGenerator,
                                   IIndentProcessor indentProcessor,
                                   ILocater locater,
                                   ITemplateLoader templateLoader,
                                   ITextReader textReader,
                                   ITextWriter textWriter,
                                   ITokenProcessor tokenProcessor) : base(logger,
                                                                          consoleReader,
                                                                          messageWriter,
                                                                          fileService,
                                                                          pathValidater,
                                                                          defaultSegmentNameGenerator,
                                                                          indentProcessor,
                                                                          locater,
                                                                          templateLoader,
                                                                          textReader,
                                                                          textWriter,
                                                                          tokenProcessor)
            {
            }

            internal MyConsoleBase(string templateFilePath,
                                   ILogger logger,
                                   IConsoleReader consoleReader,
                                   IMessageWriter messageWriter,
                                   IFileAndDirectoryService fileService,
                                   IPathValidater pathValidater,
                                   IDefaultSegmentNameGenerator defaultSegmentNameGenerator,
                                   IIndentProcessor indentProcessor,
                                   ILocater locater,
                                   ITemplateLoader templateLoader,
                                   ITextReader textReader,
                                   ITextWriter textWriter,
                                   ITokenProcessor tokenProcessor) : base(templateFilePath,
                                                                          logger,
                                                                          consoleReader,
                                                                          messageWriter,
                                                                          fileService,
                                                                          pathValidater,
                                                                          defaultSegmentNameGenerator,
                                                                          indentProcessor,
                                                                          locater,
                                                                          templateLoader,
                                                                          textReader,
                                                                          textWriter,
                                                                          tokenProcessor)
            {
            }
        }
    }
}