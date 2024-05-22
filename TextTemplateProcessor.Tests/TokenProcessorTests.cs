namespace TextTemplateProcessor
{
    public class TokenProcessorTests
    {
        private readonly Mock<ILocater> _locater = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<INameValidater> _nameValidater = new();
        private readonly MethodCallOrderVerifier _verifier = new();

        [Fact]
        public void ClearTokens_TokenDictionaryContainsTokens_ClearsTheTokenDictionary()
        {
            // Arrange
            InitializeMocks();
            TokenProcessor processor = GetTokenProcessor();
            processor.TokenDictionary.Add("token1", "value1");
            processor.TokenDictionary.Add("token2", "value2");
            processor.TokenDictionary.Add("token3", "value3");

            // Act
            processor.ClearTokens();

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("")]
        [InlineData("+")]
        [InlineData("-")]
        [InlineData("=")]
        [InlineData(" ")]
        [InlineData("+ ")]
        [InlineData("- ")]
        [InlineData("= ")]
        public void ExtractTokens_LineContainsDuplicateTokens_ExtractsTokenOnlyOnce(string prefix)
        {
            // Arrange
            InitializeMocks();
            string tokenName = "token1";
            string tokenA = GenerateToken(tokenName);
            string tokenB = GenerateToken(prefix + tokenName);
            string text = $"text {tokenA} text {tokenB} text";
            string expected = $"text {tokenA} text {tokenB} text";
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Exactly(2));
            TokenProcessor processor = GetTokenProcessor();

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
            VerifyMocks();
        }

        [Theory]
        [InlineData("text", "text")]
        [InlineData("text", "")]
        [InlineData("", "text")]
        [InlineData("", "")]
        public void ExtractTokens_LineContainsEscapedToken_IgnoresEscapedToken(string text1, string text2)
        {
            // Arrange
            InitializeMocks();
            string tokenName = "token1";
            string token = GenerateToken(tokenName, true);
            string text = $"{text1}{token}{text2}";
            string expected = $"{text1}{token}{text2}";
            TokenProcessor processor = GetTokenProcessor();

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            text
                .Should()
                .Be(expected);
            MocksVerifyNoOtherCalls();
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
            InitializeMocks();
            string tokenName1 = "token1";
            string token1 = GenerateToken(tokenName1);
            string tokenName2 = "token2";
            string token2 = GenerateToken("- " + tokenName2);
            int expectedTokenCount = 2;
            string text = $"{text1}{token1}{text2}{token2}{text3}";
            string expectedText = $"{text1}{token1}{text2}{token2}{text3}";
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName, 1))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName, 2))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.NameValidater_IsValidName, 1, 2);
            TokenProcessor processor = GetTokenProcessor();

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .HaveCount(expectedTokenCount);
            processor.TokenDictionary
                .Should()
                .ContainKey(tokenName1);
            processor.TokenDictionary
                .Should()
                .ContainKey(tokenName2);
            text
                .Should()
                .Be(expectedText);
            VerifyMocks();
        }

        [Theory]
        [InlineData("text", "text", "")]
        [InlineData("text", "", "")]
        [InlineData("", "text", "")]
        [InlineData("", "", "")]
        [InlineData("text", "text", "+")]
        [InlineData("text", "", "-")]
        [InlineData("", "text", "=")]
        [InlineData("", "", "+")]
        public void ExtractTokens_LineContainsOneValidToken_ExtractsToken(string text1, string text2, string prefix)
        {
            // Arrange
            InitializeMocks();
            string tokenName = "token1";
            string token = GenerateToken(prefix + tokenName);
            string text = $"{text1}{token}{text2}";
            string expected = $"{text1}{token}{text2}";
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            TokenProcessor processor = GetTokenProcessor();

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
            VerifyMocks();
        }

        [Fact]
        public void ExtractTokens_LineContainsTokenWithMissingEndDelimiter_EscapesTokenAndLogsMessage()
        {
            // Arrange
            InitializeMocks();
            string text = $"text{TokenStart}token";
            string expected = $"text{TokenEscapeChar}{TokenStart}token";
            _logger
                .Setup(logger => logger.Log(MsgTokenMissingEndDelimiter, null, null))
                .Verifiable(Times.Once);
            TokenProcessor processor = GetTokenProcessor();

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            text
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ExtractTokens_LineDoesNotContainAnyTokens_DoesNothing()
        {
            // Arrange
            InitializeMocks();
            string text = "this is a text line";
            string expected = "this is a text line";
            TokenProcessor processor = GetTokenProcessor();

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            text
                .Should()
                .Be(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("  ", "  ", "")]
        [InlineData("  ", "", "")]
        [InlineData("", "  ", "")]
        [InlineData("  ", "  ", "+")]
        [InlineData("  ", "", "-")]
        [InlineData("", "  ", "=")]
        public void ExtractTokens_TokenContainsEmbeddedSpaces_TrimsSpacesAndExtractsTokenName(string text1, string text2, string prefix)
        {
            // Arrange
            InitializeMocks();
            string tokenName = "token1";
            string paddedName = $"{prefix}{text1}{tokenName}{text2}";
            string token = GenerateToken(paddedName);
            string text = $"text{token}text";
            string expected = $"text{token}text";
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            TokenProcessor processor = GetTokenProcessor();

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
            VerifyMocks();
        }

        [Fact]
        public void ExtractTokens_TokenNameIsInvalid_EscapesTokenAndLogsMessage()
        {
            // Arrange
            InitializeMocks();
            string tokenName = "1 bad Name";
            string token = GenerateToken(tokenName);
            string text = $"text{token}text";
            string expected = $"text{TokenEscapeChar}{token}text";
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(false)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTokenHasInvalidName, tokenName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.Logger_Log_Message);
            TokenProcessor processor = GetTokenProcessor();

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            text
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ExtractTokens_TokenNameIsWhitespace_EscapesTokenAndLogsMessage()
        {
            // Arrange
            InitializeMocks();
            string text = $"text{TokenStart}{Whitespace}{TokenEnd}text";
            string expected = $"text{TokenEscapeChar}{TokenStart}{Whitespace}{TokenEnd}text";
            _logger
                .Setup(logger => logger.Log(MsgMissingTokenName, null, null))
                .Verifiable(Times.Once);
            TokenProcessor processor = GetTokenProcessor();

            // Act
            processor.ExtractTokens(ref text);

            // Assert
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            text
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void LoadTokenValues_EmptyTokenValuesDictionary_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgTokenDictionaryIsEmpty, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
            TokenProcessor processor = GetTokenProcessor();
            Dictionary<string, string> tokenValues = new();

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void LoadTokenValues_NullTokenValuesDictionary_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgTokenDictionaryIsNull, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
            TokenProcessor processor = GetTokenProcessor();

            // Act
            processor.LoadTokenValues(null!);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void LoadTokenValues_TokenHasInvalidName_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string tokenName = "1 bad Name";
            string segmentName = "Segment2";
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(false)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTokenDictionaryContainsInvalidTokenName, segmentName, tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
            TokenProcessor processor = GetTokenProcessor();
            Dictionary<string, string> tokenValues = new()
            {
                { tokenName, "test" }
            };

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void LoadTokenValues_TokenValueIsEmptyString_LogsMessageAndSetsValueToEmptyString()
        {
            // Arrange
            InitializeMocks();
            string tokenName = "Token";
            string segmentName = "Segment1";
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTokenWithEmptyValue, segmentName, tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
            TokenProcessor processor = GetTokenProcessor();
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
            VerifyMocks();
        }

        [Fact]
        public void LoadTokenValues_TokenValueIsNotEmptyString_SetsTokenValue()
        {
            // Arrange
            InitializeMocks();
            string tokenName = "Token2";
            string expected = "new value";
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            TokenProcessor processor = GetTokenProcessor();
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
            VerifyMocks();
        }

        [Fact]
        public void LoadTokenValues_TokenValueIsNull_LogsMessageAndSetsValueToEmptyString()
        {
            // Arrange
            InitializeMocks();
            string tokenName = "Token";
            string segmentName = "Segment1";
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTokenWithNullValue, segmentName, tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
            TokenProcessor processor = GetTokenProcessor();
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
            VerifyMocks();
        }

        [Fact]
        public void LoadTokenValues_UnknownTokenName_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string tokenName = "Token";
            string segmentName = "Segment2";
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnknownTokenName, segmentName, tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
            TokenProcessor processor = GetTokenProcessor();
            Dictionary<string, string> tokenValues = new()
            {
                { tokenName, "test" }
            };

            // Act
            processor.LoadTokenValues(tokenValues);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void ReplaceTokens_MultipleTokenErrors_LogsAllErrors()
        {
            // Arrange
            InitializeMocks();
            string tokenName1 = Whitespace;
            string tokenName2 = "escapedToken";
            string tokenName3 = "invalidName";
            string tokenName4 = "validToken";
            string tokenName5 = "unknownToken";
            string tokenName6 = "tokenWithEmptyValue";
            string tokenName7 = "noEndDelimiter";
            string tokenValue2 = "unused";
            string tokenValue3 = "unused";
            string tokenValue4 = "value";
            string tokenValue6 = "";
            string token1 = GenerateToken(tokenName1);
            string token2 = GenerateToken(tokenName2);
            string token3 = GenerateToken(tokenName3);
            string token4 = GenerateToken("=" + tokenName4);
            string token5 = GenerateToken(tokenName5);
            string token6 = GenerateToken("-" + tokenName6);
            string token7 = $"{TokenStart}+ {tokenName7}";
            string segmentName = "Segment1";
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeast(2));
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName, 1))
                .Returns(false)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName4))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName, 2))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName5))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName, 3))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName6))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName, 4))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgMissingTokenName, null, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTokenHasInvalidName, tokenName3, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTokenNameNotFound, segmentName, tokenName5))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 3))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTokenValueIsEmpty, segmentName, tokenName6))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 4))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTokenMissingEndDelimiter, null, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 5))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.NameValidater_IsValidName, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.Logger_Log_Message, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.NameValidater_IsValidName, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.NameValidater_IsValidName, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.Logger_Log_Message, 3, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message, 0, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.NameValidater_IsValidName, 3, 4);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.Logger_Log_Message, 4, 4);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_Log_Message, 4, 5);
            TokenProcessor processor = GetTokenProcessor();
            processor.TokenDictionary.Add(tokenName3, tokenValue3);
            processor.TokenDictionary.Add(tokenName2, tokenValue2);
            processor.TokenDictionary.Add(tokenName6, tokenValue6);
            processor.TokenDictionary.Add(tokenName4, tokenValue4);
            string text = $"Text {token1} {TokenEscapeChar}{token2} {token3} {token4} {token5} {token6} {token7}";
            string expected = $"Text {token1} {token2} {token3} {tokenValue4} {token5}  {token7}";

            // Act
            string actual = processor.ReplaceTokens(text);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ReplaceTokens_TextContainsEscapedTokens_RemovesTheEscapeCharacters()
        {
            // Arrange
            InitializeMocks();
            TokenProcessor processor = GetTokenProcessor();
            string token1 = GenerateToken(" test ");
            string token2 = GenerateToken("token2");
            string middleText = " middle ";
            string endText = " end";
            string text = $"{TokenEscapeChar}{token1}{middleText}{TokenEscapeChar}{token2}{endText}";
            string expected = $"{token1}{middleText}{token2}{endText}";

            // Act
            string actual = processor.ReplaceTokens(text);

            // Assert
            actual
                .Should()
                .Be(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ReplaceTokens_TextDoesNotContainTokens_ReturnsTextUnchanged()
        {
            // Arrange
            InitializeMocks();
            TokenProcessor processor = GetTokenProcessor();
            string expected = "Text line without any tokens";

            // Act
            string actual = processor.ReplaceTokens(expected);

            // Assert
            actual
                .Should()
                .Be(expected);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("t", "t", "est")]
        [InlineData("T", "t", "est")]
        [InlineData("t", "t", "1")]
        [InlineData("T", "t", "1")]
        [InlineData("t", "t", "")]
        [InlineData("T", "t", "")]
        public void ReplaceTokens_TokenContainsLowercaseFlag_TokenValueStartsWithLowercase(string firstIn, string firstOut, string remaining)
        {
            // Arrange
            InitializeMocks();
            string tokenName = "token";
            string token = GenerateToken("-" + tokenName);
            string tokenValue = firstIn + remaining;
            string expectedValue = firstOut + remaining;
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Returns(true)
                .Verifiable(Times.Once);
            TokenProcessor processor = GetTokenProcessor();
            string text = $"Text line {token} end";
            string expected = $"Text line {expectedValue} end";
            processor.TokenDictionary.Add(tokenName, tokenValue);

            // Act
            string actual = processor.ReplaceTokens(text);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Theory]
        [InlineData("test")]
        [InlineData("Test")]
        [InlineData("t1")]
        [InlineData("T1")]
        [InlineData("t")]
        [InlineData("T")]
        public void ReplaceTokens_TokenContainsSameCaseFlag_TokenValueIsUnchanged(string tokenValue)
        {
            // Arrange
            InitializeMocks();
            string tokenName = "token";
            string token = GenerateToken("=" + tokenName);
            string expectedValue = tokenValue;
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Returns(true)
                .Verifiable(Times.Once);
            TokenProcessor processor = GetTokenProcessor();
            string text = $"Text line {token} end";
            string expected = $"Text line {expectedValue} end";
            processor.TokenDictionary.Add(tokenName, tokenValue);

            // Act
            string actual = processor.ReplaceTokens(text);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Theory]
        [InlineData("t", "T", "est")]
        [InlineData("T", "T", "est")]
        [InlineData("t", "T", "1")]
        [InlineData("T", "T", "1")]
        [InlineData("t", "T", "")]
        [InlineData("T", "T", "")]
        public void ReplaceTokens_TokenContainsUppercaseFlag_TokenValueStartsWithUppercase(string firstIn, string firstOut, string remaining)
        {
            // Arrange
            InitializeMocks();
            string tokenName = "token";
            string token = GenerateToken("+ " + tokenName);
            string tokenValue = firstIn + remaining;
            string expectedValue = firstOut + remaining;
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Returns(true)
                .Verifiable(Times.Once);
            TokenProcessor processor = GetTokenProcessor();
            string text = $"Text line {token} end";
            string expected = $"Text line {expectedValue} end";
            processor.TokenDictionary.Add(tokenName, tokenValue);

            // Act
            string actual = processor.ReplaceTokens(text);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ReplaceTokens_TokenIsMissingEndDelimiter_LogsMessageAndOutputsTokenUnchanged()
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(logger => logger.Log(MsgTokenMissingEndDelimiter, null, null))
                .Verifiable(Times.Once);
            TokenProcessor processor = GetTokenProcessor();
            string tokenName = "token";
            string expected = $"Text line with {TokenStart}{tokenName}";
            processor.TokenDictionary.Add(tokenName, "value");

            // Act
            string actual = processor.ReplaceTokens(expected);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ReplaceTokens_TokenNameIsInvalid_LogsMessageAndOutputsTokenUnchanged()
        {
            // Arrange
            InitializeMocks();
            string tokenName = "invalid name";
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(false)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTokenHasInvalidName, tokenName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.Logger_Log_Message);
            TokenProcessor processor = GetTokenProcessor();
            string expected = $"Text line {TokenStart}{tokenName}{TokenEnd} end";
            processor.TokenDictionary.Add(tokenName, "value");

            // Act
            string actual = processor.ReplaceTokens(expected);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ReplaceTokens_TokenNameIsWhitespace_LogsMessageAndOutputsTokenUnchanged()
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(logger => logger.Log(MsgMissingTokenName, null, null))
                .Verifiable(Times.Once);
            TokenProcessor processor = GetTokenProcessor();
            string expected = $"Text line {TokenStart}{Whitespace}{TokenEnd} end";

            // Act
            string actual = processor.ReplaceTokens(expected);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ReplaceTokens_TokenNameNotInTokenDictionary_LogsMessageAndOutputsTokenUnchanged()
        {
            // Arrange
            InitializeMocks();
            string tokenName = "token";
            string segmentName = "Segment1";
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTokenNameNotFound, segmentName, tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
            TokenProcessor processor = GetTokenProcessor();
            string expected = $"Text line {TokenStart}{tokenName}{TokenEnd} end";
            processor.TokenDictionary.Add("anotherToken", "value");

            // Act
            string actual = processor.ReplaceTokens(expected);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ReplaceTokens_TokenValueIsEmptyString_LogsMessageAndRemovesToken()
        {
            // Arrange
            InitializeMocks();
            string tokenName = "token";
            string segmentName = "Segment1";
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTokenValueIsEmpty, segmentName, tokenName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.NameValidater_IsValidName, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
            TokenProcessor processor = GetTokenProcessor();
            string text = $"Text line {TokenStart}{tokenName}{TokenEnd} end";
            string expected = "Text line  end";
            processor.TokenDictionary.Add(tokenName, "");

            // Act
            string actual = processor.ReplaceTokens(text);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ReplaceTokens_TokenValueIsNotEmpty_ReplacesTokenWithTokenValue()
        {
            // Arrange
            InitializeMocks();
            string tokenName = "token";
            string tokenValue = "value";
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(tokenName))
                .Returns(true)
                .Verifiable(Times.Once);
            TokenProcessor processor = GetTokenProcessor();
            string text = $"Text line {TokenStart}{tokenName}{TokenEnd} end";
            string expected = $"Text line {tokenValue} end";
            processor.TokenDictionary.Add(tokenName, tokenValue);

            // Act
            string actual = processor.ReplaceTokens(text);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void SetTokenDelimiters_TokenEndAndTokenEscapeAreSameValue_LogsMessageAndReturnsFalse()
        {
            char escape = '!';
            string delimiter = escape.ToString();
            SetTokenDelimiters_Test("<<", delimiter, escape, false, MsgTokenEndAndTokenEscapeAreSame, delimiter, delimiter);
        }

        [Theory]
        [InlineData("")]
        [InlineData(Whitespace)]
        public void SetTokenDelimiters_TokenEndIsEmptyOrWhitespace_LogsMessageAndReturnsFalse(string tokenEnd)
            => SetTokenDelimiters_Test("<<", tokenEnd, '!', false, MsgTokenEndDelimiterIsEmpty);

        [Fact]
        public void SetTokenDelimiters_TokenEndIsNull_LogsMessageAndReturnsFalse()
            => SetTokenDelimiters_Test("<<", null!, '!', false, MsgTokenEndDelimiterIsNull);

        [Fact]
        public void SetTokenDelimiters_TokenStartAndTokenEndAreSameValue_LogsMessageAndReturnsFalse()
        {
            string delimiter = "##";
            SetTokenDelimiters_Test(delimiter, delimiter, '!', false, MsgTokenStartAndTokenEndAreSame, delimiter, delimiter);
        }

        [Fact]
        public void SetTokenDelimiters_TokenStartAndTokenEscapeAreSameValue_LogsMessageAndReturnsFalse()
        {
            char escape = '!';
            string delimiter = escape.ToString();
            SetTokenDelimiters_Test(delimiter, ">>", escape, false, MsgTokenStartAndTokenEscapeAreSame, delimiter, delimiter);
        }

        [Theory]
        [InlineData("=")]
        [InlineData("+")]
        [InlineData("-")]
        public void SetTokenDelimiters_TokenStartDelimiterEndsInCaseFlagCharacter_LogsMessageAndSetsDelimitersAndReturnsTrue(string caseFlag)
            => SetTokenDelimiters_Test($"<<{caseFlag}", ">>", '!', true, MsgTokenStartDelimiterWarning);

        [Theory]
        [InlineData("")]
        [InlineData(Whitespace)]
        public void SetTokenDelimiters_TokenStartIsEmptyOrWhitespace_LogsMessageAndReturnsFalse(string tokenStart)
            => SetTokenDelimiters_Test(tokenStart, ">>", '!', false, MsgTokenStartDelimiterIsEmpty);

        [Fact]
        public void SetTokenDelimiters_TokenStartIsNull_LogsMessageAndReturnsFalse()
            => SetTokenDelimiters_Test(null!, ">>", '!', false, MsgTokenStartDelimiterIsNull);

        [Fact]
        public void SetTokenDelimiters_ValidDelimiters_SetsDelimiterValuesAndReturnsTrue()
            => SetTokenDelimiters_Test("<<", ">>", '!');

        [Fact]
        public void TokenDictionary_AddItemsToDictionary_DictionaryShouldContainAddedItems()
        {
            // Arrange
            InitializeMocks();
            TokenProcessor processor = GetTokenProcessor();
            string[] expectedKeys = new[] { "token1", "token2", "token3" };
            int expectedTokenCount = expectedKeys.Length;

            // Act
            processor.TokenDictionary.Add("token1", "value1");
            processor.TokenDictionary.Add("token2", "value2");
            processor.TokenDictionary.Add("token3", "value3");

            // Assert
            processor.TokenDictionary
                .Should()
                .HaveCount(expectedTokenCount);
            processor.TokenDictionary
                .Should()
                .ContainKeys(expectedKeys);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TokenProcessor_ConstructWithNullLocater_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () => { TokenProcessor processor = new(_logger.Object, null!, _nameValidater.Object); };
            string expected = GetNullDependencyMessage(ClassNames.TokenProcessorClass,
                                                       ServiceNames.LocaterService,
                                                       ServiceParameterNames.LocaterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TokenProcessor_ConstructWithNullLogger_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () => { TokenProcessor processor = new(null!, _locater.Object, _nameValidater.Object); };
            string expected = GetNullDependencyMessage(ClassNames.TokenProcessorClass,
                                                       ServiceNames.LoggerService,
                                                       ServiceParameterNames.LoggerParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TokenProcessor_ConstructWithNullNameValidater_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () => { TokenProcessor processor = new(_logger.Object, _locater.Object, null!); };
            string expected = GetNullDependencyMessage(ClassNames.TokenProcessorClass,
                                                       ServiceNames.NameValidaterService,
                                                       ServiceParameterNames.NameValidaterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TokenProcessor_ConstructWithValidDependencies_InitializesEmptyTokenDictionary()
        {
            // Arrange/Act
            InitializeMocks();
            TokenProcessor processor = GetTokenProcessor();

            // Assert
            processor.TokenDictionary
                .Should()
                .NotBeNull();
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            MocksVerifyNoOtherCalls();
        }

        private static string GenerateToken(string tokenName, bool isEscaped = false)
            => isEscaped
            ? $"{TokenEscapeChar}{TokenStart}{tokenName}{TokenEnd}"
            : $"{TokenStart}{tokenName}{TokenEnd}";

        private TokenProcessor GetTokenProcessor()
            => new(_logger.Object, _locater.Object, _nameValidater.Object);

        private void InitializeMocks()
        {
            _locater.Reset();
            _logger.Reset();
            _nameValidater.Reset();
            _verifier.Reset();
        }

        private void MocksVerifyNoOtherCalls()
        {
            _locater.VerifyNoOtherCalls();
            _logger.VerifyNoOtherCalls();
            _nameValidater.VerifyNoOtherCalls();
        }

        private void SetTokenDelimiters_Test(string tokenStart,
                                             string tokenEnd,
                                             char tokenEscapeChar,
                                             bool isValid = true,
                                             string? message = null,
                                             string? arg1 = null,
                                             string? arg2 = null)
        {
            // Arrange
            InitializeMocks();

            if (message is not null)
            {
                _logger
                    .Setup(logger => logger.Log(message, arg1, arg2))
                    .Verifiable(Times.Once);
            }

            TokenProcessor processor = GetTokenProcessor();

            // Act
            bool actual = processor.SetTokenDelimiters(tokenStart, tokenEnd, tokenEscapeChar);

            // Assert
            actual
                .Should()
                .Be(isValid);

            if (isValid)
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
                    .Be(TokenStart);
                processor.TokenEnd
                    .Should()
                    .Be(TokenEnd);
                processor.TokenEscapeChar
                    .Should()
                    .Be(TokenEscapeChar);
            }

            VerifyMocks();
        }

        private void VerifyMocks()
        {
            if (_logger.Setups.Any())
            {
                _logger.Verify();
            }

            if (_locater.Setups.Any())
            {
                _locater.Verify();
            }

            if (_nameValidater.Setups.Any())
            {
                _nameValidater.Verify();
            }

            MocksVerifyNoOtherCalls();
            _verifier.Verify();
        }
    }
}