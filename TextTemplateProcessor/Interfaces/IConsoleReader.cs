namespace TextTemplateProcessor.Interfaces
{
    /// <summary>
    /// An interface that provides a single method for reading text from the <see cref="Console" />.
    /// </summary>
    internal interface IConsoleReader
    {
        /// <summary>
        /// Read a line of text from the <see cref="Console" />.
        /// </summary>
        /// <returns>
        /// The text that was entered in the <see cref="Console" /> by the user.
        /// </returns>
        string ReadLine();
    }
}