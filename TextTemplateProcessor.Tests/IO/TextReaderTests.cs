namespace TextTemplateProcessor.IO
{
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
        public void TextReader_ConstructWithNullFileServiceReference_ThrowsException()
        {
            // Arrange
            Action action = () => { TextReader reader = new(_logger.Object, null!, _pathValidater.Object); };
            string expected = string.Format(MsgDependencyIsNull, nameof(TextReader), nameof(IFileAndDirectoryService)) + " (Parameter 'fileAndDirectoryService')";

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
            Action action = () => { TextReader reader = new(null!, _fileService.Object, _pathValidater.Object); };
            string expected = string.Format(MsgDependencyIsNull, nameof(TextReader), nameof(ILogger)) + " (Parameter 'logger')";

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
            Action action = () => { TextReader reader = new(_logger.Object, _fileService.Object, null!); };
            string expected = string.Format(MsgDependencyIsNull, nameof(TextReader), nameof(IPathValidater)) + " (Parameter 'pathValidater')";

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TextReader_ConstructWithValidServiceReferences_InitializesProperties()
        {
            // Arrange/Act
            TextReader reader = new(_logger.Object, _fileService.Object, _pathValidater.Object);

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