namespace TextTemplateProcessor.IO
{
    using System.IO;

    public class FileAndDirectoryServiceTests
    {
        [Fact]
        public void ClearDirectory_DirectoryDoesNotExist_DoesNothing()
        {
            // Arrange
            FileAndDirectoryService service = new();
            Action action = () => { service.ClearDirectory(NextAbsoluteName); };

            // Act/Assert
            action
                .Should()
                .NotThrow();
        }

        [Fact]
        public void ClearDirectory_DirectoryExistsAndContainsFiles_DeletesTheFiles()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string path = NextAbsoluteName;
            CreateTestFiles(path, 3);

            // Act
            service.ClearDirectory(path);

            // Assert
            Directory.GetFiles(path)
                .Should()
                .BeEmpty();

            // Cleanup
            DeleteTestFiles(path);
        }

        [Fact]
        public void ClearDirectory_DirectoryExistsAndIsEmpty_DoesNothing()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string path = NextAbsoluteName;
            CreateTestFiles(path, true);
            Action action = () => { service.ClearDirectory(TemplateDirectoryPath); };

            // Act/Assert
            action
                .Should()
                .NotThrow();

            // Cleanup
            DeleteTestFiles(path);
        }

        [Fact]
        public void CombineDirectoryAndFileName_AbsoluteDirectoryPath_ReturnsFullFilePath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string absolutePath = NextAbsoluteName;
            string fileName = NextFileName;
            string expected = $@"{absolutePath}{Sep}{fileName}";

            // Act
            string actual = service.CombineDirectoryAndFileName(absolutePath, fileName);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Fact]
        public void CombineDirectoryAndFileName_RelativeDirectoryPath_ReturnsRelativeFilePath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string relativePath = NextRelativeName;
            string fileName = NextFileName;
            string expected = $@"{relativePath}{Sep}{fileName}";

            // Act
            string actual = service.CombineDirectoryAndFileName(relativePath, fileName);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Fact]
        public void CreateDirectoryWith1Arg_PathIsAbsolute_CreatesDirectory()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string path = NextAbsoluteName;

            // Act
            service.CreateDirectory(path);

            // Assert
            Directory.Exists(path)
                .Should()
                .BeTrue();

            // Cleanup
            DeleteTestFiles(path);
        }

        [Theory]
        [InlineData("")]
        [InlineData(Whitespace)]
        public void CreateDirectoryWith1Arg_PathIsEmptyOrWhitespace_ShouldNotThrow(string directoryPath)
        {
            // Arrange
            FileAndDirectoryService service = new();
            Action action = () => { service.CreateDirectory(directoryPath); };

            // Act/Assert
            action
                .Should()
                .NotThrow();
        }

        [Fact]
        public void CreateDirectoryWith1Arg_PathIsNull_ThrowsException()
        {
            // Arrange
            FileAndDirectoryService service = new();
            Action action = () => { service.CreateDirectory(null!); };

            // Act/Assert
            action
                .Should()
                .Throw<FilePathException>()
                .WithMessage(MsgNullDirectoryPath);
        }

        [Fact]
        public void CreateDirectoryWith1Arg_PathIsRelative_CreatesDirectory()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string path = NextRelativeName;
            string fullPath = $"{CurrentDirectory}{Sep}{path}";

            // Act
            service.CreateDirectory(path);

            // Assert
            Directory.Exists(fullPath)
                .Should()
                .BeTrue();

            // Cleanup
            DeleteTestFiles(fullPath);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", Whitespace)]
        [InlineData(Whitespace, "")]
        [InlineData(Whitespace, Whitespace)]
        public void CreateDirectoryWith2Args_PathAndRootAreEmptyOrWhitespace_ReturnsCurrentDirectory(string path, string root)
        {
            // Arrange
            FileAndDirectoryService service = new();
            string expected = CurrentDirectory;

            // Act
            string actual = service.CreateDirectory(path, root);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Fact]
        public void CreateDirectoryWith2Args_PathAndRootAreRelative_CreatesDirectoryRelativeToCurrentAndReturnsFullPath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string relativeRoot = NextRootName;
            string relativePath = NextRelativeName;
            string fullRoot = $"{CurrentDirectory}{Sep}{relativeRoot}";
            string expected = $"{fullRoot}{Sep}{relativePath}";
            DeleteTestFiles(fullRoot);

            // Act
            string actual = service.CreateDirectory(relativePath, relativeRoot);

            // Assert
            actual
                .Should()
                .Be(expected);
            Directory.Exists(expected)
                .Should()
                .BeTrue();

            // Cleanup
            DeleteTestFiles(fullRoot);
        }

        [Fact]
        public void CreateDirectoryWith2Args_PathIsEmptyOrWhitespaceAndRootIsAbsolute_CreatesRootPathAndReturnsFullPath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string root = NextAbsoluteName;
            DeleteTestFiles(root);

            // Act
            string actual = service.CreateDirectory(string.Empty, root);

            // Assert
            actual
                .Should()
                .Be(root);
            Directory.Exists(root)
                .Should()
                .Be(true);

            // Cleanup
            DeleteTestFiles(root);
        }

        [Fact]
        public void CreateDirectoryWith2Args_PathIsEmptyOrWhitespaceAndRootIsRelative_CreatesRootPathAndReturnsFullPath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string relativeDirectory = NextRelativeName;
            string expected = $"{CurrentDirectory}{Sep}{relativeDirectory}";
            DeleteTestFiles(expected);

            // Act
            string actual = service.CreateDirectory(string.Empty, relativeDirectory);

            // Assert
            actual
                .Should()
                .Be(expected);
            Directory.Exists(expected)
                .Should()
                .Be(true);

            // Cleanup
            DeleteTestFiles(expected);
        }

        [Fact]
        public void CreateDirectoryWith2Args_PathIsNull_ThrowsException()
        {
            // Arrange
            FileAndDirectoryService service = new();
            Action action = () => { service.CreateDirectory(null!, NextAbsoluteName); };

            // Act/Assert
            action
                .Should()
                .Throw<FilePathException>()
                .WithMessage(MsgNullDirectoryPath);
        }

        [Fact]
        public void CreateDirectoryWith2Args_PathIsRelativeAndRootIsAbsolute_CreatesAbsoluteDirectoryAndReturnsFullPath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string absoluteRoot = NextAbsoluteName;
            string relativePath = NextRelativeName;
            string expected = $@"{absoluteRoot}{Sep}{relativePath}";
            DeleteTestFiles(absoluteRoot);

            // Act
            string actual = service.CreateDirectory(relativePath, absoluteRoot);

            // Assert
            actual
                .Should()
                .Be(expected);
            Directory.Exists(expected)
                .Should()
                .BeTrue();

            // Cleanup
            DeleteTestFiles(absoluteRoot);
        }

        [Fact]
        public void CreateDirectoryWith2Args_PathIsRelativeAndRootIsEmpty_CreatesDirectoryRelativeToCurrentAndReturnsFullPath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string relativeDirectory = NextRelativeName;
            string expected = $"{CurrentDirectory}{Sep}{relativeDirectory}";
            DeleteTestFiles(expected);

            // Act
            string actual = service.CreateDirectory(relativeDirectory, string.Empty);

            // Assert
            actual
                .Should()
                .Be(expected);
            Directory.Exists(expected)
                .Should()
                .BeTrue();

            // Cleanup
            DeleteTestFiles(expected);
        }

        [Fact]
        public void CreateDirectoryWith2Args_PathIsRooted_CreatesRootedPathAndReturnsFullPath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string path = NextRootedName;
            string expected = Path.GetFullPath(path);
            DeleteTestFiles(expected);

            // Act
            string actual = service.CreateDirectory(path, NextAbsoluteName);

            // Assert
            actual
                .Should()
                .Be(expected);
            Directory.Exists(expected)
                .Should()
                .BeTrue();

            // Cleanup
            DeleteTestFiles(expected);
        }

        [Fact]
        public void CreateDirectoryWith2Args_RootPathIsNull_ThrowsException()
        {
            // Arrange
            FileAndDirectoryService service = new();
            Action action = () => { service.CreateDirectory(NextRelativeName, null!); };

            // Act/Assert
            action
                .Should()
                .Throw<FilePathException>()
                .WithMessage(MsgRootPathIsNull);
        }

        [Fact]
        public void GetDirectoryName_DirectorySeparatorAtStartOfPath_ReturnsEmptyString()
        {
            // Arrange
            FileAndDirectoryService service = new();

            // Act
            string actual = service.GetDirectoryName($@"{Sep}file.txt");

            // Assert
            actual
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetDirectoryName_NoDirectorySeparatorInPath_ReturnsEmptyString()
        {
            // Arrange
            FileAndDirectoryService service = new();

            // Act
            string actual = service.GetDirectoryName("file.txt");

            // Assert
            actual
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetDirectoryName_PathIsAbsolute_ReturnsDirectoryPath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string expected = $@"{VolumeRoot}{Sep}test{Sep}directory";
            string filePath = $@"{expected}{Sep}file.txt";

            // Act
            string actual = service.GetDirectoryName(filePath);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData(Whitespace)]
        public void GetDirectoryName_PathIsEmptyOrWhitespace_ReturnsEmptyString(string path)
        {
            // Arrange
            FileAndDirectoryService service = new();

            // Act
            string actual = service.GetDirectoryName(path);

            // Assert
            actual
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetDirectoryName_PathIsNull_ReturnsEmptyString()
        {
            // Arrange
            FileAndDirectoryService service = new();

            // Act
            string actual = service.GetDirectoryName(null!);

            // Assert
            actual
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetDirectoryName_PathIsRelative_ReturnsDirectoryPath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string expected = $@"test{Sep}directory";
            string filePath = $@"{expected}{Sep}file.txt";

            // Act
            string actual = service.GetDirectoryName(filePath);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Fact]
        public void GetDirectoryName_PathStartsWithVolume_ReturnsEmptyString()
        {
            // Arrange
            FileAndDirectoryService service = new();

            // Act
            string actual = service.GetDirectoryName($@"{VolumeRoot}{Sep}file.txt");

            // Assert
            actual
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetFileName_DirectorySeparatorAtEndOfPath_ReturnsEmptyString()
        {
            // Arrange
            FileAndDirectoryService service = new();

            // Act
            string actual = service.GetFileName($@"{VolumeRoot}{Sep}directory{Sep}");

            // Assert
            actual
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetFileName_PathIsAbsolute_ReturnsFileName()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string expected = "file.txt";
            string filePath = $@"{VolumeRoot}{Sep}test{Sep}directory{Sep}{expected}";

            // Act
            string actual = service.GetFileName(filePath);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData(Whitespace)]
        public void GetFileName_PathIsEmptyOrWhitespace_ReturnsEmptyString(string path)
        {
            // Arrange
            FileAndDirectoryService service = new();

            // Act
            string actual = service.GetFileName(path);

            // Assert
            actual
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetFileName_PathIsNull_ReturnsEmptyString()
        {
            // Arrange
            FileAndDirectoryService service = new();

            // Act
            string actual = service.GetFileName(null!);

            // Assert
            actual
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void GetFullPath_DirectoryPathIsNull_ThrowsException()
        {
            // Arrange
            FileAndDirectoryService service = new();
            Action action = () => { service.GetFullPath(null!, ""); };

            // Act/Assert
            action
                .Should()
                .Throw<FilePathException>()
                .WithMessage(MsgNullDirectoryPath);
        }

        [Fact]
        public void GetFullPath_FilePathIsNull_ThrowsException()
        {
            // Arrange
            FileAndDirectoryService service = new();
            Action action = () => { service.GetFullPath(null!, "", true); };

            // Act/Assert
            action
                .Should()
                .Throw<FilePathException>()
                .WithMessage(MsgNullFilePath);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("", Whitespace)]
        [InlineData(Whitespace, "")]
        [InlineData(Whitespace, Whitespace)]
        public void GetFullPath_PathAndRootPathAreBothEmptyOrWhitespace_ReturnsCurrentDirectory(string path, string rootPath)
        {
            // Arrange
            FileAndDirectoryService service = new();

            // Act
            string actual = service.GetFullPath(path, rootPath);

            // Assert
            actual
                .Should()
                .Be(CurrentDirectory);
        }

        [Fact]
        public void GetFullPath_PathAndRootPathAreBothRelative_ReturnsFullPath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string path = "file.txt";
            string rootPath = $@"test{Sep}directory";
            string expected = $@"{CurrentDirectory}{Sep}{rootPath}{Sep}{path}";

            // Act
            string actual = service.GetFullPath(path, rootPath);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData(Whitespace)]
        public void GetFullPath_PathIsEmptyOrWhitespaceAndRootIsAbsolute_ReturnsFullRootDirectory(string path)
        {
            // Arrange
            FileAndDirectoryService service = new();
            string expected = $@"{VolumeRoot}{Sep}test{Sep}directory";

            // Act
            string actual = service.GetFullPath(path, expected);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData(Whitespace)]
        public void GetFullPath_PathIsEmptyOrWhitespaceAndRootIsRelative_ReturnsFullRootDirectory(string path)
        {
            // Arrange
            FileAndDirectoryService service = new();
            string rootPath = $@"test{Sep}directory";
            string expected = $@"{CurrentDirectory}{Sep}{rootPath}";

            // Act
            string actual = service.GetFullPath(path, rootPath);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData(Whitespace)]
        public void GetFullPath_PathIsRelativeAndRootIsEmptyOrWhitespace_ReturnsFullPath(string rootPath)
        {
            // Arrange
            FileAndDirectoryService service = new();
            string path = $@"test{Sep}directory{Sep}file.txt";
            string expected = $@"{CurrentDirectory}{Sep}{path}";

            // Act
            string actual = service.GetFullPath(path, rootPath);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Fact]
        public void GetFullPath_PathIsRelativeAndRootPathIsAbsolute_ReturnsFullPath()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string path = "file.txt";
            string rootPath = $@"{VolumeRoot}{Sep}test{Sep}directory";
            string expected = $@"{rootPath}{Sep}{path}";

            // Act
            string actual = service.GetFullPath(path, rootPath);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Theory]
        [InlineData("")]
        [InlineData(Whitespace)]
        [InlineData($@"test{Sep}directory")]
        [InlineData($@"C:{Sep}test{Sep}directory")]
        public void GetFullPath_PathIsRooted_ReturnsFullPath(string root)
        {
            // Arrange
            FileAndDirectoryService service = new();
            string path = $@"{Sep}test{Sep}directory{Sep}file.txt";
            string expected = $"{VolumeRoot}{path}";
            string rootPath = root.Length > 2 && root[0..2] == "C:" ? $"{VolumeRoot}{root[2..]}" : root;

            // Act
            string actual = service.GetFullPath(path, rootPath);

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Fact]
        public void GetFullPath_RootPathIsNull_ThrowsException()
        {
            // Arrange
            FileAndDirectoryService service = new();
            Action action = () => { service.GetFullPath("", null!); };

            // Act/Assert
            action
                .Should()
                .Throw<FilePathException>()
                .WithMessage(MsgRootPathIsNull);
        }

        [Fact]
        public void GetSolutionDirectory_WhenCalled_ReturnsSolutionDirectory()
        {
            // Arrange
            FileAndDirectoryService service = new();

            // Act
            string actual = service.GetSolutionDirectory();

            // Assert
            actual
                .Should()
                .Be(SolutionDirectory);
        }

        [Fact]
        public void ReadTextFile_FileDoesNotExist_ThrowsException()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string filePath = NextAbsoluteFilePath;
            string expected = MsgFileNotFound + filePath;
            Action action = () =>
            {
                IEnumerable<string> actual = service.ReadTextFile(filePath);
                // Following required to force ReadTextFile to be called.
                if (actual.Any())
                {
                }
            };

            // Act/Assert
            action
                .Should()
                .Throw<FilePathException>()
                .WithMessage(expected);
        }

        [Fact]
        public void ReadTextFile_FileExists_ReadsFileContents()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string path = NextAbsoluteName;
            string filePath = CreateTestFiles(path, SampleText);

            // Act
            IEnumerable<string> actual = service.ReadTextFile(filePath);

            // Assert
            actual
                .Should()
                .ContainInConsecutiveOrder(SampleText);

            // Cleanup
            DeleteTestFiles(path);
        }

        [Fact]
        public void WriteTextFile_TextNotNull_WritesTextFile()
        {
            // Arrange
            FileAndDirectoryService service = new();
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            string[] text = ["Text 1", "Text 2"];
            Directory.CreateDirectory(directoryPath);

            // Act
            service.WriteTextFile(filePath, text);

            // Assert
            File.ReadAllLines(filePath)
                .Should()
                .ContainInConsecutiveOrder(text);

            // Cleanup
            DeleteTestFiles(directoryPath);
        }
    }
}