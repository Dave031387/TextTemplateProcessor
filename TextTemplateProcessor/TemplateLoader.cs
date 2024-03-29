namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="TemplateLoader" /> class is used for parsing and loading a text template
    /// file.
    /// </summary>
    internal class TemplateLoader : ITemplateLoader
    {
        private readonly IDefaultSegmentNameGenerator _defaultSegmentNameGenerator;
        private readonly ILocater _locater;
        private readonly ILogger _logger;
        private readonly ISegmentHeaderParser _segmentHeaderParser;
        private readonly List<string> _segmentsWithPad = new();
        private readonly ITextLineParser _textLineParser;
        private Dictionary<string, ControlItem> _controlDictionary = new();
        private Dictionary<string, List<TextItem>> _segmentDictionary = new();
        private int _textLineCount = 0;

        /// <summary>
        /// Default constructor that creates an instance of the <see cref="TemplateLoader" /> class.
        /// </summary>
        public TemplateLoader() : this(ServiceLocater.Current.Get<ILogger>(),
                                       ServiceLocater.Current.Get<IDefaultSegmentNameGenerator>(),
                                       ServiceLocater.Current.Get<ILocater>(),
                                       ServiceLocater.Current.Get<ISegmentHeaderParser>(),
                                       ServiceLocater.Current.Get<ITextLineParser>())
        {
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="TemplateLoader" /> class and
        /// initializes dependencies.
        /// </summary>
        /// <param name="logger">
        /// </param>
        /// <param name="defaultSegmentNameGenerator">
        /// </param>
        /// <param name="locater">
        /// </param>
        /// <param name="segmentHeaderParser">
        /// </param>
        /// <param name="textLineParser">
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal TemplateLoader(ILogger logger,
                                IDefaultSegmentNameGenerator defaultSegmentNameGenerator,
                                ILocater locater,
                                ISegmentHeaderParser segmentHeaderParser,
                                ITextLineParser textLineParser)
        {
            Utility.NullDependencyCheck(logger,
                                        ClassNames.TemplateLoaderClass,
                                        ServiceNames.LoggerService,
                                        ServiceParameterNames.LoggerParameter);

            Utility.NullDependencyCheck(defaultSegmentNameGenerator,
                                        ClassNames.TemplateLoaderClass,
                                        ServiceNames.DefaultSegmentNameGeneratorService,
                                        ServiceParameterNames.DefaultSegmentNameGeneratorParameter);

            Utility.NullDependencyCheck(locater,
                                        ClassNames.TemplateLoaderClass,
                                        ServiceNames.LocaterService,
                                        ServiceParameterNames.LocaterParameter);

            Utility.NullDependencyCheck(segmentHeaderParser,
                                        ClassNames.TemplateLoaderClass,
                                        ServiceNames.SegmentHeaderParserService,
                                        ServiceParameterNames.SegmentHeaderParserParameter);

            Utility.NullDependencyCheck(textLineParser,
                                        ClassNames.TemplateLoaderClass,
                                        ServiceNames.TextLineParserService,
                                        ServiceParameterNames.TextLineParserParameter);

            _logger = logger;
            _defaultSegmentNameGenerator = defaultSegmentNameGenerator;
            _locater = locater;
            _segmentHeaderParser = segmentHeaderParser;
            _textLineParser = textLineParser;
        }

        /// <summary>
        /// This method parses and loads a text template file for processing. Individual segments
        /// are identified, and control information is extracted.
        /// </summary>
        /// <param name="templateLines">
        /// A collection of text strings. Each string is a line from the text template file.
        /// </param>
        /// <param name="segmentDictionary">
        /// A dictionary of key/value pairs where the key is a segment name from the text template
        /// file and the value is a collection of text lines that belong to that segment.
        /// </param>
        /// <param name="controlDictionary">
        /// A dictionary of key/value pairs where the key is a segment name from the text template
        /// file and the value is a <see cref="ControlItem" /> object containing the control
        /// information for that segment.
        /// </param>
        public void LoadTemplate(IEnumerable<string> templateLines,
                                 Dictionary<string, List<TextItem>> segmentDictionary,
                                 Dictionary<string, ControlItem> controlDictionary)
        {
            _logger.SetLogEntryType(LogEntryType.Parsing);
            _segmentDictionary = segmentDictionary;
            _controlDictionary = controlDictionary;
            _segmentDictionary.Clear();
            _controlDictionary.Clear();
            _segmentsWithPad.Clear();
            _locater.CurrentSegment = string.Empty;
            int lineCount = 0;

            foreach (string templateLine in templateLines)
            {
                _locater.LineNumber = ++lineCount;

                if (_textLineParser.IsValidPrefix(templateLine))
                {
                    ParseTemplateLine(templateLine);
                }
                else
                {
                    CheckForMissingSegmentHeader();
                }
            }

            CheckForEmptySegment();
        }

        private void AddSegmentToControlDictionary(string segmentName, ControlItem controlItem)
        {
            if (_controlDictionary.ContainsKey(segmentName))
            {
                _locater.CurrentSegment = _defaultSegmentNameGenerator.Next;
                _logger.Log(MsgFoundDuplicateSegmentName,
                            segmentName,
                            _locater.CurrentSegment);
            }

            controlItem.PadSegment = ValidatePadSegmentOption(segmentName, controlItem.PadSegment);

            _controlDictionary[_locater.CurrentSegment] = controlItem;
            _textLineCount = 0;
            _logger.Log(MsgSegmentHasBeenAdded,
                        _locater.CurrentSegment);
        }

        private void AddTextItemToSegmentDictionary(TextItem textItem)
        {
            if (_segmentDictionary.ContainsKey(_locater.CurrentSegment))
            {
                _segmentDictionary[_locater.CurrentSegment].Add(textItem);
            }
            else
            {
                _segmentDictionary[_locater.CurrentSegment] = new List<TextItem>() { textItem };
            }

            _textLineCount++;
        }

        private void CheckForEmptySegment()
        {
            if (string.IsNullOrEmpty(_locater.CurrentSegment))
            {
                return;
            }
            else if (_textLineCount == 0)
            {
                _logger.Log(MsgNoTextLinesFollowingSegmentHeader,
                            _locater.CurrentSegment);
                _controlDictionary.Remove(_locater.CurrentSegment);
            }
        }

        private void CheckForMissingSegmentHeader()
        {
            if (string.IsNullOrEmpty(_locater.CurrentSegment))
            {
                _locater.CurrentSegment = _defaultSegmentNameGenerator.Next;
                _controlDictionary[_locater.CurrentSegment] = new();
                _textLineCount = 0;
                _logger.Log(MsgMissingInitialSegmentHeader,
                            _locater.CurrentSegment);
            }
        }

        private void ParseTemplateLine(string templateLine)
        {
            if (_textLineParser.IsCommentLine(templateLine))
            {
                return;
            }
            else if (_textLineParser.IsSegmentHeader(templateLine))
            {
                CheckForEmptySegment();

                // The following two lines were added to facilitate unit testing using mocks. In
                // order for the mocks to work I need to capture the current segment name before
                // parsing the segment header.
                string segmentName = templateLine.Split(' ', ',')[1];
                _locater.CurrentSegment = segmentName;

                ControlItem controlItem = _segmentHeaderParser.ParseSegmentHeader(templateLine);
                AddSegmentToControlDictionary(_locater.CurrentSegment, controlItem);
            }
            else
            {
                CheckForMissingSegmentHeader();

                TextItem textItem = _textLineParser.ParseTextLine(templateLine);
                AddTextItemToSegmentDictionary(textItem);
            }
        }

        private string ValidatePadSegmentOption(string segmentName, string? padSegment)
        {
            string result = string.Empty;

            if (string.IsNullOrEmpty(padSegment))
            {
            }
            else if (padSegment == segmentName)
            {
                _logger.Log(MsgPadSegmentNameSameAsSegmentHeaderName,
                            segmentName);
            }
            else if (_controlDictionary.ContainsKey(padSegment))
            {
                _segmentsWithPad.Add(segmentName);

                if (_segmentsWithPad.Contains(padSegment))
                {
                    _logger.Log(MsgMultipleLevelsOfPadSegments,
                                segmentName,
                                padSegment);
                }
                else
                {
                    result = padSegment;
                }
            }
            else
            {
                _logger.Log(MsgPadSegmentMustBeDefinedEarlier,
                            segmentName,
                            padSegment);
            }

            return result;
        }
    }
}