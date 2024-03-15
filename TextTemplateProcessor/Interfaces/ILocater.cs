namespace TextTemplateProcessor.Interfaces
{
    using global::TextTemplateProcessor.Core;

    /// <summary>
    /// An interface that defines methods and properties for tracking the current location in a text
    /// template file.
    /// </summary>
    internal interface ILocater
    {
        /// <summary>
        /// Gets the name of the current segment being processed in the text template file.
        /// </summary>
        string CurrentSegment { get; set; }

        /// <summary>
        /// Gets the current line number of the segment that is being processed in the text template
        /// file.
        /// </summary>
        int LineNumber { get; set; }

        /// <summary>
        /// Gets the current location in the text template file.
        /// </summary>
        /// <returns>
        /// A <see langword="(string, int)" /> tuple containing the current segment name and the
        /// current line number within that segment.
        /// </returns>
        (string currentSegment, int lineNumber) Location { get; }

        /// <summary>
        /// Resets the current segment name to an empty string and the line number to 0.
        /// </summary>
        void Reset();

        /// <summary>
        /// Generates a string representation of the <see cref="Locater" /> class object.
        /// </summary>
        /// <returns>
        /// A <see langword="string" /> containing the current segment name and line number.
        /// </returns>
        string ToString();
    }
}