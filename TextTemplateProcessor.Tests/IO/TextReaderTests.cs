namespace TextTemplateProcessor.IO
{
    public class TextReaderTests
    {
        private readonly Mock<IFileAndDirectoryService> _fileService = new(MockBehavior.Strict);
        private readonly Mock<ILogger> _logger = new(MockBehavior.Strict);
        private readonly Mock<IPathValidater> _pathValidater = new(MockBehavior.Strict);
        private readonly MethodCallOrderVerifier _verifier = new();

        [Fact]
        public void ReadTextFile_ExceptionWhileReadingFile_LogsErrorMessage()
        {
            // Arrange
            InitializeMocks();
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";

            TextReader reader = GetTextReader(directoryPath, fileName, filePath);
            InitializeMocks();
            _logger
                .Setup(logger => logger.Log(MsgAttemptingToReadFile, filePath, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgErrorWhileReadingTemplateFile, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.ReadTextFile(filePath))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_ReadTextFile))
                .Throws<ArgumentException>()
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, FileAndDirectoryService_ReadTextFile, 1);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_ReadTextFile, Logger_Log_Message, 0, 2);

            // Act
            List<string> actual = reader.ReadTextFile().ToList();

            // Assert
            actual
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Fact]
        public void ReadTextFile_FilePathIsSet_ReadsTextFileAndLogsMessage()
        {
            // Arrange
            InitializeMocks();
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            TextReader reader = GetTextReader(directoryPath, fileName, filePath);
            InitializeMocks();
            _logger
                .Setup(logger => logger.Log(MsgAttemptingToReadFile, filePath, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgFileSuccessfullyRead, null, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.ReadTextFile(filePath))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_ReadTextFile))
                .Returns(SampleText)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, FileAndDirectoryService_ReadTextFile, 1);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_ReadTextFile, Logger_Log_Message, 0, 2);

            // Act
            List<string> actual = reader.ReadTextFile().ToList();

            // Assert
            actual
                .Should()
                .Equal(SampleText);
            VerifyMocks();
        }

        [Fact]
        public void ReadTextFile_FilePathNotSet_LogsErrorAndReturnsEmptyList()
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(logger => logger.Log(MsgTemplateFilePathNotSet, null, null))
                .Verifiable(Times.Once);
            TextReader reader = GetTextReader();

            // Act
            IEnumerable<string> actual = reader.ReadTextFile();

            // Assert
            actual
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Fact]
        public void SetFilePath_InvalidFilePath_LogsAnErrorAndInitializesFilePathProperties()
        {
            // Arrange (part 1) - set up a valid file path
            InitializeMocks();
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            TextReader reader = GetTextReader(directoryPath, fileName, filePath);

            // Arrange (part 2) - try to set an invalid file path
            InitializeMocks();
            string invalidFilePath = $@"{VolumeRoot}{Sep}invalid;path{Sep}file?";
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(invalidFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidateFullPath))
                .Throws<ArgumentException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToSetTemplateFilePath, invalidFilePath, It.IsAny<string>()))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, Logger_Log_Message);

            // Act
            reader.SetFilePath(invalidFilePath);

            // Assert
            reader.DirectoryPath
                .Should()
                .BeEmpty();
            reader.FileName
                .Should()
                .BeEmpty();
            reader.FullFilePath
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Fact]
        public void SetFilePath_ValidFilePath_SetsTheFilePathProperties()
        {
            // Arrange
            InitializeMocks();
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(filePath, true, true))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidateFullPath))
                .Returns(filePath)
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetDirectoryName(filePath))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetDirectoryName))
                .Returns(directoryPath)
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFileName(filePath))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetFileName))
                .Returns(fileName)
                .Verifiable(Times.Once);
            TextReader reader = GetTextReader();
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, FileAndDirectoryService_GetDirectoryName);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, FileAndDirectoryService_GetFileName);

            // Act
            reader.SetFilePath(filePath);

            // Assert
            reader.DirectoryPath
                .Should()
                .Be(directoryPath);
            reader.FileName
                .Should()
                .Be(fileName);
            reader.FullFilePath
                .Should()
                .Be(filePath);
            VerifyMocks();
        }

        [Fact]
        public void TextReader_ConstructWithInvalidFilePath_LogsAnError()
        {
            // Arrange
            InitializeMocks();
            string invalidFilePath = $@"{VolumeRoot}{Sep}invalid;path{Sep}file?";

            // Act
            TextReader reader = GetTextReader(string.Empty, string.Empty, invalidFilePath, true);

            // Assert
            reader.DirectoryPath
                .Should()
                .BeEmpty();
            reader.FileName
                .Should()
                .BeEmpty();
            reader.FullFilePath
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Fact]
        public void TextReader_ConstructWithNullFileServiceReference_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () =>
            {
                TextReader reader = new(null!,
                                        _logger.Object,
                                        _pathValidater.Object);
            };
            string expected = GetNullDependencyMessage(ClassNames.TextReaderClass,
                                                       ServiceNames.FileAndDirectoryService,
                                                       ServiceParameterNames.FileAndDirectoryServiceParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextReader_ConstructWithNullLoggerReference_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () =>
            {
                TextReader reader = new(_fileService.Object,
                                        null!,
                                        _pathValidater.Object);
            };
            string expected = GetNullDependencyMessage(ClassNames.TextReaderClass,
                                                       ServiceNames.LoggerService,
                                                       ServiceParameterNames.LoggerParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextReader_ConstructWithNullPathValidaterReference_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () =>
            {
                TextReader reader = new(_fileService.Object,
                                        _logger.Object,
                                        null!);
            };
            string expected = GetNullDependencyMessage(ClassNames.TextReaderClass,
                                                       ServiceNames.PathValidaterService,
                                                       ServiceParameterNames.PathValidaterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextReader_ConstructWithValidFilePath_SavesPathInfo()
        {
            // Arrange
            InitializeMocks();
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";

            // Act
            TextReader reader = GetTextReader(directoryPath, fileName, filePath);

            // Assert
            reader.DirectoryPath
                .Should()
                .Be(directoryPath);
            reader.FileName
                .Should()
                .Be(fileName);
            reader.FullFilePath
                .Should()
                .Be(filePath);
            VerifyMocks();
        }

        [Fact]
        public void TextReader_ConstructWithValidServiceReferences_InitializesProperties()
        {
            // Arrange/Act
            InitializeMocks();
            TextReader reader = GetTextReader();

            // Assert
            reader.DirectoryPath
                .Should()
                .BeEmpty();
            reader.FileName
                .Should()
                .BeEmpty();
            reader.FullFilePath
                .Should()
                .BeEmpty();
            MocksVerifyNoOtherCalls();
        }

        private TextReader GetTextReader() => new(_fileService.Object,
                                                  _logger.Object,
                                                  _pathValidater.Object);

        private TextReader GetTextReader(string directoryPath, string fileName, string filePath, bool shouldThrow = false)
        {
            if (shouldThrow)
            {
                _pathValidater
                    .Setup(pathValidater => pathValidater.ValidateFullPath(filePath, true, true))
                    .Callback(_verifier.GetCallOrderAction(PathValidater_ValidateFullPath))
                    .Throws<ArgumentException>()
                    .Verifiable(Times.Once);
                _logger
                    .Setup(logger => logger.Log(MsgUnableToSetTemplateFilePath, filePath, It.IsAny<string>()))
                    .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                    .Verifiable(Times.Once);
                _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, Logger_Log_Message);
            }
            else
            {
                _pathValidater
                    .Setup(pathValidater => pathValidater.ValidateFullPath(filePath, true, true))
                    .Callback(_verifier.GetCallOrderAction(PathValidater_ValidateFullPath))
                    .Returns(filePath)
                    .Verifiable(Times.Once);
                _fileService
                    .Setup(fileAndDirectoryService => fileAndDirectoryService.GetDirectoryName(filePath))
                    .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetDirectoryName))
                    .Returns(directoryPath)
                    .Verifiable(Times.Once);
                _fileService
                    .Setup(fileAndDirectoryService => fileAndDirectoryService.GetFileName(filePath))
                    .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_GetFileName))
                    .Returns(fileName)
                    .Verifiable(Times.Once);
                _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, FileAndDirectoryService_GetDirectoryName);
                _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, FileAndDirectoryService_GetFileName);
            }

            TextReader textReader = new(_fileService.Object,
                                        _logger.Object,
                                        _pathValidater.Object);
            textReader.SetFilePath(filePath);
            return textReader;
        }

        private void InitializeMocks()
        {
            _fileService.Reset();
            _logger.Reset();
            _pathValidater.Reset();
            _verifier.Reset();
        }

        private void MocksVerifyNoOtherCalls()
        {
            _fileService.VerifyNoOtherCalls();
            _logger.VerifyNoOtherCalls();
            _pathValidater.VerifyNoOtherCalls();
        }

        private void VerifyMocks()
        {
            if (_fileService.Setups.Any())
            {
                _fileService.Verify();
            }

            if (_logger.Setups.Any())
            {
                _logger.Verify();
            }

            if (_pathValidater.Setups.Any())
            {
                _pathValidater.Verify();
            }

            MocksVerifyNoOtherCalls();
            _verifier.Verify();
        }
    }
}