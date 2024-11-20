namespace TextTemplateProcessor.Interfaces
{
    using global::TextTemplateProcessor.Core;
    using System.Collections.Generic;

    /// <summary>
    /// An interface that defines methods for a logger object.
    /// </summary>
    internal interface ILogger
    {
        /// <summary>
        /// Gets the collection of <see cref="LogEntry" /> objects that haven't been written to the
        /// <see cref="Console" /> yet.
        /// </summary>
        IEnumerable<LogEntry> LogEntries { get; }

        /// <summary>
        /// Clears the list of <see cref="LogEntry" /> objects that have been written to the log.
        /// </summary>
        void Clear();

        /// <summary>
        /// Get the type of log entry currently being processed.
        /// </summary>
        LogEntryType GetLogEntryType();

        /// <summary>
        /// Adds a new <see cref="LogEntry" /> object to the <see cref="LogEntries" /> collection.
        /// </summary>
        /// <param name="message">
        /// The log message that is being added to the <see cref="LogEntries" /> collection.
        /// </param>
        /// <param name="arg1">
        /// An optional <see langword="string" /> value to be substituted for the first string
        /// format argument in the <paramref name="message" /> parameter.
        /// </param>
        /// <param name="arg2">
        /// An optional <see langword="string" /> value to be substituted for the second string
        /// format argument in the <paramref name="message" /> parameter.
        /// </param>
        void Log(string message, string? arg1 = null, string? arg2 = null);

        /// <summary>
        /// Sets the current log entry type to the specified value.
        /// </summary>
        /// <param name="logEntryType">
        /// The new value for the current log entry type.
        /// </param>
        void SetLogEntryType(LogEntryType logEntryType);

        /// <summary>
        /// Writes the contents of the <see cref="LogEntries" /> collection to the
        /// <see cref="Console" /> and then clears the <see cref="LogEntries" /> collection.
        /// </summary>
        void WriteLogEntries();
    }
}