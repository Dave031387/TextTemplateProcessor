namespace TextTemplateProcessor
{
    using System.Linq.Expressions;

    public class IndentProcessorTests
    {
        private const int DefaultTabSize = 4;
        private const int LineNumber = 5;
        private const string SegmentName = "Segment1";
        private readonly Expression<Func<ILocater, string>> _currentSegmentExpression = x => x.CurrentSegment;
        private readonly Expression<Func<ILocater, int>> _lineNumberExpression = x => x.LineNumber;
        private readonly Mock<ILocater> _locater = new();
        private readonly (string, int) _location = (SegmentName, LineNumber);
        private readonly Expression<Func<ILocater, (string, int)>> _locationExpression = x => x.Location;
        private readonly Mock<ILogger> _logger = new();

        public IndentProcessorTests()
        {
            _locater.Reset();
            _logger.Reset();
            _locater.Setup(_currentSegmentExpression).Returns(SegmentName);
            _locater.Setup(_lineNumberExpression).Returns(LineNumber);
            _locater.Setup(_locationExpression).Returns(_location);
        }

        // Case 01 / firstTimeOffset = 0 / isRelative = true / indent < 0 / calculated value < 0 /
        // isOneTime = true
        [Fact]
        public void GetFirstTimeIndent_Case01()
        {
            int firstTimeOffset = 0;
            int initialIndent = 1;
            int textIndent = -2;
            bool isRelative = true;
            bool isOneTime = true;
            int expectedCurrentIndent = initialIndent * DefaultTabSize;
            int expectedReturnValue = 0;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue,
                MsgLeftIndentHasBeenTruncated);
        }

        // Case 02 / firstTimeOffset = 0 / isRelative = true / indent < 0 / calculated value < 0 /
        // isOneTime = false
        [Fact]
        public void GetFirstTimeIndent_Case02()
        {
            int firstTimeOffset = 0;
            int initialIndent = 1;
            int textIndent = -2;
            bool isRelative = true;
            bool isOneTime = false;
            int expectedCurrentIndent = 0;
            int expectedReturnValue = 0;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue,
                MsgLeftIndentHasBeenTruncated);
        }

        // Case 03 / firstTimeOffset = 0 / isRelative = true / indent < 0 / calculated value = 0 /
        // isOneTime = true
        [Fact]
        public void GetFirstTimeIndent_Case03()
        {
            int firstTimeOffset = 0;
            int initialIndent = 1;
            int textIndent = -1;
            bool isRelative = true;
            bool isOneTime = true;
            int expectedCurrentIndent = initialIndent * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 04 / firstTimeOffset = 0 / isRelative = true / indent < 0 / calculated value = 0 /
        // isOneTime = false
        [Fact]
        public void GetFirstTimeIndent_Case04()
        {
            int firstTimeOffset = 0;
            int initialIndent = 1;
            int textIndent = -1;
            bool isRelative = true;
            bool isOneTime = false;
            int expectedCurrentIndent = (initialIndent + textIndent) * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 05 / firstTimeOffset = 0 / isRelative = true / indent < 0 / calculated value > 0 /
        // isOneTime = true
        [Fact]
        public void GetFirstTimeIndent_Case05()
        {
            int firstTimeOffset = 0;
            int initialIndent = 2;
            int textIndent = -1;
            bool isRelative = true;
            bool isOneTime = true;
            int expectedCurrentIndent = initialIndent * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 06 / firstTimeOffset = 0 / isRelative = true / indent < 0 / calculated value > 0 /
        // isOneTime = false
        [Fact]
        public void GetFirstTimeIndent_Case06()
        {
            int firstTimeOffset = 0;
            int initialIndent = 2;
            int textIndent = -1;
            bool isRelative = true;
            bool isOneTime = false;
            int expectedCurrentIndent = (initialIndent + textIndent) * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 07 / firstTimeOffset = 0 / isRelative = true / indent = 0 / calculated value n/a /
        // isOneTime = n/a
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetFirstTimeIndent_Case07(bool isOneTime)
        {
            int firstTimeOffset = 0;
            int initialIndent = 1;
            int textIndent = 0;
            bool isRelative = true;
            int expectedCurrentIndent = initialIndent * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 08 / firstTimeOffset = 0 / isRelative = true / indent > 0 / calculated value > 0 /
        // isOneTime = true
        [Fact]
        public void GetFirstTimeIndent_Case08()
        {
            int firstTimeOffset = 0;
            int initialIndent = 1;
            int textIndent = 1;
            bool isRelative = true;
            bool isOneTime = true;
            int expectedCurrentIndent = initialIndent * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 09 / firstTimeOffset = 0 / isRelative = true / indent > 0 / calculated value > 0 /
        // isOneTime = false
        [Fact]
        public void GetFirstTimeIndent_Case09()
        {
            int firstTimeOffset = 0;
            int initialIndent = 1;
            int textIndent = 1;
            bool isRelative = true;
            bool isOneTime = false;
            int expectedCurrentIndent = (initialIndent + textIndent) * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 10 / firstTimeOffset = 0 / isRelative = false / indent < 0 / calculated value n/a /
        // isOneTime = true
        [Fact]
        public void GetFirstTimeIndent_Case10()
        {
            int firstTimeOffset = 0;
            int initialIndent = 2;
            int textIndent = -1;
            bool isRelative = false;
            bool isOneTime = true;
            int expectedCurrentIndent = initialIndent * DefaultTabSize;
            int expectedReturnValue = 0;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue,
                MsgLeftIndentHasBeenTruncated);
        }

        // Case 11 / firstTimeOffset = 0 / isRelative = false / indent < 0 / calculated value n/a /
        // isOneTime = false
        [Fact]
        public void GetFirstTimeIndent_Case11()
        {
            int firstTimeOffset = 0;
            int initialIndent = 2;
            int textIndent = -1;
            bool isRelative = false;
            bool isOneTime = false;
            int expectedCurrentIndent = 0;
            int expectedReturnValue = 0;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue,
                MsgLeftIndentHasBeenTruncated);
        }

        // Case 12 / firstTimeOffset = 0 / isRelative = false / indent > 0 / calculated value n/a /
        // isOneTime = true
        [Fact]
        public void GetFirstTimeIndent_Case12()
        {
            int firstTimeOffset = 0;
            int initialIndent = 1;
            int textIndent = 0;
            bool isRelative = false;
            bool isOneTime = true;
            int expectedCurrentIndent = initialIndent * DefaultTabSize;
            int expectedReturnValue = 0;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 13 / firstTimeOffset = 0 / isRelative = false / indent > 0 / calculated value n/a /
        // isOneTime = false
        [Fact]
        public void GetFirstTimeIndent_Case13()
        {
            int firstTimeOffset = 0;
            int initialIndent = 1;
            int textIndent = 0;
            bool isRelative = false;
            bool isOneTime = false;
            int expectedCurrentIndent = 0;
            int expectedReturnValue = 0;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 14 / firstTimeOffset = 0 / isRelative = false / indent > 0 / calculated value n/a /
        // isOneTime = true
        [Fact]
        public void GetFirstTimeIndent_Case14()
        {
            int firstTimeOffset = 0;
            int initialIndent = 1;
            int textIndent = 1;
            bool isRelative = false;
            bool isOneTime = true;
            int expectedCurrentIndent = initialIndent * DefaultTabSize;
            int expectedReturnValue = textIndent * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 15 / firstTimeOffset = 0 / isRelative = false / indent > 0 / calculated value n/a /
        // isOneTime = true
        [Fact]
        public void GetFirstTimeIndent_Case15()
        {
            int firstTimeOffset = 0;
            int initialIndent = 2;
            int textIndent = 1;
            bool isRelative = false;
            bool isOneTime = false;
            int expectedCurrentIndent = textIndent * DefaultTabSize;
            int expectedReturnValue = textIndent * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 16 / firstTimeOffset < 0 / isRelative = n/a / indent n/a / calculated value < 0 /
        // isOneTime = n/a
        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void GetFirstTimeIndent_Case16(bool isRelative, bool isOneTime)
        {
            int firstTimeOffset = -2;
            int initialIndent = 1;
            int textIndent = 2;
            int expectedCurrentIndent = 0;
            int expectedReturnValue = 0;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue,
                MsgFirstTimeIndentHasBeenTruncated);
        }

        // Case 17 / firstTimeOffset < 0 / isRelative = n/a / indent n/a / calculated value = 0 /
        // isOneTime = n/a
        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void GetFirstTimeIndent_Case17(bool isRelative, bool isOneTime)
        {
            int firstTimeOffset = -1;
            int initialIndent = 1;
            int textIndent = 2;
            int expectedCurrentIndent = (initialIndent + firstTimeOffset) * DefaultTabSize;
            int expectedReturnValue = (initialIndent + firstTimeOffset) * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 18 / firstTimeOffset < 0 / isRelative = n/a / indent n/a / calculated value > 0 /
        // isOneTime = n/a
        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void GetFirstTimeIndent_Case18(bool isRelative, bool isOneTime)
        {
            int firstTimeOffset = -1;
            int initialIndent = 2;
            int textIndent = -2;
            int expectedCurrentIndent = (initialIndent + firstTimeOffset) * DefaultTabSize;
            int expectedReturnValue = (initialIndent + firstTimeOffset) * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case 19 / firstTimeOffset < 0 / isRelative = n/a / indent n/a / calculated value > 0 /
        // isOneTime = n/a
        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void GetFirstTimeIndent_Case19(bool isRelative, bool isOneTime)
        {
            int firstTimeOffset = 1;
            int initialIndent = 2;
            int textIndent = -2;
            int expectedCurrentIndent = (initialIndent + firstTimeOffset) * DefaultTabSize;
            int expectedReturnValue = (initialIndent + firstTimeOffset) * DefaultTabSize;

            Test_GetFirstTimeIndent(
                firstTimeOffset,
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentIndent,
                expectedReturnValue);
        }

        // Case01 / indent < 0 / isRelative = true / isOneTime = true / calculated value < 0
        [Fact]
        public void GetIndent_Case01()
        {
            int initialIndent = 1;
            int textIndent = -2;
            bool isRelative = true;
            bool isOneTime = true;
            int expectedCurrentValue = initialIndent * DefaultTabSize;
            int expectedReturnValue = 0;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue,
                MsgLeftIndentHasBeenTruncated);
        }

        // Case02 / indent < 0 / isRelative = true / isOneTime = false / calculated value < 0
        [Fact]
        public void GetIndent_Case02()
        {
            int initialIndent = 1;
            int textIndent = -2;
            bool isRelative = true;
            bool isOneTime = false;
            int expectedCurrentValue = 0;
            int expectedReturnValue = 0;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue,
                MsgLeftIndentHasBeenTruncated);
        }

        // Case03 / indent < 0 / isRelative = true / isOneTime = true / calculated value = 0
        [Fact]
        public void GetIndent_Case03()
        {
            int initialIndent = 1;
            int textIndent = -1;
            bool isRelative = true;
            bool isOneTime = true;
            int expectedCurrentValue = initialIndent * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue);
        }

        // Case04 / indent < 0 / isRelative = true / isOneTime = false / calculated value = 0
        [Fact]
        public void GetIndent_Case04()
        {
            int initialIndent = 1;
            int textIndent = -1;
            bool isRelative = true;
            bool isOneTime = false;
            int expectedCurrentValue = (initialIndent + textIndent) * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue);
        }

        // Case05 / indent < 0 / isRelative = true / isOneTime = true / calculated value > 0
        [Fact]
        public void GetIndent_Case05()
        {
            int initialIndent = 2;
            int textIndent = -1;
            bool isRelative = true;
            bool isOneTime = true;
            int expectedCurrentValue = initialIndent * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue);
        }

        // Case06 / indent < 0 / isRelative = true / isOneTime = false / calculated value > 0
        [Fact]
        public void GetIndent_Case06()
        {
            int initialIndent = 2;
            int textIndent = -1;
            bool isRelative = true;
            bool isOneTime = false;
            int expectedCurrentValue = (initialIndent + textIndent) * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue);
        }

        // Case07 / indent = 0 / isRelative = true / isOneTime = n/a / calculated value n/a
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetIndent_Case07(bool isOneTime)
        {
            int initialIndent = 1;
            int textIndent = 0;
            bool isRelative = true;
            int expectedCurrentValue = initialIndent * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue);
        }

        // Case08 / indent > 0 / isRelative = true / isOneTime = true / calculated value n/a
        [Fact]
        public void GetIndent_Case08()
        {
            int initialIndent = 1;
            int textIndent = 1;
            bool isRelative = true;
            bool isOneTime = true;
            int expectedCurrentValue = initialIndent * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue);
        }

        // Case09 / indent > 0 / isRelative = true / isOneTime = false / calculated value n/a
        [Fact]
        public void GetIndent_Case09()
        {
            int initialIndent = 1;
            int textIndent = 1;
            bool isRelative = true;
            bool isOneTime = false;
            int expectedCurrentValue = (initialIndent + textIndent) * DefaultTabSize;
            int expectedReturnValue = (initialIndent + textIndent) * DefaultTabSize;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue);
        }

        // Case10 / indent < 0 / isRelative = false / isOneTime = true / calculated value n/a
        [Fact]
        public void GetIndent_Case10()
        {
            int initialIndent = 2;
            int textIndent = -1;
            bool isRelative = false;
            bool isOneTime = true;
            int expectedCurrentValue = initialIndent * DefaultTabSize;
            int expectedReturnValue = 0;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue,
                MsgLeftIndentHasBeenTruncated);
        }

        // Case11 / indent < 0 / isRelative = false / isOneTime = false / calculated value n/a
        [Fact]
        public void GetIndent_Case11()
        {
            int initialIndent = 2;
            int textIndent = -1;
            bool isRelative = false;
            bool isOneTime = false;
            int expectedCurrentValue = 0;
            int expectedReturnValue = 0;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue,
                MsgLeftIndentHasBeenTruncated);
        }

        // Case12 / indent = 0 / isRelative = false / isOneTime = true / calculated value n/a
        [Fact]
        public void GetIndent_Case12()
        {
            int initialIndent = 1;
            int textIndent = 0;
            bool isRelative = false;
            bool isOneTime = true;
            int expectedCurrentValue = initialIndent * DefaultTabSize;
            int expectedReturnValue = 0;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue);
        }

        // Case13 / indent = 0 / isRelative = false / isOneTime = false / calculated value n/a
        [Fact]
        public void GetIndent_Case13()
        {
            int initialIndent = 1;
            int textIndent = 0;
            bool isRelative = false;
            bool isOneTime = false;
            int expectedCurrentValue = 0;
            int expectedReturnValue = 0;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue);
        }

        // Case14 / indent > 0 / isRelative = false / isOneTime = true / calculated value n/a
        [Fact]
        public void GetIndent_Case14()
        {
            int initialIndent = 1;
            int textIndent = 1;
            bool isRelative = false;
            bool isOneTime = true;
            int expectedCurrentValue = initialIndent * DefaultTabSize;
            int expectedReturnValue = textIndent * DefaultTabSize;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue);
        }

        // Case15 / indent > 0 / isRelative = false / isOneTime = false / calculated value n/a
        [Fact]
        public void GetIndent_Case15()
        {
            int initialIndent = 1;
            int textIndent = 1;
            bool isRelative = false;
            bool isOneTime = false;
            int expectedCurrentValue = textIndent * DefaultTabSize;
            int expectedReturnValue = textIndent * DefaultTabSize;

            Test_GetIndent(
                initialIndent,
                textIndent,
                isRelative,
                isOneTime,
                expectedCurrentValue,
                expectedReturnValue);
        }

        [Fact]
        public void IndentProcessor_LocaterIsNull_ThrowsException()
        {
            // Arrange
            Action action = () => { IndentProcessor processor = new(_logger.Object, null!); };
            string expected = string.Format(MsgDependencyIsNull, nameof(IndentProcessor), nameof(ILocater))
                + " (Parameter 'locater')";

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void IndentProcessor_LoggerIsNull_ThrowsException()
        {
            // Arrange
            Action action = () => { IndentProcessor processor = new(null!, _locater.Object); };
            string expected = string.Format(MsgDependencyIsNull, nameof(IndentProcessor), nameof(ILogger))
                + " (Parameter 'logger')";

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void IndentProcessor_OnInitialization_InitializesProperties()
        {
            // Arrange/Act
            IndentProcessor processor = new(_logger.Object, _locater.Object);

            // Assert
            processor.CurrentIndent
                .Should()
                .Be(0);
            processor.TabSize
                .Should()
                .Be(4);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("1x")]
        public void IsValidIndentValue_StringIsNotANumber_LogsMessageAndReturnsFalse(string? numberString)
        {
            // Arrange
            Expression<Action<ILogger>> loggerExpression = GetLoggerExpression(
                LogEntryType.Parsing,
                _location,
                MsgIndentValueMustBeValidNumber,
                numberString!);
            _logger.Setup(loggerExpression);
            IndentProcessor processor = new(_logger.Object, _locater.Object);

            // Act
            bool isValid = processor.IsValidIndentValue(numberString!, out int actual);

            // Assert
            isValid
                .Should()
                .BeFalse();
            actual
                .Should()
                .Be(0);
            _logger
                .Verify(loggerExpression, Times.Once);
            _locater
                .Verify(_locationExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("-9", -9)]
        [InlineData("-8", -8)]
        [InlineData("-7", -7)]
        [InlineData("0", 0)]
        [InlineData("7", 7)]
        [InlineData("8", 8)]
        [InlineData("9", 9)]
        public void IsValidIndentValue_ValueIsInRange_ParsesValueAndReturnsTrue(string numberString, int expected)
        {
            // Arrange
            IndentProcessor processor = new(_logger.Object, _locater.Object);

            // Act
            bool isValid = processor.IsValidIndentValue(numberString, out int actual);

            // Assert
            isValid
                .Should()
                .BeTrue();
            actual
                .Should()
                .Be(expected);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("-12")]
        [InlineData("-11")]
        [InlineData("-10")]
        [InlineData("10")]
        [InlineData("11")]
        [InlineData("12")]
        public void IsValidIndentValue_ValueIsOutOfRange_LogsMessageAndReturnsFalse(string numberString)
        {
            // Arrange
            Expression<Action<ILogger>> loggerExpression = GetLoggerExpression(
                LogEntryType.Parsing,
                _location,
                MsgIndentValueOutOfRange,
                numberString);
            _logger.Setup(loggerExpression);
            IndentProcessor processor = new(_logger.Object, _locater.Object);

            // Act
            bool isValid = processor.IsValidIndentValue(numberString, out int actual);

            // Assert
            isValid
                .Should()
                .BeFalse();
            actual
                .Should()
                .Be(0);
            _logger
                .Verify(loggerExpression, Times.Once);
            _locater
                .Verify(_locationExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("nine")]
        [InlineData("3 4")]
        public void IsValidTabSizeValue_StringIsNotANumber_LogsMessageAndReturnsFalse(string? numberString)
        {
            // Arrange
            Expression<Action<ILogger>> loggerExpression = GetLoggerExpression(
                LogEntryType.Parsing,
                _location,
                MsgTabSizeValueMustBeValidNumber,
                numberString!);
            _logger.Setup(loggerExpression);
            IndentProcessor processor = new(_logger.Object, _locater.Object);

            // Act
            bool isValid = processor.IsValidTabSizeValue(numberString!, out int actual);

            // Assert
            isValid
                .Should()
                .BeFalse();
            actual
                .Should()
                .Be(0);
            _logger
                .Verify(loggerExpression, Times.Once);
            _locater
                .Verify(_locationExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("2", 2)]
        [InlineData("3", 3)]
        [InlineData("7", 7)]
        [InlineData("8", 8)]
        [InlineData("9", 9)]
        public void IsValidTabSizeValue_ValueIsInRange_ParsesValueAndReturnsTrue(string numberString, int expected)
        {
            // Arrange
            IndentProcessor processor = new(_logger.Object, _locater.Object);

            // Act
            bool isValid = processor.IsValidTabSizeValue(numberString, out int actual);

            // Assert
            isValid
                .Should()
                .BeTrue();
            actual
                .Should()
                .Be(expected);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("-2")]
        [InlineData("-1")]
        [InlineData("0")]
        [InlineData("10")]
        [InlineData("11")]
        [InlineData("12")]
        public void IsValidTabSizeValue_ValueIsOutOfRange_LogsMessageAndReturnsFalse(string numberString)
        {
            // Arrange
            Expression<Action<ILogger>> loggerExpression = GetLoggerExpression(
                LogEntryType.Parsing,
                _location,
                MsgTabSizeValueOutOfRange,
                numberString);
            _logger.Setup(loggerExpression);
            IndentProcessor processor = new(_logger.Object, _locater.Object);

            // Act
            bool isValid = processor.IsValidTabSizeValue(numberString, out int actual);

            // Assert
            isValid
                .Should()
                .BeFalse();
            actual
                .Should()
                .Be(0);
            _logger
                .Verify(loggerExpression, Times.Once);
            _locater
                .Verify(_locationExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void Reset_WhenCalled_ResetsCurrentIndentAndTabSize()
        {
            // Arrange
            IndentProcessor processor = new(_logger.Object, _locater.Object);
            SetCurrentIndent(processor, 2);
            processor.SetTabSize(2);

            // Act
            processor.Reset();

            // Assert
            processor.CurrentIndent
                .Should()
                .Be(0);
            processor.TabSize
                .Should()
                .Be(DefaultTabSize);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void RestoreCurrentIndentLocation_SavedLocationDoesNotExist_DoesNothing()
        {
            // Arrange
            IndentProcessor processor = new(_logger.Object, _locater.Object);
            int expectedTabSize = DefaultTabSize + 1;
            processor.SetTabSize(expectedTabSize);
            int expectedIndent = 3;
            SetCurrentIndent(processor, expectedIndent);

            // Act
            processor.RestoreCurrentIndentLocation();

            // Assert
            processor.TabSize
                .Should()
                .Be(expectedTabSize);
            processor.CurrentIndent
                .Should()
                .Be(expectedIndent * expectedTabSize);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        [Fact]
        public void RestoreCurrentIndentLocation_SavedLocationExists_RestoresSavedLocation()
        {
            // Arrange
            static void currentSegmentAction(ILocater x)
            {
                x.CurrentSegment = SegmentName;
            }

            static void lineNumberAction(ILocater x)
            {
                x.LineNumber = LineNumber;
            }

            _locater.SetupSet(currentSegmentAction);
            _locater.SetupSet(lineNumberAction);
            IndentProcessor processor = new(_logger.Object, _locater.Object);
            int expectedTabSize = DefaultTabSize + 1;
            processor.SetTabSize(expectedTabSize);
            int expectedIndent = 3;
            SetCurrentIndent(processor, expectedIndent);
            processor.SaveCurrentIndentLocation();
            SetCurrentIndent(processor, 6);
            processor.SetTabSize(DefaultTabSize - 1);

            // Act
            processor.RestoreCurrentIndentLocation();

            // Assert
            processor.TabSize
                .Should()
                .Be(expectedTabSize);
            processor.CurrentIndent
                .Should()
                .Be(expectedIndent * expectedTabSize);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .Verify(_currentSegmentExpression, Times.Once);
            _locater
                .Verify(_lineNumberExpression, Times.Once);
            _locater
                .VerifySet(currentSegmentAction, Times.Once);
            _locater
                .VerifySet(lineNumberAction, Times.Once);
            _locater
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-2)]
        public void SetTabSize_NewValueLessThanMinimum_LogsMessageAndSetsTabSizeToMinimum(int tabSize)
        {
            // Arrange
            IndentProcessor processor = new(_logger.Object, _locater.Object);
            int expectedTabSize = 1;
            Expression<Action<ILogger>> loggerExpression = GetLoggerExpression(
                LogEntryType.Setup,
                MsgTabSizeTooSmall,
                expectedTabSize.ToString());
            _logger.Setup(loggerExpression);

            // Act
            processor.SetTabSize(tabSize);

            // Assert
            processor.TabSize
                .Should()
                .Be(expectedTabSize);
            _logger
                .Verify(loggerExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        public void SetTabSize_NewValueMoreThanMaximum_LogsMessageAndSetsTabSizeToMaximum(int tabSize)
        {
            // Arrange
            IndentProcessor processor = new(_logger.Object, _locater.Object);
            int expectedTabSize = 9;
            Expression<Action<ILogger>> loggerExpression = GetLoggerExpression(
                LogEntryType.Setup,
                MsgTabSizeTooLarge,
                expectedTabSize.ToString());
            _logger.Setup(loggerExpression);

            // Act
            processor.SetTabSize(tabSize);

            // Assert
            processor.TabSize
                .Should()
                .Be(expectedTabSize);
            _logger
                .Verify(loggerExpression, Times.Once);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(8)]
        [InlineData(9)]
        public void SetTabSize_NewValueWithinValidRange_SetsTabSizeToValue(int expectedTabSize)
        {
            // Arrange
            IndentProcessor processor = new(_logger.Object, _locater.Object);

            // Act
            processor.SetTabSize(expectedTabSize);

            // Assert
            processor.TabSize
                .Should()
                .Be(expectedTabSize);
            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        private static Expression<Action<ILogger>> GetLoggerExpression(
            LogEntryType type,
            (string segmentName, int lineNumber) location,
            string message,
            string arg) => x => x.Log(type, location, message, arg);

        private static Expression<Action<ILogger>> GetLoggerExpression(
            LogEntryType type,
            string message,
            string arg) => x => x.Log(type, message, arg);

        private static void SetCurrentIndent(IIndentProcessor processor, int indent)
        {
            TextItem textItem = new(indent, false, false, string.Empty);
            processor.GetIndent(textItem);
        }

        private void Test_GetFirstTimeIndent(
            int firstTimeOffset,
            int initialIndent,
            int textIndent,
            bool isRelative,
            bool isOneTime,
            int expectedCurrentIndent,
            int expectedReturnValue,
            string? message = null)
        {
            // Arrange
            Expression<Action<ILogger>>? loggerExpression = null;

            if (message is not null)
            {
                loggerExpression = GetLoggerExpression(
                    LogEntryType.Generating,
                    _location,
                    message,
                    SegmentName);

                _logger.Setup(loggerExpression);
            }

            IndentProcessor processor = new(_logger.Object, _locater.Object);
            SetCurrentIndent(processor, initialIndent);
            TextItem textItem = new(textIndent, isRelative, isOneTime, string.Empty);

            // Act
            int actual = processor.GetFirstTimeIndent(firstTimeOffset, textItem);

            // Assert
            actual
                .Should()
                .Be(expectedReturnValue);
            processor.CurrentIndent
                .Should()
                .Be(expectedCurrentIndent);

            if (message is not null)
            {
                _logger
                    .Verify(loggerExpression!, Times.Once);
                _locater
                    .Verify(_currentSegmentExpression, Times.Once);
                _locater
                    .Verify(_locationExpression, Times.Once);
            }

            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }

        private void Test_GetIndent(
            int initialIndent,
            int textIndent,
            bool isRelative,
            bool isOneTime,
            int expectedCurrentIndent,
            int expectedReturnValue,
            string? message = null)
        {
            // Arrange
            Expression<Action<ILogger>>? loggerExpression = null;

            if (message is not null)
            {
                loggerExpression = GetLoggerExpression(
                    LogEntryType.Generating,
                    _location,
                    message,
                    SegmentName);

                _logger.Setup(loggerExpression);
            }

            IndentProcessor processor = new(_logger.Object, _locater.Object);
            SetCurrentIndent(processor, initialIndent);
            TextItem textItem = new(textIndent, isRelative, isOneTime, string.Empty);

            // Act
            int actual = processor.GetIndent(textItem);

            // Assert
            actual
                .Should()
                .Be(expectedReturnValue);
            processor.CurrentIndent
                .Should()
                .Be(expectedCurrentIndent);

            if (message is not null)
            {
                _logger
                    .Verify(loggerExpression!, Times.Once);
                _locater
                    .Verify(_currentSegmentExpression, Times.Once);
                _locater
                    .Verify(_locationExpression, Times.Once);
            }

            _logger
                .VerifyNoOtherCalls();
            _locater
                .VerifyNoOtherCalls();
        }
    }
}