namespace TextTemplateProcessor.Interfaces
{
    using global::TextTemplateProcessor.Core;
    using System.Collections.Generic;

    /// <summary>
    /// An interface that provides a method for parsing and loading a text template file.
    /// </summary>
    internal interface ITemplateLoader
    {
        /// <summary>
        /// This method parses and loads a text template file for processing. Individual segments
        /// are identified, and control information is extracted.
        /// </summary>
        /// <param name="templateLines">
        /// A collection of text strings. Each string is a line from the text template file.
        /// </param>
        /// <param name="segmentDictionary">
        /// A dictionary of key/value pairs where the key is a segment name from the text template
        /// file and the value is a collection of text lines that belong to that segment.
        /// </param>
        /// <param name="controlDictionary">
        /// A dictionary of key/value pairs where the key is a segment name from the text template
        /// file and the value is a <see cref="ControlItem" /> object containing the control
        /// information for that segment.
        /// </param>
        void LoadTemplate(
            IEnumerable<string> templateLines,
            Dictionary<string, List<TextItem>> segmentDictionary,
            Dictionary<string, ControlItem> controlDictionary);
    }
}