namespace TextTemplateProcessor.IO
{
    public class TextReaderTests
    {
        private readonly Mock<IFileAndDirectoryService> _fileService = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<IPathValidater> _pathValidater = new();
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
                .Setup(x => x.Log(MsgAttemptingToReadFile, filePath, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_FirstMessage))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgErrorWhileReadingTemplateFile, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_SecondMessage))
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.ReadTextFile(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_ReadTextFile))
                .Throws<ArgumentException>()
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.FileAndDirectoryService_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_ReadTextFile, MethodCall.Logger_Log_SecondMessage);

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
            List<string> expected = new()
            {
                "Line 1",
                "Line 2",
                "Line 3"
            };
            _logger
                .Setup(x => x.Log(MsgAttemptingToReadFile, filePath, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_FirstMessage))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgFileSuccessfullyRead, null, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_SecondMessage))
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.ReadTextFile(filePath))
                .Returns(expected)
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_ReadTextFile))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.FileAndDirectoryService_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.FileAndDirectoryService_ReadTextFile, MethodCall.Logger_Log_SecondMessage);

            // Act
            List<string> actual = reader.ReadTextFile().ToList();

            // Assert
            actual
                .Should()
                .Equal(expected);
            VerifyMocks();
        }

        [Fact]
        public void ReadTextFile_FilePathNotSet_LogsErrorAndReturnsEmptyList()
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(x => x.Log(MsgTemplateFilePathNotSet, null, null))
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
                .Setup(x => x.ValidateFullPath(invalidFilePath, true, true))
                .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                .Throws<ArgumentException>()
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgUnableToSetTemplateFilePath, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.Logger_Log_Message);

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
                .Setup(x => x.ValidateFullPath(filePath, true, true))
                .Returns(filePath)
                .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.GetDirectoryName(filePath))
                .Returns(directoryPath)
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetDirectoryName))
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.GetFileName(filePath))
                .Returns(fileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFileName))
                .Verifiable(Times.Once);
            TextReader reader = GetTextReader();
            _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.FileAndDirectoryService_GetDirectoryName);
            _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.FileAndDirectoryService_GetFileName);

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
        public void TextReader_ConstructWithFilePathAndNullFileServiceReference_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            string filePath = NextAbsoluteFilePath;
            Action action = () =>
            {
                TextReader reader = new(filePath,
                                        _logger.Object,
                                        null!,
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
        public void TextReader_ConstructWithFilePathAndNullLoggerReference_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            string filePath = NextAbsoluteFilePath;
            Action action = () =>
            {
                TextReader reader = new(filePath,
                                        null!,
                                        _fileService.Object,
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
        public void TextReader_ConstructWithFilePathAndNullPathValidaterReference_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            string filePath = NextAbsoluteFilePath;
            Action action = () =>
            {
                TextReader reader = new(filePath,
                                        _logger.Object,
                                        _fileService.Object,
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
                TextReader reader = new(_logger.Object,
                                        null!,
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
                TextReader reader = new(null!,
                                        _fileService.Object,
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
                TextReader reader = new(_logger.Object,
                                        _fileService.Object,
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

        private TextReader GetTextReader() => new(_logger.Object,
                                                  _fileService.Object,
                                                  _pathValidater.Object);

        private TextReader GetTextReader(string directoryPath, string fileName, string filePath, bool shouldThrow = false)
        {
            if (shouldThrow)
            {
                _pathValidater
                    .Setup(x => x.ValidateFullPath(filePath, true, true))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                    .Throws<ArgumentException>()
                    .Verifiable(Times.Once);
                _logger
                    .Setup(x => x.Log(MsgUnableToSetTemplateFilePath, It.IsAny<string>(), null))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                    .Verifiable(Times.Once);
                _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.Logger_Log_Message);
            }
            else
            {
                _pathValidater
                    .Setup(x => x.ValidateFullPath(filePath, true, true))
                    .Returns(filePath)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.PathValidater_ValidateFullPath))
                    .Verifiable(Times.Once);
                _fileService
                    .Setup(x => x.GetDirectoryName(filePath))
                    .Returns(directoryPath)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetDirectoryName))
                    .Verifiable(Times.Once);
                _fileService
                    .Setup(x => x.GetFileName(filePath))
                    .Returns(fileName)
                    .Callback(_verifier.GetCallOrderAction(MethodCall.FileAndDirectoryService_GetFileName))
                    .Verifiable(Times.Once);
                _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.FileAndDirectoryService_GetDirectoryName);
                _verifier.DefineExpectedCallOrder(MethodCall.PathValidater_ValidateFullPath, MethodCall.FileAndDirectoryService_GetFileName);
            }

            return new(filePath,
                       _logger.Object,
                       _fileService.Object,
                       _pathValidater.Object);
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