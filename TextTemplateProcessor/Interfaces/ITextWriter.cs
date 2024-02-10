namespace TextTemplateProcessor.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// An interface the provides a method for writing text to a text file.
    /// </summary>
    internal interface ITextWriter
    {
        /// <summary>
        /// Write the contents of the given string collection to the specified file path.
        /// </summary>
        /// <param name="filePath">
        /// The file path where the text file is to be written.
        /// </param>
        /// <param name="textLines">
        /// The <see cref="IEnumerable{T}" /> collection of <see langword="string" /> objects to be
        /// written to the text file.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the file is successfully written to. Otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// The output file will be overwritten if it already exists. Otherwise, the output file
        /// path will be created before writing the text to the output file.
        /// </remarks>
        bool WriteTextFile(string filePath, IEnumerable<string> textLines);
    }
}