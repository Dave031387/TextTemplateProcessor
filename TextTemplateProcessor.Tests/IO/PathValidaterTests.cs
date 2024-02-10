namespace TextTemplateProcessor.IO
{
    public class PathValidaterTests
    {
        [Theory]
        [MemberData(nameof(TestData.Whitespace), MemberType = typeof(TestData))]
        public void ValidateFullPath_DirectoryPathIsEmptyOrWhitespace_ReturnsFullFilePath(string whitespace)
        {
            // Arrange
            string fileName = NextFileName;
            string filePath = $@"{whitespace}\{fileName}";
            string fullFilePath = Path.Combine(CurrentDirectory, fileName);

            // Act/Assert
            AssertValidCall(
                filePath,
                true,
                false,
                fullFilePath,
                true);
        }

        [Theory]
        [MemberData(nameof(TestData.InvalidPathCharacters), MemberType = typeof(TestData))]
        public void ValidateFullPath_FileDirectoryPathContainsInvalidPathCharacters_ThrowsException(string invalidChar)
        {
            // Arrange
            string filePath = $@"{VolumeRoot}\x{invalidChar}x\{NextFileName}";

            // Act/Assert
            AssertException(
                filePath,
                true,
                false,
                MsgInvalidDirectoryCharacters,
                true);
        }

        [Theory]
        [MemberData(nameof(TestData.InvalidFileNameCharacters), MemberType = typeof(TestData))]
        public void ValidateFullPath_FileNameContainsInvalidFileNameCharacters_ThrowsException(string invalidChar)
        {
            // Arrange
            string filePath = $@"{NextAbsoluteName}\x{invalidChar}x.test";

            // Act/Assert
            AssertException(
                filePath,
                true,
                false,
                MsgInvalidFileNameCharacters,
                true);
        }

        [Theory]
        [MemberData(nameof(TestData.Whitespace), MemberType = typeof(TestData))]
        public void ValidateFullPath_FileNameIsEmptyOrWhitespace_ThrowsException(string whitespace)
        {
            // Arrange
            string filePath = $@"{NextAbsoluteName}\{whitespace}";

            // Act/Assert
            AssertException(
                filePath,
                true,
                false,
                MsgMissingFileName,
                true);
        }

        [Theory]
        [MemberData(nameof(TestData.Whitespace), MemberType = typeof(TestData))]
        public void ValidateFullPath_FilePathIsEmptyOrWhitespace_ThrowsException(string whitespace)
        {
            // Act/Assert
            AssertException(
                whitespace,
                true,
                false,
                MsgFilePathIsEmptyOrWhitespace,
                true);
        }

        [Fact]
        public void ValidateFullPath_OkayIfAbsoluteDirectoryPathNotFound_ReturnsAbsoluteDirectoryPath()
        {
            // Arrange
            string absolutePath = NextAbsoluteName;

            // Act/Assert
            AssertValidCall(
                absolutePath,
                false,
                false,
                absolutePath,
                true);
        }

        [Fact]
        public void ValidateFullPath_OkayIfAbsoluteFilePathNotFound_ReturnsAbsoluteFilePath()
        {
            // Arrange
            string filePath = NextAbsoluteFilePath;

            // Act/Assert
            AssertValidCall(
                filePath,
                true,
                false,
                filePath,
                true);
        }

        [Fact]
        public void ValidateFullPath_OkayIfRelativeDirectoryPathNotFound_ReturnsDirectoryFilePath()
        {
            // Arrange
            string relativePath = NextRelativeName;
            string fullFilePath = Path.Combine(CurrentDirectory, relativePath);

            // Act/Assert
            AssertValidCall(
                relativePath,
                false,
                false,
                fullFilePath,
                true);
        }

        [Fact]
        public void ValidateFullPath_OkayIfRelativeFilePathNotFound_ReturnsFullFilePath()
        {
            // Arrange
            string filePath = NextRelativeFilePath;
            string fullFilePath = Path.Combine(CurrentDirectory, filePath);

            // Act/Assert
            AssertValidCall(
                filePath,
                true,
                false,
                fullFilePath,
                true);
        }

        [Theory]
        [MemberData(nameof(TestData.InvalidPathCharacters), MemberType = typeof(TestData))]
        public void ValidateFullPath_PathContainsInvalidPathCharacters_ThrowsException(string invalidChar)
        {
            // Arrange
            string filePath = $@"{VolumeRoot}\x{invalidChar}x";

            // Act/Assert
            AssertException(
                filePath,
                false,
                false,
                MsgInvalidDirectoryCharacters,
                true);
        }

        [Theory]
        [MemberData(nameof(TestData.Whitespace), MemberType = typeof(TestData))]
        public void ValidateFullPath_PathIsEmptyOrWhitespace_ThrowsException(string whitespace)
        {
            // Act/Assert
            AssertException(
                whitespace,
                false,
                false,
                MsgDirectoryPathIsEmptyOrWhitespace,
                true);
        }

        [Fact]
        public void ValidateFullPath_PathIsNull_ThrowsException()
        {
            // Act/Assert
            AssertException(
                null,
                false,
                false,
                MsgNullDirectoryPath,
                true);
        }

        [Fact]
        public void ValidateFullPath_RequiredAbsoluteDirectoryPathExists_ReturnsAbsoluteDirectoryPath()
        {
            // Arrange
            string absolutePath = NextAbsoluteName;
            CreateTestFiles(absolutePath, true);

            // Act/Assert
            AssertValidCall(
                absolutePath,
                false,
                true,
                absolutePath,
                true);

            // Cleanup
            DeleteTestFiles(absolutePath);
        }

        [Fact]
        public void ValidateFullPath_RequiredAbsoluteDirectoryPathNotFound_ThrowsException()
        {
            // Arrange
            string absolutePath = NextAbsoluteName;

            // Act/Assert
            AssertException(
                absolutePath,
                false,
                true,
                MsgDirectoryNotFound + absolutePath,
                true);
        }

        [Fact]
        public void ValidateFullPath_RequiredAbsoluteFilePathExists_ReturnsAbsoluteFilePath()
        {
            // Arrange
            string absolutePath = NextAbsoluteName;
            string fileName = CreateTestFiles(absolutePath);
            string filePath = $@"{absolutePath}\{fileName}";

            // Act/Assert
            AssertValidCall(
                filePath,
                true,
                true,
                filePath,
                true);

            // Cleanup
            DeleteTestFiles(absolutePath);
        }

        [Fact]
        public void ValidateFullPath_RequiredAbsoluteFilePathNotFound_ThrowsException()
        {
            // Arrange
            string filePath = NextAbsoluteFilePath;

            // Act/Assert
            AssertException(
                filePath,
                true,
                true,
                MsgFileNotFound + filePath,
                true);
        }

        [Fact]
        public void ValidateFullPath_RequiredRelativeDirectoryPathExists_ReturnsFullDirectoryPath()
        {
            // Arrange
            string relativePath = NextRelativeName;
            string fullDirectoryPath = Path.Combine(CurrentDirectory, relativePath);
            CreateTestFiles(fullDirectoryPath, true);

            // Act/Assert
            AssertValidCall(
                relativePath,
                false,
                true,
                fullDirectoryPath,
                true);

            // Cleanup
            DeleteTestFiles(fullDirectoryPath);
        }

        [Fact]
        public void ValidateFullPath_RequiredRelativeDirectoryPathNotFound_ThrowsException()
        {
            // Arrange
            string relativePath = NextRelativeName;
            string fullDirectoryPath = Path.Combine(CurrentDirectory, relativePath);

            // Act/Assert
            AssertException(
                relativePath,
                false,
                true,
                MsgDirectoryNotFound + fullDirectoryPath,
                true);
        }

        [Fact]
        public void ValidateFullPath_RequiredRelativeFilePathExists_ReturnsFullFilePath()
        {
            // Arrange
            string relativePath = NextRelativeName;
            string fullDirectoryPath = Path.Combine(CurrentDirectory, relativePath);
            string fileName = CreateTestFiles(fullDirectoryPath);
            string filePath = $@"{relativePath}\{fileName}";
            string fullFilePath = Path.Combine(CurrentDirectory, filePath);

            // Act/Assert
            AssertValidCall(
                filePath,
                true,
                true,
                fullFilePath,
                true);

            // Cleanup
            DeleteTestFiles(fullDirectoryPath);
        }

        [Fact]
        public void ValidateFullPath_RequiredRelativeFilePathNotFound_ThrowsException()
        {
            // Arrange
            string filePath = NextRelativeFilePath;
            string fullFilePath = Path.Combine(CurrentDirectory, filePath);

            // Act/Assert
            AssertException(
                filePath,
                true,
                true,
                MsgFileNotFound + fullFilePath,
                true);
        }

        [Theory]
        [MemberData(nameof(TestData.InvalidPathCharacters), MemberType = typeof(TestData))]
        public void ValidatePath_DirectoryPathContainsInvalidPathCharacters_ThrowsException(string invalidChar)
        {
            // Arrange
            string filePath = $@"{VolumeRoot}\x{invalidChar}x\{NextFileName}";

            // Act/Assert
            AssertException(
                filePath,
                true,
                false,
                MsgInvalidDirectoryCharacters,
                false);
        }

        [Theory]
        [MemberData(nameof(TestData.Whitespace), MemberType = typeof(TestData))]
        public void ValidatePath_DirectoryPathIsEmptyOrWhitespace_DoesNotThrow(string whitespace)
        {
            // Arrange
            string filePath = $@"{whitespace}\{NextFileName}";

            // Act/Assert
            AssertValidCall(
                filePath,
                true,
                false,
                string.Empty,
                false);
        }

        [Theory]
        [MemberData(nameof(TestData.InvalidFileNameCharacters), MemberType = typeof(TestData))]
        public void ValidatePath_FileNameContainsInvalidFileNameCharacters_ThrowsException(string invalidChar)
        {
            // Arrange
            string filePath = $@"{NextAbsoluteName}\x{invalidChar}x.test";

            // Act/Assert
            AssertException(
                filePath,
                true,
                false,
                MsgInvalidFileNameCharacters,
                false);
        }

        [Theory]
        [MemberData(nameof(TestData.Whitespace), MemberType = typeof(TestData))]
        public void ValidatePath_FileNameIsEmptyOrWhitespace_ThrowsException(string whitespace)
        {
            // Arrange
            string filePath = $@"{NextAbsoluteName}\{whitespace}";

            // Act/Assert
            AssertException(
                filePath,
                true,
                false,
                MsgMissingFileName,
                false);
        }

        [Theory]
        [MemberData(nameof(TestData.Whitespace), MemberType = typeof(TestData))]
        public void ValidatePath_FilePathIsEmptyOrWhitespace_ThrowsException(string whitespace)
        {
            // Act/Assert
            AssertException(
                whitespace,
                true,
                false,
                MsgFilePathIsEmptyOrWhitespace,
                false);
        }

        [Fact]
        public void ValidatePath_OkayIfAbsoluteDirectoryPathNotFound_DoesNotThrow()
        {
            // Act/Assert
            AssertValidCall(
                NextAbsoluteName,
                false,
                false,
                string.Empty,
                false);
        }

        [Fact]
        public void ValidatePath_OkayIfAbsoluteFilePathNotFound_DoesNotThrow()
        {
            // Act/Assert
            AssertValidCall(
                NextAbsoluteFilePath,
                true,
                false,
                string.Empty,
                false);
        }

        [Fact]
        public void ValidatePath_OkayIfRelativeDirectoryPathNotFound_DoesNotThrow()
        {
            // Act/Assert
            AssertValidCall(
                NextRelativeName,
                false,
                false,
                string.Empty,
                false);
        }

        [Fact]
        public void ValidatePath_OkayIfRelativeFilePathNotFound_DoesNotThrow()
        {
            // Act/Assert
            AssertValidCall(
                NextRelativeFilePath,
                true,
                false,
                string.Empty,
                false);
        }

        [Theory]
        [MemberData(nameof(TestData.InvalidPathCharacters), MemberType = typeof(TestData))]
        public void ValidatePath_PathContainsInvalidPathCharacters_ThrowsException(string invalidChar)
        {
            // Arrange
            string filePath = $@"{VolumeRoot}\x{invalidChar}x";

            // Act/Assert
            AssertException(
                filePath,
                false,
                false,
                MsgInvalidDirectoryCharacters,
                false);
        }

        [Theory]
        [MemberData(nameof(TestData.Whitespace), MemberType = typeof(TestData))]
        public void ValidatePath_PathIsEmptyOrWhitespace_ThrowsException(string whitespace)
        {
            // Act/Assert
            AssertException(
                whitespace,
                false,
                false,
                MsgDirectoryPathIsEmptyOrWhitespace,
                false);
        }

        [Fact]
        public void ValidatePath_PathIsNull_ThrowsException()
        {
            // Act/Assert
            AssertException(
                null,
                false,
                false,
                MsgNullDirectoryPath,
                false);
        }

        [Fact]
        public void ValidatePath_RequiredAbsoluteDirectoryPathExists_DoesNotThrow()
        {
            // Arrange
            string path = NextAbsoluteName;
            CreateTestFiles(path, true);

            // Act/Assert
            AssertValidCall(
                path,
                false,
                true,
                string.Empty,
                false);

            // Cleanup
            DeleteTestFiles(path);
        }

        [Fact]
        public void ValidatePath_RequiredAbsoluteDirectoryPathNotFound_ThrowsException()
        {
            // Arrange
            string path = NextAbsoluteName;

            // Act/Assert
            AssertException(
                path,
                false,
                true,
                MsgDirectoryNotFound + path,
                false);
        }

        [Fact]
        public void ValidatePath_RequiredAbsoluteFilePathExists_DoesNotThrow()
        {
            // Arrange
            string absolutePath = NextAbsoluteName;
            string fileName = CreateTestFiles(absolutePath);
            string filePath = $@"{absolutePath}\{fileName}";

            // Act/Assert
            AssertValidCall(
                filePath,
                true,
                true,
                string.Empty,
                false);

            // Cleanup
            DeleteTestFiles(absolutePath);
        }

        [Fact]
        public void ValidatePath_RequiredAbsoluteFilePathNotFound_ThrowsException()
        {
            // Arrange
            string filePath = NextAbsoluteFilePath;

            // Act/Assert
            AssertException(
                filePath,
                true,
                true,
                MsgFileNotFound + filePath,
                false);
        }

        [Fact]
        public void ValidatePath_RequiredRelativeDirectoryPathExists_DoesNotThrow()
        {
            // Arrange
            string relativePath = NextRelativeName;
            string fullDirectoryPath = Path.Combine(CurrentDirectory, relativePath);
            CreateTestFiles(fullDirectoryPath, true);

            // Act/Assert
            AssertValidCall(
                relativePath,
                false,
                true,
                string.Empty,
                false);

            // Cleanup
            DeleteTestFiles(fullDirectoryPath);
        }

        [Fact]
        public void ValidatePath_RequiredRelativeDirectoryPathNotFound_ThrowsException()
        {
            // Arrange
            string relativePath = NextRelativeName;
            string fullDirectoryPath = Path.Combine(CurrentDirectory, relativePath);

            // Act/Assert
            AssertException(
                relativePath,
                false,
                true,
                MsgDirectoryNotFound + fullDirectoryPath,
                false);
        }

        [Fact]
        public void ValidatePath_RequiredRelativeFilePathExists_DoesNotThrow()
        {
            // Arrange
            string relativePath = NextRelativeName;
            string fullDirectoryPath = Path.Combine(CurrentDirectory, relativePath);
            string fileName = CreateTestFiles(fullDirectoryPath);
            string filePath = $@"{relativePath}\{fileName}";

            // Act/Assert
            AssertValidCall(
                filePath,
                true,
                true,
                string.Empty,
                false);

            // Cleanup
            DeleteTestFiles(fullDirectoryPath);
        }

        [Fact]
        public void ValidatePath_RequiredRelativeFilePathNotFound_ThrowsException()
        {
            // Arrange
            string filePath = NextRelativeFilePath;
            string fullFilePath = Path.Combine(CurrentDirectory, filePath);

            // Act/Assert
            AssertException(
                filePath,
                true,
                true,
                MsgFileNotFound + fullFilePath,
                false);
        }

        private static void AssertException(
            string? path,
            bool isFilePath,
            bool shouldExist,
            string expectedMessage,
            bool validateFullPath)
        {
            // Arrange
            PathValidater pathValidater = new();
            Action action;
            if (validateFullPath)
            {
                action = () => { pathValidater.ValidateFullPath(path!, isFilePath, shouldExist); };
            }
            else
            {
                action = () => { pathValidater.ValidatePath(path!, isFilePath, shouldExist); };
            }

            // Act/Assert
            action
                .Should()
                .Throw<FilePathException>()
                .WithMessage(expectedMessage);
        }

        private static void AssertValidCall(string path, bool isFilePath, bool shouldExist, string expected, bool validateFullPath)
        {
            // Arrange
            PathValidater pathValidater = new();

            // Act/Assert
            if (validateFullPath)
            {
                string actual = pathValidater.ValidateFullPath(path, isFilePath, shouldExist);
                actual
                    .Should()
                    .Be(expected);
            }
            else
            {
                Action action = () => { pathValidater.ValidatePath(path, isFilePath, shouldExist); };
                action
                    .Should()
                    .NotThrow();
            }
        }
    }
}