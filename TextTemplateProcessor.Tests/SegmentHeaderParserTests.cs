namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Interfaces;
    using System.Linq.Expressions;

    public class SegmentHeaderParserTests
    {
        private const int LineNumber = 1;
        private const string SegmentName = "Segment1";
        private readonly Mock<IDefaultSegmentNameGenerator> _defaultSegmentNameGenerator = new();
        private readonly Mock<IIndentProcessor> _indentProcessor = new();
        private readonly Mock<ILocater> _locater = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly MockHelper _mh = new();
        private readonly Mock<INameValidater> _nameValidater = new();
        private Expression<Action<ILogger>> _loggerExpression = x => x.Log("test", null, null);
        private Expression<Func<INameValidater, bool>> _nameValidaterExpression1 = x => false;
        private Expression<Func<INameValidater, bool>> _nameValidaterExpression2 = x => false;

        public SegmentHeaderParserTests()
        {
            _defaultSegmentNameGenerator.Reset();
            _indentProcessor.Reset();
            _locater.Reset();
            _logger.Reset();
            _nameValidater.Reset();
            _mh.SetupLocater(_locater, SegmentName, LineNumber);
            _nameValidaterExpression1 = MockHelper.SetupNameValidater(_nameValidater, SegmentName, true);
        }

        [Theory]
        [InlineData("-10")]
        [InlineData("10")]
        [InlineData("X")]
        public void ParseSegmentHeader_FirstTimeIndentIsInvalid_LogsMessage(string optionValue)
        {
            // Arrange
            int indentValue = 0;
            _mh.SetupIndentProcessorForIndentValues(_indentProcessor,
                                                    optionValue,
                                                    false,
                                                    indentValue,
                                                    0);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgFirstTimeIndentIsInvalid,
                                                       SegmentName,
                                                       optionValue);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {FirstTimeIndentOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _indentProcessor.Verify(_mh.IsValidIndentValueExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("-9", -9)]
        [InlineData("-5", -5)]
        [InlineData("-1", -1)]
        [InlineData("1", 1)]
        [InlineData("5", 5)]
        [InlineData("9", 9)]
        public void ParseSegmentHeader_FirstTimeIndentIsValidAndNotZero_SavesTheFirstTimeIndentValue(string optionValue, int indentValue)
        {
            // Arrange
            _mh.SetupIndentProcessorForIndentValues(_indentProcessor,
                                                    optionValue,
                                                    true,
                                                    indentValue,
                                                    indentValue);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {FirstTimeIndentOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new()
            {
                FirstTimeIndent = indentValue
            };

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _indentProcessor.Verify(_mh.IsValidIndentValueExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_FirstTimeIndentIsZero_LogsMessageAndSavesFirstTimeIndentValue()
        {
            // Arrange
            string optionValue = "0";
            int indentValue = 0;
            _mh.SetupIndentProcessorForIndentValues(_indentProcessor,
                                                    optionValue,
                                                    true,
                                                    indentValue,
                                                    0);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgFirstTimeIndentSetToZero,
                                                       SegmentName);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {FirstTimeIndentOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _indentProcessor.Verify(_mh.IsValidIndentValueExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(FirstTimeIndentOption, "1")]
        [InlineData(TabSizeOption, "1")]
        [InlineData(PadSegmentNameOption, "Duplicate")]
        public void ParseSegmentHeader_HeaderHasDuplicateSegmentOptions_LogsMessage(string optionName, string optionValue)
        {
            // Arrange
            string indentStringValue = "2";
            int indentIntegerValue = 2;
            _mh.SetupIndentProcessorForIndentValues(_indentProcessor,
                                                    indentStringValue,
                                                    true,
                                                    indentIntegerValue,
                                                    indentIntegerValue);
            string tabSizeStringValue = "3";
            int tabSizeIntegerValue = 3;
            _mh.SetupIndentProcessorForTabSizeValues(_indentProcessor,
                                                     tabSizeStringValue,
                                                     true,
                                                     tabSizeIntegerValue,
                                                     tabSizeIntegerValue);
            string padSegmentValue = "PadSegment";
            _nameValidaterExpression2 = MockHelper.SetupNameValidater(_nameValidater, padSegmentValue, true);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgFoundDuplicateOptionNameOnHeaderLine,
                                                       SegmentName,
                                                       optionName);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} "
                + $"{PadSegmentNameOption}={padSegmentValue}, "
                + $"{FirstTimeIndentOption}={indentIntegerValue}, "
                + $"{TabSizeOption}={tabSizeIntegerValue}, "
                + $"{optionName}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new()
            {
                FirstTimeIndent = indentIntegerValue,
                TabSize = tabSizeIntegerValue,
                PadSegment = padSegmentValue
            };

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _nameValidater.Verify(_nameValidaterExpression2, Times.Once);
            _indentProcessor.Verify(_mh.IsValidIndentValueExpression, Times.Once);
            _indentProcessor.Verify(_mh.IsValidTabSizeExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(PadSegmentNameOption, "Pad1", FirstTimeIndentOption, "2", TabSizeOption, "3", "Pad1", 2, 3)]
        [InlineData(PadSegmentNameOption, "Pad2", TabSizeOption, "5", FirstTimeIndentOption, "-1", "Pad2", -1, 5)]
        [InlineData(FirstTimeIndentOption, "-2", PadSegmentNameOption, "Pad3", TabSizeOption, "4", "Pad3", -2, 4)]
        [InlineData(FirstTimeIndentOption, "1", TabSizeOption, "6", PadSegmentNameOption, "Pad4", "Pad4", 1, 6)]
        [InlineData(TabSizeOption, "2", PadSegmentNameOption, "Pad5", FirstTimeIndentOption, "-4", "Pad5", -4, 2)]
        [InlineData(TabSizeOption, "9", FirstTimeIndentOption, "3", PadSegmentNameOption, "Pad6", "Pad6", 3, 9)]
        public void ParseSegmentHeader_HeaderHasMultipleValidOptions_ParsesAllOptionsAndSavesValues(string option1,
                                                                                                    string value1,
                                                                                                    string option2,
                                                                                                    string value2,
                                                                                                    string option3,
                                                                                                    string value3,
                                                                                                    string padSegment,
                                                                                                    int firstTimeIndent,
                                                                                                    int tabSizeValue)
        {
            // Arrange
            string indentStringValue = option1 is FirstTimeIndentOption ? value1 : option2 is FirstTimeIndentOption ? value2 : value3;
            _mh.SetupIndentProcessorForIndentValues(_indentProcessor,
                                                    indentStringValue,
                                                    true,
                                                    firstTimeIndent,
                                                    firstTimeIndent);
            string tabSizeStringValue = option1 is TabSizeOption ? value1 : option2 is TabSizeOption ? value2 : value3;
            _mh.SetupIndentProcessorForTabSizeValues(_indentProcessor,
                                                     tabSizeStringValue,
                                                     true,
                                                     tabSizeValue,
                                                     tabSizeValue);
            _nameValidaterExpression2 = MockHelper.SetupNameValidater(_nameValidater, padSegment, true);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {option1}={value1}, {option2}={value2}, {option3}={value3}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new()
            {
                FirstTimeIndent = firstTimeIndent,
                TabSize = tabSizeValue,
                PadSegment = padSegment
            };

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _nameValidater.Verify(_nameValidaterExpression2, Times.Once);
            _indentProcessor.Verify(_mh.IsValidIndentValueExpression, Times.Once);
            _indentProcessor.Verify(_mh.IsValidTabSizeExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_HeaderLineLessThanFiveCharacters_LogsMessageAndReturnsDefaultControlItem()
        {
            // Arrange
            string segmentHeader = $"{SegmentHeaderCode} ";
            string segmentName = $"{DefaultSegmentNamePrefix}1";
            _mh.SetupLocater(_locater, segmentName, LineNumber);
            _mh.SetupDefaultSegmentNameGenerator(_defaultSegmentNameGenerator);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgSegmentNameMustStartInColumn5,
                                                       segmentName);
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _defaultSegmentNameGenerator.Verify(_mh.DefaultSegmentNameExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_OptionNameIsInvalid_LogsMessage()
        {
            // Arrange
            string optionString = "Option1=value";
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgUnknownSegmentOptionFound,
                                                       SegmentName,
                                                       optionString);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {optionString}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_OptionNameMissingBeforeEqualsSign_LogsMessage()
        {
            // Arrange
            string optionString = "=value";
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgOptionNameMustPrecedeEqualsSign,
                                                       SegmentName);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {optionString}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_OptionNameNotFollowedByEqualsSign_LogsMessage()
        {
            // Arrange
            string optionName = "Option1";
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgInvalidFormOfOption,
                                                       SegmentName,
                                                       optionName);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {optionName}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("fti", "tab", "pad")]
        [InlineData("Fti", "TaB", "PAd")]
        [InlineData("fTI", "tAb", "paD")]
        public void ParseSegmentHeader_OptionNamesAreMixedCase_SavesOptionValues(string firstTimeIndentOption,
                                                                                 string tabSizeOption,
                                                                                 string padSegmentOption)
        {
            // Arrange
            string indentStringValue = "1";
            int indentIntegerValue = 1;
            _mh.SetupIndentProcessorForIndentValues(_indentProcessor,
                                                    indentStringValue,
                                                    true,
                                                    indentIntegerValue,
                                                    indentIntegerValue);
            string tabSizeStringValue = "3";
            int tabSizeIntegerValue = 3;
            _mh.SetupIndentProcessorForTabSizeValues(_indentProcessor,
                                                     tabSizeStringValue,
                                                     true,
                                                     tabSizeIntegerValue,
                                                     tabSizeIntegerValue);
            string padSegmentValue = "PadSegment";
            _nameValidaterExpression2 = MockHelper.SetupNameValidater(_nameValidater, padSegmentValue, true);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} "
                + $"{firstTimeIndentOption}={indentStringValue}, "
                + $"{tabSizeOption}={tabSizeIntegerValue}, "
                + $"{padSegmentOption}={padSegmentValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new()
            {
                FirstTimeIndent = indentIntegerValue,
                TabSize = tabSizeIntegerValue,
                PadSegment = padSegmentValue
            };

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _nameValidater.Verify(_nameValidaterExpression2, Times.Once);
            _indentProcessor.Verify(_mh.IsValidIndentValueExpression, Times.Once);
            _indentProcessor.Verify(_mh.IsValidTabSizeExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_OptionValueMissingAfterEqualsSign_LogsMessage()
        {
            // Arrange
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgOptionValueMustFollowEqualsSign,
                                                       SegmentName,
                                                       PadSegmentNameOption);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {PadSegmentNameOption}=";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_PadSegmentHasInvalidName_LogsMessage()
        {
            // Arrange
            string optionValue = "invalidName";
            _nameValidaterExpression2 = MockHelper.SetupNameValidater(_nameValidater, optionValue, false);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgInvalidPadSegmentName,
                                                       SegmentName,
                                                       optionValue);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {PadSegmentNameOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _nameValidater.Verify(_nameValidaterExpression2, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_PadSegmentHasValidName_SavesPadSegmentName()
        {
            // Arrange
            string optionValue = "validName";
            _nameValidaterExpression2 = MockHelper.SetupNameValidater(_nameValidater, optionValue, true);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {PadSegmentNameOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new()
            {
                PadSegment = optionValue
            };

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _nameValidater.Verify(_nameValidaterExpression2, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_SegmentNameIsInvalid_LogsMessageAndUsesDefaultName()
        {
            // Arrange
            string invalidSegmentName = "Invalid";
            string defaultSegmentName = $"{DefaultSegmentNamePrefix}1";
            string segmentHeader = $"{SegmentHeaderCode} {invalidSegmentName}";
            _mh.SetupLocater(_locater, defaultSegmentName, LineNumber);
            _mh.SetupDefaultSegmentNameGenerator(_defaultSegmentNameGenerator);
            _nameValidaterExpression2 = MockHelper.SetupNameValidater(_nameValidater, invalidSegmentName, false);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgInvalidSegmentName,
                                                       invalidSegmentName,
                                                       defaultSegmentName);
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression2, Times.Once);
            _defaultSegmentNameGenerator.Verify(_mh.DefaultSegmentNameExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_SegmentNameNotFoundInFifthColumn_LogsMessageAndUsesDefaultName()
        {
            // Arrange
            string segmentHeader = $"{SegmentHeaderCode}  ";
            string segmentName = $"{DefaultSegmentNamePrefix}1";
            _mh.SetupLocater(_locater, segmentName, LineNumber);
            _mh.SetupDefaultSegmentNameGenerator(_defaultSegmentNameGenerator);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgSegmentNameMustStartInColumn5,
                                                       segmentName,
                                                       segmentHeader);
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _defaultSegmentNameGenerator.Verify(_mh.DefaultSegmentNameExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Exactly(2));
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("-1")]
        [InlineData("0")]
        [InlineData("10")]
        [InlineData("11")]
        [InlineData("X")]
        public void ParseSegmentHeader_TabSizeIsInvalid_LogsMessage(string optionValue)
        {
            // Arrange
            int tabSizeValue = 0;
            _mh.SetupIndentProcessorForTabSizeValues(_indentProcessor,
                                                     optionValue,
                                                     false,
                                                     tabSizeValue,
                                                     0);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgInvalidTabSizeOption,
                                                       SegmentName);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {TabSizeOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _indentProcessor.Verify(_mh.IsValidTabSizeExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("5", 5)]
        [InlineData("9", 9)]
        public void ParseSegmentHeader_TabSizeIsValid_SavesTheTabSizeValue(string optionValue, int tabSizeValue)
        {
            // Arrange
            _mh.SetupIndentProcessorForTabSizeValues(_indentProcessor,
                                                     optionValue,
                                                     true,
                                                     tabSizeValue,
                                                     tabSizeValue);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {TabSizeOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new()
            {
                TabSize = tabSizeValue
            };

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _indentProcessor.Verify(_mh.IsValidTabSizeExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_ValidSegmentNameWithNoOptions_UsesValidSegmentName()
        {
            // Arrange
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression1, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void SegmentHeaderParser_ConstructUsingNullDefaultSegmentNameGenerator_ThrowsException()
        {
            // Arrange
            string expected = GetNullDependencyMessage(ClassNames.SegmentHeaderParserClass,
                                                       ServiceNames.DefaultSegmentNameGeneratorService,
                                                       ServiceParameterNames.DefaultSegmentNameGeneratorParameter);
            Action action = () =>
            {
                SegmentHeaderParser parser = new(_logger.Object,
                                                 null!,
                                                 _locater.Object,
                                                 _indentProcessor.Object,
                                                 _nameValidater.Object);
            };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void SegmentHeaderParser_ConstructUsingNullIndentProcessor_ThrowsException()
        {
            // Arrange
            string expected = GetNullDependencyMessage(ClassNames.SegmentHeaderParserClass,
                                                       ServiceNames.IndentProcessorService,
                                                       ServiceParameterNames.IndentProcessorParameter);
            Action action = () =>
            {
                SegmentHeaderParser parser = new(_logger.Object,
                                                 _defaultSegmentNameGenerator.Object,
                                                 _locater.Object,
                                                 null!,
                                                 _nameValidater.Object);
            };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void SegmentHeaderParser_ConstructUsingNullLocater_ThrowsException()
        {
            // Arrange
            string expected = GetNullDependencyMessage(ClassNames.SegmentHeaderParserClass,
                                                       ServiceNames.LocaterService,
                                                       ServiceParameterNames.LocaterParameter);
            Action action = () =>
            {
                SegmentHeaderParser parser = new(_logger.Object,
                                                 _defaultSegmentNameGenerator.Object,
                                                 null!,
                                                 _indentProcessor.Object,
                                                 _nameValidater.Object);
            };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void SegmentHeaderParser_ConstructUsingNullLogger_ThrowsException()
        {
            // Arrange
            string expected = GetNullDependencyMessage(ClassNames.SegmentHeaderParserClass,
                                                       ServiceNames.LoggerService,
                                                       ServiceParameterNames.LoggerParameter);
            Action action = () =>
            {
                SegmentHeaderParser parser = new(null!,
                                                 _defaultSegmentNameGenerator.Object,
                                                 _locater.Object,
                                                 _indentProcessor.Object,
                                                 _nameValidater.Object);
            };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void SegmentHeaderParser_ConstructUsingNullNameValidater_ThrowsException()
        {
            // Arrange
            string expected = GetNullDependencyMessage(ClassNames.SegmentHeaderParserClass,
                                                       ServiceNames.NameValidaterService,
                                                       ServiceParameterNames.NameValidaterParameter);
            Action action = () =>
            {
                SegmentHeaderParser parser = new(_logger.Object,
                                                 _defaultSegmentNameGenerator.Object,
                                                 _locater.Object,
                                                 _indentProcessor.Object,
                                                 null!);
            };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void SegmentHeaderParser_ConstructWithValidDependencies_ShouldNotThrow()
        {
            // Arrange
            Action action = () =>
            {
                SegmentHeaderParser parser = GetSegmentHeaderParser();
            };

            // Act/Assert
            action
                .Should()
                .NotThrow();
            MocksVerifyNoOtherCalls();
        }

        private SegmentHeaderParser GetSegmentHeaderParser()
            => new(_logger.Object,
                   _defaultSegmentNameGenerator.Object,
                   _locater.Object,
                   _indentProcessor.Object,
                   _nameValidater.Object);

        private void MocksVerifyNoOtherCalls()
        {
            _logger.VerifyNoOtherCalls();
            _defaultSegmentNameGenerator.VerifyNoOtherCalls();
            _locater.VerifyNoOtherCalls();
            _indentProcessor.VerifyNoOtherCalls();
            _nameValidater.VerifyNoOtherCalls();
        }
    }
}