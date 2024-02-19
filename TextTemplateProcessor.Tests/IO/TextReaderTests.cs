namespace TextTemplateProcessor.IO
{
    using System.Linq.Expressions;

    public class TextReaderTests
    {
        private readonly Mock<IFileAndDirectoryService> _fileService = new();
        private readonly Mock<ILogger> _logger = new();
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
            Expression<Action<ILogger>> loggerExpression1 = GetLoggerExpression(
                LogEntryType.Loading,
                MsgAttemptingToReadFile,
                filePath);
            Expression<Action<ILogger>> loggerExpression2 = GetLoggerExpression(
                LogEntryType.Loading,
                MsgErrorWhileReadingTemplateFile,
                true);
            Expression<Func<IFileAndDirectoryService, string>> fileServiceExpression1 =
                x => x.GetDirectoryName(filePath);
            Expression<Func<IFileAndDirectoryService, string>> fileServiceExpression2 =
                x => x.GetFileName(filePath);
            Expression<Func<IFileAndDirectoryService, IEnumerable<string>>> fileServiceExpression3 =
                x => x.ReadTextFile(filePath);
            Expression<Func<IPathValidater, string>> pathValidaterExpression =
                x => x.ValidateFullPath(filePath, true, true);
            _logger.Setup(loggerExpression1);
            _logger.Setup(loggerExpression2);
            _fileService.Setup(fileServiceExpression1).Returns(directoryPath);
            _fileService.Setup(fileServiceExpression2).Returns(fileName);
            _fileService.Setup(fileServiceExpression3).Throws<ArgumentException>();
            _pathValidater.Setup(pathValidaterExpression).Returns(filePath);
            TextReader reader = new(filePath, _logger.Object, _fileService.Object, _pathValidater.Object);

            // Act
            List<string> actual = reader.ReadTextFile().ToList();

            // Assert
            actual
                .Should()
                .BeEmpty();
            _logger
                .Verify(loggerExpression1, Times.Once);
            _logger
                .Verify(loggerExpression2, Times.Once);
            _fileService
                .Verify(fileServiceExpression1, Times.Once);
            _fileService
                .Verify(fileServiceExpression2, Times.Once);
            _fileService
                .Verify(fileServiceExpression3, Times.Once);
            _pathValidater
                .Verify(pathValidaterExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _fileService
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void ReadTextFile_FilePathIsSet_ReadsTextFileAndLogsMessage()
        {
            // Arrange
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = Path.Combine(directoryPath, fileName);
            Expression<Action<ILogger>> loggerExpression1 = GetLoggerExpression(
                LogEntryType.Loading,
                MsgAttemptingToReadFile,
                filePath);
            Expression<Action<ILogger>> loggerExpression2 = GetLoggerExpression(
                LogEntryType.Loading,
                MsgFileSuccessfullyRead);
            List<string> expected = new()
            {
                "Line 1",
                "Line 2",
                "Line 3"
            };
            Expression<Func<IFileAndDirectoryService, string>> fileServiceExpression1 =
                x => x.GetDirectoryName(filePath);
            Expression<Func<IFileAndDirectoryService, string>> fileServiceExpression2 =
                x => x.GetFileName(filePath);
            Expression<Func<IFileAndDirectoryService, IEnumerable<string>>> fileServiceExpression3 =
                x => x.ReadTextFile(filePath);
            Expression<Func<IPathValidater, string>> pathValidaterExpression =
                x => x.ValidateFullPath(filePath, true, true);
            _logger.Setup(loggerExpression1);
            _logger.Setup(loggerExpression2);
            _fileService.Setup(fileServiceExpression1).Returns(directoryPath);
            _fileService.Setup(fileServiceExpression2).Returns(fileName);
            _fileService.Setup(fileServiceExpression3).Returns(expected);
            _pathValidater.Setup(pathValidaterExpression).Returns(filePath);
            TextReader reader = new(filePath, _logger.Object, _fileService.Object, _pathValidater.Object);

            // Act
            List<string> actual = reader.ReadTextFile().ToList();

            // Assert
            actual
                .Should()
                .Equal(expected);
            _logger
                .Verify(loggerExpression1, Times.Once);
            _logger
                .Verify(loggerExpression2, Times.Once);
            _fileService
                .Verify(fileServiceExpression1, Times.Once);
            _fileService
                .Verify(fileServiceExpression2, Times.Once);
            _fileService
                .Verify(fileServiceExpression3, Times.Once);
            _pathValidater
                .Verify(pathValidaterExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _fileService
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void ReadTextFile_FilePathNotSet_LogsErrorAndReturnsEmptyList()
        {
            // Arrange
            Expression<Action<ILogger>> loggerExpression = GetLoggerExpression(
                LogEntryType.Loading,
                MsgTemplateFilePathNotSet);
            _logger.Setup(loggerExpression);
            TextReader reader = new(_logger.Object, _fileService.Object, _pathValidater.Object);

            // Act
            IEnumerable<string> actual = reader.ReadTextFile();

            // Assert
            actual
                .Should()
                .BeEmpty();
            _logger
                .Verify(loggerExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _fileService
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void SetFilePath_InvalidFilePath_LogsAnErrorAndInitializesFilePathProperties()
        {
            // Arrange (part 1) - set up a valid file path
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = Path.Combine(directoryPath, fileName);
            _pathValidater.Setup(x => x.ValidateFullPath(filePath, true, true)).Returns(filePath);
            _fileService.Setup(x => x.GetDirectoryName(filePath)).Returns(directoryPath);
            _fileService.Setup(x => x.GetFileName(filePath)).Returns(fileName);
            TextReader reader = new(filePath, _logger.Object, _fileService.Object, _pathValidater.Object);
            _fileService.Reset();
            _pathValidater.Reset();

            // Arrange (part 2) - try to set an invalid file path
            string invalidFilePath = @"C:\invalid;path\file?";
            Expression<Func<IPathValidater, string>> pathValidaterExpression =
                x => x.ValidateFullPath(invalidFilePath, true, true);
            _pathValidater.Setup(pathValidaterExpression).Throws<ArgumentException>();
            Expression<Action<ILogger>> loggerExpression = GetLoggerExpression(
                LogEntryType.Loading,
                MsgUnableToSetTemplateFilePath,
                true);
            _logger.Setup(loggerExpression);

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
            _logger
                .Verify(loggerExpression, Times.Once);
            _pathValidater
                .Verify(pathValidaterExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _fileService
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void SetFilePath_ValidFilePath_SetsTheFilePathProperties()
        {
            // Arrange
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = Path.Combine(directoryPath, fileName);
            Expression<Func<IFileAndDirectoryService, string>> fileServiceExpression1 =
                x => x.GetDirectoryName(filePath);
            Expression<Func<IFileAndDirectoryService, string>> fileServiceExpression2 =
                x => x.GetFileName(filePath);
            Expression<Func<IPathValidater, string>> pathValidaterExpression =
                x => x.ValidateFullPath(filePath, true, true);
            _fileService.Setup(fileServiceExpression1).Returns(directoryPath);
            _fileService.Setup(fileServiceExpression2).Returns(fileName);
            _pathValidater.Setup(pathValidaterExpression).Returns(filePath);
            TextReader reader = new(_logger.Object, _fileService.Object, _pathValidater.Object);

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
            _fileService
                .Verify(fileServiceExpression1, Times.Once);
            _fileService
                .Verify(fileServiceExpression2, Times.Once);
            _pathValidater
                .Verify(pathValidaterExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _fileService
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void TextReader_ConstructWithFilePathAndNullFileServiceReference_ThrowsException()
        {
            // Arrange
            string filePath = NextAbsoluteFilePath;
            Action action = () =>
            {
                TextReader reader = new(
                    filePath,
                    _logger.Object,
                    null!,
                    _pathValidater.Object);
            };
            string expected = GetNullDependencyMessage(
                ClassNames.TextReaderClass,
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
                TextReader reader = new(
                    filePath,
                    null!,
                    _fileService.Object,
                    _pathValidater.Object);
            };
            string expected = GetNullDependencyMessage(
                ClassNames.TextReaderClass,
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
                TextReader reader = new(
                    filePath,
                    _logger.Object,
                    _fileService.Object,
                    null!);
            };
            string expected = GetNullDependencyMessage(
                ClassNames.TextReaderClass,
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
            string invalidFilePath = @"C:\invalid;path\file?";
            Expression<Func<IPathValidater, string>> pathValidaterExpression =
                x => x.ValidateFullPath(invalidFilePath, true, true);
            _pathValidater
                .Setup(pathValidaterExpression)
                .Throws<ArgumentException>();
            Expression<Action<ILogger>> loggerExpression = GetLoggerExpression(
                LogEntryType.Loading,
                MsgUnableToSetTemplateFilePath,
                true);
            _logger
                .Setup(loggerExpression);

            // Act
            TextReader reader = new(
                invalidFilePath,
                _logger.Object,
                _fileService.Object,
                _pathValidater.Object);

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
            _pathValidater
                .Verify(pathValidaterExpression, Times.Once);
            _logger
                .Verify(loggerExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _fileService
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void TextReader_ConstructWithNullFileServiceReference_ThrowsException()
        {
            // Arrange
            Action action = () =>
            {
                TextReader reader = new(
                    _logger.Object,
                    null!,
                    _pathValidater.Object);
            };
            string expected = GetNullDependencyMessage(
                ClassNames.TextReaderClass,
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
                TextReader reader = new(
                    null!,
                    _fileService.Object,
                    _pathValidater.Object);
            };
            string expected = GetNullDependencyMessage(
                ClassNames.TextReaderClass,
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
                TextReader reader = new(
                    _logger.Object,
                    _fileService.Object,
                    null!);
            };
            string expected = GetNullDependencyMessage(
                ClassNames.TextReaderClass,
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
            Expression<Func<IPathValidater, string>> pathValidaterExpression =
                x => x.ValidateFullPath(expectedFullFilePath, true, true);
            Expression<Func<IFileAndDirectoryService, string>> directoryExpression =
                x => x.GetDirectoryName(expectedFullFilePath);
            Expression<Func<IFileAndDirectoryService, string>> fileNameExpression =
                x => x.GetFileName(expectedFullFilePath);
            _pathValidater
                .Setup(pathValidaterExpression)
                .Returns(expectedFullFilePath);
            _fileService
                .Setup(directoryExpression)
                .Returns(expectedDirectoryPath);
            _fileService
                .Setup(fileNameExpression)
                .Returns(expectedFileName);

            // Act
            TextReader reader = new(
                expectedFullFilePath,
                _logger.Object,
                _fileService.Object,
                _pathValidater.Object);

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
            _pathValidater
                .Verify(pathValidaterExpression, Times.Once);
            _fileService
                .Verify(directoryExpression, Times.Once);
            _fileService
                .Verify(fileNameExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _fileService
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void TextReader_ConstructWithValidServiceReferences_InitializesProperties()
        {
            // Arrange/Act
            TextReader reader = new(
                _logger.Object,
                _fileService.Object,
                _pathValidater.Object);

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
            _logger
                .VerifyNoOtherCalls();
            _fileService
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }
    }
}