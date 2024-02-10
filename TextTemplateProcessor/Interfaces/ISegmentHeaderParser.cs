namespace TextTemplateProcessor.Interfaces
{
    using global::TextTemplateProcessor.Core;

    /// <summary>
    /// An interface that provides a method for parsing the segment headers in a text template file.
    /// </summary>
    internal interface ISegmentHeaderParser
    {
        /// <summary>
        /// This method parses a segment header line from a text template file and extracts the
        /// segment name and control information.
        /// </summary>
        /// <param name="headerLine">
        /// A segment header line from a text template file.
        /// </param>
        /// <returns>
        /// a <see cref="ControlItem" /> object containing the segment name and control information.
        /// </returns>
        ControlItem ParseSegmentHeader(string headerLine);
    }
}