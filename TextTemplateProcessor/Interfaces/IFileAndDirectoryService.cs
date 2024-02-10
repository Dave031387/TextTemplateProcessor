namespace TextTemplateProcessor.Interfaces
{
    using global::TextTemplateProcessor.IO;
    using System.Collections.Generic;

    /// <summary>
    /// An interface that provides file- and directory-related services.
    /// </summary>
    internal interface IFileAndDirectoryService
    {
        /// <summary>
        /// Clears the contents of the given directory if the directory exists.
        /// </summary>
        /// <param name="directoryPath">
        /// The directory whose contents are to be cleared.
        /// </param>
        void ClearDirectory(string directoryPath);

        /// <summary>
        /// Combine the given directory path and file name to create the full file path.
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <returns>
        /// A string representation of the full file path.
        /// </returns>
        string CombineDirectoryAndFileName(string directoryPath, string fileName);

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
        string CreateDirectory(string path, string rootDirectory);

        /// <summary>
        /// Creates the given directory path if it doesn't already exist.
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path to be created.
        /// </param>
        void CreateDirectory(string directoryPath);

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
        string GetDirectoryName(string path);

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
        string GetFileName(string path);

        /// <summary>
        /// Gets the full path for the given path string.
        /// </summary>
        /// <param name="path">
        /// A file or directory path (may be absolute or relative).
        /// </param>
        /// <param name="defaultRootPath">
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
        /// <paramref name="defaultRootPath" /> is null or empty.
        /// </exception>
        string GetFullPath(string path, string defaultRootPath, bool isFilePath = false);

        /// <summary>
        /// Gets the full path to the solution directory.
        /// </summary>
        /// <returns>
        /// A string representation of the full solution directory path.
        /// </returns>
        string GetSolutionDirectory();

        /// <summary>
        /// Reads the text file identified by the given file path.
        /// </summary>
        /// <param name="fullFilePath">
        /// The full file path of the text file to be read.
        /// </param>
        /// <returns>
        /// A collection of text strings read in from the text file.
        /// </returns>
        IEnumerable<string> ReadTextFile(string fullFilePath);

        /// <summary>
        /// Writes a collection of text strings to the specified text file.
        /// </summary>
        /// <param name="filePath">
        /// The file path where the text is to be written to.
        /// </param>
        /// <param name="textLines">
        /// The collection of text strings to be written to the text file.
        /// </param>
        void WriteTextFile(string filePath, IEnumerable<string> textLines);
    }
}