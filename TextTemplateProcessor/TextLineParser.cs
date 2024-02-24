namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="TextLineParser" /> class is used for parsing and validating text lines from a
    /// text template file.
    /// </summary>
    internal class TextLineParser : ITextLineParser
    {
        private const char AbsoluteIndent = '=';
        private const string Comment = "///";
        private const string IndentAbsolute = "@=";
        private const string IndentAbsoluteOneTime = "O=";
        private const string IndentLeftOneTime = "O-";
        private const string IndentLeftRelative = "@-";
        private const string IndentRightOneTime = "O+";
        private const string IndentRightRelative = "@+";
        private const string IndentUnchanged = "   ";
        private const char OneTimeIndent = 'O';
        private const string SegmentHeaderCode = "###";

        private readonly ILocater _locater;
        private readonly ILogger _logger;
        private readonly ITokenProcessor _tokenProcessor;

        /// <summary>
        /// Default constructor that creates an instance of the <see cref="TextLineParser" /> class.
        /// </summary>
        public TextLineParser() : this(ServiceLocater.Current.Get<ILogger>(),
                                       ServiceLocater.Current.Get<ILocater>(),
                                       ServiceLocater.Current.Get<ITokenProcessor>())
        {
        }

        /// <summary>
        /// Constructor that creates an instance of the <see cref="TextLineParser" /> class and
        /// initializes dependencies.
        /// </summary>
        /// <param name="logger">
        /// A reference to a logger object for logging messages.
        /// </param>
        /// <param name="locater">
        /// A reference to a locater object for keeping track of our location within the text
        /// template file.
        /// </param>
        /// <param name="tokenProcessor">
        /// A reference to a token processor object for parsing and extracting tokens from the text
        /// template file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal TextLineParser(ILogger logger,
                                ILocater locater,
                                ITokenProcessor tokenProcessor)
        {
            Utility.NullDependencyCheck(logger,
                                        ClassNames.TextLineParserClass,
                                        ServiceNames.LoggerService,
                                        ServiceParameterNames.LoggerParameter);

            Utility.NullDependencyCheck(locater,
                                        ClassNames.TextLineParserClass,
                                        ServiceNames.LocaterService,
                                        ServiceParameterNames.LocaterParameter);

            Utility.NullDependencyCheck(tokenProcessor,
                                        ClassNames.TextLineParserClass,
                                        ServiceNames.TokenProcessorService,
                                        ServiceParameterNames.TokenProcessorParameter);

            _logger = logger;
            _locater = locater;
            _tokenProcessor = tokenProcessor;
        }

        /// <summary>
        /// Determines whether or not the given line from the text template file is a comment line.
        /// </summary>
        /// <param name="templateLine">
        /// A line from a text template file.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the line is a comment line. Otherwise, returns
        /// <see langword="false" />.
        /// </returns>
        public bool IsCommentLine(string templateLine) => templateLine[..3] == Comment;

        /// <summary>
        /// Determines whether or not the given line from the text template file is a segment
        /// header.
        /// </summary>
        /// <param name="templateLine">
        /// A line from a text template file.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the line is a segment header line. Otherwise, returns
        /// <see langword="false" />.
        /// </returns>
        public bool IsSegmentHeader(string templateLine) => templateLine[..3] == SegmentHeaderCode;

        /// <summary>
        /// Determines whether or not the given line from the text template file is a text line.
        /// </summary>
        /// <param name="templateLine">
        /// A line from a text template file.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the line is a text line. Otherwise, returns
        /// <see langword="false" />.
        /// </returns>
        public bool IsTextLine(string templateLine)
            => templateLine[..3] is IndentUnchanged
            || ((templateLine[..2] is IndentAbsolute
                                   or IndentAbsoluteOneTime
                                   or IndentLeftOneTime
                                   or IndentLeftRelative
                                   or IndentRightOneTime
                                   or IndentRightRelative)
            && templateLine[2] is >= '0' and <= '9');

        /// <summary>
        /// Determines whether or not the prefix portion of a line from a text template file is
        /// valid.
        /// </summary>
        /// <param name="templateLine">
        /// A line from a text template file.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the first four characters of the text line form a valid
        /// prefix. Otherwise, returns <see langword="false" />.
        /// </returns>
        public bool IsValidPrefix(string templateLine)
        {
            if (templateLine.Length < 3)
            {
                _logger.Log(LogEntryType.Parsing,
                            _locater.Location,
                            MsgMinimumLineLengthInTemplateFileIs3);
                return false;
            }

            if (templateLine.Length > 3 && templateLine[3] is not ' ')
            {
                _logger.Log(LogEntryType.Parsing,
                            _locater.Location,
                            MsgFourthCharacterMustBeBlank,
                            templateLine);
                return false;
            }

            if (IsTextLine(templateLine) || IsSegmentHeader(templateLine) || IsCommentLine(templateLine))
            {
                return true;
            }

            _logger.Log(LogEntryType.Parsing,
                        _locater.Location,
                        MsgInvalidControlCode,
                        templateLine);
            return false;
        }

        /// <summary>
        /// Parses a text line from a text template file and extracts control information and
        /// tokens.
        /// </summary>
        /// <param name="textLine">
        /// A text line from a text template file.
        /// </param>
        /// <returns>
        /// a <see cref="TextItem" /> object containing the text from the text line plus the control
        /// information.
        /// </returns>
        public TextItem ParseTextLine(string textLine)
        {
            string indentString = textLine[1..3].Replace('=', '+');
            int indent = 0;
            string text = textLine.Length > 4 ? textLine[4..] : string.Empty;
            bool isRelative = true;
            bool isOneTime = false;

            if (indentString is not "  ")
            {
                _ = int.TryParse(indentString, out indent);
            }

            if (textLine[0] is OneTimeIndent)
            {
                isOneTime = true;
            }

            if (textLine[1] is AbsoluteIndent)
            {
                isRelative = false;
            }

            _tokenProcessor.ExtractTokens(ref text);

            return new(indent, isRelative, isOneTime, text);
        }
    }
}