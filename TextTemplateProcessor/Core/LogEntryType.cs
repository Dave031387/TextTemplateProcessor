namespace TextTemplateProcessor.Core
{
    /// <summary>
    /// This is an <see langword="enum" /> that represents the type of a <see cref="LogEntry" />
    /// object.
    /// </summary>
    internal enum LogEntryType
    {
        /// <summary>
        /// Log entry type assigned to log messages pertaining to setup operations.
        /// </summary>
        Setup,

        /// <summary>
        /// Log entry type assigned to log messages pertaining to reading and loading text template
        /// files.
        /// </summary>
        Loading,

        /// <summary>
        /// Log entry type assigned to log messages pertaining to parsing of text template files.
        /// </summary>
        Parsing,

        /// <summary>
        /// Log entry type assigned to log messages pertaining to the generation of the text output
        /// file.
        /// </summary>
        Generating,

        /// <summary>
        /// Log entry type assigned to log messages pertaining to the writing of the text output
        /// file.
        /// </summary>
        Writing,

        /// <summary>
        /// Log entry type assigned to log messages pertaining to reset operations.
        /// </summary>
        Reset,

        /// <summary>
        /// Log entry type assigned to log messages pertaining to user interactions.
        /// </summary>
        User
    }
}