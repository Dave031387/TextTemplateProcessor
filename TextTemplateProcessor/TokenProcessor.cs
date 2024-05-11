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
        private const char LowercaseFlag = '-';
        private const char SameCaseFlag = '=';
        private const char UppercaseFlag = '+';
        private record TokenInfo(string TokenString, string TokenName, char Case);
        private record TokenSearchResult(bool IsValid, int IndexValue);

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

            Logger = logger;
            Locater = locater;
            NameValidater = nameValidater;
        }

        internal Dictionary<string, string> TokenDictionary { get; } = new();
        internal string TokenEnd { get; private set; } = "#>";
        internal char TokenEscapeChar { get; private set; } = '\\';
        internal string TokenStart { get; private set; } = "<#";

        private ILocater Locater { get; init; }

        private ILogger Logger { get; init; }

        private INameValidater NameValidater { get; init; }

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
                TokenInfo tokenInfo = FindToken(ref startIndex, ref text);

                if (string.IsNullOrEmpty(tokenInfo.TokenString))
                {
                    continue;
                }

                if (TokenDictionary.ContainsKey(tokenInfo.TokenName) is false)
                {
                    TokenDictionary.Add(tokenInfo.TokenName, string.Empty);
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
                    Logger.Log(MsgTokenDictionaryIsEmpty,
                               Locater.CurrentSegment);
                }
            }
            else
            {
                Logger.Log(MsgTokenDictionaryIsNull,
                           Locater.CurrentSegment);
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
                TokenInfo tokenInfo = FindToken(ref startIndex, ref text);

                if (string.IsNullOrEmpty(tokenInfo.TokenName))
                {
                    break;
                }

                if (TokenDictionary.ContainsKey(tokenInfo.TokenName))
                {
                    string tokenValue = TokenDictionary[tokenInfo.TokenName];
                    string replacementValue = GetReplacementValue(tokenInfo, tokenValue);

                    _ = builder.Replace(tokenInfo.TokenString, replacementValue);
                }
                else
                {
                    Logger.Log(MsgTokenNameNotFound,
                               Locater.CurrentSegment,
                               tokenInfo.TokenName);
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
                Logger.Log(MsgTokenStartDelimiterIsNull);
                return false;
            }

            if (tokenEnd is null)
            {
                Logger.Log(MsgTokenEndDelimiterIsNull);
                return false;
            }

            if (string.IsNullOrWhiteSpace(tokenStart))
            {
                Logger.Log(MsgTokenStartDelimiterIsEmpty);
                return false;
            }

            if (string.IsNullOrWhiteSpace(tokenEnd))
            {
                Logger.Log(MsgTokenEndDelimiterIsEmpty);
                return false;
            }

            if (tokenStart == tokenEnd)
            {
                Logger.Log(MsgTokenStartAndTokenEndAreSame,
                           tokenStart,
                           tokenEnd);
                return false;
            }

            if (tokenStart == tokenEscapeChar.ToString())
            {
                Logger.Log(MsgTokenStartAndTokenEscapeAreSame,
                           tokenStart,
                           tokenEscapeChar.ToString());
                return false;
            }

            if (tokenEnd == tokenEscapeChar.ToString())
            {
                Logger.Log(MsgTokenEndAndTokenEscapeAreSame,
                           tokenEnd,
                           tokenEscapeChar.ToString());
                return false;
            }

            if (tokenStart[^1] is LowercaseFlag or UppercaseFlag or SameCaseFlag)
            {
                Logger.Log(MsgTokenStartDelimiterWarning);
            }

            TokenStart = tokenStart;
            TokenEnd = tokenEnd;
            TokenEscapeChar = tokenEscapeChar;
            return true;
        }

        private TokenInfo ExtractToken(int tokenStart, int tokenEnd, ref string text)
        {
            int tokenNameStart = tokenStart + TokenStart.Length;
            int tokenNameEnd = tokenEnd;
            tokenEnd += TokenEnd.Length;
            string tokenString = text[tokenStart..tokenEnd];
            string tokenName;
            bool isValidToken = true;
            char initCase = SameCaseFlag;

            if (text[tokenNameStart] is LowercaseFlag or UppercaseFlag or SameCaseFlag)
            {
                initCase = text[tokenNameStart];
                tokenNameStart++;
            }

            tokenName = text[tokenNameStart..tokenNameEnd].Trim();

            if (string.IsNullOrWhiteSpace(tokenName))
            {
                Logger.Log(MsgMissingTokenName);
                isValidToken = false;
            }
            else if (NameValidater.IsValidName(tokenName) is false)
            {
                Logger.Log(MsgTokenHasInvalidName,
                           tokenName);
                isValidToken = false;
            }

            if (isValidToken is false)
            {
                text = InsertEscapeCharacter(tokenStart, text);
                tokenString = string.Empty;
                tokenName = string.Empty;
            }

            return new(tokenString, tokenName, initCase);
        }

        private TokenInfo FindToken(ref int startIndex, ref string text)
        {
            TokenInfo result = new(string.Empty, string.Empty, SameCaseFlag);

            if (startIndex < 0)
            {
                startIndex = 0;
            }

            while (startIndex < text.Length
                && string.IsNullOrEmpty(result.TokenString))
            {
                TokenSearchResult tokenStart = LocateTokenStartDelimiter(startIndex, text);

                if (tokenStart.IsValid is false)
                {
                    startIndex = tokenStart.IndexValue;
                    continue;
                }

                TokenSearchResult tokenEnd = LocateTokenEndDelimiter(tokenStart.IndexValue, ref text);

                if (tokenEnd.IsValid is false)
                {
                    startIndex = tokenEnd.IndexValue;
                    break;
                }

                result = ExtractToken(tokenStart.IndexValue, tokenEnd.IndexValue, ref text);
                startIndex = tokenEnd.IndexValue;
            }

            return result;
        }

        private string GetReplacementValue(TokenInfo tokenInfo, string tokenValue)
        {
            string replacementValue = string.Empty;

            if (string.IsNullOrEmpty(tokenValue))
            {
                Logger.Log(MsgTokenValueIsEmpty,
                           Locater.CurrentSegment,
                           tokenInfo.TokenName);
            }
            else
            {
                string firstChar = tokenValue[0..1];
                string remaining = tokenValue.Length > 1
                    ? tokenValue[1..]
                    : string.Empty;

                replacementValue = tokenInfo.Case == LowercaseFlag
                    ? firstChar.ToLowerInvariant() + remaining
                        : tokenInfo.Case == UppercaseFlag
                        ? firstChar.ToUpperInvariant() + remaining
                        : tokenValue;
            }

            return replacementValue;
        }

        private string InsertEscapeCharacter(int tokenStart, string text) => text.Insert(tokenStart, TokenEscapeChar.ToString());

        private TokenSearchResult LocateTokenEndDelimiter(int tokenStart, ref string text)
        {
            int tokenEnd = text.IndexOf(TokenEnd, tokenStart, StringComparison.Ordinal);

            if (tokenEnd < 0)
            {
                Logger.Log(MsgTokenMissingEndDelimiter);
                text = InsertEscapeCharacter(tokenStart, text);
                return new(false, text.Length);
            }

            return new(true, tokenEnd);
        }

        private TokenSearchResult LocateTokenStartDelimiter(int startIndex, string text)
        {
            int tokenStart = text.IndexOf(TokenStart, startIndex, StringComparison.Ordinal);

            return tokenStart < 0
                ? new(false, text.Length)
                : tokenStart > 0 && text[tokenStart - 1] == TokenEscapeChar
                ? new(false, tokenStart + TokenStart.Length)
                : new(true, tokenStart);
        }

        private void UpdateTokenDictionary(KeyValuePair<string, string> keyValuePair)
        {
            string tokenName = keyValuePair.Key;
            string tokenValue = keyValuePair.Value;

            if (NameValidater.IsValidName(tokenName))
            {
                if (TokenDictionary.ContainsKey(tokenName))
                {
                    if (tokenValue is null)
                    {
                        Logger.Log(MsgTokenWithNullValue,
                                   Locater.CurrentSegment,
                                   tokenName);
                        tokenValue = string.Empty;
                    }
                    else if (string.IsNullOrEmpty(tokenValue))
                    {
                        Logger.Log(MsgTokenWithEmptyValue,
                                   Locater.CurrentSegment,
                                   tokenName);
                    }

                    TokenDictionary[tokenName] = tokenValue;
                }
                else
                {
                    Logger.Log(MsgUnknownTokenName,
                               Locater.CurrentSegment,
                               tokenName);
                }
            }
            else
            {
                Logger.Log(MsgTokenDictionaryContainsInvalidTokenName,
                           Locater.CurrentSegment,
                           tokenName);
            }
        }
    }
}