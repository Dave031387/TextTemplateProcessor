namespace TextTemplateProcessor.Console
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using System;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="TextTemplateConsoleBase" /> class is an abstract class that derives from the
    /// <see cref="TextTemplateProcessor" /> class and is intended as a basis for building your own
    /// console app for processing text template files.
    /// </summary>
    public abstract class TextTemplateConsoleBase : TextTemplateProcessor
    {
        private readonly IConsoleReader _consoleReader;
        private readonly IFileAndDirectoryService _fileAndDirectoryService;
        private readonly ILogger _logger;
        private readonly IMessageWriter _messageWriter;
        private readonly IPathValidater _pathValidater;

        /// <summary>
        /// Default constructor that creates an instance of the
        /// <see cref="TextTemplateConsoleBase" /> class.
        /// </summary>
        public TextTemplateConsoleBase() : this(
            ServiceLocater.Current.Get<ILogger>(),
            ServiceLocater.Current.Get<IConsoleReader>(),
            ServiceLocater.Current.Get<IMessageWriter>(),
            ServiceLocater.Current.Get<IFileAndDirectoryService>(),
            ServiceLocater.Current.Get<IPathValidater>())
        {
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="TextTemplateConsoleBase" /> class
        /// and initializes the template file path to the given value.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path (relative or absolute) of the text template file.
        /// </param>
        public TextTemplateConsoleBase(string templateFilePath) : this()
            => LoadTemplate(templateFilePath);

        /// <summary>
        /// Constructor that creates an instance of the <see cref="TextTemplateConsoleBase" /> class
        /// and initializes dependencies.
        /// </summary>
        /// <param name="logger">
        /// A reference to a logger object that is used for logging messages.
        /// </param>
        /// <param name="consoleReader">
        /// A reference to a console reader object for reading text from the <see cref="Console" />.
        /// </param>
        /// <param name="messageWriter">
        /// A reference to a console writer object that is used for writing messages to the
        /// <see cref="Console" />.
        /// </param>
        /// <param name="fileAndDirectoryService">
        /// A reference to a path service object used for performing file I/O functions.
        /// </param>
        /// <param name="pathValidater">
        /// A reference to a path validater object that is used for validating file and directory
        /// paths.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal TextTemplateConsoleBase(
            ILogger logger,
            IConsoleReader consoleReader,
            IMessageWriter messageWriter,
            IFileAndDirectoryService fileAndDirectoryService,
            IPathValidater pathValidater)
        {
            if (logger is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TextTemplateConsoleBase), nameof(ILogger));
                throw new ArgumentNullException(nameof(logger), message);
            }

            if (consoleReader is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TextTemplateConsoleBase), nameof(IConsoleReader));
                throw new ArgumentNullException(nameof(consoleReader), message);
            }

            if (messageWriter is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TextTemplateConsoleBase), nameof(IMessageWriter));
                throw new ArgumentNullException(nameof(messageWriter), message);
            }

            if (fileAndDirectoryService is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TextTemplateConsoleBase), nameof(IFileAndDirectoryService));
                throw new ArgumentNullException(nameof(fileAndDirectoryService), message);
            }

            if (pathValidater is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TextTemplateConsoleBase), nameof(IPathValidater));
                throw new ArgumentNullException(nameof(pathValidater), message);
            }

            _logger = logger;
            _consoleReader = consoleReader;
            _messageWriter = messageWriter;
            _fileAndDirectoryService = fileAndDirectoryService;
            _pathValidater = pathValidater;
            OutputDirectory = string.Empty;

            try
            {
                SolutionDirectory = _fileAndDirectoryService.GetSolutionDirectory();
            }
            catch (Exception ex)
            {
                _logger.Log(LogEntryType.Setup, MsgErrorWhenLocatingSolutionDirectory, ex.Message);
                SolutionDirectory = string.Empty;
            }
        }

        /// <summary>
        /// Gets the current output directory where the generated text will be written.
        /// </summary>
        /// <remarks>
        /// An empty string will be returned if no directory has been set.
        /// </remarks>
        public string OutputDirectory { get; private set; }

        /// <summary>
        /// Gets the directory where the solution file is located for the current code project that
        /// is using this class library.
        /// </summary>
        public string SolutionDirectory { get; private set; }

        /// <summary>
        /// Clears the contents of the directory that is pointed to by the
        /// <see cref="OutputDirectory" /> property.
        /// </summary>
        /// <remarks>
        /// This doesn't delete the directory, only the files and folders contained within the
        /// directory.
        /// </remarks>
        public void ClearOutputDirectory()
        {
            if (!string.IsNullOrWhiteSpace(OutputDirectory))
            {
                _messageWriter.WriteLine(MsgClearTheOutputDirectory, OutputDirectory);
                string response = ShowContinuationPrompt(MsgYesNoPrompt);

                if (response.ToUpper() == "Y")
                {
                    try
                    {
                        _fileAndDirectoryService.ClearDirectory(OutputDirectory);
                        _logger.Log(LogEntryType.Reset, MsgOutputDirectoryCleared);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogEntryType.Reset, MsgErrorWhenClearingOutputDirectory, ex.Message);
                    }
                }

                _logger.WriteLogEntries();
            }
        }

        /// <summary>
        /// Load the given text template file into memory so that it can be processed.
        /// </summary>
        /// <param name="filePath">
        /// The file path (relative or absolute) of the text template file that is to be loaded.
        /// </param>
        public new void LoadTemplate(string filePath)
        {
            try
            {
                string fullFilePath = _fileAndDirectoryService.GetFullPath(filePath, SolutionDirectory, true);
                string templateFilePath = _pathValidater.ValidateFullPath(fullFilePath, true, true);
                base.LoadTemplate(templateFilePath);
            }
            catch (Exception ex)
            {
                _logger.Log(LogEntryType.Loading, MsgUnableToLoadTemplateFile, ex.Message);
                ResetAll();
            }

            _logger.WriteLogEntries();
        }

        /// <summary>
        /// Sets the output directory path to the given value.
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path (relative or absolute) of the directory where the generated text
        /// files are to be written.
        /// </param>
        public void SetOutputDirectory(string directoryPath)
        {
            try
            {
                _pathValidater.ValidatePath(directoryPath);
                OutputDirectory = _fileAndDirectoryService.CreateDirectory(directoryPath, SolutionDirectory);
            }
            catch (Exception ex)
            {
                _logger.Log(LogEntryType.Setup, MsgErrorWhenCreatingOutputDirectory, ex.Message);
                OutputDirectory = string.Empty;
            }

            _logger.WriteLogEntries();
        }

        /// <summary>
        /// Displays a continuation prompt on the <see cref="Console" />.
        /// </summary>
        /// <param name="message">
        /// An optional message to be displayed on the <see cref="Console" />.
        /// <para>
        /// "Press [ENTER] to continue..." will be displayed if the parameter is omitted.
        /// </para>
        /// </param>
        /// <returns>
        /// The text string, if any, that the user types in the <see cref="Console" /> before
        /// hitting the ENTER key.
        /// <para> An empty string will be returned if the user doesn't type anything. </para>
        /// </returns>
        public string ShowContinuationPrompt(string message = MsgContinuationPrompt)
        {
            _logger.WriteLogEntries();
            _messageWriter.WriteLine("\n" + message + "\n");
            string userResponse;

            try
            {
                userResponse = _consoleReader.ReadLine();
            }
            catch (Exception ex)
            {
                _logger.Log(LogEntryType.Setup, MsgErrorWhileReadingUserResponse, ex.Message);
                return string.Empty;
            }

            return userResponse ?? string.Empty;
        }

        /// <summary>
        /// Writes the contents of the generated text buffer to the given output file.
        /// </summary>
        /// <param name="fileName">
        /// The file name of the output file that is to be written to.
        /// </param>
        /// <param name="resetGeneratedText">
        /// An optional boolean value that indicates whether or not the generated text buffer should
        /// be cleared after the output file has been successfully written to.
        /// <para> The default is <see langword="true" /> (clear the generated text buffer). </para>
        /// </param>
        public new void WriteGeneratedTextToFile(string fileName, bool resetGeneratedText = true)
        {
            string filePath;

            if (string.IsNullOrWhiteSpace(OutputDirectory))
            {
                _logger.Log(LogEntryType.Writing, MsgOutputDirectoryNotSet);
            }
            else
            {
                try
                {
                    filePath = string.IsNullOrWhiteSpace(fileName)
                        ? OutputDirectory
                        : _fileAndDirectoryService.CombineDirectoryAndFileName(OutputDirectory, fileName);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogEntryType.Writing, MsgErrorWhileConstructingFilePath, ex.Message);
                    filePath = string.Empty;
                }

                base.WriteGeneratedTextToFile(filePath, resetGeneratedText);
            }

            _logger.WriteLogEntries();
        }
    }
}