namespace TextTemplateProcessor.Console
{
    using global::TextTemplateProcessor.Interfaces;
    using System;

    /// <summary>
    /// The <see cref="ConsoleReader" /> class provides a single method for reading user input from
    /// the <see cref="Console" />.
    /// </summary>
    internal class ConsoleReader : IConsoleReader
    {
        /// <summary>
        /// Default constructor that creates an instance of the <see cref="ConsoleReader" /> class.
        /// </summary>
        public ConsoleReader()
        {
        }

        /// <summary>
        /// Read a line of text from the <see cref="Console" />.
        /// </summary>
        /// <returns>
        /// The text that was entered in the <see cref="Console" /> by the user.
        /// </returns>
        public string ReadLine() => Console.ReadLine() ?? string.Empty;
    }
}