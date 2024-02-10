namespace TextTemplateProcessor.Interfaces
{
    /// <summary>
    /// An interface the provides a single method for writing text to the <see cref="Console" />.
    /// </summary>
    internal interface IConsoleWriter
    {
        /// <summary>
        /// Write a line of text to the <see cref="Console" />.
        /// </summary>
        /// <param name="text">
        /// </param>
        void WriteLine(string text);
    }
}