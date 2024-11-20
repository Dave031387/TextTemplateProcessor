namespace TextTemplateProcessor.Console
{
    using TextTemplateProcessor.Core;
    using TextTemplateProcessor.Interfaces;
    using System;
    using static TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="TextTemplateConsoleBase" /> class is an abstract class that is intended as a
    /// basis for building your own console app for processing text template files.
    /// </summary>
    public abstract class TextTemplateConsoleBase
    {
        internal readonly Dictionary<string, ControlItem> _controlDictionary = [];
        internal readonly List<string> _generatedText = [];
        internal readonly Dictionary<string, List<TextItem>> _segmentDictionary = [];
        private const string DefaultFileNamePrefix = "File_";
        private const string DefaultFileNameSuffix = ".txt";
        private int _outputFileNumber = 0;

        /// <summary>
        /// Create an instance of the <see cref="TextTemplateConsoleBase" /> class.
        /// </summary>
        public TextTemplateConsoleBase()
            : this(ServiceLocater.Get<IConsoleReader>(),
                   ServiceLocater.Get<IDefaultSegmentNameGenerator>(),
                   ServiceLocater.Get<IFileAndDirectoryService>(),
                   ServiceLocater.Get<IIndentProcessor>(),
                   ServiceLocater.Get<ILocater>(),
                   ServiceLocater.Get<ILogger>(),
                   ServiceLocater.Get<IMessageWriter>(),
                   ServiceLocater.Get<IPathValidater>(),
                   ServiceLocater.Get<ITemplateLoader>(),
                   ServiceLocater.Get<ITextReader>(),
                   ServiceLocater.Get<ITextWriter>(),
                   ServiceLocater.Get<ITokenProcessor>())
        {
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="TextTemplateConsoleBase" /> class
        /// and initializes dependencies. Intended primarily for unit tests.
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
        internal TextTemplateConsoleBase(IConsoleReader consoleReader,
                                         IDefaultSegmentNameGenerator defaultSegmentNameGenerator,
                                         IFileAndDirectoryService fileAndDirectoryService,
                                         IIndentProcessor indentProcessor,
                                         ILocater locater,
                                         ILogger logger,
                                         IMessageWriter messageWriter,
                                         IPathValidater pathValidater,
                                         ITemplateLoader templateLoader,
                                         ITextReader textReader,
                                         ITextWriter textWriter,
                                         ITokenProcessor tokenProcessor)
        {
            Utility.NullDependencyCheck(consoleReader,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.ConsoleReaderService,
                                        ServiceParameterNames.ConsoleReaderParameter);

            Utility.NullDependencyCheck(defaultSegmentNameGenerator,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.DefaultSegmentNameGeneratorService,
                                        ServiceParameterNames.DefaultSegmentNameGeneratorParameter);

            Utility.NullDependencyCheck(fileAndDirectoryService,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.FileAndDirectoryService,
                                        ServiceParameterNames.FileAndDirectoryServiceParameter);

            Utility.NullDependencyCheck(indentProcessor,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.IndentProcessorService,
                                        ServiceParameterNames.IndentProcessorParameter);

            Utility.NullDependencyCheck(locater,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.LocaterService,
                                        ServiceParameterNames.LocaterParameter);

            Utility.NullDependencyCheck(logger,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.LoggerService,
                                        ServiceParameterNames.LoggerParameter);

            Utility.NullDependencyCheck(messageWriter,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.MessageWriterService,
                                        ServiceParameterNames.MessageWriterParameter);

            Utility.NullDependencyCheck(pathValidater,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.PathValidaterService,
                                        ServiceParameterNames.PathValidaterParameter);

            Utility.NullDependencyCheck(templateLoader,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.TemplateLoaderService,
                                        ServiceParameterNames.TemplateLoaderParameter);

            Utility.NullDependencyCheck(textReader,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.TextReaderService,
                                        ServiceParameterNames.TextReaderParameter);

            Utility.NullDependencyCheck(textWriter,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.TextWriterService,
                                        ServiceParameterNames.TextWriterParameter);

            Utility.NullDependencyCheck(tokenProcessor,
                                        ClassNames.TextTemplateConsoleBaseClass,
                                        ServiceNames.TokenProcessorService,
                                        ServiceParameterNames.TokenProcessorParameter);

            ConsoleReader = consoleReader;
            DefaultSegmentNameGenerator = defaultSegmentNameGenerator;
            FileAndDirectoryService = fileAndDirectoryService;
            IndentProcessor = indentProcessor;
            Locater = locater;
            Logger = logger;
            MessageWriter = messageWriter;
            PathValidater = pathValidater;
            TemplateLoader = templateLoader;
            TextReader = textReader;
            TextWriter = textWriter;
            TokenProcessor = tokenProcessor;
            GetSolutionDirectoryPath();
        }

        /// <summary>
        /// Gets the current indent value.
        /// </summary>
        /// <remarks>
        /// The indent value is the number of tabs that will get inserted at the beginning of the
        /// next generated text line.
        /// </remarks>
        public int CurrentIndent => IndentProcessor.CurrentIndent;

        /// <summary>
        /// Gets the name of the current segment that is being processed in the text template file.
        /// </summary>
        public string CurrentSegment
        {
            get => Locater.CurrentSegment;
            private set => Locater.CurrentSegment = value;
        }

        /// <summary>
        /// Gets the collection of strings that make up the generated text buffer.
        /// </summary>
        /// <remarks>
        /// Note that this method returns a copy of the generated text buffer. Any changes made to
        /// the collection returned from this method will have absolutely no effect on the original
        /// generated text buffer.
        /// </remarks>
        public IEnumerable<string> GeneratedText => new List<string>(_generatedText);

        /// <summary>
        /// Gets a boolean value that is <see langword="true" /> if the generated text buffer has
        /// been written to the output file.
        /// </summary>
        public bool IsOutputFileWritten { get; internal set; }

        /// <summary>
        /// Gets a boolean value that is <see langword="true" /> if a template file has been loaded
        /// into the text template processor.
        /// </summary>
        public bool IsTemplateLoaded { get; internal set; }

        /// <summary>
        /// Gets the ordinal number of the current text line that is being processed within the
        /// current segment of the text template file.
        /// </summary>
        public int LineNumber
        {
            get => Locater.LineNumber;
            private set => Locater.LineNumber = value;
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

        /// <summary>
        /// Gets the current tab size.
        /// </summary>
        /// <remarks>
        /// The tab size is the number of space characters that make up one tab.
        /// </remarks>
        public int TabSize => IndentProcessor.TabSize;

        /// <summary>
        /// Gets the name of the text template file that is loaded into memory.
        /// </summary>
        /// <remarks>
        /// This will return an empty string if a template file hasn't yet been loaded.
        /// </remarks>
        public string TemplateFileName => TextReader.FileName;

        /// <summary>
        /// Gets the full file path of the text template file that is loaded in memory.
        /// </summary>
        /// <remarks>
        /// This will return an empty string if a template file hasn't yet been loaded.
        /// </remarks>
        public string TemplateFilePath => TextReader.FullFilePath;

        private IConsoleReader ConsoleReader { get; init; }

        private IDefaultSegmentNameGenerator DefaultSegmentNameGenerator { get; init; }

        private IFileAndDirectoryService FileAndDirectoryService { get; init; }

        private IIndentProcessor IndentProcessor { get; init; }

        private ILocater Locater { get; init; }

        private ILogger Logger { get; init; }

        private IMessageWriter MessageWriter { get; init; }

        private string NextDefaultFileName => $"{DefaultFileNamePrefix}{++_outputFileNumber:D4}{DefaultFileNameSuffix}";

        private IPathValidater PathValidater { get; init; }

        private ITemplateLoader TemplateLoader { get; init; }

        private ITextReader TextReader { get; init; }

        private ITextWriter TextWriter { get; init; }

        private ITokenProcessor TokenProcessor { get; init; }

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
                LogEntryType logEntryType = Logger.GetLogEntryType();
                Logger.SetLogEntryType(LogEntryType.Reset);
                MessageWriter.WriteLine(MsgClearTheOutputDirectory, OutputDirectory);
                string response = PromptUserForInput(MsgYesNoPrompt);

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
                Logger.SetLogEntryType(logEntryType);
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
        public void GenerateSegment(string segmentName, Dictionary<string, string>? tokenValues = null)
        {
            Logger.SetLogEntryType(LogEntryType.Generating);

            try
            {
                CurrentSegment = string.IsNullOrWhiteSpace(segmentName) ? string.Empty : segmentName;
                LineNumber = 0;

                if (SegmentCanBeGenerated(CurrentSegment))
                {
                    ControlItem controlItem = _controlDictionary[CurrentSegment];

                    LoadTokenValues(tokenValues);
                    GeneratePadSegment(controlItem);

                    Logger.Log(MsgProcessingSegment,
                               CurrentSegment);

                    AdjustTabSize(controlItem);
                    GenerateTextLines(controlItem, _segmentDictionary[CurrentSegment]);

                    IsOutputFileWritten = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MsgUnableToGenerateSegment, CurrentSegment, ex.Message);
            }

            Logger.WriteLogEntries();
        }

        /// <summary>
        /// Load the given text template file into memory so that it can be processed.
        /// </summary>
        /// <param name="filePath">
        /// The file path (relative or absolute) of the text template file that is to be loaded.
        /// </param>
        public void LoadTemplate(string filePath)
        {
            Logger.SetLogEntryType(LogEntryType.Loading);

            try
            {
                string fullFilePath = FileAndDirectoryService.GetFullPath(filePath, SolutionDirectory, true);
                string templateFilePath = PathValidater.ValidateFullPath(fullFilePath, true, true);
                string lastFileName = TemplateFileName;
                string lastFilePath = TemplateFilePath;

                if (TrySetTemplateFilePath(filePath))
                {
                    if (IsTemplateLoaded && TemplateFilePath == lastFilePath)
                    {
                        Logger.Log(MsgAttemptToLoadMoreThanOnce,
                                   lastFileName);
                    }
                    else
                    {
                        if (!IsOutputFileWritten && !string.IsNullOrEmpty(lastFilePath))
                        {
                            Logger.Log(MsgNextLoadRequestBeforeFirstIsWritten,
                                       TemplateFileName,
                                       lastFileName);
                        }

                        LoadTemplateLines();
                    }
                }
                else
                {
                    IsTemplateLoaded = false;
                    ResetAll();
                }
            }
            catch (Exception ex)
            {
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
            LogEntryType logEntryType = Logger.GetLogEntryType();
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

            Logger.SetLogEntryType(logEntryType);
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
        public void ResetAll(bool shouldDisplayMessage = true)
        {
            LogEntryType logEntryType = Logger.GetLogEntryType();
            Logger.SetLogEntryType(LogEntryType.Reset);

            try
            {
                _generatedText.Clear();
                Locater.Reset();
                _segmentDictionary.Clear();
                _controlDictionary.Clear();
                IsTemplateLoaded = false;
                IsOutputFileWritten = false;
                DefaultSegmentNameGenerator.Reset();
                IndentProcessor.Reset();
                TokenProcessor.ClearTokens();
                TokenProcessor.ResetTokenDelimiters();

                if (shouldDisplayMessage)
                {
                    Logger.Log(MsgTemplateHasBeenReset,
                               TemplateFileName);
                }
            }
            catch (Exception ex)
            {
                _generatedText.Clear();
                _segmentDictionary.Clear();
                _controlDictionary.Clear();
                IsTemplateLoaded = false;
                IsOutputFileWritten = false;
                Logger.Log(MsgUnableToResetAll, ex.Message);
            }

            Logger.WriteLogEntries();
            Logger.SetLogEntryType(logEntryType);
        }

        /// <summary>
        /// Clears the generated text buffer and resets the locater and indent processor.
        /// </summary>
        /// <param name="shouldDisplayMessage">
        /// An optional boolean value indicating whether or not a message should be logged after the
        /// buffer has been cleared.
        /// <para> The default is <see langword="true" /> (message will be logged). </para>
        /// </param>
        public void ResetGeneratedText(bool shouldDisplayMessage = true)
        {
            LogEntryType logEntryType = Logger.GetLogEntryType();
            Logger.SetLogEntryType(LogEntryType.Reset);

            try
            {
                _generatedText.Clear();
                Locater.Reset();
                IndentProcessor.Reset();
                IsOutputFileWritten = false;
                ResetAllSegments();

                if (shouldDisplayMessage)
                {
                    Logger.Log(MsgGeneratedTextHasBeenReset,
                               TemplateFileName);
                }
            }
            catch (Exception ex)
            {
                _generatedText.Clear();
                IsOutputFileWritten = false;
                ResetAllSegments();
                Logger.Log(MsgUnableToResetGeneratedText, ex.Message);
            }

            Logger.WriteLogEntries();
            Logger.SetLogEntryType(logEntryType);
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
        public void ResetSegment(string segmentName)
        {
            LogEntryType logEntryType = Logger.GetLogEntryType();
            Logger.SetLogEntryType(LogEntryType.Reset);

            try
            {
                CurrentSegment = segmentName is null ? string.Empty : segmentName;
                LineNumber = 0;

                if (_controlDictionary.TryGetValue(CurrentSegment, out ControlItem? value))
                {
                    value.IsFirstTime = true;
                    Logger.Log(MsgSegmentHasBeenReset,
                               CurrentSegment);
                }
                else
                {
                    Logger.Log(MsgUnableToResetUnknownSegment,
                               CurrentSegment);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MsgUnableToResetSegment, segmentName, ex.Message);
            }

            Logger.WriteLogEntries();
            Logger.SetLogEntryType(logEntryType);
        }

        /// <summary>
        /// Resets the token start and token end delimiters and the token escape character to their
        /// default values.
        /// </summary>
        public void ResetTokenDelimiters()
        {
            LogEntryType logEntryType = Logger.GetLogEntryType();
            Logger.SetLogEntryType(LogEntryType.Reset);
            TokenProcessor.ResetTokenDelimiters();
            Logger.WriteLogEntries();
            Logger.SetLogEntryType(logEntryType);
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
            LogEntryType logEntryType = Logger.GetLogEntryType();
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
            Logger.SetLogEntryType(logEntryType);
        }

        /// <summary>
        /// Sets the tab size to be used when generating text lines.
        /// </summary>
        /// <param name="tabSize">
        /// The new tab size value.
        /// </param>
        public void SetTabSize(int tabSize)
        {
            LogEntryType logEntryType = Logger.GetLogEntryType();
            Logger.SetLogEntryType(LogEntryType.Setup);
            IndentProcessor.SetTabSize(tabSize);
            Logger.WriteLogEntries();
            Logger.SetLogEntryType(logEntryType);
        }

        /// <summary>
        /// Sets the template file path to the given value.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path of an existing text template file.
        /// </param>
        public void SetTemplateFilePath(string templateFilePath)
        {
            LogEntryType logEntryType = Logger.GetLogEntryType();
            Logger.SetLogEntryType(LogEntryType.Setup);

            try
            {
                TextReader.SetFilePath(templateFilePath);
            }
            catch (Exception ex)
            {
                Logger.Log(MsgUnableToSetTemplateFilePath, templateFilePath, ex.Message);
            }

            Logger.WriteLogEntries();
            Logger.SetLogEntryType(logEntryType);
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
        public bool SetTokenDelimiters(string tokenStart, string tokenEnd, char tokenEscapeChar)
        {
            bool result;
            LogEntryType logEntryType = Logger.GetLogEntryType();
            Logger.SetLogEntryType(LogEntryType.Setup);
            result = TokenProcessor.SetTokenDelimiters(tokenStart, tokenEnd, tokenEscapeChar);
            Logger.WriteLogEntries();
            Logger.SetLogEntryType(logEntryType);
            return result;
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
        public void WriteGeneratedTextToFile(string filePath, bool resetGeneratedText = true)
        {
            LogEntryType logEntryType = Logger.GetLogEntryType();
            Logger.SetLogEntryType(LogEntryType.Writing);

            try
            {
                if (string.IsNullOrWhiteSpace(OutputDirectory))
                {
                    Logger.Log(MsgOutputDirectoryNotSet);
                }
                else
                {
                    WriteGeneratedText(filePath, resetGeneratedText);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MsgUnableToWriteGeneratedTextToFile, ex.Message);
            }

            Logger.WriteLogEntries();
            Logger.SetLogEntryType(logEntryType);
        }

        private static bool IsEmptyTemplateFile(IEnumerable<string> textLines)
            => !textLines.Any() || (textLines.Count() == 1 && string.IsNullOrWhiteSpace(textLines.FirstOrDefault()));

        private void AdjustTabSize(ControlItem controlItem)
        {
            if (controlItem.TabSize > 0)
            {
                SetTabSize(controlItem.TabSize);
            }
        }

        private void GeneratePadSegment(ControlItem controlItem)
        {
            if (controlItem.ShouldGeneratePadSegment)
            {
                string saveSegmentName = CurrentSegment;
                IndentProcessor.SaveCurrentState();
                GenerateSegment(controlItem.PadSegment);
                IndentProcessor.RestoreCurrentState();
                CurrentSegment = saveSegmentName;
                LineNumber = 0;
            }
        }

        private void GenerateTextLine(ControlItem controlItem, TextItem textItem)
        {
            int indent;

            if (controlItem.IsFirstTime)
            {
                indent = IndentProcessor.GetFirstTimeIndent(controlItem.FirstTimeIndent, textItem);
                controlItem.IsFirstTime = false;
            }
            else
            {
                indent = IndentProcessor.GetIndent(textItem);
            }

            string pad = new(' ', indent);
            string text = TokenProcessor.ReplaceTokens(textItem.Text);
            _generatedText.Add(pad + text);
        }

        private void GenerateTextLines(ControlItem controlItem, List<TextItem> textItems)
        {
            foreach (TextItem textItem in textItems)
            {
                LineNumber++;
                GenerateTextLine(controlItem, textItem);
            }
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

        private bool IsValidSegmentWithTextLines(string segmentName)
        {
            bool result = false;

            if (_controlDictionary.ContainsKey(segmentName))
            {
                if (_segmentDictionary.TryGetValue(segmentName, out List<TextItem>? value) && value.Count != 0)
                {
                    result = true;
                }
                else
                {
                    Logger.Log(MsgSegmentHasNoTextLines,
                               segmentName);
                }
            }
            else
            {
                Logger.Log(MsgUnknownSegmentName,
                           segmentName);
            }

            return result;
        }

        private bool IsValidTemplateFilePath() => !string.IsNullOrWhiteSpace(TemplateFilePath);

        private void LoadTemplateLines()
        {
            List<string> templateLines = TextReader.ReadTextFile().ToList();

            ResetAll(false);

            if (IsEmptyTemplateFile(templateLines))
            {
                Logger.Log(MsgTemplateFileIsEmpty,
                           TemplateFilePath);
            }
            else
            {
                Logger.Log(MsgLoadingTemplateFile,
                           TemplateFileName);
                TemplateLoader.LoadTemplate(templateLines, _segmentDictionary, _controlDictionary);
                IsTemplateLoaded = true;
            }
        }

        private void LoadTokenValues(Dictionary<string, string>? tokenValues)
        {
            if (tokenValues is not null)
            {
                TokenProcessor.LoadTokenValues(tokenValues);
            }
        }

        private void ResetAllSegments()
        {
            foreach (string segmentName in _controlDictionary.Keys)
            {
                _controlDictionary[segmentName].IsFirstTime = true;
            }
        }

        private bool SegmentCanBeGenerated(string segmentName)
        {
            bool result = false;

            if (string.IsNullOrWhiteSpace(segmentName))
            {
                Logger.Log(MsgSegmentNameIsNullOrWhitespace);
            }
            else if (IsTemplateLoaded)
            {
                result = IsValidSegmentWithTextLines(segmentName);
            }
            else
            {
                Logger.Log(MsgAttemptToGenerateSegmentBeforeItWasLoaded,
                           segmentName);
            }

            return result;
        }

        private bool TrySetTemplateFilePath(string filePath)
        {
            TextReader.SetFilePath(filePath);
            return IsValidTemplateFilePath();
        }

        private void WriteGeneratedText(string filePath, bool resetGeneratedText)
        {
            string outputFilePath = GetOutputFilePath(filePath);

            if (!string.IsNullOrEmpty(outputFilePath))
            {
                if (TextWriter.WriteTextFile(outputFilePath, _generatedText))
                {
                    if (resetGeneratedText)
                    {
                        ResetGeneratedText();
                    }

                    IsOutputFileWritten = true;
                }
            }
        }
    }
}