namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="SegmentHeaderParser" /> class is used for parsing segment headers in a text
    /// template file to extract the segment names and control options.
    /// </summary>
    internal class SegmentHeaderParser : ISegmentHeaderParser
    {
        private const string FirstTimeIndentOption = "FTI";
        private const string PadSegmentNameOption = "PAD";
        private const string TabSizeOption = "TAB";
        private readonly IDefaultSegmentNameGenerator _defaultSegmentNameGenerator;
        private readonly IIndentProcessor _indentProcessor;
        private readonly ILocater _locater;
        private readonly ILogger _logger;
        private readonly INameValidater _nameValidater;

        /// <summary>
        /// Default constructor that creates an instance of the <see cref="SegmentHeaderParser" />
        /// class.
        /// </summary>
        public SegmentHeaderParser() : this(ServiceLocater.Current.Get<ILogger>(),
                                            ServiceLocater.Current.Get<IDefaultSegmentNameGenerator>(),
                                            ServiceLocater.Current.Get<ILocater>(),
                                            ServiceLocater.Current.Get<IIndentProcessor>(),
                                            ServiceLocater.Current.Get<INameValidater>())
        {
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="SegmentHeaderParser" /> class and
        /// initializes dependencies.
        /// </summary>
        /// <param name="logger">
        /// A reference to a logger object used for logging messages.
        /// </param>
        /// <param name="defaultSegmentNameGenerator">
        /// A reference to a default segment name generator object.
        /// </param>
        /// <param name="locater">
        /// A reference to a locater object for keeping track of the current location being
        /// processed within a text template file.
        /// </param>
        /// <param name="indentProcessor">
        /// A reference to an indent processor object used for managing line indentation in the
        /// generated text file.
        /// </param>
        /// <param name="nameValidater">
        /// A reference to a name validater object used for validating segment names.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal SegmentHeaderParser(ILogger logger,
                                     IDefaultSegmentNameGenerator defaultSegmentNameGenerator,
                                     ILocater locater,
                                     IIndentProcessor indentProcessor,
                                     INameValidater nameValidater)
        {
            Utility.NullDependencyCheck(logger,
                                        ClassNames.SegmentHeaderParserClass,
                                        ServiceNames.LoggerService,
                                        ServiceParameterNames.LoggerParameter);

            Utility.NullDependencyCheck(defaultSegmentNameGenerator,
                                        ClassNames.SegmentHeaderParserClass,
                                        ServiceNames.DefaultSegmentNameGeneratorService,
                                        ServiceParameterNames.DefaultSegmentNameGeneratorParameter);

            Utility.NullDependencyCheck(locater,
                                        ClassNames.SegmentHeaderParserClass,
                                        ServiceNames.LocaterService,
                                        ServiceParameterNames.LocaterParameter);

            Utility.NullDependencyCheck(indentProcessor,
                                        ClassNames.SegmentHeaderParserClass,
                                        ServiceNames.IndentProcessorService,
                                        ServiceParameterNames.IndentProcessorParameter);

            Utility.NullDependencyCheck(nameValidater,
                                        ClassNames.SegmentHeaderParserClass,
                                        ServiceNames.NameValidaterService,
                                        ServiceParameterNames.NameValidaterParameter);

            _logger = logger;
            _defaultSegmentNameGenerator = defaultSegmentNameGenerator;
            _locater = locater;
            _indentProcessor = indentProcessor;
            _nameValidater = nameValidater;
        }

        /// <summary>
        /// This method parses a segment header line from a text template file and extracts the
        /// segment name and control information.
        /// </summary>
        /// <param name="headerLine">
        /// A segment header line from a text template file.
        /// </param>
        /// <returns>
        /// a <see cref="ControlItem" /> object containing the segment name and control information.
        /// </returns>
        public ControlItem ParseSegmentHeader(string headerLine)
        {
            ControlItem controlItem = new();

            if (headerLine.Length < 5)
            {
                _locater.CurrentSegment = _defaultSegmentNameGenerator.Next;
                _logger.Log(MsgSegmentNameMustStartInColumn5,
                            _locater.CurrentSegment);
                return controlItem;
            }

            if (headerLine[4] == ' ')
            {
                _logger.Log(MsgSegmentNameMustStartInColumn5,
                            _locater.CurrentSegment);
                headerLine = headerLine.Insert(4, _defaultSegmentNameGenerator.Next);
            }

            string[] args = headerLine.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            string segmentName = args[1];

            if (_nameValidater.IsValidName(segmentName))
            {
                _locater.CurrentSegment = segmentName;
            }
            else
            {
                _locater.CurrentSegment = _defaultSegmentNameGenerator.Next;
                _logger.Log(MsgInvalidSegmentName,
                            segmentName,
                            _locater.CurrentSegment);
            }

            if (args.Length > 2)
            {
                controlItem = ParseSegmentOptions(args);
            }

            return controlItem;
        }

        private (string optionName, string optionValue) ParseSegmentOption(string arg)
        {
            (string optionName, string optionValue) result = (string.Empty, string.Empty);

            int optionIndex;

            if (arg.Contains('='))
            {
                optionIndex = arg.IndexOf('=');
            }
            else
            {
                _logger.Log(MsgInvalidFormOfOption,
                            arg);
                return result;
            }

            if (optionIndex < 1)
            {
                _logger.Log(MsgOptionNameMustPrecedeEqualsSign,
                            _locater.CurrentSegment);
                return result;
            }

            result.optionName = arg[..optionIndex].ToUpperInvariant();

            if (result.optionName is not FirstTimeIndentOption and not PadSegmentNameOption and not TabSizeOption)
            {
                _logger.Log(MsgUnknownSegmentOptionFound,
                            _locater.CurrentSegment,
                            arg);
                return result;
            }

            optionIndex++;

            if (optionIndex == arg.Length)
            {
                _logger.Log(MsgOptionValueMustFollowEqualsSign,
                            _locater.CurrentSegment,
                            result.optionName);
                return result;
            }

            result.optionValue = arg[optionIndex..];

            return result;
        }

        private ControlItem ParseSegmentOptions(string[] args)
        {
            ControlItem controlItem = new();
            bool firstTimeIndentOptionFound = false;
            bool padSegmentOptionFound = false;
            bool tabOptionFound = false;

            for (int i = 2; i < args.Length; i++)
            {
                (string optionName, string optionValue) = ParseSegmentOption(args[i]);

                if (string.IsNullOrEmpty(optionValue))
                {
                    continue;
                }

                if ((optionName == FirstTimeIndentOption && firstTimeIndentOptionFound)
                    || (optionName == PadSegmentNameOption && padSegmentOptionFound)
                    || (optionName == TabSizeOption && tabOptionFound))
                {
                    _logger.Log(MsgFoundDuplicateOptionNameOnHeaderLine,
                                _locater.CurrentSegment,
                                optionName);
                    continue;
                }

                switch (optionName)
                {
                    case FirstTimeIndentOption:
                        SetFirstTimeIndentOption(controlItem, optionValue);
                        firstTimeIndentOptionFound = true;
                        break;

                    case PadSegmentNameOption:
                        SetPadSegmentOption(controlItem, optionValue);
                        padSegmentOptionFound = true;
                        break;

                    case TabSizeOption:
                        SetTabSizeOption(controlItem, optionValue);
                        tabOptionFound = true;
                        break;

                    default:
                        break;
                }
            }

            return controlItem;
        }

        private void SetFirstTimeIndentOption(ControlItem controlItem, string optionValue)
        {
            if (_indentProcessor.IsValidIndentValue(optionValue, out int indentValue))
            {
                if (indentValue == 0)
                {
                    _logger.Log(MsgFirstTimeIndentSetToZero);
                }

                controlItem.FirstTimeIndent = indentValue;
            }
        }

        private void SetPadSegmentOption(ControlItem controlItem, string optionValue)
        {
            if (_nameValidater.IsValidName(optionValue))
            {
                controlItem.PadSegment = optionValue;
            }
            else
            {
                _logger.Log(MsgInvalidPadSegmentName,
                            optionValue,
                            _locater.CurrentSegment);
            }
        }

        private void SetTabSizeOption(ControlItem controlItem, string optionValue)
        {
            if (_indentProcessor.IsValidTabSizeValue(optionValue, out int tabValue))
            {
                controlItem.FirstTimeIndent = tabValue;
            }
        }
    }
}