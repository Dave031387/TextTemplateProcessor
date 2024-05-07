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
        private readonly MethodCallOrderVerifier _verifier = new();

        [Theory]
        [InlineData("-10")]
        [InlineData("10")]
        [InlineData("X")]
        public void ParseSegmentHeader_FirstTimeIndentIsInvalid_LogsMessage(string optionValue)
        {
            // Arrange
            InitializeMocks();
            int indentValue = 0;
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {FirstTimeIndentOption}={optionValue}";
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.IsValidIndentValue(optionValue, out indentValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_IsValidIndentValue))
                .Returns(false)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgFirstTimeIndentIsInvalid, SegmentName, optionValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.IndentProcessor_IsValidIndentValue);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_IsValidIndentValue, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
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
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {FirstTimeIndentOption}={optionValue}";
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.IsValidIndentValue(optionValue, out indentValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_IsValidIndentValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.IndentProcessor_IsValidIndentValue);
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
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {FirstTimeIndentOption}={optionValue}";
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.IsValidIndentValue(optionValue, out indentValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_IsValidIndentValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgFirstTimeIndentSetToZero, SegmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.IndentProcessor_IsValidIndentValue);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_IsValidIndentValue, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
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
            string padSegmentValue = "PadSegment";
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} "
                + $"{PadSegmentNameOption}={padSegmentValue}, "
                + $"{FirstTimeIndentOption}={indentIntegerValue}, "
                + $"{TabSizeOption}={tabSizeIntegerValue}, "
                + $"{optionName}={optionValue}";
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.IsValidIndentValue(indentStringValue, out indentIntegerValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_IsValidIndentValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.IsValidTabSizeValue(tabSizeStringValue, out tabSizeIntegerValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_IsValidTabSizeValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName_FirstName))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(padSegmentValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName_SecondName))
                .Returns(true)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgFoundDuplicateOptionNameOnHeaderLine, SegmentName, optionName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName_FirstName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.NameValidater_IsValidName_SecondName);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName_SecondName, MethodCall.IndentProcessor_IsValidIndentValue);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_IsValidIndentValue, MethodCall.IndentProcessor_IsValidTabSizeValue);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_IsValidTabSizeValue, MethodCall.Logger_Log_Message);
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
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {option1}={value1}, {option2}={value2}, {option3}={value3}";
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.IsValidIndentValue(indentStringValue, out firstTimeIndent))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_IsValidIndentValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.IsValidTabSizeValue(tabSizeStringValue, out tabSizeValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_IsValidTabSizeValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName_FirstName))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(padSegment))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName_SecondName))
                .Returns(true)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName_FirstName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.IndentProcessor_IsValidIndentValue);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.IndentProcessor_IsValidTabSizeValue);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.NameValidater_IsValidName_SecondName);
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
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Next)
                .Callback(_verifier.GetCallOrderAction(MethodCall.DefaultSegmentNameGenerator_Next))
                .Returns(segmentName)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = segmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgSegmentNameMustStartInColumn5, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Next, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
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
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {optionString}";
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgUnknownSegmentOptionFound, SegmentName, optionString))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
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
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {optionString}";
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgOptionNameMustPrecedeEqualsSign, SegmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
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
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {optionName}";
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgInvalidFormOfOption, SegmentName, optionName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
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
            string padSegmentValue = "PadSegment";
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} "
                + $"{firstTimeIndentOption}={indentStringValue}, "
                + $"{tabSizeOption}={tabSizeIntegerValue}, "
                + $"{padSegmentOption}={padSegmentValue}";
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.IsValidIndentValue(indentStringValue, out indentIntegerValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_IsValidIndentValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.IsValidTabSizeValue(tabSizeStringValue, out tabSizeIntegerValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_IsValidTabSizeValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName_FirstName))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(padSegmentValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName_SecondName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName_FirstName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.IndentProcessor_IsValidIndentValue);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.IndentProcessor_IsValidTabSizeValue);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.NameValidater_IsValidName_SecondName);
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
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {PadSegmentNameOption}=";
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgOptionValueMustFollowEqualsSign, SegmentName, PadSegmentNameOption))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
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
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {PadSegmentNameOption}={optionValue}";
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgInvalidPadSegmentName, SegmentName, optionValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName_FirstName))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(optionValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName_SecondName))
                .Returns(false)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName_FirstName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.NameValidater_IsValidName_SecondName);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName_SecondName, MethodCall.Logger_Log_Message);
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
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName_FirstName))
                .Returns(true)
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(optionValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName_SecondName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName_FirstName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.NameValidater_IsValidName_SecondName);
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
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Next)
                .Callback(_verifier.GetCallOrderAction(MethodCall.DefaultSegmentNameGenerator_Next))
                .Returns(defaultSegmentName)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = defaultSegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(defaultSegmentName)
                .Verifiable(Times.AtLeastOnce);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(invalidSegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName))
                .Returns(false)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgInvalidSegmentName, invalidSegmentName, defaultSegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName, MethodCall.DefaultSegmentNameGenerator_Next);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Next, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
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
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Next)
                .Callback(_verifier.GetCallOrderAction(MethodCall.DefaultSegmentNameGenerator_Next))
                .Returns(segmentName)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = segmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgSegmentNameMustStartInColumn5, segmentName, segmentHeader))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Next, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
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
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {TabSizeOption}={optionValue}";
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.IsValidTabSizeValue(optionValue, out tabSizeValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_IsValidTabSizeValue))
                .Returns(false)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter))
                .Returns(SegmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.Log(MsgInvalidTabSizeOption, SegmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.IndentProcessor_IsValidTabSizeValue);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_IsValidTabSizeValue, MethodCall.Logger_Log_Message);
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
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName} {TabSizeOption}={optionValue}";
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.IsValidTabSizeValue(optionValue, out tabSizeValue))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_IsValidTabSizeValue))
                .Returns(true)
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.IndentProcessor_IsValidTabSizeValue);
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
            string segmentHeader = $"{SegmentHeaderCode} {SegmentName}";
            _locater
                .SetupSet(locater => locater.CurrentSegment = SegmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _nameValidater
                .Setup(nameValidater => nameValidater.IsValidName(SegmentName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.NameValidater_IsValidName))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.NameValidater_IsValidName, MethodCall.Locater_CurrentSegment_Setter);
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
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void SegmentHeaderParser_ConstructUsingNullIndentProcessor_ThrowsException()
        {
            // Arrange
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void SegmentHeaderParser_ConstructUsingNullLocater_ThrowsException()
        {
            // Arrange
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void SegmentHeaderParser_ConstructUsingNullLogger_ThrowsException()
        {
            // Arrange
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void SegmentHeaderParser_ConstructUsingNullNameValidater_ThrowsException()
        {
            // Arrange
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
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
            _verifier.Reset();
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
            _verifier.Verify();
        }
    }
}