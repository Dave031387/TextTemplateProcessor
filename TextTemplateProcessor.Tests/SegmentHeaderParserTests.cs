namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Interfaces;

    public class SegmentHeaderParserTests
    {
        private const string SegmentName = "Segment1";
        private readonly Mock<IDefaultSegmentNameGenerator> _defaultSegmentNameGenerator = new();
        private readonly Mock<IIndentProcessor> _indentProcessor = new();
        private readonly Mock<ILocater> _locater = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<INameValidater> _nameValidater = new();

        [Theory]
        [InlineData("-10")]
        [InlineData("10")]
        [InlineData("X")]
        public void ParseSegmentHeader_FirstTimeIndentIsInvalid_LogsMessage(string optionValue)
        {
            // Arrange
            InitializeMocks();
            int indentValue = 0;
            _indentProcessor
                .Setup(x => x.IsValidIndentValue(optionValue, out indentValue))
                .Returns(false)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.Log(MsgFirstTimeIndentIsInvalid, SegmentName, optionValue))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {FirstTimeIndentOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
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
            InitializeMocks();
            _indentProcessor
                .Setup(x => x.IsValidIndentValue(optionValue, out indentValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
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
            VerifyMocks();
        }

        [Fact]
        public void ParseSegmentHeader_FirstTimeIndentIsZero_LogsMessageAndSavesFirstTimeIndentValue()
        {
            // Arrange
            InitializeMocks();
            string optionValue = "0";
            int indentValue = 0;
            _indentProcessor
                .Setup(x => x.IsValidIndentValue(optionValue, out indentValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.Log(MsgFirstTimeIndentSetToZero, SegmentName, null))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {FirstTimeIndentOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Theory]
        [InlineData(FirstTimeIndentOption, "1")]
        [InlineData(TabSizeOption, "1")]
        [InlineData(PadSegmentNameOption, "Duplicate")]
        public void ParseSegmentHeader_HeaderHasDuplicateSegmentOptions_LogsMessage(string optionName, string optionValue)
        {
            // Arrange
            InitializeMocks();
            string indentStringValue = "2";
            int indentIntegerValue = 2;
            string tabSizeStringValue = "3";
            int tabSizeIntegerValue = 3;
            _indentProcessor
                .Setup(x => x.IsValidIndentValue(indentStringValue, out indentIntegerValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(x => x.IsValidTabSizeValue(tabSizeStringValue, out tabSizeIntegerValue))
                .Returns(true)
                .Verifiable(Times.Once);
            string padSegmentValue = "PadSegment";
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(padSegmentValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.Log(MsgFoundDuplicateOptionNameOnHeaderLine, SegmentName, optionName))
                .Verifiable(Times.Once);
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
            VerifyMocks();
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
            InitializeMocks();
            string indentStringValue = option1 is FirstTimeIndentOption ? value1 : option2 is FirstTimeIndentOption ? value2 : value3;
            string tabSizeStringValue = option1 is TabSizeOption ? value1 : option2 is TabSizeOption ? value2 : value3;
            _indentProcessor
                .Setup(x => x.IsValidIndentValue(indentStringValue, out firstTimeIndent))
                .Returns(true)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(x => x.IsValidTabSizeValue(tabSizeStringValue, out tabSizeValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(padSegment))
                .Returns(true)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
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
            VerifyMocks();
        }

        [Fact]
        public void ParseSegmentHeader_HeaderLineLessThanFiveCharacters_LogsMessageAndReturnsDefaultControlItem()
        {
            // Arrange
            InitializeMocks();
            string segmentHeader = $"{SegmentHeaderCode} ";
            string segmentName = $"{DefaultSegmentNamePrefix}1";
            _defaultSegmentNameGenerator
                .Setup(x => x.Next)
                .Returns(segmentName)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.CurrentSegment = segmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.Log(MsgSegmentNameMustStartInColumn5, segmentName, null))
                .Verifiable(Times.Once);
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ParseSegmentHeader_OptionNameIsInvalid_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string optionString = "Option1=value";
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.Log(MsgUnknownSegmentOptionFound, SegmentName, optionString))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {optionString}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ParseSegmentHeader_OptionNameMissingBeforeEqualsSign_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string optionString = "=value";
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.Log(MsgOptionNameMustPrecedeEqualsSign, SegmentName, null))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {optionString}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ParseSegmentHeader_OptionNameNotFollowedByEqualsSign_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string optionName = "Option1";
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.Log(MsgInvalidFormOfOption, SegmentName, optionName))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {optionName}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
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
            InitializeMocks();
            string indentStringValue = "1";
            int indentIntegerValue = 1;
            string tabSizeStringValue = "3";
            int tabSizeIntegerValue = 3;
            _indentProcessor
                .Setup(x => x.IsValidIndentValue(indentStringValue, out indentIntegerValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(x => x.IsValidTabSizeValue(tabSizeStringValue, out tabSizeIntegerValue))
                .Returns(true)
                .Verifiable(Times.Once);
            string padSegmentValue = "PadSegment";
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(padSegmentValue))
                .Returns(true)
                .Verifiable(Times.Once);
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
            VerifyMocks();
        }

        [Fact]
        public void ParseSegmentHeader_OptionValueMissingAfterEqualsSign_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.Log(MsgOptionValueMustFollowEqualsSign, SegmentName, PadSegmentNameOption))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {PadSegmentNameOption}=";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ParseSegmentHeader_PadSegmentHasInvalidName_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string optionValue = "invalidName";
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.Log(MsgInvalidPadSegmentName, SegmentName, optionValue))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(optionValue))
                .Returns(false)
                .Verifiable(Times.Once);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {PadSegmentNameOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ParseSegmentHeader_PadSegmentHasValidName_SavesPadSegmentName()
        {
            // Arrange
            InitializeMocks();
            string optionValue = "validName";
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(optionValue))
                .Returns(true)
                .Verifiable(Times.Once);
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
            VerifyMocks();
        }

        [Fact]
        public void ParseSegmentHeader_SegmentNameIsInvalid_LogsMessageAndUsesDefaultName()
        {
            // Arrange
            InitializeMocks();
            string invalidSegmentName = "Invalid";
            string defaultSegmentName = $"{DefaultSegmentNamePrefix}1";
            string segmentHeader = $"{SegmentHeaderCode} {invalidSegmentName}";
            _defaultSegmentNameGenerator
                .Setup(x => x.Next)
                .Returns(defaultSegmentName)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.CurrentSegment = defaultSegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(defaultSegmentName)
                .Verifiable(Times.AtLeastOnce);
            _nameValidater
                .Setup(x => x.IsValidName(invalidSegmentName))
                .Returns(false)
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgInvalidSegmentName, invalidSegmentName, defaultSegmentName))
                .Verifiable(Times.Once);
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void ParseSegmentHeader_SegmentNameNotFoundInFifthColumn_LogsMessageAndUsesDefaultName()
        {
            // Arrange
            InitializeMocks();
            string segmentHeader = $"{SegmentHeaderCode}  ";
            string segmentName = $"{DefaultSegmentNamePrefix}1";
            _defaultSegmentNameGenerator
                .Setup(x => x.Next)
                .Returns(segmentName)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.CurrentSegment = segmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.Log(MsgSegmentNameMustStartInColumn5, segmentName, segmentHeader))
                .Verifiable(Times.Once);
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
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
            InitializeMocks();
            int tabSizeValue = 0;
            _indentProcessor
                .Setup(x => x.IsValidTabSizeValue(optionValue, out tabSizeValue))
                .Returns(false)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.Log(MsgInvalidTabSizeOption, SegmentName, null))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {TabSizeOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("5", 5)]
        [InlineData("9", 9)]
        public void ParseSegmentHeader_TabSizeIsValid_SavesTheTabSizeValue(string optionValue, int tabSizeValue)
        {
            // Arrange
            InitializeMocks();
            _indentProcessor
                .Setup(x => x.IsValidTabSizeValue(optionValue, out tabSizeValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
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
            VerifyMocks();
        }

        [Fact]
        public void ParseSegmentHeader_ValidSegmentNameWithNoOptions_UsesValidSegmentName()
        {
            // Arrange
            InitializeMocks();
            _locater
                .SetupSet(x => x.CurrentSegment = SegmentName)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(x => x.IsValidName(SegmentName))
                .Returns(true)
                .Verifiable(Times.Once);
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
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
            InitializeMocks();
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

        private void InitializeMocks()
        {
            _defaultSegmentNameGenerator.Reset();
            _indentProcessor.Reset();
            _locater.Reset();
            _logger.Reset();
            _nameValidater.Reset();
        }

        private void MocksVerifyNoOtherCalls()
        {
            _defaultSegmentNameGenerator.VerifyNoOtherCalls();
            _indentProcessor.VerifyNoOtherCalls();
            _locater.VerifyNoOtherCalls();
            _logger.VerifyNoOtherCalls();
            _nameValidater.VerifyNoOtherCalls();
        }

        private void VerifyMocks()
        {
            if (_defaultSegmentNameGenerator.Setups.Any())
            {
                _defaultSegmentNameGenerator.Verify();
            }

            if (_indentProcessor.Setups.Any())
            {
                _indentProcessor.Verify();
            }

            if (_locater.Setups.Any())
            {
                _locater.Verify();
            }

            if (_logger.Setups.Any())
            {
                _logger.Verify();
            }

            if (_nameValidater.Setups.Any())
            {
                _nameValidater.Verify();
            }

            MocksVerifyNoOtherCalls();
        }
    }
}