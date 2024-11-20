namespace TextTemplateProcessor.IO
{
    using global::TextTemplateProcessor.Interfaces;
    using System.IO;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="FileAndDirectoryService" /> class provides services for managing files and
    /// directories.
    /// </summary>
    internal class FileAndDirectoryService : IFileAndDirectoryService
    {
        private const string SolutionFileSearchPattern = "*.sln";

        /// <summary>
        /// Default constructor that creates an instance of the
        /// <see cref="FileAndDirectoryService" /> class.
        /// </summary>
        public FileAndDirectoryService()
        {
        }

        /// <summary>
        /// Clears the contents of the given directory if the directory exists.
        /// </summary>
        /// <param name="directoryPath">
        /// The directory whose contents are to be cleared.
        /// </param>
        public void ClearDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                DirectoryInfo directoryInfo = new(directoryPath);

                foreach (FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
            }
        }

        /// <summary>
        /// Combine the given path strings to create the resulting file path string.
        /// </summary>
        /// <param name="path1">
        /// The first part of the path that is to be combined to generate the file path.
        /// </param>
        /// <param name="path2">
        /// The second part of the path that is to be combined to generate the file path.
        /// </param>
        /// <returns>
        /// The file path obtained by combining <paramref name="path1" /> and
        /// <paramref name="path2" />.
        /// </returns>
        public string CombinePaths(string path1, string path2)
            => Path.Combine(path1, path2);

        /// <summary>
        /// Validates the given directory path and then creates the directory if it doesn't exist.
        /// </summary>
        /// <param name="path">
        /// The directory path (either relative or absolute).
        /// </param>
        /// <param name="rootDirectory">
        /// The directory that is used as the root of the full directory path if the
        /// <paramref name="path" /> parameter is a relative path.
        /// </param>
        /// <returns>
        /// A string representation of the full directory path.
        /// </returns>
        /// <exception cref="FilePathException">
        /// Exception is thrown if the <paramref name="rootDirectory" /> isn't a rooted path.
        /// </exception>
        public string CreateDirectory(string path, string rootDirectory)
        {
            string fullDirectoryPath = GetFullPath(path, rootDirectory);

            if (!Directory.Exists(fullDirectoryPath))
            {
                _ = Directory.CreateDirectory(fullDirectoryPath);
            }

            return fullDirectoryPath;
        }

        /// <summary>
        /// Creates the given directory path if it doesn't already exist.
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path to be created.
        /// </param>
        public void CreateDirectory(string directoryPath)
        {
            string fullDirectoryPath = GetFullPath(directoryPath, string.Empty);

            Directory.CreateDirectory(fullDirectoryPath);
        }

        /// <summary>
        /// Gets the directory name from the given path string.
        /// </summary>
        /// <param name="path">
        /// A directory or file path string.
        /// </param>
        /// <returns>
        /// The directory path string from the <paramref name="path" />, or an empty string if the
        /// directory name can't be determined.
        /// </returns>
        /// <remarks>
        /// This method assumes that <paramref name="path" /> is a well-formed file path string, or
        /// it is null, empty, or only whitespace.
        /// </remarks>
        public string GetDirectoryName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            int sep = path.LastIndexOf(Path.DirectorySeparatorChar);

            return sep <= 0 ? string.Empty : sep == 2 && path[1] == ':' ? string.Empty : path[..sep];
        }

        /// <summary>
        /// Gets the file name from the given path string.
        /// </summary>
        /// <param name="path">
        /// A directory or file path string.
        /// </param>
        /// <returns>
        /// The file name string from the <paramref name="path" />, or an empty string if the file
        /// name can't be determined.
        /// </returns>
        /// <remarks>
        /// This method assumes that <paramref name="path" /> is a well-formed file path string, or
        /// it is null, empty, or only whitespace.
        /// </remarks>
        public string GetFileName(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            int sep = path.LastIndexOf(Path.DirectorySeparatorChar) + 1;

            return sep >= path.Length ? string.Empty : path[sep..];
        }

        /// <summary>
        /// Gets the full path for the given path string.
        /// </summary>
        /// <param name="path">
        /// A file or directory path (may be absolute or relative).
        /// </param>
        /// <param name="rootPath">
        /// The default rooted path that will be used to construct the full file path if the
        /// <paramref name="path" /> parameter represents a relative path.
        /// </param>
        /// <param name="isFilePath">
        /// An optional boolean parameter that indicates whether or not the given
        /// <paramref name="path" /> parameter represents a file path.
        /// <para>
        /// The default is <see langword="false" /> ( <paramref name="path" /> is a directory path).
        /// </para>
        /// </param>
        /// <returns>
        /// A string representation of the full file path or directory path.
        /// </returns>
        /// <exception cref="FilePathException">
        /// An exception is thrown if either <paramref name="path" /> or
        /// <paramref name="rootPath" /> is null or empty.
        /// </exception>
        public string GetFullPath(string path, string rootPath, bool isFilePath = false)
        {
            if (path is null)
            {
                string msg = isFilePath ? MsgNullFilePath : MsgNullDirectoryPath;
                throw new FilePathException(msg);
            }

            return rootPath is null
                ? throw new FilePathException(MsgRootPathIsNull)
                : string.IsNullOrWhiteSpace(path)
                    ? string.IsNullOrWhiteSpace(rootPath) ? Directory.GetCurrentDirectory() : Path.GetFullPath(rootPath)
                    : Path.IsPathRooted(path)
                        ? Path.GetFullPath(path)
                        : string.IsNullOrWhiteSpace(rootPath)
                            ? Path.Combine(Directory.GetCurrentDirectory(), path)
                            : Path.GetFullPath(Path.Combine(rootPath, path));
        }

        /// <summary>
        /// Gets the full path to the solution directory.
        /// </summary>
        /// <returns>
        /// A string representation of the full solution directory path.
        /// </returns>
        public string GetSolutionDirectory()
        {
            string? path = Path.GetDirectoryName(GetType().Assembly.Location);
            int pathIndex;

            if (path is null)
            {
                throw new FilePathException(MsgUnableToLocateSolutionDirectory);
            }

            while (true)
            {
                pathIndex = path.LastIndexOf(Path.DirectorySeparatorChar);

                if (pathIndex < 0)
                {
                    throw new FilePathException(MsgUnableToLocateSolutionDirectory);
                }

                path = path[..pathIndex];

                string[] files = Directory.GetFiles(path, SolutionFileSearchPattern);

                if (files.Length > 0)
                {
                    break;
                }
            }

            return path;
        }

        /// <summary>
        /// Reads the text file identified by the given file path.
        /// </summary>
        /// <param name="fullFilePath">
        /// The full file path of the text file to be read.
        /// </param>
        /// <returns>
        /// A collection of text strings read in from the text file.
        /// </returns>
        public IEnumerable<string> ReadTextFile(string fullFilePath)
        {
            if (!File.Exists(fullFilePath))
            {
                throw new FilePathException(MsgFileNotFound + fullFilePath);
            }

            using StreamReader reader = new(fullFilePath);
            while (!reader.EndOfStream)
            {
                string? textLine = reader.ReadLine();

                if (textLine is not null)
                {
                    yield return textLine;
                }
            }
        }

        /// <summary>
        /// Writes a collection of text strings to the specified text file.
        /// </summary>
        /// <param name="filePath">
        /// The file path where the text is to be written to.
        /// </param>
        /// <param name="textLines">
        /// The collection of text strings to be written to the text file.
        /// </param>
        public void WriteTextFile(string filePath, IEnumerable<string> textLines)
        {
            using StreamWriter writer = new(filePath);
            foreach (string textLine in textLines)
            {
                writer.WriteLine(textLine);
            }
        }
    }
}