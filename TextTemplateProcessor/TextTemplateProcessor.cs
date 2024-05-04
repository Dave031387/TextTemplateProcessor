namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="TextTemplateProcessor" /> class is used for loading, parsing, and processing
    /// a text template file to generate a new text file.
    /// </summary>
    public class TextTemplateProcessor : ITextTemplateProcessor
    {
        internal readonly Dictionary<string, ControlItem> _controlDictionary = new();
        internal readonly List<string> _generatedText = new();
        internal readonly Dictionary<string, List<TextItem>> _segmentDictionary = new();

        /// <summary>
        /// Default constructor that creates an instance of the <see cref="TextTemplateProcessor" />
        /// class.
        /// </summary>
        public TextTemplateProcessor() : this(ServiceLocater.Current.Get<ILogger>(),
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
        /// Constructor that creates an instance of the <see cref="TextTemplateProcessor" /> class
        /// and then sets the text template file path to the given value.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path of the text template file to be processed.
        /// </param>
        public TextTemplateProcessor(string templateFilePath) : this(templateFilePath,
                                                                     ServiceLocater.Current.Get<ILogger>(),
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
        /// Constructor that creates an instance of the <see cref="TextTemplateProcessor" /> class
        /// and initializes dependencies.
        /// </summary>
        /// <param name="logger">
        /// A reference to a logger object used for logging messages.
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
        internal TextTemplateProcessor(ILogger logger,
                                       IDefaultSegmentNameGenerator defaultSegmentNameGenerator,
                                       IIndentProcessor indentProcessor,
                                       ILocater locater,
                                       ITemplateLoader templateLoader,
                                       ITextReader textReader,
                                       ITextWriter textWriter,
                                       ITokenProcessor tokenProcessor)
        {
            Utility.NullDependencyCheck(logger,
                                        ClassNames.TextTemplateProcessorClass,
                                        ServiceNames.LoggerService,
                                        ServiceParameterNames.LoggerParameter);

            Utility.NullDependencyCheck(defaultSegmentNameGenerator,
                                        ClassNames.TextTemplateProcessorClass,
                                        ServiceNames.DefaultSegmentNameGeneratorService,
                                        ServiceParameterNames.DefaultSegmentNameGeneratorParameter);

            Utility.NullDependencyCheck(indentProcessor,
                                        ClassNames.TextTemplateProcessorClass,
                                        ServiceNames.IndentProcessorService,
                                        ServiceParameterNames.IndentProcessorParameter);

            Utility.NullDependencyCheck(locater,
                                        ClassNames.TextTemplateProcessorClass,
                                        ServiceNames.LocaterService,
                                        ServiceParameterNames.LocaterParameter);

            Utility.NullDependencyCheck(templateLoader,
                                        ClassNames.TextTemplateProcessorClass,
                                        ServiceNames.TemplateLoaderService,
                                        ServiceParameterNames.TemplateLoaderParameter);

            Utility.NullDependencyCheck(textReader,
                                        ClassNames.TextTemplateProcessorClass,
                                        ServiceNames.TextReaderService,
                                        ServiceParameterNames.TextReaderParameter);

            Utility.NullDependencyCheck(textWriter,
                                        ClassNames.TextTemplateProcessorClass,
                                        ServiceNames.TextWriterService,
                                        ServiceParameterNames.TextWriterParameter);

            Utility.NullDependencyCheck(tokenProcessor,
                                        ClassNames.TextTemplateProcessorClass,
                                        ServiceNames.TokenProcessorService,
                                        ServiceParameterNames.TokenProcessorParameter);

            Logger = logger;
            DefaultSegmentNameGenerator = defaultSegmentNameGenerator;
            IndentProcessor = indentProcessor;
            Locater = locater;
            TemplateLoader = templateLoader;
            TextReader = textReader;
            TextWriter = textWriter;
            TokenProcessor = tokenProcessor;
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="TextTemplateProcessor" /> class,
        /// initializes dependencies, and sets the template file path.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path of the text template file to be processed.
        /// </param>
        /// <param name="logger">
        /// A reference to a logger object used for logging messages.
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
        internal TextTemplateProcessor(string templateFilePath,
                                       ILogger logger,
                                       IDefaultSegmentNameGenerator defaultSegmentNameGenerator,
                                       IIndentProcessor indentProcessor,
                                       ILocater locater,
                                       ITemplateLoader templateLoader,
                                       ITextReader textReader,
                                       ITextWriter textWriter,
                                       ITokenProcessor tokenProcessor) : this(logger,
                                                                              defaultSegmentNameGenerator,
                                                                              indentProcessor,
                                                                              locater,
                                                                              templateLoader,
                                                                              textReader,
                                                                              textWriter,
                                                                              tokenProcessor)
        {
            Logger.SetLogEntryType(LogEntryType.Setup);
            TextReader.SetFilePath(templateFilePath);
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
        /// the collection returned from this method will have absolutely no effect on the actual
        /// generated text buffer that is maintained by the <see cref="TextTemplateProcessor" />
        /// class object.
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

        internal IDefaultSegmentNameGenerator DefaultSegmentNameGenerator { get; init; }

        internal IIndentProcessor IndentProcessor { get; init; }

        internal ILocater Locater { get; init; }

        internal ILogger Logger { get; init; }

        internal ITemplateLoader TemplateLoader { get; init; }

        internal ITextReader TextReader { get; init; }

        internal ITextWriter TextWriter { get; init; }

        internal ITokenProcessor TokenProcessor { get; init; }

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
            CurrentSegment = segmentName is null ? string.Empty : segmentName;
            LineNumber = 0;

            if (SegmentCanBeGenerated(CurrentSegment))
            {
                Logger.Log(MsgProcessingSegment,
                           CurrentSegment);

                ControlItem controlItem = _controlDictionary[CurrentSegment];

                if (tokenValues is not null)
                {
                    TokenProcessor.LoadTokenValues(tokenValues);
                }

                if (controlItem.ShouldGeneratePadSegment)
                {
                    string saveSegmentName = CurrentSegment;
                    IndentProcessor.SaveCurrentState();
                    GenerateSegment(controlItem.PadSegment);
                    IndentProcessor.RestoreCurrentState();
                    CurrentSegment = saveSegmentName;
                    LineNumber = 0;
                }

                if (controlItem.TabSize > 0)
                {
                    SetTabSize(controlItem.TabSize);
                    Logger.SetLogEntryType(LogEntryType.Generating);
                }

                foreach (TextItem textItem in _segmentDictionary[CurrentSegment])
                {
                    LineNumber++;
                    GenerateTextLine(controlItem, textItem);
                }

                IsOutputFileWritten = false;
            }
        }

        /// <summary>
        /// Loads a text template file into memory to be processed.
        /// </summary>
        /// <remarks>
        /// The file path of the text template file must have previously been loaded either via the
        /// constructor that takes a file path as an argument, or via the
        /// <see cref="SetTemplateFilePath(string)" /> method.
        /// </remarks>
        public void LoadTemplate()
        {
            Logger.SetLogEntryType(LogEntryType.Loading);

            if (IsTemplateLoaded)
            {
                Logger.Log(MsgAttemptToLoadMoreThanOnce,
                           TextReader.FileName);
            }
            else if (IsValidTemplateFilePath())
            {
                LoadTemplateLines();
            }
            else
            {
                Logger.Log(MsgTemplateFilePathNotSet);
            }
        }

        /// <summary>
        /// Sets the template file path to the given value and then loads the template file into
        /// memory to be processed.
        /// </summary>
        /// <param name="filePath">
        /// The file path of the text template file that is to be processed.
        /// </param>
        public void LoadTemplate(string filePath)
        {
            Logger.SetLogEntryType(LogEntryType.Loading);

            string lastFileName = TemplateFileName;
            string lastFilePath = TemplateFilePath;

            if (IsValidTemplateFilePath(filePath))
            {
                if (IsTemplateLoaded && TemplateFilePath == lastFilePath)
                {
                    Logger.Log(MsgAttemptToLoadMoreThanOnce,
                               lastFileName);
                }
                else
                {
                    if ((IsOutputFileWritten || string.IsNullOrEmpty(lastFilePath)) is false)
                    {
                        Logger.Log(MsgNextLoadRequestBeforeFirstIsWritten,
                                   TextReader.FileName,
                                   lastFileName);
                    }

                    LoadTemplateLines();
                }
            }
            else
            {
                IsTemplateLoaded = false;
            }
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
            _generatedText.Clear();
            Locater.Reset();
            _segmentDictionary.Clear();
            _controlDictionary.Clear();
            IsTemplateLoaded = false;
            IsOutputFileWritten = false;
            DefaultSegmentNameGenerator.Reset();
            IndentProcessor.Reset();
            TokenProcessor.ClearTokens();

            if (shouldDisplayMessage)
            {
                Logger.SetLogEntryType(LogEntryType.Reset);
                Logger.Log(MsgTemplateHasBeenReset,
                           TextReader.FileName);
            }
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
            Logger.SetLogEntryType(LogEntryType.Reset);
            _generatedText.Clear();
            Locater.Reset();
            IndentProcessor.Reset();

            foreach (string segmentName in _controlDictionary.Keys)
            {
                _controlDictionary[segmentName].IsFirstTime = true;
            }

            if (shouldDisplayMessage)
            {
                Logger.Log(MsgGeneratedTextHasBeenReset,
                           TextReader.FileName);
            }
        }

        /// <summary>
        /// Resets the current line number of the given segment to zero and resets the "is first
        /// time" control flag for the segment to <see langword="true" />.
        /// </summary>
        /// <param name="segmentName">
        /// The name of the segment to be reset.
        /// </param>
        /// <remarks>
        /// The <see cref="CurrentSegment" /> property will be set to the given segment name.
        /// </remarks>
        public void ResetSegment(string segmentName)
        {
            Logger.SetLogEntryType(LogEntryType.Reset);
            CurrentSegment = segmentName is null ? string.Empty : segmentName;
            LineNumber = 0;

            if (_controlDictionary.ContainsKey(CurrentSegment))
            {
                _controlDictionary[CurrentSegment].IsFirstTime = true;
                Logger.Log(MsgSegmentHasBeenReset,
                           CurrentSegment);
            }
            else
            {
                Logger.Log(MsgUnableToResetSegment,
                           CurrentSegment);
            }
        }

        /// <summary>
        /// Sets the tab size to be used when generating text lines.
        /// </summary>
        /// <param name="tabSize">
        /// The new tab size value.
        /// </param>
        public void SetTabSize(int tabSize)
        {
            Logger.SetLogEntryType(LogEntryType.Setup);
            IndentProcessor.SetTabSize(tabSize);
        }

        /// <summary>
        /// Sets the template file path to the given value.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path of an existing text template file.
        /// </param>
        public void SetTemplateFilePath(string templateFilePath)
        {
            Logger.SetLogEntryType(LogEntryType.Setup);
            TextReader.SetFilePath(templateFilePath);
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
            Logger.SetLogEntryType(LogEntryType.Setup);
            return TokenProcessor.SetTokenDelimiters(tokenStart, tokenEnd, tokenEscapeChar);
        }

        /// <summary>
        /// Writes the generated text buffer to the given output file path and optionally clears the
        /// buffer.
        /// </summary>
        /// <param name="filePath">
        /// The file path where the generated text buffer is to be written.
        /// </param>
        /// <param name="resetGeneratedText">
        /// An optional boolean value indicating whether or not the generated text buffer should be
        /// cleared after it is written.
        /// <para> The default is <see langword="true" /> (the buffer will be cleared). </para>
        /// </param>
        /// <remarks>
        /// The output file will be created if it doesn't already exist. Otherwise, the existing
        /// file will be overwritten. However, nothing will be written if the generated text buffer
        /// is empty.
        /// </remarks>
        public void WriteGeneratedTextToFile(string filePath, bool resetGeneratedText = true)
        {
            Logger.SetLogEntryType(LogEntryType.Writing);

            if (TextWriter.WriteTextFile(filePath, _generatedText))
            {
                if (resetGeneratedText)
                {
                    ResetGeneratedText();
                }

                IsOutputFileWritten = true;
            }
        }

        private static bool IsEmptyTemplateFile(IEnumerable<string> textLines)
            => textLines.Any() is false || (textLines.Count() == 1 && string.IsNullOrWhiteSpace(textLines.FirstOrDefault()));

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

        private bool IsValidSegmentWithTextLines(string segmentName)
        {
            bool result = false;

            if (_controlDictionary.ContainsKey(segmentName))
            {
                if (_segmentDictionary.ContainsKey(segmentName)
                    && _segmentDictionary[segmentName].Any())
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

        private bool IsValidTemplateFilePath() => string.IsNullOrWhiteSpace(TextReader.FullFilePath) is false;

        private bool IsValidTemplateFilePath(string filePath)
        {
            TextReader.SetFilePath(filePath);
            return IsValidTemplateFilePath();
        }

        private void LoadTemplateLines()
        {
            List<string> templateLines = TextReader.ReadTextFile().ToList();

            ResetAll(false);

            if (IsEmptyTemplateFile(templateLines))
            {
                Logger.Log(MsgTemplateFileIsEmpty,
                           TemplateFilePath);
                IsTemplateLoaded = false;
                IsOutputFileWritten = false;
            }
            else
            {
                Logger.Log(MsgLoadingTemplateFile,
                           TemplateFileName);
                TemplateLoader.LoadTemplate(templateLines, _segmentDictionary, _controlDictionary);
                IsTemplateLoaded = true;
                IsOutputFileWritten = false;
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
    }
}