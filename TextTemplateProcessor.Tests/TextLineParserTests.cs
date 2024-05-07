namespace TextTemplateProcessor
{
    using System;

    public class TextLineParserTests
    {
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<ITokenProcessor> _tokenProcessor = new();

        [Fact]
        public void IsCommentLine_TextIsCommentLine_ReturnsTrue()
        {
            // Arrange
            InitializeMocks();
            string text = GenerateTextLine(Comment, "This is a comment");
            TextLineParser parser = GetTextLineParser();

            // Act
            bool actual = parser.IsCommentLine(text);

            // Assert
            actual
                .Should()
                .BeTrue();
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("// ")]
        [InlineData("/  ")]
        [InlineData(" //")]
        [InlineData("  /")]
        [InlineData(" / ")]
        [InlineData("/ /")]
        [InlineData(IndentUnchanged)]
        [InlineData(SegmentHeaderCode)]
        public void IsCommentLine_TextIsNotCommentLine_ReturnsFalse(string controlCode)
        {
            // Arrange
            InitializeMocks();
            string text = GenerateTextLine(controlCode, "This is not a comment");
            TextLineParser parser = GetTextLineParser();

            // Act
            bool actual = parser.IsCommentLine(text);

            // Assert
            actual
                .Should()
                .BeFalse();
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("## ")]
        [InlineData("#  ")]
        [InlineData(" ##")]
        [InlineData("  #")]
        [InlineData(" # ")]
        [InlineData("# #")]
        [InlineData(IndentUnchanged)]
        [InlineData(Comment)]
        public void IsSegmentHeader_TextIsNotSegmentHeader_ReturnsFalse(string controlCode)
        {
            // Arrange
            InitializeMocks();
            string text = GenerateTextLine(controlCode, "Not a segment header");
            TextLineParser parser = GetTextLineParser();

            // Act
            bool actual = parser.IsSegmentHeader(text);

            // Assert
            actual
                .Should()
                .BeFalse();
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void IsSegmentHeader_TextIsSegmentHeader_ReturnsTrue()
        {
            // Arrange
            InitializeMocks();
            string text = GenerateTextLine(SegmentHeaderCode, "Segment1");
            TextLineParser parser = GetTextLineParser();

            // Act
            bool actual = parser.IsSegmentHeader(text);

            // Assert
            actual
                .Should()
                .BeTrue();
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void IsTextLine_ControlCodeIsBlank_ReturnsTrue()
        {
            // Arrange
            InitializeMocks();
            string text = GenerateTextLine(IndentUnchanged, "This is a text line");
            TextLineParser parser = GetTextLineParser();

            // Act
            bool actual = parser.IsTextLine(text);

            // Assert
            actual
                .Should()
                .BeTrue();
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData($"{IndentAbsolute}a")]
        [InlineData($"{IndentAbsoluteOneTime}b")]
        [InlineData($"{IndentLeftOneTime}c")]
        [InlineData($"{IndentLeftRelative}d")]
        [InlineData($"{IndentRightOneTime}e")]
        [InlineData($"{IndentRightRelative}f")]
        [InlineData("  1")]
        [InlineData(Comment)]
        [InlineData(SegmentHeaderCode)]
        public void IsTextLine_NotValidTextLine_ReturnsFalse(string controlCode)
        {
            // Arrange
            InitializeMocks();
            string text = GenerateTextLine(controlCode, "Not a valid text line");
            TextLineParser parser = GetTextLineParser();

            // Act
            bool actual = parser.IsTextLine(text);

            // Assert
            actual
                .Should()
                .BeFalse();
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData($"{IndentAbsolute}0")]
        [InlineData($"{IndentAbsoluteOneTime}1")]
        [InlineData($"{IndentLeftOneTime}2")]
        [InlineData($"{IndentLeftRelative}7")]
        [InlineData($"{IndentRightOneTime}8")]
        [InlineData($"{IndentRightRelative}9")]
        public void IsTextLine_ValidControlCode_ReturnsTrue(string controlCode)
        {
            // Arrange
            InitializeMocks();
            string text = GenerateTextLine(controlCode, "This is a text line");
            TextLineParser parser = GetTextLineParser();

            // Act
            bool actual = parser.IsTextLine(text);

            // Assert
            actual
                .Should()
                .BeTrue();
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void IsValidPrefix_FourthCharacterNotBlank_LogsMessageAndReturnsFalse()
        {
            // Arrange
            InitializeMocks();
            string text = $"{SegmentHeaderCode}#Segment1";
            _logger
                .Setup(logger => logger.Log(MsgFourthCharacterMustBeBlank, text, null))
                .Verifiable(Times.Once);
            TextLineParser parser = GetTextLineParser();

            // Act
            bool actual = parser.IsValidPrefix(text);

            // Assert
            actual
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void IsValidPrefix_InvalidControlCode_LogsMessageAndReturnsFalse()
        {
            // Arrange
            InitializeMocks();
            string text = GenerateTextLine("@.1", "Text");
            _logger
                .Setup(logger => logger.Log(MsgInvalidControlCode, text, null))
                .Verifiable(Times.Once);
            TextLineParser parser = GetTextLineParser();

            // Act
            bool actual = parser.IsValidPrefix(text);

            // Assert
            actual
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Theory]
        [InlineData(IndentAbsolute)]
        [InlineData("#")]
        [InlineData("")]
        public void IsValidPrefix_LineLengthLessThanThree_LogsMessageAndReturnsFalse(string text)
        {
            // Arrange
            InitializeMocks();
            _logger
                .Setup(logger => logger.Log(MsgMinimumLineLengthInTemplateFileIs3, null, null))
                .Verifiable(Times.Once);
            TextLineParser parser = GetTextLineParser();

            // Act
            bool actual = parser.IsValidPrefix(text);

            // Assert
            actual
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Theory]
        [InlineData($"{IndentAbsolute}0")]
        [InlineData($"{IndentAbsoluteOneTime}3")]
        [InlineData($"{IndentLeftOneTime}4")]
        [InlineData($"{IndentLeftRelative}5")]
        [InlineData($"{IndentRightOneTime}6")]
        [InlineData($"{IndentRightRelative}9")]
        [InlineData(IndentUnchanged)]
        [InlineData(Comment)]
        [InlineData(SegmentHeaderCode)]
        public void IsValidPrefix_ValidControlCode_ReturnsTrue(string controlCode)
        {
            // Arrange
            InitializeMocks();
            string textLine = GenerateTextLine(controlCode, "Text line");
            TextLineParser parser = GetTextLineParser();

            // Act
            bool actual = parser.IsValidPrefix(textLine);

            // Assert
            actual
                .Should()
                .BeTrue();
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(IndentUnchanged, "", 0, true, false)]
        [InlineData($"{IndentAbsolute}1", "Text line 1", 1, false, false)]
        [InlineData($"{IndentAbsoluteOneTime}3", "Text line 2", 3, false, true)]
        [InlineData($"{IndentLeftOneTime}4", "", -4, true, true)]
        [InlineData($"{IndentLeftRelative}5", "Text line 3", -5, true, false)]
        [InlineData($"{IndentRightOneTime}6", "Text line 4", 6, true, true)]
        [InlineData($"{IndentRightRelative}9", "Text line 5", 9, true, false)]
        public void ParseTextLine_ValidTextLine_ReturnsExpectedTextItem(string controlCode,
                                                                        string expectedText,
                                                                        int indent,
                                                                        bool isRelative,
                                                                        bool isOneTime)
        {
            // Arrange
            InitializeMocks();
            string textLine = GenerateTextLine(controlCode, expectedText);
            string actualText = string.Empty;
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ExtractTokens(ref It.Ref<string>.IsAny))
                .Callback((ref string passedText) =>
                {
                    actualText = passedText;
                })
                .Verifiable(Times.Once);
            TextLineParser parser = GetTextLineParser();
            TextItem expectedTextItem = new(indent, isRelative, isOneTime, expectedText);

            // Act
            TextItem actualTextItem = parser.ParseTextLine(textLine);

            // Assert
            actualTextItem
                .Should()
                .Be(expectedTextItem);
            actualText
                .Should()
                .Be(expectedText);
            VerifyMocks();
        }

        [Fact]
        public void TextLineParser_ConstructWithNullLogger_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            string expected = GetNullDependencyMessage(ClassNames.TextLineParserClass,
                                                       ServiceNames.LoggerService,
                                                       ServiceParameterNames.LoggerParameter);
            Action action = () => { TextLineParser parser = new(null!, _tokenProcessor.Object); };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextLineParser_ConstructWithNullTokenProcessor_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            string expected = GetNullDependencyMessage(ClassNames.TextLineParserClass,
                                                       ServiceNames.TokenProcessorService,
                                                       ServiceParameterNames.TokenProcessorParameter);
            Action action = () => { TextLineParser parser = new(_logger.Object, null!); };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void TextLineParser_ConstructWithValidDependencies_ShouldNotThrow()
        {
            // Arrange
            InitializeMocks();
            Action action = () => GetTextLineParser();

            // Act/Assert
            action
                .Should()
                .NotThrow();
            MocksVerifyNoOtherCalls();
        }

        private static string GenerateTextLine(string controlCode, string text) => $"{controlCode} {text}";

        private TextLineParser GetTextLineParser()
            => new(_logger.Object, _tokenProcessor.Object);

        private void InitializeMocks()
        {
            _logger.Reset();
            _tokenProcessor.Reset();
        }

        private void MocksVerifyNoOtherCalls()
        {
            _logger.VerifyNoOtherCalls();
            _tokenProcessor.VerifyNoOtherCalls();
        }

        private void VerifyMocks()
        {
            if (_logger.Setups.Any())
            {
                _logger.Verify();
            }

            if (_tokenProcessor.Setups.Any())
            {
                _tokenProcessor.Verify();
            }

            MocksVerifyNoOtherCalls();
        }
    }
}