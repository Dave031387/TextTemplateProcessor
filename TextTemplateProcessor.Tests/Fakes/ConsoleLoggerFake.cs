namespace TextTemplateProcessor.Fakes
{
    using System.Collections.Generic;

    internal class ConsoleLoggerFake : ILogger
    {
        private readonly List<LogEntry> _logEntries = new();
        private int _writeLogEntriesCallCount = 0;

        public IEnumerable<LogEntry> LogEntries => _logEntries;

        public int WriteLogEntriesCallCount => _writeLogEntriesCallCount;

        public void Clear() => _logEntries.Clear();

        public void Log(LogEntryType type, string message) => _logEntries.Add(new(type, string.Empty, 0, message));

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

        public void Log(LogEntryType type, string message, string arg)
        {
            string formattedMessage = string.Format(message, arg);
            Log(type, formattedMessage);
        }

        public void Log(LogEntryType type, string message, string arg1, string arg2)
        {
            string formattedMessage = string.Format(message, arg1, arg2);
            Log(type, formattedMessage);
        }

        public void Log(LogEntryType type, (string segmentName, int lineNumber) location, string message, string arg)
        {
            string formattedMessage = string.Format(message, arg);
            Log(type, location, formattedMessage);
        }

        public void Log(LogEntryType type, (string segmentName, int lineNumber) location, string message, string arg1, string arg2)
        {
            string formattedMessage = string.Format(message, arg1, arg2);
            Log(type, location, formattedMessage);
        }

        public void Reset()
        {
            _writeLogEntriesCallCount = 0;
            Clear();
        }

        public void WriteLogEntries() => _writeLogEntriesCallCount++;
    }
}