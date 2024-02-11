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
            string expected = string.Format(
                MsgDependencyIsNull,
                nameof(TextReader),
                nameof(IFileAndDirectoryService))
                + " (Parameter 'fileAndDirectoryService')";

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
            string expected = string.Format(
                MsgDependencyIsNull,
                nameof(TextReader),
                nameof(ILogger))
                + " (Parameter 'logger')";

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
            string expected = string.Format(
                MsgDependencyIsNull,
                nameof(TextReader),
                nameof(IPathValidater))
                + " (Parameter 'pathValidater')";

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TextReader_ConstructWithInvalidFileName_LogsAnError()
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
            string expected = string.Format(
                MsgDependencyIsNull,
                nameof(TextReader),
                nameof(IFileAndDirectoryService))
                + " (Parameter 'fileAndDirectoryService')";

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
            string expected = string.Format(
                MsgDependencyIsNull,
                nameof(TextReader),
                nameof(ILogger))
                + " (Parameter 'logger')";

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
            string expected = string.Format(
                MsgDependencyIsNull,
                nameof(TextReader),
                nameof(IPathValidater))
                + " (Parameter 'pathValidater')";

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