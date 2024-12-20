﻿namespace TextTemplateProcessor.IO
{
    public class TextWriterTests
    {
        private readonly Mock<IFileAndDirectoryService> _fileService = new(MockBehavior.Strict);
        private readonly Mock<ILogger> _logger = new(MockBehavior.Strict);
        private readonly Mock<IPathValidater> _pathValidater = new(MockBehavior.Strict);
        private readonly MethodCallOrderVerifier _verifier = new();

        [Fact]
        public void TextWriter_ConstructWithNullFileService_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () =>
            {
                TextWriter writer = new(null!,
                                        _logger.Object,
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
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextWriter_ConstructWithNullLogger_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () =>
            {
                TextWriter writer = new(_fileService.Object,
                                        null!,
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
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextWriter_ConstructWithNullPathValidater_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () =>
            {
                TextWriter writer = new(_fileService.Object,
                                        _logger.Object,
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
            MocksVerifyNoOtherCalls();
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
                .Setup(logger => logger.Log(MsgUnableToWriteFile, It.IsAny<string>(), null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(filePath, true, false))
                .Callback(_verifier.GetCallOrderAction(PathValidater_ValidateFullPath))
                .Throws<ArgumentException>()
                .Verifiable(Times.Once);
            TextWriter writer = GetTextWriter();
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, Logger_Log_Message);

            // Act
            bool actual = writer.WriteTextFile(filePath, SampleText);

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
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(filePath, true, false))
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
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextIsEmpty, fileName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            TextWriter writer = GetTextWriter();
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, FileAndDirectoryService_GetDirectoryName);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, FileAndDirectoryService_GetFileName);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetDirectoryName, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetFileName, Logger_Log_Message);

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
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(filePath, true, false))
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
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextIsNull, null, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            TextWriter writer = GetTextWriter();
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, FileAndDirectoryService_GetDirectoryName);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, FileAndDirectoryService_GetFileName);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetDirectoryName, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetFileName, Logger_Log_Message);

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
            string filePath = $"{directoryPath}{Sep}{fileName}";
            _logger
                .Setup(logger => logger.Log(MsgWritingTextFile, fileName, null))
                .Callback(_verifier.GetCallOrderAction(Logger_Log_Message))
                .Verifiable(Times.Once);
            _pathValidater
                .Setup(pathValidater => pathValidater.ValidateFullPath(filePath, true, false))
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
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.CreateDirectory(directoryPath))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_CreateDirectory))
                .Verifiable(Times.Once);
            _fileService
                .Setup(fileAndDirectoryService => fileAndDirectoryService.WriteTextFile(filePath, SampleText))
                .Callback(_verifier.GetCallOrderAction(FileAndDirectoryService_WriteTextFile))
                .Verifiable(Times.Once);
            TextWriter writer = GetTextWriter();
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, FileAndDirectoryService_GetDirectoryName);
            _verifier.DefineExpectedCallOrder(PathValidater_ValidateFullPath, FileAndDirectoryService_GetFileName);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetDirectoryName, FileAndDirectoryService_CreateDirectory);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_GetFileName, FileAndDirectoryService_CreateDirectory);
            _verifier.DefineExpectedCallOrder(FileAndDirectoryService_CreateDirectory, Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(Logger_Log_Message, FileAndDirectoryService_WriteTextFile);

            // Act
            bool actual = writer.WriteTextFile(filePath, SampleText);

            // Assert
            actual
                .Should()
                .BeTrue();
            VerifyMocks();
        }

        private TextWriter GetTextWriter()
            => new(_fileService.Object, _logger.Object, _pathValidater.Object);

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