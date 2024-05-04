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
        public string OutputDirectory { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the directory where the solution file is located for the current code project that
        /// is using this class library.
        /// </summary>
        public string SolutionDirectory { get; private set; } = string.Empty;

        private IConsoleReader ConsoleReader { get; init; }

        private IFileAndDirectoryService FileAndDirectoryService { get; init; }

        private IMessageWriter MessageWriter { get; init; }

        private IPathValidater PathValidater { get; init; }

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
            if (string.IsNullOrWhiteSpace(OutputDirectory) is false)
            {
                MessageWriter.WriteLine(MsgClearTheOutputDirectory, OutputDirectory);
                string response = ShowContinuationPrompt(MsgYesNoPrompt);

                // The following lines assume that the log entry type has already been set to "User"
                // by the ShowContinuationPrompt method.
                if (response.ToUpper() == "Y")
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
        /// Load the given text template file into memory so that it can be processed.
        /// </summary>
        /// <param name="filePath">
        /// The file path (relative or absolute) of the text template file that is to be loaded.
        /// </param>
        public new void LoadTemplate(string filePath)
        {
            Logger.SetLogEntryType(LogEntryType.Loading);

            try
            {
                string fullFilePath = FileAndDirectoryService.GetFullPath(filePath, SolutionDirectory, true);
                string templateFilePath = PathValidater.ValidateFullPath(fullFilePath, true, true);
                base.LoadTemplate(templateFilePath);
            }
            catch (Exception ex)
            {
                Logger.Log(MsgUnableToLoadTemplateFile,
                           ex.Message);
                ResetAll();
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
        public void SetOutputDirectory(string directoryPath)
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
                Logger.Log(MsgErrorWhileReadingUserResponse,
                           ex.Message);
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
            Logger.SetLogEntryType(LogEntryType.Writing);

            if (string.IsNullOrWhiteSpace(OutputDirectory))
            {
                Logger.Log(MsgOutputDirectoryNotSet);
            }
            else
            {
                filePath = GetOutputFilePath(fileName);
                base.WriteGeneratedTextToFile(filePath, resetGeneratedText);
            }

            Logger.WriteLogEntries();
        }

        private string GetOutputFilePath(string fileName)
        {
            try
            {
                return string.IsNullOrWhiteSpace(fileName)
                    ? OutputDirectory
                    : FileAndDirectoryService.CombineDirectoryAndFileName(OutputDirectory, fileName);
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