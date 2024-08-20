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
        private const string DefaultFileNamePrefix = "File_";
        private const string DefaultFileNameSuffix = ".txt";
        private int _outputFileNumber = 0;

        /// <summary>
        /// Default constructor that creates an instance of the
        /// <see cref="TextTemplateConsoleBase" /> class.
        /// </summary>
        public TextTemplateConsoleBase() : this(ServiceLocater.Current.Get<ILogger>(),
                                                ServiceLocater.Current.Get<IConsoleReader>(),
                                                ServiceLocater.Current.Get<IMessageWriter>(),
                                                ServiceLocater.Current.Get<IFileAndDirectoryService>(),
                                                ServiceLocater.Current.Get<IPathValidater>(),
                                                ServiceLocater.Current.Get<IDefaultSegmentNameGenerator>(),
                                                ServiceLocater.Current.Get<IIndentProcessor>(),
                                                ServiceLocater.Current.Get<ILocater>(),
                                                ServiceLocater.Current.Get<ITemplateLoader>(),
                                                ServiceLocater.Current.Get<ITextReader>(),
                                                ServiceLocater.Current.Get<ITextWriter>(),
                                                ServiceLocater.Current.Get<ITokenProcessor>())
        {
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="TextTemplateConsoleBase" /> class
        /// and initializes the template file path to the given value.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path (relative or absolute) of the text template file.
        /// </param>
        public TextTemplateConsoleBase(string templateFilePath) : this(templateFilePath,
                                                                       ServiceLocater.Current.Get<ILogger>(),
                                                                       ServiceLocater.Current.Get<IConsoleReader>(),
                                                                       ServiceLocater.Current.Get<IMessageWriter>(),
                                                                       ServiceLocater.Current.Get<IFileAndDirectoryService>(),
                                                                       ServiceLocater.Current.Get<IPathValidater>(),
                                                                       ServiceLocater.Current.Get<IDefaultSegmentNameGenerator>(),
                                                                       ServiceLocater.Current.Get<IIndentProcessor>(),
                                                                       ServiceLocater.Current.Get<ILocater>(),
                                                                       ServiceLocater.Current.Get<ITemplateLoader>(),
                                                                       ServiceLocater.Current.Get<ITextReader>(),
                                                                       ServiceLocater.Current.Get<ITextWriter>(),
                                                                       ServiceLocater.Current.Get<ITokenProcessor>())
        {
        }

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
        /// <param name="defaultSegmentNameGenerator">
        /// A reference to a default segment name generator object.
        /// </param>
        /// <param name="indentProcessor">
        /// A reference to an indent processor object used for managing indentation of the text that
        /// is generated from a text template file.
        /// </param>
        /// <param name="locater">
        /// A reference to a locater object used for keeping track of the current location being
        /// processed in a text template file.
        /// </param>
        /// <param name="templateLoader">
        /// A reference to a template loader object used for parsing and loading a text template
        /// file so that it can be processed.
        /// </param>
        /// <param name="textReader">
        /// A reference to a text reader object used for reading text files.
        /// </param>
        /// <param name="textWriter">
        /// A reference to a text writer object used for writing text files.
        /// </param>
        /// <param name="tokenProcessor">
        /// A reference to a token processor object used for parsing, extracting, and managing
        /// tokens in a text template file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal TextTemplateConsoleBase(ILogger logger,
                                         IConsoleReader consoleReader,
                                         IMessageWriter messageWriter,
                                         IFileAndDirectoryService fileAndDirectoryService,
                                         IPathValidater pathValidater,
                                         IDefaultSegmentNameGenerator defaultSegmentNameGenerator,
                                         IIndentProcessor indentProcessor,
                                         ILocater locater,
                                         ITemplateLoader templateLoader,
                                         ITextReader textReader,
                                         ITextWriter textWriter,
                                         ITokenProcessor tokenProcessor) : base(logger,
                                                                                defaultSegmentNameGenerator,
                                                                                indentProcessor,
                                                                                locater,
                                                                                templateLoader,
                                                                                textReader,
                                                                                textWriter,
                                                                                tokenProcessor)
        {
            Utility.NullDependencyCheck(consoleReader,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.ConsoleReaderService,
                                        ServiceParameterNames.ConsoleReaderParameter);

            Utility.NullDependencyCheck(messageWriter,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.MessageWriterService,
                                        ServiceParameterNames.MessageWriterParameter);

            Utility.NullDependencyCheck(fileAndDirectoryService,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.FileAndDirectoryService,
                                        ServiceParameterNames.FileAndDirectoryServiceParameter);

            Utility.NullDependencyCheck(pathValidater,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.PathValidaterService,
                                        ServiceParameterNames.PathValidaterParameter);

            ConsoleReader = consoleReader;
            MessageWriter = messageWriter;
            FileAndDirectoryService = fileAndDirectoryService;
            PathValidater = pathValidater;
            GetSolutionDirectoryPath();
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="TextTemplateConsoleBase" /> class
        /// and initializes dependencies.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path (relative or absolute) of the text template file.
        /// </param>
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
        /// <param name="defaultSegmentNameGenerator">
        /// A reference to a default segment name generator object.
        /// </param>
        /// <param name="indentProcessor">
        /// A reference to an indent processor object used for managing indentation of the text that
        /// is generated from a text template file.
        /// </param>
        /// <param name="locater">
        /// A reference to a locater object used for keeping track of the current location being
        /// processed in a text template file.
        /// </param>
        /// <param name="templateLoader">
        /// A reference to a template loader object used for parsing and loading a text template
        /// file so that it can be processed.
        /// </param>
        /// <param name="textReader">
        /// A reference to a text reader object used for reading text files.
        /// </param>
        /// <param name="textWriter">
        /// A reference to a text writer object used for writing text files.
        /// </param>
        /// <param name="tokenProcessor">
        /// A reference to a token processor object used for parsing, extracting, and managing
        /// tokens in a text template file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal TextTemplateConsoleBase(string templateFilePath,
                                         ILogger logger,
                                         IConsoleReader consoleReader,
                                         IMessageWriter messageWriter,
                                         IFileAndDirectoryService fileAndDirectoryService,
                                         IPathValidater pathValidater,
                                         IDefaultSegmentNameGenerator defaultSegmentNameGenerator,
                                         IIndentProcessor indentProcessor,
                                         ILocater locater,
                                         ITemplateLoader templateLoader,
                                         ITextReader textReader,
                                         ITextWriter textWriter,
                                         ITokenProcessor tokenProcessor) : base(logger,
                                                                                defaultSegmentNameGenerator,
                                                                                indentProcessor,
                                                                                locater,
                                                                                templateLoader,
                                                                                textReader,
                                                                                textWriter,
                                                                                tokenProcessor)
        {
            Utility.NullDependencyCheck(consoleReader,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.ConsoleReaderService,
                                        ServiceParameterNames.ConsoleReaderParameter);

            Utility.NullDependencyCheck(messageWriter,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.MessageWriterService,
                                        ServiceParameterNames.MessageWriterParameter);

            Utility.NullDependencyCheck(fileAndDirectoryService,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.FileAndDirectoryService,
                                        ServiceParameterNames.FileAndDirectoryServiceParameter);

            Utility.NullDependencyCheck(pathValidater,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.PathValidaterService,
                                        ServiceParameterNames.PathValidaterParameter);

            ConsoleReader = consoleReader;
            MessageWriter = messageWriter;
            FileAndDirectoryService = fileAndDirectoryService;
            PathValidater = pathValidater;
            GetSolutionDirectoryPath();
            LoadTemplate(templateFilePath);
        }

        /// <summary>
        /// Gets the current output directory where the generated text will be written.
        /// </summary>
        /// <remarks>
        /// An empty string will be returned if no directory has been set.
        /// </remarks>
        public string OutputDirectory { get; internal set; } = string.Empty;

        /// <summary>
        /// Gets the directory where the solution file is located for the current code project that
        /// is using this class library.
        /// </summary>
        public string SolutionDirectory { get; private set; } = string.Empty;

        private IConsoleReader ConsoleReader { get; init; }

        private IFileAndDirectoryService FileAndDirectoryService { get; init; }

        private IMessageWriter MessageWriter { get; init; }

        private string NextDefaultFileName => $"{DefaultFileNamePrefix}{++_outputFileNumber:D4}{DefaultFileNameSuffix}";

        private IPathValidater PathValidater { get; init; }

        /// <summary>
        /// Clears the contents of the directory that is pointed to by the
        /// <see cref="OutputDirectory" /> property.
        /// </summary>
        /// <remarks>
        /// This doesn't delete the directory, only the files and folders contained within the
        /// directory.
        /// </remarks>
        public virtual void ClearOutputDirectory()
        {
            if (string.IsNullOrWhiteSpace(OutputDirectory) is false)
            {
                MessageWriter.WriteLine(MsgClearTheOutputDirectory, OutputDirectory);
                string response = PromptUserForInput(MsgYesNoPrompt);

                // The following lines assume that the log entry type has already been set to "User"
                // by the ShowContinuationPrompt method.
                if (response.ToUpper() is "Y" or "YES")
                {
                    try
                    {
                        FileAndDirectoryService.ClearDirectory(OutputDirectory);
                        Logger.Log(MsgOutputDirectoryCleared);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(MsgErrorWhenClearingOutputDirectory,
                                   ex.Message);
                    }
                }

                Logger.WriteLogEntries();
            }
        }

        /// <summary>
        /// Generates the text lines for the given segment in the text template file.
        /// </summary>
        /// <param name="segmentName">
        /// The name of the segment to be generated.
        /// </param>
        /// <param name="tokenValues">
        /// An optional dictionary of key/value pairs where the key is a token name and the value is
        /// the substitution value for that token.
        /// </param>
        /// <remarks>
        /// Each text line will indented according to the indent controls and all tokens will be
        /// replaced with their respective substitution values.
        /// </remarks>
        public override void GenerateSegment(string segmentName, Dictionary<string, string>? tokenValues = null)
        {
            try
            {
                base.GenerateSegment(segmentName, tokenValues);
            }
            catch (Exception ex)
            {
                Logger.SetLogEntryType(LogEntryType.Generating);
                Logger.Log(MsgUnableToGenerateSegment, CurrentSegment, ex.Message);
            }

            Logger.WriteLogEntries();
        }

        /// <summary>
        /// Loads a text template file into memory to be processed.
        /// </summary>
        /// <remarks>
        /// The file path of the text template file must have previously been loaded either via the
        /// constructor that takes a file path as an argument, or via the
        /// <see cref="SetTemplateFilePath(string)" /> method.
        /// </remarks>
        public override void LoadTemplate()
        {
            try
            {
                base.LoadTemplate();
            }
            catch (Exception ex)
            {
                Logger.SetLogEntryType(LogEntryType.Loading);
                Logger.Log(MsgUnableToLoadTemplateFile, TemplateFilePath, ex.Message);
                ResetAll();
            }

            Logger.WriteLogEntries();
        }

        /// <summary>
        /// Load the given text template file into memory so that it can be processed.
        /// </summary>
        /// <param name="filePath">
        /// The file path (relative or absolute) of the text template file that is to be loaded.
        /// </param>
        public override void LoadTemplate(string filePath)
        {
            try
            {
                Logger.SetLogEntryType(LogEntryType.Loading);
                string fullFilePath = FileAndDirectoryService.GetFullPath(filePath, SolutionDirectory, true);
                string templateFilePath = PathValidater.ValidateFullPath(fullFilePath, true, true);
                base.LoadTemplate(templateFilePath);
            }
            catch (Exception ex)
            {
                Logger.SetLogEntryType(LogEntryType.Loading);
                Logger.Log(MsgUnableToLoadTemplateFile, filePath, ex.Message);
                ResetAll();
            }

            Logger.WriteLogEntries();
        }

        /// <summary>
        /// Displays a prompt on the <see cref="Console" /> and returns the user's response.
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
        /// <para>
        /// An empty string will be returned if the user doesn't type anything or if an error occurs
        /// while reading the user's response.
        /// </para>
        /// </returns>
        public virtual string PromptUserForInput(string message = MsgContinuationPrompt)
        {
            Logger.WriteLogEntries();
            Logger.SetLogEntryType(LogEntryType.User);
            MessageWriter.WriteLine("\n" + message + "\n");
            string userResponse = string.Empty;

            try
            {
                userResponse = ConsoleReader.ReadLine();
            }
            catch (Exception ex)
            {
                Logger.Log(MsgUnableToGetUserResponse,
                           ex.Message);
            }

            return userResponse ?? string.Empty;
        }

        /// <summary>
        /// Resets the text template processor back to its initial state
        /// </summary>
        /// <param name="shouldDisplayMessage">
        /// An optional boolean value indicating whether or not a message should be logged after the
        /// state has been reset.
        /// <para> The default is <see langword="true" /> (message will be logged). </para>
        /// </param>
        public override void ResetAll(bool shouldDisplayMessage = true)
        {
            try
            {
                base.ResetAll(shouldDisplayMessage);
            }
            catch (Exception ex)
            {
                Logger.SetLogEntryType(LogEntryType.Reset);
                Logger.Log(MsgUnableToResetAll, ex.Message);
            }

            Logger.WriteLogEntries();
        }

        /// <summary>
        /// Clears the generated text buffer and resets the locater and indent processor.
        /// </summary>
        /// <param name="shouldDisplayMessage">
        /// An optional boolean value indicating whether or not a message should be logged after the
        /// buffer has been cleared.
        /// <para> The default is <see langword="true" /> (message will be logged). </para>
        /// </param>
        public override void ResetGeneratedText(bool shouldDisplayMessage = true)
        {
            try
            {
                base.ResetGeneratedText(shouldDisplayMessage);
            }
            catch (Exception ex)
            {
                Logger.SetLogEntryType(LogEntryType.Reset);
                Logger.Log(MsgUnableToResetGeneratedText, ex.Message);
            }

            Logger.WriteLogEntries();
        }

        /// <summary>
        /// Resets the current line number of the given segment to zero and resets the "is first
        /// time" control flag for the segment to <see langword="true" />.
        /// </summary>
        /// <param name="segmentName">
        /// The name of the segment to be reset.
        /// </param>
        /// <remarks>
        /// The CurrentSegment property will be set to the given segment name.
        /// </remarks>
        public override void ResetSegment(string segmentName)
        {
            try
            {
                base.ResetSegment(segmentName);
            }
            catch (Exception ex)
            {
                Logger.SetLogEntryType(LogEntryType.Reset);
                Logger.Log(MsgUnableToResetSegment, segmentName, ex.Message);
            }

            Logger.WriteLogEntries();
        }

        /// <summary>
        /// Resets the token start and token end delimiters and the token escape character to their
        /// default values.
        /// </summary>
        public override void ResetTokenDelimiters()
        {
            try
            {
                base.ResetTokenDelimiters();
            }
            catch (Exception ex)
            {
                Logger.SetLogEntryType(LogEntryType.Reset);
                Logger.Log(MsgUnableToResetTokenDelimiters, ex.Message);
            }

            Logger.WriteLogEntries();
        }

        /// <summary>
        /// Sets the output directory path to the given value.
        /// </summary>
        /// <param name="directoryPath">
        /// The directory path (relative or absolute) of the directory where the generated text
        /// files are to be written.
        /// </param>
        public virtual void SetOutputDirectory(string directoryPath)
        {
            Logger.SetLogEntryType(LogEntryType.Setup);

            try
            {
                PathValidater.ValidatePath(directoryPath);
                OutputDirectory = FileAndDirectoryService.CreateDirectory(directoryPath, SolutionDirectory);
            }
            catch (Exception ex)
            {
                Logger.Log(MsgErrorWhenCreatingOutputDirectory,
                           ex.Message);
                OutputDirectory = string.Empty;
            }

            Logger.WriteLogEntries();
        }

        /// <summary>
        /// Sets the tab size to be used when generating text lines.
        /// </summary>
        /// <param name="tabSize">
        /// The new tab size value.
        /// </param>
        public override void SetTabSize(int tabSize)
        {
            try
            {
                base.SetTabSize(tabSize);
            }
            catch (Exception ex)
            {
                Logger.SetLogEntryType(LogEntryType.Setup);
                Logger.Log(MsgUnableToSetTabSize, ex.Message);
            }

            Logger.WriteLogEntries();
        }

        /// <summary>
        /// Sets the template file path to the given value.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path of an existing text template file.
        /// </param>
        public override void SetTemplateFilePath(string templateFilePath)
        {
            try
            {
                base.SetTemplateFilePath(templateFilePath);
            }
            catch (Exception ex)
            {
                Logger.SetLogEntryType(LogEntryType.Setup);
                Logger.Log(MsgUnableToSetTemplateFilePath, templateFilePath, ex.Message);
            }

            Logger.WriteLogEntries();
        }

        /// <summary>
        /// Sets the token start and token end delimiters and the token escape character to the
        /// specified values.
        /// </summary>
        /// <param name="tokenStart">
        /// The new token start delimiter string.
        /// </param>
        /// <param name="tokenEnd">
        /// The new token end delimiter string.
        /// </param>
        /// <param name="tokenEscapeChar">
        /// The new token escape character.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the delimiter values were successfully changed. Otherwise,
        /// returns <see langword="false" />.
        /// </returns>
        public override bool SetTokenDelimiters(string tokenStart, string tokenEnd, char tokenEscapeChar)
        {
            try
            {
                return base.SetTokenDelimiters(tokenStart, tokenEnd, tokenEscapeChar);
            }
            catch (Exception ex)
            {
                Logger.SetLogEntryType(LogEntryType.Setup);
                Logger.Log(MsgUnableToSetTokenDelimiters, ex.Message);
            }

            Logger.WriteLogEntries();
            return false;
        }

        /// <summary>
        /// Writes the contents of the generated text buffer to the given output file.
        /// </summary>
        /// <param name="filePath">
        /// The absolute or relative file path of the output file where the generated text will be
        /// written.
        /// </param>
        /// <param name="resetGeneratedText">
        /// An optional boolean value that indicates whether or not the generated text buffer should
        /// be cleared after the output file has been successfully written to.
        /// <para> The default is <see langword="true" /> (clear the generated text buffer). </para>
        /// </param>
        public override void WriteGeneratedTextToFile(string filePath, bool resetGeneratedText = true)
        {
            try
            {
                string outputFilePath;
                Logger.SetLogEntryType(LogEntryType.Writing);

                if (string.IsNullOrWhiteSpace(OutputDirectory))
                {
                    Logger.Log(MsgOutputDirectoryNotSet);
                }
                else
                {
                    outputFilePath = GetOutputFilePath(filePath);

                    if (string.IsNullOrEmpty(outputFilePath) is false)
                    {
                        base.WriteGeneratedTextToFile(outputFilePath, resetGeneratedText);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.SetLogEntryType(LogEntryType.Writing);
                Logger.Log(MsgUnableToWriteGeneratedTextToFile, ex.Message);
            }

            Logger.WriteLogEntries();
        }

        private string GetOutputFilePath(string filePath)
        {
            try
            {
                string outputFilePath = string.IsNullOrWhiteSpace(filePath)
                    ? NextDefaultFileName
                    : filePath;
                return FileAndDirectoryService.CombinePaths(OutputDirectory, outputFilePath);
            }
            catch (Exception ex)
            {
                Logger.Log(MsgErrorWhileConstructingFilePath,
                           ex.Message);
                return string.Empty;
            }
        }

        private void GetSolutionDirectoryPath()
        {
            Logger.SetLogEntryType(LogEntryType.Setup);

            try
            {
                SolutionDirectory = FileAndDirectoryService.GetSolutionDirectory();
            }
            catch (Exception ex)
            {
                Logger.Log(MsgErrorWhenLocatingSolutionDirectory,
                           ex.Message);
                SolutionDirectory = string.Empty;
            }
        }
    }
}