namespace TextTemplateProcessor.IO
{
    using global::TextTemplateProcessor.TestShared;
    using System.Linq.Expressions;

    public class TextReaderTests
    {
        private readonly Mock<IFileAndDirectoryService> _fileService = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly MockHelper _mh = new();
        private readonly Mock<IPathValidater> _pathValidater = new();

        public TextReaderTests()
        {
            _fileService.Reset();
            _logger.Reset();
            _pathValidater.Reset();
        }

        [Fact]
        public void ReadTextFile_ExceptionWhileReadingFile_LogsErrorMessage()
        {
            // Arrange
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = Path.Combine(directoryPath, fileName);
            Expression<Action<ILogger>> loggerExpression1
                = MockHelper.SetupLogger(_logger,
                                         MsgAttemptingToReadFile,
                                         filePath);
            Expression<Action<ILogger>> loggerExpression2
                = MockHelper.SetupLogger(_logger,
                                         MsgErrorWhileReadingTemplateFile,
                                         AnyString);
            _mh.SetupPathValidater(_pathValidater,
                                   filePath,
                                   true,
                                   true,
                                   filePath);
            _mh.SetupFileAndDirectoryService(_fileService,
                                             directoryPath,
                                             fileName,
                                             filePath,
                                             new[] { "test" },
                                             true);
            TextReader reader = GetTextReader(filePath);

            // Act
            List<string> actual = reader.ReadTextFile().ToList();

            // Assert
            actual
                .Should()
                .BeEmpty();
            _logger.Verify(loggerExpression1, Times.Once);
            _logger.Verify(loggerExpression2, Times.Once);
            _fileService.Verify(_mh.GetDirectoryNameExpression, Times.Once);
            _fileService.Verify(_mh.GetFileNameExpression, Times.Once);
            _fileService.Verify(_mh.ReadTextFileExpression, Times.Once);
            _pathValidater.Verify(_mh.ValidateFullPathExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ReadTextFile_FilePathIsSet_ReadsTextFileAndLogsMessage()
        {
            // Arrange
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = Path.Combine(directoryPath, fileName);
            List<string> expected = new()
            {
                "Line 1",
                "Line 2",
                "Line 3"
            };
            Expression<Action<ILogger>> loggerExpression1
                = MockHelper.SetupLogger(_logger,
                                         MsgAttemptingToReadFile,
                                         filePath);
            Expression<Action<ILogger>> loggerExpression2
                = MockHelper.SetupLogger(_logger,
                                         MsgFileSuccessfullyRead);
            _mh.SetupPathValidater(_pathValidater,
                                   filePath,
                                   true,
                                   true,
                                   filePath);
            _mh.SetupFileAndDirectoryService(_fileService,
                                             directoryPath,
                                             fileName,
                                             filePath,
                                             expected);
            TextReader reader = GetTextReader(filePath);

            // Act
            List<string> actual = reader.ReadTextFile().ToList();

            // Assert
            actual
                .Should()
                .Equal(expected);
            _logger.Verify(loggerExpression1, Times.Once);
            _logger.Verify(loggerExpression2, Times.Once);
            _fileService.Verify(_mh.GetDirectoryNameExpression, Times.Once);
            _fileService.Verify(_mh.GetFileNameExpression, Times.Once);
            _fileService.Verify(_mh.ReadTextFileExpression, Times.Once);
            _pathValidater.Verify(_mh.ValidateFullPathExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ReadTextFile_FilePathNotSet_LogsErrorAndReturnsEmptyList()
        {
            // Arrange
            Expression<Action<ILogger>> loggerExpression
                = MockHelper.SetupLogger(_logger,
                                         MsgTemplateFilePathNotSet);
            TextReader reader = GetTextReader();

            // Act
            IEnumerable<string> actual = reader.ReadTextFile();

            // Assert
            actual
                .Should()
                .BeEmpty();
            _logger.Verify(loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void SetFilePath_InvalidFilePath_LogsAnErrorAndInitializesFilePathProperties()
        {
            // Arrange (part 1) - set up a valid file path
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = Path.Combine(directoryPath, fileName);
            _mh.SetupPathValidater(_pathValidater,
                                   filePath,
                                   true,
                                   true,
                                   filePath);
            _mh.SetupFileAndDirectoryService(_fileService,
                                             directoryPath,
                                             fileName,
                                             filePath);
            TextReader reader = GetTextReader(filePath);
            _fileService.Reset();
            _pathValidater.Reset();

            // Arrange (part 2) - try to set an invalid file path
            string invalidFilePath = $@"{VolumeRoot}{Sep}invalid;path{Sep}file?";
            _mh.SetupPathValidater(_pathValidater,
                                   invalidFilePath,
                                   true,
                                   true,
                                   true);
            Expression<Action<ILogger>> loggerExpression
                = MockHelper.SetupLogger(_logger,
                                         MsgUnableToSetTemplateFilePath,
                                         AnyString);

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
            _logger.Verify(loggerExpression, Times.Once);
            _pathValidater.Verify(_mh.ValidateFullPathExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void SetFilePath_ValidFilePath_SetsTheFilePathProperties()
        {
            // Arrange
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = Path.Combine(directoryPath, fileName);
            _mh.SetupPathValidater(_pathValidater,
                                   filePath,
                                   true,
                                   true,
                                   filePath);
            _mh.SetupFileAndDirectoryService(_fileService,
                                             directoryPath,
                                             fileName,
                                             filePath);
            TextReader reader = GetTextReader();

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
            _fileService.Verify(_mh.GetDirectoryNameExpression, Times.Once);
            _fileService.Verify(_mh.GetFileNameExpression, Times.Once);
            _pathValidater.Verify(_mh.ValidateFullPathExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextReader_ConstructWithFilePathAndNullFileServiceReference_ThrowsException()
        {
            // Arrange
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
        }

        [Fact]
        public void TextReader_ConstructWithFilePathAndNullLoggerReference_ThrowsException()
        {
            // Arrange
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
        }

        [Fact]
        public void TextReader_ConstructWithFilePathAndNullPathValidaterReference_ThrowsException()
        {
            // Arrange
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
        }

        [Fact]
        public void TextReader_ConstructWithInvalidFilePath_LogsAnError()
        {
            // Arrange
            string invalidFilePath = $@"{VolumeRoot}{Sep}invalid;path{Sep}file?";
            _mh.SetupPathValidater(_pathValidater,
                                   invalidFilePath,
                                   true,
                                   true,
                                   true);
            Expression<Action<ILogger>> loggerExpression
                = MockHelper.SetupLogger(_logger,
                                         MsgUnableToSetTemplateFilePath,
                                         AnyString);

            // Act
            TextReader reader = GetTextReader(invalidFilePath);

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
            _pathValidater.Verify(_mh.ValidateFullPathExpression, Times.Once);
            _logger.Verify(loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextReader_ConstructWithNullFileServiceReference_ThrowsException()
        {
            // Arrange
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
        }

        [Fact]
        public void TextReader_ConstructWithNullLoggerReference_ThrowsException()
        {
            // Arrange
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
        }

        [Fact]
        public void TextReader_ConstructWithNullPathValidaterReference_ThrowsException()
        {
            // Arrange
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
        }

        [Fact]
        public void TextReader_ConstructWithValidFilePath_SavesPathInfo()
        {
            // Arrange
            string expectedDirectoryPath = NextAbsoluteName;
            string expectedFileName = NextFileName;
            string expectedFullFilePath = Path.Combine(expectedDirectoryPath, expectedFileName);
            _mh.SetupPathValidater(_pathValidater,
                                   expectedFullFilePath,
                                   true,
                                   true,
                                   expectedFullFilePath);
            _mh.SetupFileAndDirectoryService(_fileService,
                                             expectedDirectoryPath,
                                             expectedFileName,
                                             expectedFullFilePath);

            // Act
            TextReader reader = GetTextReader(expectedFullFilePath);

            // Assert
            reader.DirectoryPath
                .Should()
                .Be(expectedDirectoryPath);
            reader.FileName
                .Should()
                .Be(expectedFileName);
            reader.FullFilePath
                .Should()
                .Be(expectedFullFilePath);
            _pathValidater.Verify(_mh.ValidateFullPathExpression, Times.Once);
            _fileService.Verify(_mh.GetDirectoryNameExpression, Times.Once);
            _fileService.Verify(_mh.GetFileNameExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextReader_ConstructWithValidServiceReferences_InitializesProperties()
        {
            // Arrange/Act
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

        private TextReader GetTextReader(string filePath) => new(filePath,
                                                                 _logger.Object,
                                                                 _fileService.Object,
                                                                 _pathValidater.Object);

        private void MocksVerifyNoOtherCalls()
        {
            _logger.VerifyNoOtherCalls();
            _fileService.VerifyNoOtherCalls();
            _pathValidater.VerifyNoOtherCalls();
        }
    }
}