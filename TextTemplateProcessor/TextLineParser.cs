namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using System.Text.RegularExpressions;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="TextLineParser" /> class is used for parsing and validating text lines from a
    /// text template file.
    /// </summary>
    internal class TextLineParser : ITextLineParser
    {
        private const string AbsoluteIndentCode = "=";
        private const string CommentCode = "///";
        private const string IndentCode = $"({IndentCodePart1}{IndentCodePart2}{IndentValue})";
        private const string IndentCodePart1 = $"({OneTimeIndentCode}|{NormalIndentCode})";
        private const string IndentCodePart2 = $"({AbsoluteIndentCode}|{RelativeRightIndentCode}|{RelativeLeftIndentCode})";
        private const string IndentValue = "[0-9]";
        private const string NoIndentCode = "   ";
        private const string NormalIndentCode = "@";
        private const string OneTimeIndentCode = "O";
        private const string RelativeLeftIndentCode = "-";
        private const string RelativeRightIndentCode = @"\+";
        private const string RestOfLine = ".*$";
        private const string SegmentHeaderCode = "###";
        private readonly Regex _absoluteIndent = new($"^{IndentCodePart1}{AbsoluteIndentCode}{IndentValue}{RestOfLine}");
        private readonly ILogger _logger;
        private readonly Regex _oneTimeIndent = new($"^{OneTimeIndentCode}{IndentCodePart2}{IndentValue}{RestOfLine}");
        private readonly ITokenProcessor _tokenProcessor;
        private readonly Regex _validCommentLine = new($"^{CommentCode}{RestOfLine}");
        private readonly Regex _validSegmentHeaderLine = new($"^{SegmentHeaderCode}{RestOfLine}");
        private readonly Regex _validTemplateLine = new($"^({NoIndentCode}|{SegmentHeaderCode}|{CommentCode}|{IndentCode}){RestOfLine}");
        private readonly Regex _validTextLine = new($"^({NoIndentCode}|{IndentCode}){RestOfLine}");

        /// <summary>
        /// Default constructor that creates an instance of the <see cref="TextLineParser" /> class.
        /// </summary>
        public TextLineParser() : this(ServiceLocater.Current.Get<ILogger>(),
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
        /// <param name="tokenProcessor">
        /// A reference to a token processor object for parsing and extracting tokens from the text
        /// template file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal TextLineParser(ILogger logger,
                                ITokenProcessor tokenProcessor)
        {
            Utility.NullDependencyCheck(logger,
                                        ClassNames.TextLineParserClass,
                                        ServiceNames.LoggerService,
                                        ServiceParameterNames.LoggerParameter);

            Utility.NullDependencyCheck(tokenProcessor,
                                        ClassNames.TextLineParserClass,
                                        ServiceNames.TokenProcessorService,
                                        ServiceParameterNames.TokenProcessorParameter);

            _logger = logger;
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
        public bool IsCommentLine(string templateLine) => _validCommentLine.IsMatch(templateLine);

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
        public bool IsSegmentHeader(string templateLine) => _validSegmentHeaderLine.IsMatch(templateLine);

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
        public bool IsTextLine(string templateLine) => _validTextLine.IsMatch(templateLine);

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
                _logger.Log(MsgMinimumLineLengthInTemplateFileIs3);
                return false;
            }

            if (templateLine.Length > 3 && templateLine[3] is not ' ')
            {
                _logger.Log(MsgFourthCharacterMustBeBlank,
                            templateLine);
                return false;
            }

            if (_validTemplateLine.IsMatch(templateLine))
            {
                return true;
            }

            _logger.Log(MsgInvalidControlCode,
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
        /// <remarks>
        /// This method assumes that the <paramref name="textLine" /> that is passed into the method
        /// has already been validated to ensure it is a valid text line.
        /// </remarks>
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

            if (_oneTimeIndent.IsMatch(textLine))
            {
                isOneTime = true;
            }

            if (_absoluteIndent.IsMatch(textLine))
            {
                isRelative = false;
            }

            _tokenProcessor.ExtractTokens(ref text);

            return new(indent, isRelative, isOneTime, text);
        }
    }
}