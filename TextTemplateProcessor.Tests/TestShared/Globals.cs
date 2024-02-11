namespace TextTemplateProcessor.TestShared
{
    using System;
    using System.Linq.Expressions;

    public static class Globals
    {
        public const string DefaultSegmentName1 = "DefaultSegment1";
        public const string DefaultSegmentName2 = "DefaultSegment2";
        public const string FileNameWithoutDirectoryPath = "test.file";
        public const string InvalidDirectoryPath = "This is an invalid directory path.";
        public const string MsgExpectedExceptionNotThrown = "Expected exception not thrown.";
        public const string NonexistentAbsoluteDirectory = $@"C:{Sep}missing";
        public const string NonexistentFileName = "NotFound.txt";
        public const string NonexistentRelativeDirectory = $@"missing{Sep}directory";
        public const string OutputFileName = "generated.txt";
        public const string RelativeDirectoryPath = @"relative";
        public const string TemplateFileNameString1 = "test_template1.txt";
        public const string TemplateFileNameString2 = "test_template2.txt";
        public const string Whitespace = "\t\n\v\f\r \u0085\u00a0\u2002\u2003\u2028\u2029";
        private const string Sep = @"\";
        private static int _counter = 0;

        static Globals()
        {
            string path = CurrentDirectory;
            int pathIndex;

            while (true)
            {
                pathIndex = path.LastIndexOf(Path.DirectorySeparatorChar);

                if (pathIndex < 0)
                {
                    throw new DirectoryNotFoundException($"Unable to locate the solution directory in the current directory path.\n Current directory: {CurrentDirectory}");
                }

                path = path[..pathIndex];
                string[] files = Directory.GetFiles(path, "*.sln");

                if (files.Length > 0)
                {
                    SolutionDirectory = path;
                    break;
                }
            }
        }

        public static string CurrentDirectory => Directory.GetCurrentDirectory();

        public static string DirectoryPathWithInvalidCharacters => $@"{TestDirectoryRoot}|dir{Sep}ABC.tst";

        public static string FileNameWithInvalidCharacters => $@"{TestDirectoryRoot}{Sep}ABC:123*.tst";

        public static string NextAbsoluteFilePath => $@"{NextAbsoluteName}{Sep}{NextFileName}";

        public static string NextAbsoluteName => $@"{VolumeRoot}{Sep}absolute{++_counter}";

        public static string NextFileName => $"file{++_counter}.txt";

        public static string NextRelativeFilePath => $@"{NextRelativeName}{Sep}{NextFileName}";

        public static string NextRelativeName => $"relative{++_counter}";

        public static string NextRootedName => $@"{Sep}rooted{++_counter}";

        public static string NextRootName => $"root{++_counter}";

        public static string OutputDirectory => TestDirectoryRoot;

        public static string OutputFilePath => $@"{OutputDirectory}{Sep}{OutputFileName}";

        public static string PathWithMissingFileName => $@"{TestDirectoryRoot}{Sep}Missing{Sep}";

        public static string SolutionDirectory { get; private set; }

        public static string TemplateDirectoryPath => $@"{TestDirectoryRoot}{Sep}Templates";

        public static string TemplateFilePathString1 => $@"{TemplateDirectoryPath}{Sep}{TemplateFileNameString1}";

        public static string TemplateFilePathString2 => $@"{TemplateDirectoryPath}{Sep}{TemplateFileNameString2}";

        public static string TestDirectoryRoot => $@"{VolumeRoot}{Sep}Test";

        public static string VolumeRoot => CurrentDirectory[0..2];

        public static string CreateTestFiles(string path, string[] text)
        {
            string directoryPath = GetFullPath(path);
            DeleteTestFiles(directoryPath);
            Directory.CreateDirectory(directoryPath);
            string fileName = NextFileName;
            string fullFilePath = Path.Combine(directoryPath, fileName);
            File.WriteAllLines(fullFilePath, text);
            return fullFilePath;
        }

        public static void CreateTestFiles(string path, int numFiles)
        {
            string directoryPath = GetFullPath(path);
            DeleteTestFiles(directoryPath);
            Directory.CreateDirectory(directoryPath);

            for (int i = 0; i < numFiles; i++)
            {
                string filePath = Path.Combine(directoryPath, NextFileName);
                File.WriteAllLines(filePath, Array.Empty<string>());
            }
        }

        public static string CreateTestFiles(string path, bool directoryOnly = false)
        {
            string directoryPath = GetFullPath(path);
            DeleteTestFiles(directoryPath);
            Directory.CreateDirectory(directoryPath);

            if (directoryOnly)
            {
                return string.Empty;
            }

            string fileName = NextFileName;
            string fullFilePath = Path.Combine(directoryPath, fileName);
            File.WriteAllLines(fullFilePath, Array.Empty<string>());
            return fileName;
        }

        public static void DeleteTestFiles(string? path = null)
        {
            path ??= TestDirectoryRoot;
            string directoryPath = GetFullPath(path);

            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        public static string GetFullPath(string path)
            => Path.IsPathRooted(path)
            ? Path.GetFullPath(path)
            : Path.Combine(SolutionDirectory, path);

        internal static Expression<Action<ILogger>> GetLoggerExpression(
            LogEntryType logEntryType,
            string message)
            => x => x.Log(logEntryType, message);

        internal static Expression<Action<ILogger>> GetLoggerExpression(
            LogEntryType logEntryType,
            string message,
            string arg)
            => x => x.Log(logEntryType, message, arg);

        internal static Expression<Action<ILogger>> GetLoggerExpression(
            LogEntryType logEntryType,
            string message,
            bool _)
            => x => x.Log(logEntryType, message, It.IsAny<string>());

        internal static Expression<Action<ILogger>> GetLoggerExpression(
            LogEntryType logEntryType,
            string message,
            string arg1,
            string arg2)
            => x => x.Log(logEntryType, message, arg1, arg2);

        internal static Expression<Action<ILogger>> GetLoggerExpression(
            LogEntryType logEntryType,
            string segmentName,
            int lineNumber,
            string message)
        {
            (string, int) location = (segmentName, lineNumber);
            return x => x.Log(logEntryType, location, message);
        }

        internal static Expression<Action<ILogger>> GetLoggerExpression(
            LogEntryType logEntryType,
            string segmentName,
            int lineNumber,
            string message,
            string arg)
        {
            (string, int) location = (segmentName, lineNumber);
            return x => x.Log(logEntryType, location, message, arg);
        }

        internal static Expression<Action<ILogger>> GetLoggerExpression(
            LogEntryType logEntryType,
            string segmentName,
            int lineNumber,
            string message,
            string arg1,
            string arg2)
        {
            (string, int) location = (segmentName, lineNumber);
            return x => x.Log(logEntryType, location, message, arg1, arg2);
        }

        internal static Expression<Action<ILogger>> GetLoggerExpression(
            LogEntryType type,
            (string segmentName, int lineNumber) location,
            string message,
            string arg) => x => x.Log(type, location, message, arg);
    }
}