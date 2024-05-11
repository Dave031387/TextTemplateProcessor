// Ignore Spelling: Sep

namespace TextTemplateProcessor.TestShared
{
    using System;

    public static class Globals
    {
        public const string AnyString = "ANY";
        public const string Comment = "///";
        public const string DefaultSegmentNamePrefix = "DefaultSegment";
        public const int DefaultTabSize = 4;
        public const string FirstTimeIndentOption = "FTI";
        public const string GlobalShared = "GlobalShared";
        public const string IndentAbsolute = "@=";
        public const string IndentAbsoluteOneTime = "O=";
        public const string IndentLeftOneTime = "O-";
        public const string IndentLeftRelative = "@-";
        public const string IndentRightOneTime = "O+";
        public const string IndentRightRelative = "@+";
        public const string IndentUnchanged = "   ";
        public const string PadSegmentNameOption = "PAD";
        public const string SegmentHeaderCode = "###";
        public const string Sep = @"\";
        public const string TabSizeOption = "TAB";
        public const string TokenEnd = "#>";
        public const char TokenEscapeChar = '\\';
        public const string TokenStart = "<#";
        public const string Whitespace = "\t\n\v\f\r \u0085\u00a0\u2002\u2003\u2028\u2029";
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

        public static string CreateTestFiles(string path, string[] text)
        {
            string directoryPath = GetFullPath(path);
            DeleteTestFiles(directoryPath);
            Directory.CreateDirectory(directoryPath);
            string fileName = NextFileName;
            string fullFilePath = $"{directoryPath}{Sep}{fileName}";
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
                string filePath = $"{directoryPath}{Sep}{NextFileName}";
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
            string fullFilePath = $"{directoryPath}{Sep}{fileName}";
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
    }
}