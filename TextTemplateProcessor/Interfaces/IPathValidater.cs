namespace TextTemplateProcessor.Interfaces
{
    using global::TextTemplateProcessor.IO;

    /// <summary>
    /// An interface the defines a method for validating file paths and directory paths.
    /// </summary>
    internal interface IPathValidater
    {
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
        string ValidateFullPath(string path, bool isFilePath = false, bool shouldExist = false);

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
        void ValidatePath(string path, bool isFilePath = false, bool shouldExist = false);
    }
}