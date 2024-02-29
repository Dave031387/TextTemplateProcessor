namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using System.Text;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="TokenProcessor" /> class is used for parsing tokens in a text template file
    /// and replacing those tokens with substitution values.
    /// </summary>
    internal class TokenProcessor : ITokenProcessor
    {
        private readonly ILocater _locater;
        private readonly ILogger _logger;
        private readonly INameValidater _nameValidater;

        /// <summary>
        /// The default constructor that creates an instance of the <see cref="TokenProcessor" />
        /// class.
        /// </summary>
        public TokenProcessor() : this(ServiceLocater.Current.Get<ILogger>(),
                                       ServiceLocater.Current.Get<ILocater>(),
                                       ServiceLocater.Current.Get<INameValidater>())
        {
        }

        /// <summary>
        /// A constructor that creates an instance of the <see cref="TokenProcessor" /> class and
        /// initializes the dependencies.
        /// </summary>
        /// <param name="logger">
        /// A reference to a logger object used for logging messages.
        /// </param>
        /// <param name="locater">
        /// A reference to a locater object for keeping track of the current location in a text
        /// template file.
        /// </param>
        /// <param name="nameValidater">
        /// A reference to a name validater object used for validating token names.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal TokenProcessor(ILogger logger, ILocater locater, INameValidater nameValidater)
        {
            Utility.NullDependencyCheck(logger,
                                        ClassNames.TokenProcessorClass,
                                        ServiceNames.LoggerService,
                                        ServiceParameterNames.LoggerParameter);

            Utility.NullDependencyCheck(locater,
                                        ClassNames.TokenProcessorClass,
                                        ServiceNames.LocaterService,
                                        ServiceParameterNames.LocaterParameter);

            Utility.NullDependencyCheck(nameValidater,
                                        ClassNames.TokenProcessorClass,
                                        ServiceNames.NameValidaterService,
                                        ServiceParameterNames.NameValidaterParameter);

            _logger = logger;
            _locater = locater;
            _nameValidater = nameValidater;
        }

        internal Dictionary<string, string> TokenDictionary { get; } = new();

        internal string TokenEnd { get; private set; } = "#>";

        internal char TokenEscapeChar { get; private set; } = '\\';

        internal string TokenStart { get; private set; } = "<#=";

        /// <summary>
        /// Clears all tokens from the token dictionary.
        /// </summary>
        public void ClearTokens() => TokenDictionary.Clear();

        /// <summary>
        /// Searches for valid tokens in the given line of text and adds any tokens found to the
        /// token dictionary.
        /// </summary>
        /// <param name="text">
        /// A line of text possibly containing one or more tokens.
        /// </param>
        /// <remarks>
        /// If the <paramref name="text" /> parameter contains any invalid tokens, the text will be
        /// modified to insert a token escape character ahead of the token start delimiter of each
        /// invalid token.
        /// </remarks>
        public void ExtractTokens(ref string text)
        {
            int startIndex = 0;

            while (startIndex < text.Length - 1)
            {
                (string token, string tokenName) = FindToken(ref startIndex, ref text);

                if (string.IsNullOrEmpty(token))
                {
                    continue;
                }

                if (TokenDictionary.ContainsKey(tokenName) is false)
                {
                    TokenDictionary.Add(tokenName, string.Empty);
                }
            }
        }

        /// <summary>
        /// This method is used to load token substitution values into the Token Dictionary for the
        /// given token names.
        /// </summary>
        /// <param name="tokenValues">
        /// A dictionary of key/value pairs where the key is the token name and the value is the
        /// substitution value to be assigned to that token.
        /// </param>
        /// <remarks>
        /// The token names in the <paramref name="tokenValues" /> dictionary passed into this
        /// method must already exist in the Token Dictionary. Any token names not found will be
        /// ignored.
        /// </remarks>
        public void LoadTokenValues(Dictionary<string, string> tokenValues)
        {
            if (tokenValues is not null)
            {
                if (tokenValues.Any())
                {
                    foreach (KeyValuePair<string, string> keyValuePair in tokenValues)
                    {
                        UpdateTokenDictionary(keyValuePair);
                    }
                }
                else
                {
                    _logger.Log(MsgTokenDictionaryIsEmpty,
                                _locater.CurrentSegment);
                }
            }
            else
            {
                _logger.Log(MsgTokenDictionaryIsNull,
                            _locater.CurrentSegment);
            }
        }

        /// <summary>
        /// Replace tokens in the given text line with their corresponding substitution values.
        /// </summary>
        /// <param name="text">
        /// A text string that may contain one or more tokens.
        /// </param>
        /// <returns>
        /// The original <paramref name="text" /> string with all tokens replaced by their
        /// substitution values.
        /// </returns>
        /// <remarks>
        /// The token escape character will be removed from all escaped tokens in the
        /// <paramref name="text" /> string and those tokens will be output without any
        /// substitution.
        /// </remarks>
        public string ReplaceTokens(string text)
        {
            StringBuilder builder = new(text);
            int startIndex = 0;

            while (startIndex < text.Length)
            {
                (string token, string tokenName) = FindToken(ref startIndex, ref text);

                if (string.IsNullOrEmpty(tokenName))
                {
                    break;
                }

                if (TokenDictionary.ContainsKey(tokenName))
                {
                    if (string.IsNullOrEmpty(TokenDictionary[tokenName]))
                    {
                        _logger.Log(MsgTokenValueIsEmpty,
                                    _locater.CurrentSegment,
                                    tokenName);
                    }

                    _ = builder.Replace(token, TokenDictionary[tokenName]);
                }
                else
                {
                    _logger.Log(MsgTokenNameNotFound,
                                _locater.CurrentSegment,
                                tokenName);
                }
            }

            builder = builder.Replace(TokenEscapeChar + TokenStart, TokenStart);
            return builder.ToString();
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
            if (tokenStart is null)
            {
                _logger.Log(MsgTokenStartDelimiterIsNull);
                return false;
            }

            if (tokenEnd is null)
            {
                _logger.Log(MsgTokenEndDelimiterIsNull);
                return false;
            }

            if (string.IsNullOrWhiteSpace(tokenStart))
            {
                _logger.Log(MsgTokenStartDelimiterIsEmpty);
                return false;
            }

            if (string.IsNullOrWhiteSpace(tokenEnd))
            {
                _logger.Log(MsgTokenEndDelimiterIsEmpty);
                return false;
            }

            if (tokenStart == tokenEnd)
            {
                _logger.Log(MsgTokenStartAndTokenEndAreSame,
                            tokenStart,
                            tokenEnd);
                return false;
            }

            if (tokenStart == tokenEscapeChar.ToString())
            {
                _logger.Log(MsgTokenStartAndTokenEscapeAreSame,
                            tokenStart,
                            tokenEscapeChar.ToString());
                return false;
            }

            if (tokenEnd == tokenEscapeChar.ToString())
            {
                _logger.Log(MsgTokenEndAndTokenEscapeAreSame,
                            tokenEnd,
                            tokenEscapeChar.ToString());
                return false;
            }

            TokenStart = tokenStart;
            TokenEnd = tokenEnd;
            TokenEscapeChar = tokenEscapeChar;
            return true;
        }

        private (string token, string tokenName) ExtractToken(int tokenStart, ref int tokenEnd, ref string text)
        {
            int tokenNameStart = tokenStart + TokenStart.Length;
            int tokenNameEnd = tokenEnd;
            tokenEnd += TokenEnd.Length;
            string token = text[tokenStart..tokenEnd];
            string tokenName = text[tokenNameStart..tokenNameEnd].Trim();
            bool isValidToken = true;

            if (string.IsNullOrWhiteSpace(tokenName))
            {
                _logger.Log(MsgMissingTokenName);
                isValidToken = false;
            }
            else if (_nameValidater.IsValidName(tokenName) is false)
            {
                _logger.Log(MsgTokenHasInvalidName,
                            tokenName);
                isValidToken = false;
            }

            if (isValidToken is false)
            {
                text = InsertEscapeCharacter(tokenStart, text);
                tokenEnd++;
                token = string.Empty;
                tokenName = string.Empty;
            }

            return (token, tokenName);
        }

        private (string token, string tokenName) FindToken(ref int startIndex, ref string text)
        {
            (string token, string tokenName) result = (string.Empty, string.Empty);

            if (startIndex < 0)
            {
                startIndex = 0;
            }

            while (startIndex < text.Length
                && string.IsNullOrEmpty(result.token))
            {
                (bool isValidTokenStart, int tokenStart) = LocateTokenStartDelimiter(startIndex, text);

                if (isValidTokenStart is false)
                {
                    startIndex = tokenStart;
                    continue;
                }

                (bool isValidTokenEnd, int tokenEnd) = LocateTokenEndDelimiter(tokenStart, ref text);

                if (isValidTokenEnd is false)
                {
                    startIndex = tokenEnd;
                    break;
                }

                result = ExtractToken(tokenStart, ref tokenEnd, ref text);
                startIndex = tokenEnd;
            }

            return result;
        }

        private string InsertEscapeCharacter(int tokenStart, string text) => text.Insert(tokenStart, TokenEscapeChar.ToString());

        private (bool isValidTokenEnd, int tokenEnd) LocateTokenEndDelimiter(int tokenStart, ref string text)
        {
            int tokenEnd = text.IndexOf(TokenEnd, tokenStart, StringComparison.Ordinal);

            if (tokenEnd < 0)
            {
                _logger.Log(MsgTokenMissingEndDelimiter);
                text = InsertEscapeCharacter(tokenStart, text);
                return (false, text.Length);
            }

            return (true, tokenEnd);
        }

        private (bool isValidTokenStart, int newIndexValue) LocateTokenStartDelimiter(int startIndex, string text)
        {
            int tokenStart = text.IndexOf(TokenStart, startIndex, StringComparison.Ordinal);

            return tokenStart < 0
                ? (false, text.Length)
                : tokenStart > 0 && text[tokenStart - 1] == TokenEscapeChar
                ? (false, tokenStart + TokenStart.Length)
                : (true, tokenStart);
        }

        private void UpdateTokenDictionary(KeyValuePair<string, string> keyValuePair)
        {
            string tokenName = keyValuePair.Key;
            string tokenValue = keyValuePair.Value;

            if (_nameValidater.IsValidName(tokenName))
            {
                if (TokenDictionary.ContainsKey(tokenName))
                {
                    if (tokenValue is null)
                    {
                        _logger.Log(MsgTokenWithNullValue,
                                    _locater.CurrentSegment,
                                    tokenName);
                        tokenValue = string.Empty;
                    }
                    else if (string.IsNullOrEmpty(tokenValue))
                    {
                        _logger.Log(MsgTokenWithEmptyValue,
                                    _locater.CurrentSegment,
                                    tokenName);
                    }

                    TokenDictionary[tokenName] = tokenValue;
                }
                else
                {
                    _logger.Log(MsgUnknownTokenName,
                                _locater.CurrentSegment,
                                tokenName);
                }
            }
            else
            {
                _logger.Log(MsgTokenDictionaryContainsInvalidTokenName,
                            _locater.CurrentSegment,
                            tokenName);
            }
        }
    }
}