namespace TextTemplateProcessor.Logger
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using System;

    /// <summary>
    /// The <see cref="ConsoleLogger" /> class is used for logging to the <see cref="Console" />.
    /// </summary>
    internal class ConsoleLogger : ILogger
    {
        private readonly ILocater _locater;
        private readonly List<LogEntry> _logEntries = new();
        private readonly IMessageWriter _messageWriter;
        private LogEntryType _currentLogEntryType = LogEntryType.Setup;

        /// <summary>
        /// Default constructor that creates an instance of the <see cref="ConsoleLogger" /> class.
        /// </summary>
        public ConsoleLogger() : this(ServiceLocater.Current.Get<IMessageWriter>(),
                                      ServiceLocater.Current.Get<ILocater>())
        {
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="ConsoleLogger" /> class with the
        /// given <see cref="IMessageWriter" /> object.
        /// </summary>
        /// <param name="messageWriter">
        /// An instance of an object that implements the <see cref="IMessageWriter" /> interface.
        /// </param>
        /// <param name="locater">
        /// An instance of an object that implements the <see cref="ILocater" /> interface.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal ConsoleLogger(IMessageWriter messageWriter, ILocater locater)
        {
            Utility.NullDependencyCheck(messageWriter,
                                        ClassNames.ConsoleLoggerClass,
                                        ServiceNames.MessageWriterService,
                                        ServiceParameterNames.MessageWriterParameter);

            Utility.NullDependencyCheck(locater,
                                        ClassNames.ConsoleLoggerClass,
                                        ServiceNames.LocaterService,
                                        ServiceParameterNames.LocaterParameter);

            _messageWriter = messageWriter;
            _locater = locater;
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
        public void Log(string message, string? arg1 = null, string? arg2 = null)
        {
            string formattedMessage = arg1 is null ? message : arg2 is null ? string.Format(message, arg1) : string.Format(message, arg1, arg2);

            switch (_currentLogEntryType)
            {
                case LogEntryType.Setup:
                case LogEntryType.Loading:
                case LogEntryType.Writing:
                case LogEntryType.Reset:
                    _logEntries.Add(new(_currentLogEntryType, string.Empty, 0, formattedMessage));
                    break;

                case LogEntryType.Parsing:
                case LogEntryType.Generating:
                    _logEntries.Add(new(_currentLogEntryType, _locater.CurrentSegment, _locater.LineNumber, formattedMessage));
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Sets the current log entry type to the specified value.
        /// </summary>
        /// <param name="logEntryType">
        /// The new value for the current log entry type.
        /// </param>
        public void SetLogEntryType(LogEntryType logEntryType) => _currentLogEntryType = logEntryType;

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