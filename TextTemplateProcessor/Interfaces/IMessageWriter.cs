namespace TextTemplateProcessor.Interfaces
{
    /// <summary>
    /// An interface that implements methods for writing formatted strings to the console.
    /// </summary>
    /// <remarks>
    /// The formatted strings can contain at most two string format items.
    /// </remarks>
    internal interface IMessageWriter
    {
        /// <summary>
        /// Writes a single <see langword="string" /> message to the <see cref="Console" />.
        /// </summary>
        /// <param name="message">
        /// The message to be written the <see cref="Console" />.
        /// </param>
        void WriteLine(string message);

        /// <summary>
        /// Writes a string message containing a single <see langword="string" /> format item to the
        /// <see cref="Console" />.
        /// </summary>
        /// <param name="message">
        /// The message to be written to the <see cref="Console" />. <br /> (Must contain one
        /// <see langword="string" /> format item.)
        /// </param>
        /// <param name="arg">
        /// A <see langword="string" /> value that will be substituted for the
        /// <br /><see langword="string" /> format item in the <paramref name="message" /> string.
        /// </param>
        void WriteLine(string message, string arg);

        /// <summary>
        /// Writes a string message containing two <see langword="string" /> format items to the
        /// <see cref="Console" />.
        /// </summary>
        /// <param name="message">
        /// The message to be written to the <see cref="Console" />. <br /> (Must contain two
        /// <see langword="string" /> format items.)
        /// </param>
        /// <param name="arg1">
        /// A <see langword="string" /> value that will be substituted for the <br /> first
        /// <see langword="string" /> format item in the <paramref name="message" /> string.
        /// </param>
        /// <param name="arg2">
        /// A <see langword="string" /> value that will be substituted for the <br /> second
        /// <see langword="string" /> format item in the <paramref name="message" /> string.
        /// </param>
        void WriteLine(string message, string arg1, string arg2);
    }
}