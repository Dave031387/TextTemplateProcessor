namespace TextTemplateProcessor.Console
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using System;

    /// <summary>
    /// The <see cref="MessageWriter" /> class is used by the Text Template Processor for writing
    /// messages to the <see cref="Console" />.
    /// </summary>
    internal class MessageWriter : IMessageWriter
    {
        private readonly IConsoleWriter _consoleWriter;

        /// <summary>
        /// Default constructor that creates an instance of the <see cref="MessageWriter" /> class.
        /// </summary>
        public MessageWriter() : this(ServiceLocater.Current.Get<IConsoleWriter>())
        {
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="MessageWriter" /> class and
        /// initializes dependencies.
        /// </summary>
        /// <param name="consoleWriter">
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal MessageWriter(IConsoleWriter consoleWriter)
        {
            Utility.NullDependencyCheck(consoleWriter,
                                        ClassNames.MessageWriterClass,
                                        ServiceNames.ConsoleWriterService,
                                        ServiceParameterNames.ConsoleWriterParameter);

            _consoleWriter = consoleWriter;
        }

        /// <summary>
        /// Writes a single <see langword="string" /> message to the <see cref="Console" />.
        /// </summary>
        /// <param name="message">
        /// The message to be written the <see cref="Console" />.
        /// </param>
        public void WriteLine(string message)
        {
            _consoleWriter.WriteLine(message);
        }

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
        public void WriteLine(string message, string arg) => WriteLine(string.Format(message, arg));

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
        public void WriteLine(string message, string arg1, string arg2) => WriteLine(string.Format(message, arg1, arg2));
    }
}