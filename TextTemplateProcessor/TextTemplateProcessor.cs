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
        private readonly Dictionary<string, ControlItem> _controlDictionary = new();
        private readonly IDefaultSegmentNameGenerator _defaultSegmentNameGenerator;
        private readonly List<string> _generatedText = new();
        private readonly IIndentProcessor _indentProcessor;
        private readonly ILocater _locater;
        private readonly ILogger _logger;
        private readonly Dictionary<string, List<TextItem>> _segmentDictionary = new();
        private readonly ITemplateLoader _templateLoader;
        private readonly ITextReader _textReader;
        private readonly ITextWriter _textWriter;
        private readonly ITokenProcessor _tokenProcessor;

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
        public TextTemplateProcessor(string templateFilePath) : this()
            => _textReader.SetFilePath(templateFilePath);

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

            _logger = logger;
            _defaultSegmentNameGenerator = defaultSegmentNameGenerator;
            _indentProcessor = indentProcessor;
            _locater = locater;
            _templateLoader = templateLoader;
            _textReader = textReader;
            _textWriter = textWriter;
            _tokenProcessor = tokenProcessor;
        }

        /// <summary>
        /// Gets the current indent value.
        /// </summary>
        /// <remarks>
        /// The indent value is the number of tabs that will get inserted at the beginning of the
        /// next generated text line.
        /// </remarks>
        public int CurrentIndent => _indentProcessor.CurrentIndent;

        /// <summary>
        /// Gets the name of the current segment that is being processed in the text template file.
        /// </summary>
        public string CurrentSegment
        {
            get => _locater.CurrentSegment;
            private set => _locater.CurrentSegment = value;
        }

        /// <summary>
        /// Gets the collection of strings that make up the generated text buffer.
        /// </summary>
        public IEnumerable<string> GeneratedText => _generatedText;

        /// <summary>
        /// Gets a boolean value that is <see langword="true" /> if the generated text buffer has
        /// been written to the output file.
        /// </summary>
        public bool IsOutputFileWritten { get; private set; }

        /// <summary>
        /// Gets a boolean value that is <see langword="true" /> if a template file has been loaded
        /// into the text template processor.
        /// </summary>
        public bool IsTemplateLoaded { get; private set; }

        /// <summary>
        /// Gets the ordinal number of the current text line that is being processed within the
        /// current segment of the text template file.
        /// </summary>
        public int LineNumber
        {
            get => _locater.LineNumber;
            private set => _locater.LineNumber = value;
        }

        /// <summary>
        /// Gets the current tab size.
        /// </summary>
        /// <remarks>
        /// The tab size is the number of space characters that make up one tab.
        /// </remarks>
        public int TabSize => _indentProcessor.TabSize;

        /// <summary>
        /// Gets the full file path of the text template file that is loaded in memory.
        /// </summary>
        /// <remarks>
        /// This will return an empty string if the template file hasn't yet been loaded.
        /// </remarks>
        public string TemplateFilePath => _textReader.FullFilePath;

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
            CurrentSegment = segmentName is null ? string.Empty : segmentName;
            LineNumber = 0;

            if (SegmentCanBeGenerated(CurrentSegment))
            {
                ControlItem controlItem = _controlDictionary[CurrentSegment];

                if (tokenValues is not null)
                {
                    _tokenProcessor.LoadTokenValues(tokenValues);
                }

                if (controlItem.ShouldGeneratePadSegment)
                {
                    _indentProcessor.SaveCurrentIndentLocation();
                    GenerateSegment(controlItem.PadSegment);
                    _indentProcessor.RestoreCurrentIndentLocation();
                }

                if (controlItem.TabSize > 0)
                {
                    SetTabSize(controlItem.TabSize);
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
            if (IsTemplateLoaded)
            {
                _logger.Log(LogEntryType.Loading,
                            MsgAttemptToLoadMoreThanOnce,
                            _textReader.FileName);
            }
            else if (IsValidTemplateFilePath())
            {
                ResetAll(false);
                LoadTemplateLines();
            }
            else
            {
                _logger.Log(LogEntryType.Loading,
                            MsgTemplateFilePathNotSet);
                IsTemplateLoaded = false;
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
            string lastFileName = _textReader.FileName;
            string lastFilePath = TemplateFilePath;

            if (IsValidTemplateFilePath(filePath))
            {
                if (IsTemplateLoaded && TemplateFilePath == lastFilePath)
                {
                    _logger.Log(LogEntryType.Loading,
                                MsgAttemptToLoadMoreThanOnce,
                                lastFileName);
                    return;
                }

                bool isOutputFileWritten = IsOutputFileWritten;
                ResetAll(false);

                if ((isOutputFileWritten || string.IsNullOrEmpty(lastFileName)) is false)
                {
                    _logger.Log(LogEntryType.Loading,
                                MsgNextLoadRequestBeforeFirstIsWritten,
                                _textReader.FileName,
                                lastFileName);
                }

                LoadTemplateLines();
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
            ResetGeneratedText(false);
            _segmentDictionary.Clear();
            _controlDictionary.Clear();
            IsTemplateLoaded = false;
            IsOutputFileWritten = false;
            _defaultSegmentNameGenerator.Reset();
            _tokenProcessor.ClearTokens();

            if (shouldDisplayMessage)
            {
                _logger.Log(LogEntryType.Reset,
                            MsgTemplateHasBeenReset,
                            _textReader.FileName);
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
            _generatedText.Clear();
            _locater.Reset();
            _indentProcessor.Reset();

            foreach (string segmentName in _controlDictionary.Keys)
            {
                _controlDictionary[segmentName].IsFirstTime = true;
            }

            if (shouldDisplayMessage)
            {
                _logger.Log(LogEntryType.Reset,
                            MsgGeneratedTextHasBeenReset,
                            _textReader.FileName);
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
            CurrentSegment = segmentName is null ? string.Empty : segmentName;
            LineNumber = 0;

            if (segmentName is not null && _controlDictionary.ContainsKey(CurrentSegment))
            {
                _controlDictionary[CurrentSegment].IsFirstTime = true;
            }
            else
            {
                _logger.Log(LogEntryType.Generating,
                            _locater.Location,
                            MsgUnableToResetSegment,
                            CurrentSegment);
            }
        }

        /// <summary>
        /// Sets the tab size to be used when generating text lines.
        /// </summary>
        /// <param name="tabSize">
        /// The new tab size value.
        /// </param>
        public void SetTabSize(int tabSize) => _indentProcessor.SetTabSize(tabSize);

        /// <summary>
        /// Sets the template file path to the given value.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path of an existing text template file.
        /// </param>
        public void SetTemplateFilePath(string templateFilePath) => _textReader.SetFilePath(templateFilePath);

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
        /// file will be overwritten.
        /// </remarks>
        public void WriteGeneratedTextToFile(string filePath, bool resetGeneratedText = true)
        {
            if (_textWriter.WriteTextFile(filePath, _generatedText))
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
                indent = _indentProcessor.GetFirstTimeIndent(controlItem.FirstTimeIndent, textItem);
                controlItem.IsFirstTime = false;
            }
            else
            {
                indent = _indentProcessor.GetIndent(textItem);
            }

            string pad = new(' ', indent);
            string text = _tokenProcessor.ReplaceTokens(textItem.Text);
            _generatedText.Add(pad + text);
        }

        private bool IsValidTemplateFilePath() => string.IsNullOrWhiteSpace(_textReader.FullFilePath) is false;

        private bool IsValidTemplateFilePath(string filePath)
        {
            _textReader.SetFilePath(filePath);
            return IsValidTemplateFilePath();
        }

        private void LoadTemplateLines()
        {
            List<string> templateLines = _textReader.ReadTextFile().ToList();

            if (IsEmptyTemplateFile(templateLines))
            {
                _logger.Log(LogEntryType.Loading,
                            MsgTemplateFileIsEmpty);
                IsTemplateLoaded = false;
            }
            else
            {
                _logger.Log(LogEntryType.Loading,
                            MsgLoadingTemplateFile,
                            _textReader.FileName);
                _templateLoader.LoadTemplate(templateLines, _segmentDictionary, _controlDictionary);
                IsTemplateLoaded = true;
                IsOutputFileWritten = false;
            }
        }

        private bool SegmentCanBeGenerated(string segmentName)
        {
            bool result = false;

            if (IsTemplateLoaded)
            {
                if (_controlDictionary.ContainsKey(segmentName))
                {
                    if (_segmentDictionary.ContainsKey(segmentName))
                    {
                        _logger.Log(LogEntryType.Generating,
                                    _locater.Location,
                                    MsgProcessingSegment);
                        result = true;
                    }
                    else
                    {
                        _logger.Log(LogEntryType.Generating,
                                    _locater.Location,
                                    MsgSegmentHasNoTextLines,
                                    segmentName);
                    }
                }
                else
                {
                    _logger.Log(LogEntryType.Generating,
                                _locater.Location,
                                MsgUnknownSegmentName,
                                segmentName);
                }
            }
            else
            {
                _logger.Log(LogEntryType.Generating,
                            _locater.Location,
                            MsgAttemptToGenerateSegmentBeforeItWasLoaded,
                            segmentName);
            }

            return result;
        }
    }
}