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
        /// Clears the list of <see cref="LogEntry" /> objects that haven't yet been written to the
        /// <see cref="Console" />.
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds a new <see cref="LogEntry" /> object to the <see cref="LogEntries" /> collection.
        /// </summary>
        /// <param name="type">
        /// The <see cref="LogEntryType" /> enum value corresponding to the type of
        /// <see cref="LogEntry" /> object being added to the <see cref="LogEntries" /> collection.
        /// </param>
        /// <param name="message">
        /// The log message that is being added to the <see cref="LogEntries" /> collection.
        /// </param>
        void Log(LogEntryType type, string message);

        /// <summary>
        /// Adds a new <see cref="LogEntry" /> object to the <see cref="LogEntries" /> collection.
        /// </summary>
        /// <param name="type">
        /// The <see cref="LogEntryType" /> enum value corresponding to the type of
        /// <see cref="LogEntry" /> object being added to the <see cref="LogEntries" /> collection.
        /// </param>
        /// <param name="location">
        /// A tuple that gives the name of the segment and the line number within the segment where
        /// the log message was triggered.
        /// </param>
        /// <param name="message">
        /// The log message that is being added to the <see cref="LogEntries" /> collection.
        /// </param>
        void Log(LogEntryType type, (string segmentName, int lineNumber) location, string message);

        /// <summary>
        /// Adds a new <see cref="LogEntry" /> object to the <see cref="LogEntries" /> collection.
        /// </summary>
        /// <param name="type">
        /// The <see cref="LogEntryType" /> enum value corresponding to the type of
        /// <see cref="LogEntry" /> object being added to the <see cref="LogEntries" /> collection.
        /// </param>
        /// <param name="message">
        /// The log message that is being added to the <see cref="LogEntries" /> collection. <br />
        /// This must be a <see langword="string" /> that contains one string format argument.
        /// </param>
        /// <param name="arg">
        /// The <see langword="string" /> value to be substituted for the string format argument in
        /// the <paramref name="message" /> parameter.
        /// </param>
        void Log(LogEntryType type, string message, string arg);

        /// <summary>
        /// Adds a new <see cref="LogEntry" /> object to the <see cref="LogEntries" /> collection.
        /// </summary>
        /// <param name="type">
        /// The <see cref="LogEntryType" /> enum value corresponding to the type of
        /// <see cref="LogEntry" /> object being added to the <see cref="LogEntries" /> collection.
        /// </param>
        /// <param name="message">
        /// The log message that is being added to the <see cref="LogEntries" /> collection. <br />
        /// This must be a <see langword="string" /> that contains two string format arguments.
        /// </param>
        /// <param name="arg1">
        /// The <see langword="string" /> value to be substituted for the first string format
        /// argument in the <paramref name="message" /> parameter.
        /// </param>
        /// <param name="arg2">
        /// The <see langword="string" /> value to be substituted for the second string format
        /// argument in the <paramref name="message" /> parameter.
        /// </param>
        void Log(LogEntryType type, string message, string arg1, string arg2);

        /// <summary>
        /// Adds a new <see cref="LogEntry" /> object to the <see cref="LogEntries" /> collection.
        /// </summary>
        /// <param name="type">
        /// The <see cref="LogEntryType" /> enum value corresponding to the type of
        /// <see cref="LogEntry" /> object being added to the <see cref="LogEntries" /> collection.
        /// </param>
        /// <param name="location">
        /// A tuple that gives the name of the segment and the line number within the segment where
        /// the log message was triggered.
        /// </param>
        /// <param name="message">
        /// The log message that is being added to the <see cref="LogEntries" /> collection. <br />
        /// This must be a <see langword="string" /> that contains one string format argument.
        /// </param>
        /// <param name="arg">
        /// The <see langword="string" /> value to be substituted for the string format argument in
        /// the <paramref name="message" /> parameter.
        /// </param>
        void Log(LogEntryType type, (string segmentName, int lineNumber) location, string message, string arg);

        /// <summary>
        /// Adds a new <see cref="LogEntry" /> object to the <see cref="LogEntries" /> collection.
        /// </summary>
        /// <param name="type">
        /// The <see cref="LogEntryType" /> enum value corresponding to the type of
        /// <see cref="LogEntry" /> object being added to the <see cref="LogEntries" /> collection.
        /// </param>
        /// <param name="location">
        /// A tuple that gives the name of the segment and the line number within the segment where
        /// the log message was triggered.
        /// </param>
        /// <param name="message">
        /// The log message that is being added to the <see cref="LogEntries" /> collection. <br />
        /// This must be a <see langword="string" /> that contains two string format arguments.
        /// </param>
        /// <param name="arg1">
        /// The <see langword="string" /> value to be substituted for the first string format
        /// argument in the <paramref name="message" /> parameter.
        /// </param>
        /// <param name="arg2">
        /// The <see langword="string" /> value to be substituted for the second string format
        /// argument in the <paramref name="message" /> parameter.
        /// </param>
        void Log(LogEntryType type, (string segmentName, int lineNumber) location, string message, string arg1, string arg2);

        /// <summary>
        /// Writes the contents of the <see cref="LogEntries" /> collection to the
        /// <see cref="Console" /> and then clears the <see cref="LogEntries" /> collection.
        /// </summary>
        void WriteLogEntries();
    }
}