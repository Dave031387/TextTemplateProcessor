namespace TextTemplateProcessor.TestShared
{
    using System;
    using System.Linq.Expressions;

    public static class Globals
    {
        public const string AnyString = "ANY";
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
        public static string NextAbsoluteFilePath => $@"{NextAbsoluteName}{Sep}{NextFileName}";
        public static string NextAbsoluteName => $@"{VolumeRoot}{Sep}absolute{++_counter}";
        public static string NextFileName => $"file{++_counter}.txt";
        public static string NextRelativeFilePath => $@"{NextRelativeName}{Sep}{NextFileName}";
        public static string NextRelativeName => $"relative{++_counter}";
        public static string NextRootedName => $@"{Sep}rooted{++_counter}";
        public static string NextRootName => $"root{++_counter}";
        public static string SolutionDirectory { get; private set; }
        public static string TemplateDirectoryPath => $@"{TestDirectoryRoot}{Sep}Templates";
        public static string TestDirectoryRoot => $@"{VolumeRoot}{Sep}Test";
        public static string VolumeRoot => CurrentDirectory[0..2];
        internal static Expression<Func<ILocater, string>> CurrentSegmentExpression => x => x.CurrentSegment;
        internal static Expression<Func<ILocater, int>> LineNumberExpression => x => x.LineNumber;
        internal static Expression<Func<ILocater, (string, int)>> LocationExpression => x => x.Location;
        internal static Action<ILocater> SetCurrentSegmentAction { get; private set; } = (x) => x.CurrentSegment = "test";
        internal static Action<ILocater> SetLineNumberAction { get; private set; } = x => x.LineNumber = 0;

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

        public static string GetNullDependencyMessage(string className, string serviceName, string parameterName)
            => string.Format(MsgDependencyIsNull, className, serviceName) + $" (Parameter '{parameterName}')";

        internal static Expression<Action<ILogger>> GetLoggerExpression(string message,
                                                                        string? arg1 = null,
                                                                        string? arg2 = null)
        {
            if (arg1 is null)
            {
                return x => x.Log(message, null, null);
            }
            else
            {
                if (arg2 is null)
                {
                    if (arg1 is AnyString)
                    {
                        return x => x.Log(message, It.IsAny<string>(), null);
                    }
                    else
                    {
                        return x => x.Log(message, arg1, null);
                    }
                }
                else
                {
                    if (arg1 is AnyString)
                    {
                        if (arg2 is AnyString)
                        {
                            return x => x.Log(message, It.IsAny<string>(), It.IsAny<string>());
                        }
                        else
                        {
                            return x => x.Log(message, It.IsAny<string>(), arg2);
                        }
                    }
                    else
                    {
                        if (arg2 is AnyString)
                        {
                            return x => x.Log(message, arg1, It.IsAny<string>());
                        }
                        else
                        {
                            return x => x.Log(message, arg1, arg2);
                        }
                    }
                }
            }
        }

        internal static void SetupLocater(Mock<ILocater> locater,
                                          string segmentName,
                                          int lineNumber)
        {
            (string, int) location = (segmentName, lineNumber);
            SetCurrentSegmentAction = x => x.CurrentSegment = segmentName;
            SetLineNumberAction = x => x.LineNumber = lineNumber;
            locater.Setup(CurrentSegmentExpression).Returns(segmentName);
            locater.Setup(LineNumberExpression).Returns(lineNumber);
            locater.Setup(LocationExpression).Returns(location);
            locater.SetupSet(SetCurrentSegmentAction);
            locater.SetupSet(SetLineNumberAction);
        }

        internal static Expression<Action<ILogger>> SetupLogger(Mock<ILogger> logger,
                                                                string message,
                                                                string? arg1 = null,
                                                                string? arg2 = null)
        {
            Expression<Action<ILogger>> loggerExpression = GetLoggerExpression(message, arg1, arg2);
            logger.Setup(loggerExpression);
            return loggerExpression;
        }

        internal static Expression<Func<INameValidater, bool>> SetupNameValidater(Mock<INameValidater> nameValidater,
                                                                                  string name,
                                                                                  bool result)
        {
            Expression<Func<INameValidater, bool>> nameValidaterExpression = x => x.IsValidName(name);
            nameValidater.Setup(nameValidaterExpression).Returns(result);
            return nameValidaterExpression;
        }
    }
}