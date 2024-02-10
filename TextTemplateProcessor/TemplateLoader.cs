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
        private readonly ITextLineParser _textLineParser;
        private Dictionary<string, ControlItem> _controlDictionary = new();
        private Dictionary<string, List<TextItem>> _segmentDictionary = new();
        private int _textLineCount = 0;

        /// <summary>
        /// Default constructor that creates an instance of the <see cref="TemplateLoader" /> class.
        /// </summary>
        public TemplateLoader() : this(
            ServiceLocater.Current.Get<ILogger>(),
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
        internal TemplateLoader(
            ILogger logger,
            IDefaultSegmentNameGenerator defaultSegmentNameGenerator,
            ILocater locater,
            ISegmentHeaderParser segmentHeaderParser,
            ITextLineParser textLineParser)
        {
            if (logger is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TemplateLoader), nameof(ILogger));
                throw new ArgumentNullException(nameof(logger), message);
            }

            if (defaultSegmentNameGenerator is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TemplateLoader), nameof(IDefaultSegmentNameGenerator));
                throw new ArgumentNullException(nameof(defaultSegmentNameGenerator), message);
            }

            if (locater is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TemplateLoader), nameof(ILocater));
                throw new ArgumentNullException(nameof(locater), message);
            }

            if (segmentHeaderParser is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TemplateLoader), nameof(ISegmentHeaderParser));
                throw new ArgumentNullException(nameof(segmentHeaderParser), message);
            }

            if (textLineParser is null)
            {
                string message = string.Format(MsgDependencyIsNull, nameof(TemplateLoader), nameof(ITextLineParser));
                throw new ArgumentNullException(nameof(textLineParser), message);
            }

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
        public void LoadTemplate(
            IEnumerable<string> templateLines,
            Dictionary<string, List<TextItem>> segmentDictionary,
            Dictionary<string, ControlItem> controlDictionary)
        {
            _segmentDictionary = segmentDictionary;
            _controlDictionary = controlDictionary;
            _locater.LineNumber = 0;

            foreach (string templateLine in templateLines)
            {
                _locater.LineNumber++;

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
                _logger.Log(LogEntryType.Parsing, _locater.Location, MsgFoundDuplicateSegmentName, segmentName, _locater.CurrentSegment);
            }

            if (IsPadSegmentInvalid(segmentName, controlItem.PadSegment))
            {
                _logger.Log(LogEntryType.Parsing, _locater.Location, MsgPadSegmentsMustBeDefinedEarlier, segmentName, controlItem.PadSegment);
                controlItem.PadSegment = string.Empty;
            }

            _controlDictionary[_locater.CurrentSegment] = controlItem;
            _textLineCount = 0;
            _logger.Log(LogEntryType.Parsing, _locater.Location, MsgSegmentHasBeenAdded);
        }

        private void AddTextItemToSegmentDictionary(TextItem textItem)
        {
            if (_segmentDictionary.ContainsKey(_locater.CurrentSegment))
            {
                _segmentDictionary[_locater.CurrentSegment].Add(textItem);
            }
            else
            {
                _segmentDictionary.Add(_locater.CurrentSegment, new List<TextItem>() { textItem });
            }

            _textLineCount++;
        }

        private void CheckForEmptySegment()
        {
            if (_locater.HasValidSegmentName && _textLineCount == 0)
            {
                _logger.Log(LogEntryType.Parsing, _locater.Location, MsgNoTextLinesFollowingSegmentHeader, _locater.CurrentSegment);
            }
        }

        private void CheckForMissingSegmentHeader()
        {
            if (_locater.HasEmptySegmentName)
            {
                CreateDefaultSegment();
                _logger.Log(LogEntryType.Parsing, _locater.Location, MsgMissingInitialSegmentHeader, _locater.CurrentSegment);
            }
        }

        private void CreateDefaultSegment()
        {
            _locater.CurrentSegment = _defaultSegmentNameGenerator.Next;
            _controlDictionary[_locater.CurrentSegment] = new();
            _textLineCount = 0;
        }

        private bool IsPadSegmentInvalid(string segmentName, string? padSegment)
            => !string.IsNullOrEmpty(padSegment) && (!_controlDictionary.ContainsKey(padSegment) || padSegment == segmentName);

        private void ParseTemplateLine(string templateLine)
        {
            if (_textLineParser.IsCommentLine(templateLine))
            {
                return;
            }
            else if (_textLineParser.IsSegmentHeader(templateLine))
            {
                CheckForEmptySegment();

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
    }
}