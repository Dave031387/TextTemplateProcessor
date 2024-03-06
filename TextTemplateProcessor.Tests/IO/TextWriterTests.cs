namespace TextTemplateProcessor.IO
{
    using System.Linq.Expressions;

    public class TextWriterTests
    {
        private readonly Mock<IFileAndDirectoryService> _fileService = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly MockHelper _mh = new();
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
            Action action = () => { TextWriter writer = GetTextWriter(); };

            // Act/Assert
            action
                .Should()
                .NotThrow();
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void WriteTextFile_InvalidOutputFilePath_LogsMessageAndReturnsFalse()
        {
            // Arrange
            string filePath = $@"{VolumeRoot}{Sep}invalid|path{Sep}file?name";
            Expression<Action<ILogger>> loggerExpression
                = MockHelper.SetupLogger(_logger,
                                         MsgUnableToWriteFile,
                                         AnyString);
            _mh.SetupPathValidater(_pathValidater,
                                   filePath,
                                   true,
                                   false,
                                   true);
            TextWriter writer = GetTextWriter();
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
            _logger.Verify(loggerExpression, Times.Once);
            _pathValidater.Verify(_mh.ValidateFullPathExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void WriteTextFile_TextLinesIsEmptyList_LogsMessageAndReturnsFalse()
        {
            // Arrange
            string filePath = NextAbsoluteFilePath;
            Expression<Action<ILogger>> loggerExpression
                = MockHelper.SetupLogger(_logger,
                                         MsgGeneratedTextIsEmpty);
            TextWriter writer = GetTextWriter();

            // Act
            bool actual = writer.WriteTextFile(filePath, Array.Empty<string>());

            // Assert
            actual
                .Should()
                .BeFalse();
            _logger.Verify(loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void WriteTextFile_TextLinesIsNull_LogsMessageAndReturnsFalse()
        {
            // Arrange
            string filePath = NextAbsoluteFilePath;
            Expression<Action<ILogger>> loggerExpression
                = MockHelper.SetupLogger(_logger,
                                         MsgGeneratedTextIsNull);
            TextWriter writer = GetTextWriter();

            // Act
            bool actual = writer.WriteTextFile(filePath, null!);

            // Assert
            actual
                .Should()
                .BeFalse();
            _logger.Verify(loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
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
            Expression<Action<ILogger>> loggerExpression
                = MockHelper.SetupLogger(_logger,
                                         MsgWritingTextFile,
                                         fileName);
            _mh.SetupPathValidater(_pathValidater, filePath, true, false, filePath);
            _mh.SetupFileAndDirectoryService(_fileService,
                                             directoryPath,
                                             fileName,
                                             filePath,
                                             textLines);
            TextWriter writer = GetTextWriter();

            // Act
            bool actual = writer.WriteTextFile(filePath, textLines);

            // Assert
            actual
                .Should()
                .BeTrue();
            _logger.Verify(loggerExpression, Times.Once);
            _pathValidater.Verify(_mh.ValidateFullPathExpression, Times.Once);
            _fileService.Verify(_mh.GetDirectoryNameExpression, Times.Once);
            _fileService.Verify(_mh.GetFileNameExpression, Times.Once);
            _fileService.Verify(_mh.CreateDirectoryExpression, Times.Once);
            _fileService.Verify(_mh.WriteTextFileExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        private TextWriter GetTextWriter()
            => new(_logger.Object, _fileService.Object, _pathValidater.Object);

        private void MocksVerifyNoOtherCalls()
        {
            _logger.VerifyNoOtherCalls();
            _fileService.VerifyNoOtherCalls();
            _pathValidater.VerifyNoOtherCalls();
        }
    }
}