namespace TextTemplateProcessor.Core
{
    using global::TextTemplateProcessor.Interfaces;

    /// <summary>
    /// The <see cref="Locater" /> class is a class used for keeping track of the current location
    /// in the text template file as it is processed.
    /// </summary>
    internal class Locater : ILocater
    {
        /// <summary>
        /// The static constructor initializes the <see cref="Locater" /> class with an empty
        /// segment name and line number 0.
        /// </summary>
        public Locater()
        {
            CurrentSegment = string.Empty;
            LineNumber = 0;
        }

        /// <summary>
        /// Gets the name of the current segment being processed in the text template file.
        /// </summary>
        public string CurrentSegment { get; set; }

        /// <summary>
        /// Gets the current line number of the segment that is being processed in the text template
        /// file.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets the current location in the text template file.
        /// </summary>
        /// <returns>
        /// A <see langword="(string, int)" /> tuple containing the current segment name and the
        /// current line number within that segment.
        /// </returns>
        public (string currentSegment, int lineNumber) Location => (CurrentSegment, LineNumber);

        /// <summary>
        /// Resets the current segment name to an empty string and the line number to 0.
        /// </summary>
        public void Reset()
        {
            CurrentSegment = string.Empty;
            LineNumber = 0;
        }

        /// <summary>
        /// Generates a string representation of the <see cref="Locater" /> class object.
        /// </summary>
        /// <returns>
        /// A <see langword="string" /> containing the current segment name and line number.
        /// </returns>
        public override string ToString() => $"{CurrentSegment}[{LineNumber}]";
    }
}