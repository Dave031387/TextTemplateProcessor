namespace TextTemplateProcessor.IO
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using static global::TextTemplateProcessor.Core.Messages;

    internal class TextWriter : ITextWriter
    {
        private string _directoryPath = string.Empty;
        private string _fileName = string.Empty;
        private string _filePath = string.Empty;

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
        internal TextWriter(IFileAndDirectoryService fileAndDirectoryService,
                            ILogger logger,
                            IPathValidater pathValidater)
        {
            Utility.NullDependencyCheck(logger,
                                        ClassNames.TextWriterClass,
                                        ServiceNames.LoggerService,
                                        ServiceParameterNames.LoggerParameter);

            Utility.NullDependencyCheck(fileAndDirectoryService,
                                        ClassNames.TextWriterClass,
                                        ServiceNames.FileAndDirectoryService,
                                        ServiceParameterNames.FileAndDirectoryServiceParameter);

            Utility.NullDependencyCheck(pathValidater,
                                        ClassNames.TextWriterClass,
                                        ServiceNames.PathValidaterService,
                                        ServiceParameterNames.PathValidaterParameter);

            Logger = logger;
            FileAndDirectoryService = fileAndDirectoryService;
            PathValidater = pathValidater;
        }

        private IFileAndDirectoryService FileAndDirectoryService { get; init; }

        private ILogger Logger { get; init; }

        private IPathValidater PathValidater { get; init; }

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
            bool isValid = false;

            try
            {
                ValidateOutputFilePath(filePath);

                if (IsValidTextLines(textLines))
                {
                    FileAndDirectoryService.CreateDirectory(_directoryPath);
                    Logger.Log(MsgWritingTextFile,
                               _fileName);
                    FileAndDirectoryService.WriteTextFile(_filePath, textLines);
                    isValid = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MsgUnableToWriteFile,
                           ex.Message);
            }

            return isValid;
        }

        private bool IsValidTextLines(IEnumerable<string>? textLines)
        {
            if (textLines is null)
            {
                Logger.Log(MsgGeneratedTextIsNull);
                return false;
            }

            if (!textLines.Any())
            {
                Logger.Log(MsgGeneratedTextIsEmpty,
                           _fileName);
                return false;
            }

            return true;
        }

        private void ValidateOutputFilePath(string filePath)
        {
            _filePath = PathValidater.ValidateFullPath(filePath, true);
            _directoryPath = FileAndDirectoryService.GetDirectoryName(_filePath);
            _fileName = FileAndDirectoryService.GetFileName(_filePath);
        }
    }
}