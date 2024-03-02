namespace TextTemplateProcessor.Interfaces
{
    using global::TextTemplateProcessor.Core;

    /// <summary>
    /// An interface the provides methods for validating and parsing text lines from a text template
    /// file.
    /// </summary>
    internal interface ITextLineParser
    {
        /// <summary>
        /// Determines whether or not the given line from the text template file is a comment line.
        /// </summary>
        /// <param name="templateLine">
        /// A line from a text template file.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the line is a comment line. Otherwise, returns
        /// <see langword="false" />.
        /// </returns>
        bool IsCommentLine(string templateLine);

        /// <summary>
        /// Determines whether or not the given line from the text template file is a segment
        /// header.
        /// </summary>
        /// <param name="templateLine">
        /// A line from a text template file.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the line is a segment header line. Otherwise, returns
        /// <see langword="false" />.
        /// </returns>
        bool IsSegmentHeader(string templateLine);

        /// <summary>
        /// Determines whether or not the given line from the text template file is a text line.
        /// </summary>
        /// <param name="templateLine">
        /// A line from a text template file.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the line is a text line. Otherwise, returns
        /// <see langword="false" />.
        /// </returns>
        bool IsTextLine(string templateLine);

        /// <summary>
        /// Determines whether or not the prefix portion of a line from a text template file is
        /// valid.
        /// </summary>
        /// <param name="templateLine">
        /// A line from a text template file.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the first four characters of the text line form a valid
        /// prefix. Otherwise, returns <see langword="false" />.
        /// </returns>
        bool IsValidPrefix(string templateLine);

        /// <summary>
        /// Parses a text line from a text template file and extracts control information and
        /// tokens.
        /// </summary>
        /// <param name="textLine">
        /// A text line from a text template file.
        /// </param>
        /// <returns>
        /// a <see cref="TextItem" /> object containing the text from the text line plus the control
        /// information.
        /// </returns>
        /// <remarks>
        /// This method assumes that the <paramref name="textLine" /> that is passed into the method
        /// has already been validated to ensure it is a valid text line.
        /// </remarks>
        TextItem ParseTextLine(string textLine);
    }
}