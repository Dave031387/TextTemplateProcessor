namespace TextTemplateProcessor.IO
{
    using System.Linq.Expressions;

    public class TextWriterTests
    {
        private readonly Mock<IFileAndDirectoryService> _fileService = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<IPathValidater> _pathValidater = new();

        public TextWriterTests()
        {
            _fileService.Reset();
            _logger.Reset();
            _pathValidater.Reset();
        }

        [Fact]
        public void TextWriter_ConstructWithNullFileService_ThrowsException()
        {
            // Arrange
            Action action = () => { TextWriter writer = new(_logger.Object, null!, _pathValidater.Object); };
            string expected = GetNullDependencyMessage(ClassNames.TextWriterClass,
                                                       ServiceNames.FileAndDirectoryService,
                                                       ServiceParameterNames.FileAndDirectoryServiceParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TextWriter_ConstructWithNullLogger_ThrowsException()
        {
            // Arrange
            Action action = () => { TextWriter writer = new(null!, _fileService.Object, _pathValidater.Object); };
            string expected = GetNullDependencyMessage(ClassNames.TextWriterClass,
                                                       ServiceNames.LoggerService,
                                                       ServiceParameterNames.LoggerParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TextWriter_ConstructWithNullPathValidater_ThrowsException()
        {
            // Arrange
            Action action = () => { TextWriter writer = new(_logger.Object, _fileService.Object, null!); };
            string expected = GetNullDependencyMessage(ClassNames.TextWriterClass,
                                                       ServiceNames.PathValidaterService,
                                                       ServiceParameterNames.PathValidaterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TextWriter_ConstructWithValidServices_ShouldNotThrowException()
        {
            // Arrange
            Action action = () => { TextWriter writer = new(_logger.Object, _fileService.Object, _pathValidater.Object); };

            // Act/Assert
            action
                .Should()
                .NotThrow();
            _logger
                .VerifyNoOtherCalls();
            _fileService
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void WriteTextFile_InvalidOutputFilePath_LogsMessageAndReturnsFalse()
        {
            // Arrange
            string filePath = @"C:\invalid|path\file?name";
            Expression<Action<ILogger>> loggerExpression = SetupLogger(_logger,
                                                                       MsgUnableToWriteFile,
                                                                       AnyString);
            Expression<Action<IPathValidater>> pathValidaterExpression = x => x.ValidateFullPath(filePath, true, false);
            _pathValidater.Setup(pathValidaterExpression).Throws<ArgumentException>();
            TextWriter writer = new(_logger.Object, _fileService.Object, _pathValidater.Object);
            string[] textLines = new[]
            {
                "Line 1",
                "Line 2"
            };

            // Act
            bool actual = writer.WriteTextFile(filePath, textLines);

            // Assert
            actual
                .Should()
                .BeFalse();
            _logger
                .Verify(loggerExpression, Times.Once);
            _pathValidater
                .Verify(pathValidaterExpression, Times.Once);
            _fileService
                .VerifyNoOtherCalls();
            _logger
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void WriteTextFile_TextLinesIsEmptyList_LogsMessageAndReturnsFalse()
        {
            // Arrange
            string filePath = NextAbsoluteFilePath;
            Expression<Action<ILogger>> loggerExpression = SetupLogger(_logger,
                                                                       MsgGeneratedTextIsEmpty);
            TextWriter writer = new(_logger.Object, _fileService.Object, _pathValidater.Object);

            // Act
            bool actual = writer.WriteTextFile(filePath, Array.Empty<string>());

            // Assert
            actual
                .Should()
                .BeFalse();
            _logger
                .Verify(loggerExpression, Times.Once);
            _fileService
                .VerifyNoOtherCalls();
            _logger
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void WriteTextFile_TextLinesIsNull_LogsMessageAndReturnsFalse()
        {
            // Arrange
            string filePath = NextAbsoluteFilePath;
            Expression<Action<ILogger>> loggerExpression = SetupLogger(_logger,
                                                                       MsgGeneratedTextIsNull);
            TextWriter writer = new(_logger.Object, _fileService.Object, _pathValidater.Object);

            // Act
            bool actual = writer.WriteTextFile(filePath, null!);

            // Assert
            actual
                .Should()
                .BeFalse();
            _logger
                .Verify(loggerExpression, Times.Once);
            _fileService
                .VerifyNoOtherCalls();
            _logger
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void WriteTextFile_ValidFilePathAndTextLines_CreatesOutputFile()
        {
            // Arrange
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = Path.Combine(directoryPath, fileName);
            string[] textLines = new[]
            {
                "Line 1",
                "Line 2"
            };
            Expression<Action<ILogger>> loggerExpression = SetupLogger(_logger,
                                                                       MsgWritingTextFile,
                                                                       fileName);
            Expression<Func<IPathValidater, string>> pathValidaterExpression = x => x.ValidateFullPath(filePath, true, false);
            _pathValidater.Setup(pathValidaterExpression).Returns(filePath);
            Expression<Func<IFileAndDirectoryService, string>> fileServiceExpression1 =
                x => x.GetDirectoryName(filePath);
            Expression<Func<IFileAndDirectoryService, string>> fileServiceExpression2 =
                x => x.GetFileName(filePath);
            Expression<Action<IFileAndDirectoryService>> fileServiceExpression3 =
                x => x.CreateDirectory(directoryPath);
            Expression<Action<IFileAndDirectoryService>> fileServiceExpression4 =
                x => x.WriteTextFile(filePath, textLines);
            _fileService.Setup(fileServiceExpression1).Returns(directoryPath);
            _fileService.Setup(fileServiceExpression2).Returns(fileName);
            _fileService.Setup(fileServiceExpression3);
            _fileService.Setup(fileServiceExpression4);
            TextWriter writer = new(_logger.Object, _fileService.Object, _pathValidater.Object);

            // Act
            bool actual = writer.WriteTextFile(filePath, textLines);

            // Assert
            actual
                .Should()
                .BeTrue();
            _logger
                .Verify(loggerExpression, Times.Once);
            _pathValidater
                .Verify(pathValidaterExpression, Times.Once);
            _fileService
                .Verify(fileServiceExpression1, Times.Once);
            _fileService
                .Verify(fileServiceExpression2, Times.Once);
            _fileService
                .Verify(fileServiceExpression3, Times.Once);
            _fileService
                .Verify(fileServiceExpression4, Times.Once);
            _fileService
                .VerifyNoOtherCalls();
            _logger
                .VerifyNoOtherCalls();
            _pathValidater
                .VerifyNoOtherCalls();
        }
    }
}