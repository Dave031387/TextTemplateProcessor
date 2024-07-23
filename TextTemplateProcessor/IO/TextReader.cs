namespace TextTemplateProcessor.IO
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="TextReader" /> class is used for reading the contents of text files.
    /// </summary>
    internal class TextReader : ITextReader
    {
        private bool _isFilePathSet = false;

        /// <summary>
        /// Default constructor that creates an instance of the <see cref="TextReader" /> class.
        /// </summary>
        public TextReader() : this(ServiceLocater.Current.Get<ILogger>(),
                                   ServiceLocater.Current.Get<IFileAndDirectoryService>(),
                                   ServiceLocater.Current.Get<IPathValidater>())
        {
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="TextReader" /> class and sets the
        /// file path to the given value.
        /// </summary>
        /// <param name="filePath">
        /// The file path of a text file to be read.
        /// </param>
        public TextReader(string filePath) : this(filePath,
                                                  ServiceLocater.Current.Get<ILogger>(),
                                                  ServiceLocater.Current.Get<IFileAndDirectoryService>(),
                                                  ServiceLocater.Current.Get<IPathValidater>())
        {
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="TextReader" /> class and sets the
        /// file path to the given value. (Used for unit testing.)
        /// </summary>
        /// <param name="filePath">
        /// The file path of a text file to be read.
        /// </param>
        /// <param name="logger">
        /// A reference to a logger object for logging messages.
        /// </param>
        /// <param name="fileAndDirectoryService">
        /// A reference to a path service object used for performing I/O operations.
        /// </param>
        /// <param name="pathValidater">
        /// A reference to a path validater object for validating file and directory paths.
        /// </param>
        internal TextReader(string filePath,
                            ILogger logger,
                            IFileAndDirectoryService fileAndDirectoryService,
                            IPathValidater pathValidater)
            : this(logger, fileAndDirectoryService, pathValidater)
            => SetFilePath(filePath);

        /// <summary>
        /// Constructor that creates an instance of the <see cref="TextReader" /> class and
        /// initializes dependencies. (Used for unit testing.)
        /// </summary>
        /// <param name="logger">
        /// A reference to a logger object for logging messages.
        /// </param>
        /// <param name="fileAndDirectoryService">
        /// A reference to a path service object used for performing I/O operations.
        /// </param>
        /// <param name="pathValidater">
        /// A reference to a path validater object for validating file and directory paths.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal TextReader(ILogger logger,
                            IFileAndDirectoryService fileAndDirectoryService,
                            IPathValidater pathValidater)
        {
            Utility.NullDependencyCheck(logger,
                                        ClassNames.TextReaderClass,
                                        ServiceNames.LoggerService,
                                        ServiceParameterNames.LoggerParameter);

            Utility.NullDependencyCheck(fileAndDirectoryService,
                                        ClassNames.TextReaderClass,
                                        ServiceNames.FileAndDirectoryService,
                                        ServiceParameterNames.FileAndDirectoryServiceParameter);

            Utility.NullDependencyCheck(pathValidater,
                                        ClassNames.TextReaderClass,
                                        ServiceNames.PathValidaterService,
                                        ServiceParameterNames.PathValidaterParameter);

            Logger = logger;
            FileAndDirectoryService = fileAndDirectoryService;
            PathValidater = pathValidater;
            InitializeProperties();
        }

        /// <summary>
        /// Gets the directory path of the text file to be read.
        /// </summary>
        public string DirectoryPath { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the file name of the text file to be read.
        /// </summary>
        public string FileName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the full file path of the text file to be read.
        /// </summary>
        public string FullFilePath { get; private set; } = string.Empty;

        private IFileAndDirectoryService FileAndDirectoryService { get; init; }

        private ILogger Logger { get; init; }

        private IPathValidater PathValidater { get; init; }

        /// <summary>
        /// Reads the text file referenced by the <see cref="FullFilePath" /> property.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}" /> collection of <see langword="string" /> objects
        /// representing the individual lines read from the text file.
        /// </returns>
        public IEnumerable<string> ReadTextFile()
        {
            List<string> textLines = [];

            if (_isFilePathSet is false)
            {
                Logger.Log(MsgTemplateFilePathNotSet);
                return textLines;
            }

            try
            {
                Logger.Log(MsgAttemptingToReadFile,
                           FullFilePath);
                textLines = FileAndDirectoryService.ReadTextFile(FullFilePath).ToList();
                Logger.Log(MsgFileSuccessfullyRead);
            }
            catch (Exception ex)
            {
                Logger.Log(MsgErrorWhileReadingTemplateFile,
                           ex.Message);
            }

            return textLines;
        }

        /// <summary>
        /// Sets the full file path of the text file to be read.
        /// </summary>
        /// <param name="filePath">
        /// The absolute or relative path to the text file.
        /// </param>
        public void SetFilePath(string filePath)
        {
            try
            {
                FullFilePath = PathValidater.ValidateFullPath(filePath, true, true);
                DirectoryPath = FileAndDirectoryService.GetDirectoryName(FullFilePath);
                FileName = FileAndDirectoryService.GetFileName(FullFilePath);
                _isFilePathSet = true;
            }
            catch (Exception ex)
            {
                InitializeProperties();
                Logger.Log(MsgUnableToSetTemplateFilePath,
                           filePath,
                           ex.Message);
            }
        }

        private void InitializeProperties()
        {
            _isFilePathSet = false;
            DirectoryPath = string.Empty;
            FileName = string.Empty;
            FullFilePath = string.Empty;
        }
    }
}