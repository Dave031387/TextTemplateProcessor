namespace TextTemplateProcessor
{
    using System.Linq.Expressions;

    public class TokenProcessorTests
    {
        private readonly Mock<ILocater> _locater = new();
        private readonly Expression<Func<ILocater, string>> _locaterCurrentSegmentExpression = x => x.CurrentSegment;
        private readonly Expression<Func<ILocater, (string, int)>> _locaterLocationExpression = x => x.Location;
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<INameValidater> _nameValidater = new();
        private readonly string _tokenEnd = "#>";
        private readonly char _tokenEscapeChar = '\\';
        private readonly string _tokenStart = "<#=";
        private Expression<Action<ILogger>> _loggerExpression = x => x.Log(LogEntryType.Setup, "test");
        private Expression<Func<INameValidater, bool>> _nameValidaterExpression1 = x => x.IsValidName("test");
        private Expression<Func<INameValidater, bool>> _nameValidaterExpression2 = x => x.IsValidName("test");

        public TokenProcessorTests()
        {
            _locater.Reset();
            _logger.Reset();
            _nameValidater.Reset();
        }

        [Fact]
        public void ClearTokens_TokenDictionaryContainsTokens_ClearsTheTokenDictionary()
        {
            // Arrange
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            processor.TokenDictionary.Add("token1", "value1");
            processor.TokenDictionary.Add("token2", "value2");
            processor.TokenDictionary.Add("token3", "value3");

            // Act
            processor.ClearTokens();

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            VerifyMocks(0, 0, 0, 0, 0);
        }

        [Fact]
        public void ExtractTokens_LineContainsDuplicateTokens_ExtractsTokenOnlyOnce()
        {
            // Arrange
            string tokenName = "token1";
            string token = GenerateToken(tokenName);
            string text = $"text {token} text {token} text";
            string expected = $"text {token} text {token} text";
            SetupNameValidater(tokenName, true);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .ContainSingle();
            processor.TokenDictionary
                .Should()
                .ContainKey(tokenName);
            text
                .Should()
                .Be(expected);
            VerifyMocks(0, 0, 0, 2, 0);
        }

        [Theory]
        [InlineData("text", "text")]
        [InlineData("text", "")]
        [InlineData("", "text")]
        [InlineData("", "")]
        public void ExtractTokens_LineContainsEscapedToken_IgnoresEscapedToken(string text1, string text2)
        {
            // Arrange
            string tokenName = "token1";
            string token = GenerateToken(tokenName, true);
            string text = $"{text1}{token}{text2}";
            string expected = $"{text1}{token}{text2}";
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            text
                .Should()
                .Be(expected);
            VerifyMocks(0, 0, 0, 0, 0);
        }

        [Theory]
        [InlineData("text", "text", "text")]
        [InlineData("text", "text", "")]
        [InlineData("text", "", "text")]
        [InlineData("text", "", "")]
        [InlineData("", "text", "text")]
        [InlineData("", "text", "")]
        [InlineData("", "", "text")]
        [InlineData("", "", "")]
        public void ExtractTokens_LineContainsMoreThanOneValidToken_ExtractsTokens(string text1, string text2, string text3)
        {
            // Arrange
            string tokenName1 = "token1";
            string token1 = GenerateToken(tokenName1);
            string tokenName2 = "token2";
            string token2 = GenerateToken(tokenName2);
            string text = $"{text1}{token1}{text2}{token2}{text3}";
            string expected = $"{text1}{token1}{text2}{token2}{text3}";
            SetupNameValidater(tokenName1, true, tokenName2, true);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .HaveCount(2);
            processor.TokenDictionary
                .Should()
                .ContainKey(tokenName1);
            processor.TokenDictionary
                .Should()
                .ContainKey(tokenName2);
            text
                .Should()
                .Be(expected);
            VerifyMocks(0, 0, 0, 1, 1);
        }

        [Theory]
        [InlineData("text", "text")]
        [InlineData("text", "")]
        [InlineData("", "text")]
        [InlineData("", "")]
        public void ExtractTokens_LineContainsOneValidToken_ExtractsToken(string text1, string text2)
        {
            // Arrange
            string tokenName = "token1";
            string token = GenerateToken(tokenName);
            string text = $"{text1}{token}{text2}";
            string expected = $"{text1}{token}{text2}";
            SetupNameValidater(tokenName, true);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .ContainSingle();
            processor.TokenDictionary
                .Should()
                .ContainKey(tokenName);
            text
                .Should()
                .Be(expected);
            VerifyMocks(0, 0, 0, 1, 0);
        }

        [Fact]
        public void ExtractTokens_LineContainsTokenWithMissingEndDelimiter_EscapesTokenAndLogsMessage()
        {
            // Arrange
            string text = $"text{_tokenStart}token";
            string expected = $"text{_tokenEscapeChar}{_tokenStart}token";
            string segmentName = "Segment1";
            int lineNumber = 7;
            SetupLocater(segmentName, lineNumber);
            SetupLogger(GetLoggerExpression(LogEntryType.Parsing,
                                            segmentName,
                                            lineNumber,
                                            MsgTokenMissingEndDelimiter));
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            text
                .Should()
                .Be(expected);
            VerifyMocks(1, 1, 0, 0, 0);
        }

        [Fact]
        public void ExtractTokens_LineDoesNotContainAnyTokens_DoesNothing()
        {
            // Arrange
            string text = "this is a text line";
            string expected = "this is a text line";
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            text
                .Should()
                .Be(expected);
            VerifyMocks(0, 0, 0, 0, 0);
        }

        [Theory]
        [InlineData("  ", "  ")]
        [InlineData("  ", "")]
        [InlineData("", "  ")]
        public void ExtractTokens_TokenContainsEmbeddedSpaces_TrimsSpacesAndExtractsTokenName(string text1, string text2)
        {
            // Arrange
            string tokenName = "token1";
            string paddedName = $"{text1}{tokenName}{text2}";
            string token = GenerateToken(paddedName);
            string text = $"text{token}text";
            string expected = $"text{token}text";
            SetupNameValidater(tokenName, true);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .ContainSingle();
            processor.TokenDictionary
                .Should()
                .ContainKey(tokenName);
            text
                .Should()
                .Be(expected);
            VerifyMocks(0, 0, 0, 1, 0);
        }

        [Fact]
        public void ExtractTokens_TokenNameIsInvalid_EscapesTokenAndLogsMessage()
        {
            // Arrange
            string tokenName = "1badName";
            string token = GenerateToken(tokenName);
            string text = $"text{token}text";
            string expected = $"text{_tokenEscapeChar}{token}text";
            string segmentName = "Segment1";
            int lineNumber = 7;
            SetupLocater(segmentName, lineNumber);
            SetupNameValidater(tokenName, false);
            SetupLogger(GetLoggerExpression(LogEntryType.Parsing,
                                            segmentName,
                                            lineNumber,
                                            MsgTokenHasInvalidName,
                                            tokenName));
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            text
                .Should()
                .Be(expected);
            VerifyMocks(1, 1, 0, 1, 0);
        }

        [Fact]
        public void ExtractTokens_TokenNameIsWhitespace_EscapesTokenAndLogsMessage()
        {
            // Arrange
            string text = $"text{_tokenStart}{Whitespace}{_tokenEnd}text";
            string expected = $"text{_tokenEscapeChar}{_tokenStart}{Whitespace}{_tokenEnd}text";
            string segmentName = "Segment1";
            int lineNumber = 7;
            SetupLocater(segmentName, lineNumber);
            SetupLogger(GetLoggerExpression(LogEntryType.Parsing,
                                            segmentName,
                                            lineNumber,
                                            MsgMissingTokenName));
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            text
                .Should()
                .Be(expected);
            VerifyMocks(1, 1, 0, 0, 0);
        }

        [Fact]
        public void LoadTokenValues_EmptyTokenValuesDictionary_LogsMessage()
        {
            // Arrange
            string segmentName = "Segment1";
            int lineNumber = 1;
            SetupLocater(segmentName, lineNumber);
            SetupLogger(GetLoggerExpression(LogEntryType.Generating,
                                            segmentName,
                                            lineNumber,
                                            MsgTokenDictionaryIsEmpty,
                                            segmentName));
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            Dictionary<string, string> tokenValues = new();

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            VerifyMocks(1, 1, 1, 0, 0);
        }

        [Fact]
        public void LoadTokenValues_NullTokenValuesDictionary_LogsMessage()
        {
            // Arrange
            string segmentName = "Segment1";
            int lineNumber = 1;
            SetupLocater(segmentName, lineNumber);
            SetupLogger(GetLoggerExpression(LogEntryType.Generating,
                                            segmentName,
                                            lineNumber,
                                            MsgTokenDictionaryIsNull,
                                            segmentName));
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            processor.LoadTokenValues(null!);

            // Assert
            VerifyMocks(1, 1, 1, 0, 0);
        }

        [Fact]
        public void LoadTokenValues_TokenHasInvalidName_LogsMessage()
        {
            // Arrange
            string tokenName = "1badName";
            string segmentName = "Segment2";
            int lineNumber = 11;
            SetupLocater(segmentName, lineNumber);
            SetupNameValidater(tokenName, false);
            SetupLogger(GetLoggerExpression(LogEntryType.Generating,
                                            segmentName,
                                            lineNumber,
                                            MsgTokenDictionaryContainsInvalidTokenName,
                                            segmentName,
                                            tokenName));
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            Dictionary<string, string> tokenValues = new()
            {
                { tokenName, "test" }
            };

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            VerifyMocks(1, 1, 1, 1, 0);
        }

        [Fact]
        public void LoadTokenValues_TokenValueIsEmptyString_LogsMessageAndSetsValueToEmptyString()
        {
            // Arrange
            string tokenName = "Token";
            string segmentName = "Segment1";
            int lineNumber = 1;
            SetupLocater(segmentName, lineNumber);
            SetupNameValidater(tokenName, true);
            SetupLogger(GetLoggerExpression(LogEntryType.Generating,
                                            segmentName,
                                            lineNumber,
                                            MsgTokenWithEmptyValue,
                                            segmentName,
                                            tokenName));
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            processor.TokenDictionary.Add(tokenName, "test");
            Dictionary<string, string> tokenValues = new()
            {
                { tokenName, string.Empty }
            };

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            processor.TokenDictionary[tokenName]
                .Should()
                .BeEmpty();
            VerifyMocks(1, 1, 1, 1, 0);
        }

        [Fact]
        public void LoadTokenValues_TokenValueIsNotEmptyString_SetsTokenValue()
        {
            // Arrange
            string tokenName = "Token2";
            string expected = "new value";
            SetupNameValidater(tokenName, true);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            processor.TokenDictionary.Add("Token1", "test1");
            processor.TokenDictionary.Add(tokenName, "test2");
            Dictionary<string, string> tokenValues = new()
            {
                { tokenName, expected }
            };

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            processor.TokenDictionary[tokenName]
                .Should()
                .Be(expected);
            VerifyMocks(0, 0, 0, 1, 0);
        }

        [Fact]
        public void LoadTokenValues_TokenValueIsNull_LogsMessageAndSetsValueToEmptyString()
        {
            // Arrange
            string tokenName = "Token";
            string segmentName = "Segment1";
            int lineNumber = 1;
            SetupLocater(segmentName, lineNumber);
            SetupNameValidater(tokenName, true);
            SetupLogger(GetLoggerExpression(LogEntryType.Generating,
                                            segmentName,
                                            lineNumber,
                                            MsgTokenWithNullValue,
                                            segmentName,
                                            tokenName));
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            processor.TokenDictionary.Add(tokenName, "test");
            Dictionary<string, string> tokenValues = new()
            {
                { tokenName, null! }
            };

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            processor.TokenDictionary[tokenName]
                .Should()
                .BeEmpty();
            VerifyMocks(1, 1, 1, 1, 0);
        }

        [Fact]
        public void LoadTokenValues_UnknownTokenName_LogsMessage()
        {
            // Arrange
            string tokenName = "Token";
            string segmentName = "Segment2";
            int lineNumber = 3;
            SetupLocater(segmentName, lineNumber);
            SetupNameValidater(tokenName, true);
            SetupLogger(GetLoggerExpression(LogEntryType.Generating,
                                            segmentName,
                                            lineNumber,
                                            MsgUnknownTokenName,
                                            segmentName,
                                            tokenName));
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            Dictionary<string, string> tokenValues = new()
            {
                { tokenName, "test" }
            };

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            VerifyMocks(1, 1, 1, 1, 0);
        }

        [Fact]
        public void ReplaceTokens_TextContainsEscapedTokens_RemovesTheEscapeCharacter()
        {
            // Arrange
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            string token1 = GenerateToken(" test ");
            string token2 = GenerateToken("token2");
            string middleText = " middle ";
            string endText = " end";
            string text = $"{_tokenEscapeChar}{token1}{middleText}{_tokenEscapeChar}{token2}{endText}";
            string expected = $"{token1}{middleText}{token2}{endText}";

            // Act
            string actual = processor.ReplaceTokens(text);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks(0, 0, 0, 0, 0);
        }

        [Fact]
        public void ReplaceTokens_TextDoesNotContainTokens_ReturnsTextUnchanged()
        {
            // Arrange
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            string expected = "Text line without any tokens";

            // Act
            string actual = processor.ReplaceTokens(expected);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks(0, 0, 0, 0, 0);
        }

        [Fact]
        public void TokenDictionary_AddItemsToDictionary_DictionaryShouldContainAddedItems()
        {
            // Arrange
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            string[] expectedKeys = new[] { "token1", "token2", "token3" };

            // Act
            processor.TokenDictionary.Add("token1", "value1");
            processor.TokenDictionary.Add("token2", "value2");
            processor.TokenDictionary.Add("token3", "value3");

            // Assert
            processor.TokenDictionary
                .Should()
                .HaveCount(3);
            processor.TokenDictionary
                .Should()
                .ContainKeys(expectedKeys);
            VerifyMocks(0, 0, 0, 0, 0);
        }

        [Fact]
        public void TokenProcessor_ConstructWithNullLocater_ThrowsException()
        {
            // Arrange
            Action action = () => { TokenProcessor processor = new(_logger.Object, null!, _nameValidater.Object); };
            string expected = GetNullDependencyMessage(ClassNames.TokenProcessorClass,
                                                       ServiceNames.LocaterService,
                                                       ServiceParameterNames.LocaterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            VerifyMocks(0, 0, 0, 0, 0);
        }

        [Fact]
        public void TokenProcessor_ConstructWithNullLogger_ThrowsException()
        {
            // Arrange
            Action action = () => { TokenProcessor processor = new(null!, _locater.Object, _nameValidater.Object); };
            string expected = GetNullDependencyMessage(ClassNames.TokenProcessorClass,
                                                       ServiceNames.LoggerService,
                                                       ServiceParameterNames.LoggerParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            VerifyMocks(0, 0, 0, 0, 0);
        }

        [Fact]
        public void TokenProcessor_ConstructWithNullNameValidater_ThrowsException()
        {
            // Arrange
            Action action = () => { TokenProcessor processor = new(_logger.Object, _locater.Object, null!); };
            string expected = GetNullDependencyMessage(ClassNames.TokenProcessorClass,
                                                       ServiceNames.NameValidaterService,
                                                       ServiceParameterNames.NameValidaterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            VerifyMocks(0, 0, 0, 0, 0);
        }

        [Fact]
        public void TokenProcessor_ConstructWithValidDependencies_InitializesEmptyTokenDictionary()
        {
            // Arrange/Act
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Assert
            processor.TokenDictionary
                .Should()
                .NotBeNull();
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            VerifyMocks(0, 0, 0, 0, 0);
        }

        private string GenerateToken(string tokenName, bool isEscaped = false)
            => isEscaped
            ? $"{_tokenEscapeChar}{_tokenStart}{tokenName}{_tokenEnd}"
            : $"{_tokenStart}{tokenName}{_tokenEnd}";

        private void SetupLocater(string segmentName, int lineNumber)
        {
            (string, int) location = (segmentName, lineNumber);
            _locater.Setup(_locaterCurrentSegmentExpression).Returns(segmentName);
            _locater.Setup(_locaterLocationExpression).Returns(location);
        }

        private void SetupLogger(Expression<Action<ILogger>> loggerExpression)
        {
            _loggerExpression = loggerExpression;
            _logger.Setup(_loggerExpression);
        }

        private void SetupNameValidater(string name1, bool isValid1, string name2 = "n/a", bool isValid2 = false)
        {
            _nameValidaterExpression1 = x => x.IsValidName(name1);
            _nameValidater.Setup(_nameValidaterExpression1).Returns(isValid1);

            if (name2 is not "n/a")
            {
                _nameValidaterExpression2 = x => x.IsValidName(name2);
                _nameValidater.Setup(_nameValidaterExpression2).Returns(isValid2);
            }
        }

        private void VerifyMocks(int loggerCount,
                                 int locaterLocationCount,
                                 int locaterCurrentSegmentCount,
                                 int nameValidaterCount1,
                                 int nameValidaterCount2)
        {
            if (loggerCount > 0)
            {
                _logger.Verify(_loggerExpression, Times.Exactly(loggerCount));
            }

            if (locaterLocationCount > 0)
            {
                _locater.Verify(_locaterLocationExpression, Times.Exactly(locaterLocationCount));
            }

            if (locaterCurrentSegmentCount > 0)
            {
                _locater.Verify(_locaterCurrentSegmentExpression, Times.Exactly(locaterCurrentSegmentCount));
            }

            if (nameValidaterCount1 > 0)
            {
                _nameValidater.Verify(_nameValidaterExpression1, Times.Exactly(nameValidaterCount1));
            }

            if (nameValidaterCount2 > 0)
            {
                _nameValidater.Verify(_nameValidaterExpression2, Times.Exactly(nameValidaterCount2));
            }

            _logger.VerifyNoOtherCalls();
            _locater.VerifyNoOtherCalls();
            _nameValidater.VerifyNoOtherCalls();
        }
    }
}