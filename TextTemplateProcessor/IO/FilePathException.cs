namespace TextTemplateProcessor.IO
{
    using System;

    /// <summary>
    /// This exception class is used for all exceptions related to file path and directory path
    /// issues.
    /// </summary>
    [Serializable]
    public class FilePathException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilePathException" /> class.
        /// </summary>
        public FilePathException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePathException" /> class with the
        /// specified error message.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public FilePathException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePathException" /> class with the
        /// specified error message and a reference to the inner exception that is the cause of this
        /// error.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        /// <param name="inner">
        /// The exception that is the cause of the current exception.
        /// </param>
        public FilePathException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}