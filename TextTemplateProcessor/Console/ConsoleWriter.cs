namespace TextTemplateProcessor.Console
{
    using global::TextTemplateProcessor.Interfaces;
    using System;

    /// <summary>
    /// The <see cref="ConsoleWriter" /> class provides a single method for writing a string of text
    /// to the <see cref="Console" />.
    /// </summary>
    internal class ConsoleWriter : IConsoleWriter
    {
        /// <summary>
        /// Default constructor that creates an instance of the <see cref="ConsoleWriter" /> class.
        /// </summary>
        public ConsoleWriter()
        {
        }

        /// <summary>
        /// Write a line of text to the <see cref="Console" />.
        /// </summary>
        /// <param name="text">
        /// </param>
        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }
    }
}