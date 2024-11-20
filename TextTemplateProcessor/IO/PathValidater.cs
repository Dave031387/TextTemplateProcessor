namespace TextTemplateProcessor.IO
{
    using global::TextTemplateProcessor.Interfaces;
    using System.IO;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// <see cref="PathValidater" /> is used for validating directory paths and file paths to ensure
    /// they're not null, empty, contain only whitespace, or contain invalid characters.
    /// </summary>
    internal class PathValidater : IPathValidater
    {
        /// <summary>
        /// Creates an instance of the <see cref="PathValidater" /> class.
        /// </summary>
        public PathValidater()
        {
        }

        /// <summary>
        /// Validates a path string to verify that it represents a valid directory path or file
        /// path. <br /> Also, optionally validates that the directory or file exists if requested.
        /// </summary>
        /// <param name="path">
        /// A file path or directory path to be validated.
        /// </param>
        /// <param name="isFilePath">
        /// Indicates whether the <paramref name="path" /> argument is a file path (
        /// <see langword="true" />) or a directory path ( <see langword="false" />). The default if
        /// not specified is directory path ( <see langword="false" />).
        /// </param>
        /// <param name="shouldExist">
        /// Indicates whether or not the file or directory must already exist. The default if not
        /// specified is <see langword="false" /> (doesn't have to exist).
        /// </param>
        /// <returns>
        /// The full path if <paramref name="path" /> represents a valid path string. Otherwise,
        /// returns an empty string.
        /// </returns>
        /// <exception cref="FilePathException">
        /// Exception is thrown if <paramref name="path" /> isn't valid or if the path doesn't exist
        /// and <paramref name="shouldExist" /> is set to <see langword="true" />.
        /// </exception>
        public string ValidateFullPath(string path, bool isFilePath = false, bool shouldExist = false)
        {
            path = CheckForNullOrEmpty(path, isFilePath);
            GetDirectoryAndFileNameParts(path, isFilePath, out string directoryPart, out string fileNamePart);
            CheckDirectoryPath(directoryPart);

            string fullDirectoryPath = GetFullDirectoryPath(directoryPart);
            string fullPath;

            if (isFilePath)
            {
                CheckFileName(fileNamePart);
                fullPath = Path.Combine(fullDirectoryPath, fileNamePart);
            }
            else
            {
                fullPath = fullDirectoryPath;
            }

            if (shouldExist)
            {
                VerifyExists(fullPath, isFilePath);
            }

            return fullPath;
        }

        /// <summary>
        /// Validates a path string to verify that it represents a valid directory path or file
        /// path. <br /> Also, optionally validates that the directory or file exists if requested.
        /// </summary>
        /// <param name="path">
        /// A file path or directory path to be validated.
        /// </param>
        /// <param name="isFilePath">
        /// Indicates whether the <paramref name="path" /> argument is a file path (
        /// <see langword="true" />) or a directory path ( <see langword="false" />). The default if
        /// not specified is directory path ( <see langword="false" />).
        /// </param>
        /// <param name="shouldExist">
        /// Indicates whether or not the file or directory must already exist. The default if not
        /// specified is <see langword="false" /> (doesn't have to exist).
        /// </param>
        /// <exception cref="FilePathException">
        /// Exception is thrown if <paramref name="path" /> isn't valid or if the path doesn't exist
        /// and <paramref name="shouldExist" /> is set to <see langword="true" />.
        /// </exception>
        public void ValidatePath(string path, bool isFilePath = false, bool shouldExist = false)
        {
            path = CheckForNullOrEmpty(path, isFilePath);
            GetDirectoryAndFileNameParts(path, isFilePath, out string directoryPart, out string fileNamePart);
            CheckDirectoryPath(directoryPart);

            if (isFilePath)
            {
                CheckFileName(fileNamePart);
            }

            if (shouldExist)
            {
                string fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path);

                VerifyExists(fullPath, isFilePath);
            }
        }

        private static void CheckDirectoryPath(string directoryPath)
        {
            if (directoryPath.IndexOfAny(Path.GetInvalidPathChars()) > -1)
            {
                throw new FilePathException(MsgInvalidDirectoryCharacters);
            }
        }

        private static void CheckFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new FilePathException(MsgMissingFileName);
            }

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                throw new FilePathException(MsgInvalidFileNameCharacters);
            }
        }

        private static string CheckForNullOrEmpty(string path, bool isFilePath)
        {
            if (path is null)
            {
                string msg = isFilePath ? MsgNullFilePath : MsgNullDirectoryPath;
                throw new FilePathException(msg);
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                string msg = isFilePath ? MsgFilePathIsEmptyOrWhitespace : MsgDirectoryPathIsEmptyOrWhitespace;
                throw new FilePathException(msg);
            }

            return path.Trim();
        }

        private static void GetDirectoryAndFileNameParts(
            string path,
            bool isFilePath,
            out string directoryPart,
            out string fileNamePart)
        {
            int indexOfLastSeparator = path.LastIndexOf(Path.DirectorySeparatorChar);
            int fileNameStart = indexOfLastSeparator + 1;

            if (indexOfLastSeparator < 0)
            {
                directoryPart = isFilePath ? string.Empty : path;
                fileNamePart = isFilePath ? path : string.Empty;
            }
            else
            {
                if (isFilePath)
                {
                    directoryPart = indexOfLastSeparator > 0 ? path[..indexOfLastSeparator] : string.Empty;
                    fileNamePart = fileNameStart < path.Length ? path[fileNameStart..] : string.Empty;
                }
                else
                {
                    directoryPart = path;
                    fileNamePart = string.Empty;
                }
            }
        }

        private static string GetFullDirectoryPath(string directoryPath)
        {
            return string.IsNullOrWhiteSpace(directoryPath)
                ? Directory.GetCurrentDirectory()
                : Path.IsPathRooted(directoryPath) ? directoryPath : Path.GetFullPath(directoryPath);
        }

        private static void VerifyExists(string fullPath, bool isFilePath)
        {
            if (isFilePath)
            {
                if (!File.Exists(fullPath))
                {
                    throw new FilePathException(MsgFileNotFound + fullPath);
                }
            }
            else
            {
                if (!Directory.Exists(fullPath))
                {
                    throw new FilePathException(MsgDirectoryNotFound + fullPath);
                }
            }
        }
    }
}