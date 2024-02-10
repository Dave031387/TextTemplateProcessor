namespace TextTemplateProcessor.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// An interface that provides support for reading the contents of a text file.
    /// </summary>
    internal interface ITextReader
    {
        /// <summary>
        /// Gets the directory path of the text file to be read.
        /// </summary>
        string DirectoryPath { get; }

        /// <summary>
        /// Gets the file name of the text file to be read.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets the full file path of the text file to be read.
        /// </summary>
        string FullFilePath { get; }

        /// <summary>
        /// Reads the text file referenced by the <see cref="FullFilePath" /> property.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> collection of <see langword="string" /> objects
        /// representing the individual lines read from the text file.
        /// </returns>
        IEnumerable<string> ReadTextFile();

        /// <summary>
        /// Sets the full file path of the text file to be read.
        /// </summary>
        /// <param name="filePath">
        /// The absolute or relative path to the text file.
        /// </param>
        void SetFilePath(string filePath);
    }
}