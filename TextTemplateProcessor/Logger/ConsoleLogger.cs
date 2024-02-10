namespace TextTemplateProcessor.Logger
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using System;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="ConsoleLogger" /> class is used for logging to the <see cref="Console" />.
    /// </summary>
    internal class ConsoleLogger : ILogger
    {
        private readonly List<LogEntry> _logEntries = new();
        private readonly IMessageWriter _messageWriter;

        /// <summary>
        /// Default constructor that creates an instance of the <see cref="ConsoleLogger" /> class.
        /// </summary>
        public ConsoleLogger() : this(ServiceLocater.Current.Get<IMessageWriter>())
        {
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="ConsoleLogger" /> class with the
        /// given <see cref="IMessageWriter" /> object.
        /// </summary>
        /// <param name="messageWriter">
        /// An instance of an object that implements the <see cref="IMessageWriter" /> interface.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal ConsoleLogger(IMessageWriter messageWriter)
        {
            if (messageWriter is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(ConsoleLogger), nameof(IMessageWriter));
                throw new ArgumentNullException(nameof(messageWriter), message);
            }

            _messageWriter = messageWriter;
        }

        /// <summary>
        /// Gets the collection of <see cref="LogEntry" /> objects that haven't been written to the
        /// <see cref="Console" /> yet.
        /// </summary>
        public IEnumerable<LogEntry> LogEntries => _logEntries;

        /// <summary>
        /// Clears the list of <see cref="LogEntry" /> objects that haven't yet been written to the
        /// <see cref="Console" />.
        /// </summary>
        public void Clear() => _logEntries.Clear();

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
        public void Log(LogEntryType type, string message) => _logEntries.Add(new(type, string.Empty, 0, message));

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
        public void Log(LogEntryType type, (string segmentName, int lineNumber) location, string message)
        {
            switch (type)
            {
                case LogEntryType.Setup:
                case LogEntryType.Loading:
                case LogEntryType.Writing:
                case LogEntryType.Reset:
                    Log(type, message);
                    break;

                case LogEntryType.Parsing:
                case LogEntryType.Generating:
                    _logEntries.Add(new(type, location.segmentName, location.lineNumber, message));
                    break;

                default:
                    break;
            }
        }

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
        public void Log(LogEntryType type, string message, string arg)
        {
            arg ??= string.Empty;
            string formattedMessage = string.Format(message, arg);
            Log(type, formattedMessage);
        }

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
        public void Log(LogEntryType type, string message, string arg1, string arg2)
        {
            arg1 ??= string.Empty;
            arg2 ??= string.Empty;
            string formattedMessage = string.Format(message, arg1, arg2);
            Log(type, formattedMessage);
        }

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
        public void Log(LogEntryType type, (string segmentName, int lineNumber) location, string message, string arg)
        {
            arg ??= string.Empty;
            string formattedMessage = string.Format(message, arg);
            Log(type, location, formattedMessage);
        }

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
        public void Log(LogEntryType type, (string segmentName, int lineNumber) location, string message, string arg1, string arg2)
        {
            arg1 ??= string.Empty;
            arg2 ??= string.Empty;
            string formattedMessage = string.Format(message, arg1, arg2);
            Log(type, location, formattedMessage);
        }

        /// <summary>
        /// Writes the contents of the <see cref="LogEntries" /> collection to the
        /// <see cref="Console" /> and then clears the <see cref="LogEntries" /> collection.
        /// </summary>
        public void WriteLogEntries()
        {
            foreach (LogEntry logEntry in _logEntries)
            {
                _messageWriter.WriteLine(logEntry.ToString());
            }

            Clear();
        }
    }
}