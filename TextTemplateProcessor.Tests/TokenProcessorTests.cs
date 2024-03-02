﻿namespace TextTemplateProcessor
{
    using System.Linq.Expressions;

    public class TokenProcessorTests
    {
        private readonly Mock<ILocater> _locater = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<INameValidater> _nameValidater = new();
        private readonly string _tokenEnd = "#>";
        private readonly char _tokenEscapeChar = '\\';
        private readonly string _tokenStart = "<#=";
        private Expression<Action<ILogger>> _loggerExpression = x => x.Log("test", null, null);
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
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater,
                                                           tokenName,
                                                           true);
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
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName1, true);
            _nameValidaterExpression2 = SetupNameValidater(_nameValidater, tokenName2, true);
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
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, true);
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
            _loggerExpression = SetupLogger(_logger, MsgTokenMissingEndDelimiter);
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
            VerifyMocks(1, 0, 0, 0, 0);
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
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, true);
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
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, false);
            _loggerExpression = SetupLogger(_logger,
                                            MsgTokenHasInvalidName,
                                            tokenName);
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
            VerifyMocks(1, 0, 0, 1, 0);
        }

        [Fact]
        public void ExtractTokens_TokenNameIsWhitespace_EscapesTokenAndLogsMessage()
        {
            // Arrange
            string text = $"text{_tokenStart}{Whitespace}{_tokenEnd}text";
            string expected = $"text{_tokenEscapeChar}{_tokenStart}{Whitespace}{_tokenEnd}text";
            _loggerExpression = SetupLogger(_logger, MsgMissingTokenName);
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
            VerifyMocks(1, 0, 0, 0, 0);
        }

        [Fact]
        public void LoadTokenValues_EmptyTokenValuesDictionary_LogsMessage()
        {
            // Arrange
            string segmentName = "Segment1";
            int lineNumber = 1;
            SetupLocater(_locater, segmentName, lineNumber);
            _loggerExpression = SetupLogger(_logger,
                                            MsgTokenDictionaryIsEmpty,
                                            segmentName);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            Dictionary<string, string> tokenValues = new();

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            VerifyMocks(1, 0, 1, 0, 0);
        }

        [Fact]
        public void LoadTokenValues_NullTokenValuesDictionary_LogsMessage()
        {
            // Arrange
            string segmentName = "Segment1";
            int lineNumber = 1;
            SetupLocater(_locater, segmentName, lineNumber);
            _loggerExpression = SetupLogger(_logger,
                                            MsgTokenDictionaryIsNull,
                                            segmentName);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            processor.LoadTokenValues(null!);

            // Assert
            VerifyMocks(1, 0, 1, 0, 0);
        }

        [Fact]
        public void LoadTokenValues_TokenHasInvalidName_LogsMessage()
        {
            // Arrange
            string tokenName = "1badName";
            string segmentName = "Segment2";
            int lineNumber = 11;
            SetupLocater(_locater, segmentName, lineNumber);
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, false);
            _loggerExpression = SetupLogger(_logger,
                                            MsgTokenDictionaryContainsInvalidTokenName,
                                            segmentName,
                                            tokenName);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            Dictionary<string, string> tokenValues = new()
            {
                { tokenName, "test" }
            };

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            VerifyMocks(1, 0, 1, 1, 0);
        }

        [Fact]
        public void LoadTokenValues_TokenValueIsEmptyString_LogsMessageAndSetsValueToEmptyString()
        {
            // Arrange
            string tokenName = "Token";
            string segmentName = "Segment1";
            int lineNumber = 1;
            SetupLocater(_locater, segmentName, lineNumber);
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, true);
            _loggerExpression = SetupLogger(_logger,
                                            MsgTokenWithEmptyValue,
                                            segmentName,
                                            tokenName);
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
            VerifyMocks(1, 0, 1, 1, 0);
        }

        [Fact]
        public void LoadTokenValues_TokenValueIsNotEmptyString_SetsTokenValue()
        {
            // Arrange
            string tokenName = "Token2";
            string expected = "new value";
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, true);
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
            SetupLocater(_locater, segmentName, lineNumber);
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, true);
            _loggerExpression = SetupLogger(_logger,
                                            MsgTokenWithNullValue,
                                            segmentName,
                                            tokenName);
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
            VerifyMocks(1, 0, 1, 1, 0);
        }

        [Fact]
        public void LoadTokenValues_UnknownTokenName_LogsMessage()
        {
            // Arrange
            string tokenName = "Token";
            string segmentName = "Segment2";
            int lineNumber = 3;
            SetupLocater(_locater, segmentName, lineNumber);
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, true);
            _loggerExpression = SetupLogger(_logger,
                                            MsgUnknownTokenName,
                                            segmentName,
                                            tokenName);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            Dictionary<string, string> tokenValues = new()
            {
                { tokenName, "test" }
            };

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            VerifyMocks(1, 0, 1, 1, 0);
        }

        [Fact]
        public void ReplaceTokens_MultipleTokenErrors_LogsAllErrors()
        {
            // Arrange
            string tokenName1 = "noEndDelimiter";
            string tokenName2 = Whitespace;
            string tokenName3 = "invalidName";
            string tokenName4 = "escapedToken";
            string tokenName5 = "unknownToken";
            string tokenName6 = "tokenWithEmptyValue";
            string tokenName7 = "validToken";
            string tokenValue3 = "unused";
            string tokenValue4 = "unused";
            string tokenValue6 = "";
            string tokenValue7 = "value";
            string token1 = $"{_tokenStart}{tokenName1}";
            string token2 = GenerateToken(tokenName2);
            string token3 = GenerateToken(tokenName3);
            string token4 = GenerateToken(tokenName4);
            string token5 = GenerateToken(tokenName5);
            string token6 = GenerateToken(tokenName6);
            string token7 = GenerateToken(tokenName7);
            string segmentName = "Segment1";
            int lineNumber = 11;
            SetupLocater(_locater, segmentName, lineNumber);
            Expression<Func<INameValidater, bool>> nameValidaterInvalid = x => x.IsValidName(tokenName3);
            Expression<Func<INameValidater, bool>> nameValidaterUnknown = x => x.IsValidName(tokenName5);
            Expression<Func<INameValidater, bool>> nameValidaterEmpty = x => x.IsValidName(tokenName6);
            Expression<Func<INameValidater, bool>> nameValidaterValid = x => x.IsValidName(tokenName7);
            _nameValidater.Setup(nameValidaterInvalid).Returns(false);
            _nameValidater.Setup(nameValidaterUnknown).Returns(true);
            _nameValidater.Setup(nameValidaterEmpty).Returns(true);
            _nameValidater.Setup(nameValidaterValid).Returns(true);
            Expression<Action<ILogger>> loggerNoDelimiter = SetupLogger(_logger, MsgTokenMissingEndDelimiter);
            Expression<Action<ILogger>> loggerMissingName = SetupLogger(_logger, MsgMissingTokenName);
            Expression<Action<ILogger>> loggerInvalidName = SetupLogger(_logger, MsgTokenHasInvalidName, tokenName3);
            Expression<Action<ILogger>> loggerUnknownName = SetupLogger(_logger, MsgTokenNameNotFound, segmentName, tokenName5);
            Expression<Action<ILogger>> loggerEmptyValue = SetupLogger(_logger, MsgTokenValueIsEmpty, segmentName, tokenName6);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            processor.TokenDictionary.Add(tokenName3, tokenValue3);
            processor.TokenDictionary.Add(tokenName4, tokenValue4);
            processor.TokenDictionary.Add(tokenName6, tokenValue6);
            processor.TokenDictionary.Add(tokenName7, tokenValue7);
            string text = $"Text {token2} {_tokenEscapeChar}{token4} {token3} {token7} {token5} {token6} {token1}";
            string expected = $"Text {token2} {token4} {token3} {tokenValue7} {token5}  {token1}";

            // Act
            string actual = processor.ReplaceTokens(text);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(nameValidaterInvalid, Times.Once);
            _nameValidater.Verify(nameValidaterUnknown, Times.Once);
            _nameValidater.Verify(nameValidaterEmpty, Times.Once);
            _nameValidater.Verify(nameValidaterValid, Times.Once);
            _logger.Verify(loggerNoDelimiter, Times.Once);
            _logger.Verify(loggerMissingName, Times.Once);
            _logger.Verify(loggerInvalidName, Times.Once);
            _logger.Verify(loggerUnknownName, Times.Once);
            _logger.Verify(loggerEmptyValue, Times.Once);
            VerifyMocks(0, 0, 2, 0, 0);
        }

        [Fact]
        public void ReplaceTokens_TextContainsEscapedTokens_RemovesTheEscapeCharacters()
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
        public void ReplaceTokens_TokenIsMissingEndDelimiter_LogsMessageAndOutputsTokenUnchanged()
        {
            // Arrange
            _loggerExpression = SetupLogger(_logger, MsgTokenMissingEndDelimiter);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            string tokenName = "token";
            string expected = $"Text line with {_tokenStart}{tokenName}";
            processor.TokenDictionary.Add(tokenName, "value");

            // Act
            string actual = processor.ReplaceTokens(expected);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks(1, 0, 0, 0, 0);
        }

        [Fact]
        public void ReplaceTokens_TokenNameIsInvalid_LogsMessageAndOutputsTokenUnchanged()
        {
            // Arrange
            string tokenName = "invalid name";
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, false);
            _loggerExpression = SetupLogger(_logger,
                                            MsgTokenHasInvalidName,
                                            tokenName);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            string expected = $"Text line {_tokenStart}{tokenName}{_tokenEnd} end";
            processor.TokenDictionary.Add(tokenName, "value");

            // Act
            string actual = processor.ReplaceTokens(expected);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks(1, 0, 0, 1, 0);
        }

        [Fact]
        public void ReplaceTokens_TokenNameIsWhitespace_LogsMessageAndOutputsTokenUnchanged()
        {
            // Arrange
            _loggerExpression = SetupLogger(_logger, MsgMissingTokenName);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            string expected = $"Text line {_tokenStart}{Whitespace}{_tokenEnd} end";

            // Act
            string actual = processor.ReplaceTokens(expected);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks(1, 0, 0, 0, 0);
        }

        [Fact]
        public void ReplaceTokens_TokenNameNotInTokenDictionary_LogsMessageAndOutputsTokenUnchanged()
        {
            // Arrange
            string tokenName = "token";
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, true);
            string segmentName = "Segment1";
            SetupLocater(_locater, segmentName, 10);
            _loggerExpression = SetupLogger(_logger,
                                            MsgTokenNameNotFound,
                                            segmentName,
                                            tokenName);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            string expected = $"Text line {_tokenStart}{tokenName}{_tokenEnd} end";
            processor.TokenDictionary.Add("anotherToken", "value");

            // Act
            string actual = processor.ReplaceTokens(expected);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks(1, 0, 1, 1, 0);
        }

        [Fact]
        public void ReplaceTokens_TokenValueIsEmptyString_LogsMessageAndRemovesToken()
        {
            // Arrange
            string tokenName = "token";
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, true);
            string segmentName = "Segment1";
            SetupLocater(_locater, segmentName, 10);
            _loggerExpression = SetupLogger(_logger,
                                            MsgTokenValueIsEmpty,
                                            segmentName,
                                            tokenName);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            string text = $"Text line {_tokenStart}{tokenName}{_tokenEnd} end";
            string expected = "Text line  end";
            processor.TokenDictionary.Add(tokenName, "");

            // Act
            string actual = processor.ReplaceTokens(text);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks(1, 0, 1, 1, 0);
        }

        [Fact]
        public void ReplaceTokens_TokenValueIsNotEmpty_ReplacesTokenWithTokenValue()
        {
            // Arrange
            string tokenName = "token";
            string tokenValue = "value";
            _nameValidaterExpression1 = SetupNameValidater(_nameValidater, tokenName, true);
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);
            string text = $"Text line {_tokenStart}{tokenName}{_tokenEnd} end";
            string expected = $"Text line {tokenValue} end";
            processor.TokenDictionary.Add(tokenName, tokenValue);

            // Act
            string actual = processor.ReplaceTokens(text);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks(0, 0, 0, 1, 0);
        }

        [Fact]
        public void SetTokenDelimiters_TokenEndAndTokenEscapeAreSameValue_LogsMessageAndReturnsFalse()
        {
            char escape = '!';
            string delimiter = escape.ToString();
            SetTokenDelimiters_Test("<<", delimiter, escape, MsgTokenEndAndTokenEscapeAreSame, delimiter, delimiter);
        }

        [Theory]
        [InlineData("")]
        [InlineData(Whitespace)]
        public void SetTokenDelimiters_TokenEndIsEmptyOrWhitespace_LogsMessageAndReturnsFalse(string tokenEnd)
            => SetTokenDelimiters_Test("<<", tokenEnd, '!', MsgTokenEndDelimiterIsEmpty);

        [Fact]
        public void SetTokenDelimiters_TokenEndIsNull_LogsMessageAndReturnsFalse()
            => SetTokenDelimiters_Test("<<", null!, '!', MsgTokenEndDelimiterIsNull);

        [Fact]
        public void SetTokenDelimiters_TokenStartAndTokenEndAreSameValue_LogsMessageAndReturnsFalse()
        {
            string delimiter = "##";
            SetTokenDelimiters_Test(delimiter, delimiter, '!', MsgTokenStartAndTokenEndAreSame, delimiter, delimiter);
        }

        [Fact]
        public void SetTokenDelimiters_TokenStartAndTokenEscapeAreSameValue_LogsMessageAndReturnsFalse()
        {
            char escape = '!';
            string delimiter = escape.ToString();
            SetTokenDelimiters_Test(delimiter, ">>", escape, MsgTokenStartAndTokenEscapeAreSame, delimiter, delimiter);
        }

        [Theory]
        [InlineData("")]
        [InlineData(Whitespace)]
        public void SetTokenDelimiters_TokenStartIsEmptyOrWhitespace_LogsMessageAndReturnsFalse(string tokenStart)
            => SetTokenDelimiters_Test(tokenStart, ">>", '!', MsgTokenStartDelimiterIsEmpty);

        [Fact]
        public void SetTokenDelimiters_TokenStartIsNull_LogsMessageAndReturnsFalse()
            => SetTokenDelimiters_Test(null!, ">>", '!', MsgTokenStartDelimiterIsNull);

        [Fact]
        public void SetTokenDelimiters_ValidDelimiters_SetsDelimiterValuesAndReturnsTrue()
            => SetTokenDelimiters_Test("<<", ">>", '!');

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

        private void SetTokenDelimiters_Test(string tokenStart,
                                             string tokenEnd,
                                             char tokenEscapeChar,
                                             string? message = null,
                                             string? arg1 = null,
                                             string? arg2 = null)
        {
            // Arrange
            bool expected = true;
            int loggerCount = 0;

            if (message is not null)
            {
                _loggerExpression = SetupLogger(_logger, message, arg1, arg2);
                expected = false;
                loggerCount = 1;
            }

            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Act
            bool actual = processor.SetTokenDelimiters(tokenStart, tokenEnd, tokenEscapeChar);

            // Assert
            actual
                .Should()
                .Be(expected);

            if (message is null)
            {
                processor.TokenStart
                    .Should()
                    .Be(tokenStart);
                processor.TokenEnd
                    .Should()
                    .Be(tokenEnd);
                processor.TokenEscapeChar
                    .Should()
                    .Be(tokenEscapeChar);
            }
            else
            {
                processor.TokenStart
                    .Should()
                    .Be(_tokenStart);
                processor.TokenEnd
                    .Should()
                    .Be(_tokenEnd);
                processor.TokenEscapeChar
                    .Should()
                    .Be(_tokenEscapeChar);
            }

            VerifyMocks(loggerCount, 0, 0, 0, 0);
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
                _locater.Verify(LocationExpression, Times.Exactly(locaterLocationCount));
            }

            if (locaterCurrentSegmentCount > 0)
            {
                _locater.Verify(CurrentSegmentExpression, Times.Exactly(locaterCurrentSegmentCount));
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