namespace TextTemplateProcessor.IO
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using static global::TextTemplateProcessor.Core.Messages;

    internal class TextWriter : ITextWriter
    {
        private readonly IFileAndDirectoryService _fileAndDirectoryService;
        private readonly ILogger _logger;
        private readonly IPathValidater _pathValidater;
        private string _directoryPath = string.Empty;
        private string _fileName = string.Empty;
        private string _filePath = string.Empty;

        /// <summary>
        /// The default constructor for creating an instance of the <see cref="TextWriter" /> class.
        /// </summary>
        public TextWriter() : this(
            ServiceLocater.Current.Get<ILogger>(),
            ServiceLocater.Current.Get<IFileAndDirectoryService>(),
            ServiceLocater.Current.Get<IPathValidater>())
        {
        }

        /// <summary>
        /// A constructor that creates an instance of the <see cref="TextWriter" /> class and
        /// initializes its dependencies.
        /// </summary>
        /// <param name="logger">
        /// A reference to a logger object for logging messages.
        /// </param>
        /// <param name="fileAndDirectoryService">
        /// A reference to a file and directory service object for handling I/O operations.
        /// </param>
        /// <param name="pathValidater">
        /// A reference to a path validater object for validating file and directory paths.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal TextWriter(
            ILogger logger,
            IFileAndDirectoryService fileAndDirectoryService,
            IPathValidater pathValidater)
        {
            if (logger is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TextWriter), nameof(ILogger));
                throw new ArgumentNullException(nameof(logger), message);
            }

            if (fileAndDirectoryService is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TextWriter), nameof(IFileAndDirectoryService));
                throw new ArgumentNullException(nameof(fileAndDirectoryService), message);
            }

            if (pathValidater is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TextWriter), nameof(IPathValidater));
                throw new ArgumentNullException(nameof(pathValidater), message);
            }

            _logger = logger;
            _fileAndDirectoryService = fileAndDirectoryService;
            _pathValidater = pathValidater;
        }

        /// <summary>
        /// Write the contents of the given string collection to the specified file path.
        /// </summary>
        /// <param name="filePath">
        /// The file path where the text file is to be written.
        /// </param>
        /// <param name="textLines">
        /// The <see cref="IEnumerable{T}" /> collection of <see langword="string" /> objects to be
        /// written to the text file.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the file is successfully written to. Otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// The output file will be overwritten if it already exists. Otherwise, the output file
        /// path will be created before writing the text to the output file.
        /// </remarks>
        public bool WriteTextFile(string filePath, IEnumerable<string> textLines)
        {
            bool isValid = IsValidTextLines(textLines);

            if (isValid)
            {
                try
                {
                    ValidateOutputFilePath(filePath);
                    _fileAndDirectoryService.CreateDirectory(_directoryPath);
                    _logger.Log(LogEntryType.Writing, MsgWritingTextFile, _fileName);
                    _fileAndDirectoryService.WriteTextFile(_filePath, textLines);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogEntryType.Writing, MsgUnableToWriteFile, ex.Message);
                    isValid = false;
                }
            }

            return isValid;
        }

        private bool IsValidTextLines(IEnumerable<string>? textLines)
        {
            if (textLines is null)
            {
                _logger.Log(LogEntryType.Writing, MsgGeneratedTextIsNull);
                return false;
            }

            if (!textLines.Any())
            {
                _logger.Log(LogEntryType.Writing, MsgGeneratedTextIsEmpty);
                return false;
            }

            return true;
        }

        private void ValidateOutputFilePath(string filePath)
        {
            _filePath = _pathValidater.ValidateFullPath(filePath, true);
            _directoryPath = _fileAndDirectoryService.GetDirectoryName(_filePath);
            _fileName = _fileAndDirectoryService.GetFileName(_filePath);
        }
    }
}