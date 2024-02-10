namespace TextTemplateProcessor.Core
{
    /// <summary>
    /// The <see cref="LogEntry" /> class represents a single log message entry.
    /// </summary>
    internal class LogEntry
    {
        /// <summary>
        /// Creates a new instance of the <see cref="LogEntry" /> class.
        /// </summary>
        /// <param name="logEntryType">
        /// A <see cref="LogEntryType.LogEntryType" /><see langword="enum" /> value corresponding to
        /// the type of this <see cref="LogEntry" /> object.
        /// </param>
        /// <param name="segmentName">
        /// The name of the segment where the log <paramref name="message" /> was triggered.
        /// </param>
        /// <param name="lineNumber">
        /// The line number within the <paramref name="segmentName" /> where the log
        /// <paramref name="message" /> was triggered.
        /// </param>
        /// <param name="message">
        /// The log message.
        /// </param>
        /// <remarks>
        /// The <paramref name="segmentName" /> will be empty if the log message doesn't pertain to
        /// a specific segment.
        /// </remarks>
        internal LogEntry(LogEntryType logEntryType, string segmentName, int lineNumber, string message)
        {
            LogEntryType = logEntryType;
            SegmentName = segmentName;
            LineNumber = lineNumber;
            Message = message;
        }

        /// <summary>
        /// Gets the line number within the segment where the log message was triggered.
        /// </summary>
        internal int LineNumber { get; }

        /// <summary>
        /// Gets the type of the log entry.
        /// </summary>
        internal LogEntryType LogEntryType { get; }

        /// <summary>
        /// Gets the log message.
        /// </summary>
        internal string Message { get; }

        /// <summary>
        /// Gets the name of the segment where the log message was triggered.
        /// </summary>
        internal string SegmentName { get; }

        /// <summary>
        /// Determines whether the specified object is equal to this <see cref="LogEntry" /> object.
        /// </summary>
        /// <param name="obj">
        /// The object to be compared to this <see cref="LogEntry" /> object.
        /// </param>
        /// <returns>
        /// Returns <see langword="true" /> if the properties on the specified object have values
        /// equal to the corresponding properties on this <see cref="LogEntry" /> object.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return obj is not null
                && (obj is LogEntry entry
                ? LineNumber == entry.LineNumber
                    && LogEntryType == entry.LogEntryType
                    && Message == entry.Message
                    && SegmentName == entry.SegmentName
                : base.Equals(obj));
        }

        /// <summary>
        /// Generates a hash code for this <see cref="LogEntry" /> object.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
            => LineNumber.GetHashCode()
            ^ LogEntryType.GetHashCode()
            ^ Message.GetHashCode()
            ^ SegmentName.GetHashCode();

        /// <summary>
        /// Generates a string representation of this <see cref="LogEntry" /> object.
        /// </summary>
        /// <returns>
        /// A string that represents this <see cref="LogEntry" /> object.
        /// </returns>
        public override string ToString() => string.IsNullOrEmpty(SegmentName)
            ? $"<{LogEntryType}> {Message}"
            : $"<{LogEntryType}> {SegmentName}[{LineNumber}] : {Message}";
    }
}