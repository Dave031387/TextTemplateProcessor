namespace TextTemplateProcessor.Console
{
    public class TextTemplateConsoleBaseTests
    {
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
        public void TextTemplateConsoleBase_ConstructorUnableToGetFullTemplateFilePath_LogsMessageAndInitializesProperties(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.GetSolutionDirectory())
                .Returns(SolutionDirectory)
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetSolutionDirectory))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.FileAndDirectoryService_GetSolutionDirectory);

            if (useTemplateFilePath)
            {
                _logger
                    .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading, null, true))
                    .Verifiable(Times.Once);
                _fileService
                    .Setup(x => x.GetFullPath(_templateFilePath, SolutionDirectory, true))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFullPath))
                    .Throws<ArgumentException>()
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.Log(MsgUnableToLoadTemplateFile, It.IsAny<string>(), null))
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
                    .Setup(x => x.ClearTokens())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.SetLogEntryType(LogEntryType.Reset))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(x => x.FileName)
                    .Returns(_templateFileName)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.Log(MsgTemplateHasBeenReset, _templateFileName, null))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_SecondMessage))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.WriteLogEntries())
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
            void fileNameAction() => templateFileName = _templateFileName;
            void filePathAction() => templateFilePath = _templateFilePath;
            List<string> templateLines = new() { "Line 1" };
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.GetSolutionDirectory())
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetSolutionDirectory))
                .Throws<ArgumentException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgErrorWhenLocatingSolutionDirectory, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_FirstMessage))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.FileAndDirectoryService_GetSolutionDirectory);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_GetSolutionDirectory, MethodCall.Logger_Log_FirstMessage);

            if (useTemplateFilePath)
            {
                _logger
                    .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading, null, true))
                    .Verifiable(Times.Exactly(2));
                _fileService
                    .Setup(x => x.GetFullPath(_templateFilePath, string.Empty, true))
                    .Returns(_templateFilePath)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFullPath))
                    .Verifiable(Times.Once);
                _pathValidater
                    .Setup(x => x.ValidateFullPath(_templateFilePath, true, true))
                    .Returns(_templateFilePath)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(x => x.FileName)
                    .Returns(() => templateFileName)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName, null, true, fileNameAction))
                    .Verifiable(Times.AtLeast(2));
                _textReader
                    .Setup(x => x.FullFilePath)
                    .Returns(() => templateFilePath)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FullFilePath, null, true, filePathAction))
                    .Verifiable(Times.AtLeast(2));
                _textReader
                    .Setup(x => x.SetFilePath(_templateFilePath))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(x => x.ReadTextFile())
                    .Returns(templateLines)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_ReadTextFile))
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
                    .Setup(x => x.ClearTokens())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.Log(MsgLoadingTemplateFile, _templateFileName, null))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_SecondMessage))
                    .Verifiable(Times.Once);
                _templateLoader
                    .Setup(x => x.LoadTemplate(templateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TemplateLoader_LoadTemplate))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.WriteLogEntries())
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
                .Setup(x => x.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.GetSolutionDirectory())
                .Returns(SolutionDirectory)
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetSolutionDirectory))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.FileAndDirectoryService_GetSolutionDirectory);

            if (useTemplateFilePath)
            {
                _logger
                    .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading, null, true))
                    .Verifiable(Times.Once);
                _fileService
                    .Setup(x => x.GetFullPath(_templateFilePath, SolutionDirectory, true))
                    .Returns(_templateFilePath)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFullPath))
                    .Verifiable(Times.Once);
                _pathValidater
                    .Setup(x => x.ValidateFullPath(_templateFilePath, true, true))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                    .Throws<ArgumentException>()
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.Log(MsgUnableToLoadTemplateFile, It.IsAny<string>(), null))
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
                    .Setup(x => x.ClearTokens())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.SetLogEntryType(LogEntryType.Reset))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(x => x.FileName)
                    .Returns(_templateFileName)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.Log(MsgTemplateHasBeenReset, _templateFileName, null))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_SecondMessage))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.WriteLogEntries())
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
        public void TextTemplateConsoleBase_ConstructUsingValidDependencies_InitializesProperties(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
            string templateFileName = string.Empty;
            string templateFilePath = string.Empty;
            void fileNameAction() => templateFileName = _templateFileName;
            void filePathAction() => templateFilePath = _templateFilePath;
            List<string> templateLines = new() { "Line 1" };
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.GetSolutionDirectory())
                .Returns(SolutionDirectory)
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetSolutionDirectory))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.FileAndDirectoryService_GetSolutionDirectory);

            if (useTemplateFilePath)
            {
                _logger
                    .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading, null, true))
                    .Verifiable(Times.Exactly(2));
                _fileService
                    .Setup(x => x.GetFullPath(_templateFilePath, SolutionDirectory, true))
                    .Returns(_templateFilePath)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFullPath))
                    .Verifiable(Times.Once);
                _pathValidater
                    .Setup(x => x.ValidateFullPath(_templateFilePath, true, true))
                    .Returns(_templateFilePath)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(x => x.FileName)
                    .Returns(() => templateFileName)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FileName, null, true, fileNameAction))
                    .Verifiable(Times.AtLeast(2));
                _textReader
                    .Setup(x => x.FullFilePath)
                    .Returns(() => templateFilePath)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_FullFilePath, null, true, filePathAction))
                    .Verifiable(Times.AtLeast(2));
                _textReader
                    .Setup(x => x.SetFilePath(_templateFilePath))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(x => x.ReadTextFile())
                    .Returns(templateLines)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_ReadTextFile))
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
                    .Setup(x => x.ClearTokens())
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.Log(MsgLoadingTemplateFile, _templateFileName, null))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                    .Verifiable(Times.Once);
                _templateLoader
                    .Setup(x => x.LoadTemplate(templateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TemplateLoader_LoadTemplate))
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.WriteLogEntries())
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