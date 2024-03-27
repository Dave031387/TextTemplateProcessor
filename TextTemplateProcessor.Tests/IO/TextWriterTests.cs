namespace TextTemplateProcessor.IO
{
    public class TextWriterTests
    {
        private readonly Mock<IFileAndDirectoryService> _fileService = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<IPathValidater> _pathValidater = new();

        [Fact]
        public void TextWriter_ConstructWithNullFileService_ThrowsException()
        {
            // Arrange
            Action action = () =>
            {
                TextWriter writer = new(_logger.Object,
                                                null!,
                                                    _pathValidater.Object);
            };
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
            Action action = () =>
            {
                TextWriter writer = new(null!,
                                        _fileService.Object,
                                                     _pathValidater.Object);
            };
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
            Action action = () =>
            {
                TextWriter writer = new(_logger.Object,
                                                _fileService.Object,
                                                             null!);
            };
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
            InitializeMocks();
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
            InitializeMocks();
            string filePath = $@"{VolumeRoot}{Sep}invalid|path{Sep}file?name";
            _logger
                .Setup(x => x.Log(MsgUnableToWriteFile, It.IsAny<string>(), null))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(x => x.ValidateFullPath(filePath, true, false))
                .Throws<ArgumentException>()
                .Verifiable(Times.Once);
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
            VerifyMocks();
        }

        [Fact]
        public void WriteTextFile_TextLinesIsEmptyList_LogsMessageAndReturnsFalse()
        {
            // Arrange
            InitializeMocks();
            string filePath = NextAbsoluteFilePath;
            _logger
                .Setup(x => x.Log(MsgGeneratedTextIsEmpty, null, null))
                .Verifiable(Times.Once);
            TextWriter writer = GetTextWriter();

            // Act
            bool actual = writer.WriteTextFile(filePath, Array.Empty<string>());

            // Assert
            actual
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void WriteTextFile_TextLinesIsNull_LogsMessageAndReturnsFalse()
        {
            // Arrange
            InitializeMocks();
            string filePath = NextAbsoluteFilePath;
            _logger
                .Setup(x => x.Log(MsgGeneratedTextIsNull, null, null))
                .Verifiable(Times.Once);
            TextWriter writer = GetTextWriter();

            // Act
            bool actual = writer.WriteTextFile(filePath, null!);

            // Assert
            actual
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void WriteTextFile_ValidFilePathAndTextLines_CreatesOutputFile()
        {
            // Arrange
            InitializeMocks();
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = Path.Combine(directoryPath, fileName);
            string[] textLines = new[]
            {
                "Line 1",
                "Line 2"
            };
            _logger
                .Setup(x => x.Log(MsgWritingTextFile, fileName, null))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(x => x.ValidateFullPath(filePath, true, false))
                .Returns(filePath)
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.GetDirectoryName(filePath))
                .Returns(directoryPath)
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.GetFileName(filePath))
                .Returns(fileName)
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.CreateDirectory(directoryPath))
                .Verifiable(Times.Once);
            _fileService
                .Setup(x => x.WriteTextFile(filePath, textLines))
                .Verifiable(Times.Once);
            TextWriter writer = GetTextWriter();

            // Act
            bool actual = writer.WriteTextFile(filePath, textLines);

            // Assert
            actual
                .Should()
                .BeTrue();
            VerifyMocks();
        }

        private TextWriter GetTextWriter()
            => new(_logger.Object, _fileService.Object, _pathValidater.Object);

        private void InitializeMocks()
        {
            _fileService.Reset();
            _logger.Reset();
            _pathValidater.Reset();
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
        }
    }
}