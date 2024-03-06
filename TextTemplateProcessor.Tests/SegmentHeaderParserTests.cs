namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Interfaces;
    using System.Linq.Expressions;

    public class SegmentHeaderParserTests
    {
        private readonly Mock<IDefaultSegmentNameGenerator> _defaultSegmentNameGenerator = new();
        private readonly Mock<IIndentProcessor> _indentProcessor = new();
        private readonly Mock<ILocater> _locater = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly MockHelper _mh = new();
        private readonly Mock<INameValidater> _nameValidater = new();
        private Expression<Action<ILogger>> _loggerExpression = x => x.Log("test", null, null);
        private Expression<Func<INameValidater, bool>> _nameValidaterExpression = x => false;

        public SegmentHeaderParserTests()
        {
            _defaultSegmentNameGenerator.Reset();
            _indentProcessor.Reset();
            _locater.Reset();
            _logger.Reset();
            _nameValidater.Reset();
        }

        [Theory]
        [InlineData("-10")]
        [InlineData("10")]
        [InlineData("X")]
        public void ParseSegmentHeader_FirstTimeIndentIsInvalid_LogsMessage(string optionValue)
        {
            // Arrange
            string segmentName = "Segment1";
            int lineNumber = 1;
            _mh.SetupLocater(_locater, segmentName, lineNumber);
            int indentValue = 0;
            _mh.SetupIndentProcessor(_indentProcessor,
                                     optionValue,
                                     false,
                                     indentValue,
                                     0);
            _nameValidaterExpression = MockHelper.SetupNameValidater(_nameValidater, segmentName, true);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgFirstTimeIndentIsInvalid,
                                                       segmentName,
                                                       optionValue);
            string segmentHeader = $"{SegmentHeaderCode} {segmentName} {FirstTimeIndentOption}={optionValue}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression, Times.Once);
            _indentProcessor.Verify(_mh.IsValidIndentValueExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_HeaderLineLessThanFiveCharacters_LogsMessageAndReturnsDefaultControlItem()
        {
            // Arrange
            string segmentHeader = $"{SegmentHeaderCode} ";
            string segmentName = $"{DefaultSegmentNamePrefix}1";
            int lineNumber = 1;
            _mh.SetupLocater(_locater, segmentName, lineNumber);
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
            string segmentName = "Segment1";
            int lineNumber = 1;
            string optionString = "Option1=value";
            _mh.SetupLocater(_locater, segmentName, lineNumber);
            _nameValidaterExpression = MockHelper.SetupNameValidater(_nameValidater, segmentName, true);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgUnknownSegmentOptionFound,
                                                       segmentName,
                                                       optionString);
            string segmentHeader = $"{SegmentHeaderCode} {segmentName} {optionString}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_OptionNameMissingBeforeEqualsSign_LogsMessage()
        {
            // Arrange
            string segmentName = "Segment1";
            int lineNumber = 1;
            string optionString = "=value";
            _mh.SetupLocater(_locater, segmentName, lineNumber);
            _nameValidaterExpression = MockHelper.SetupNameValidater(_nameValidater, segmentName, true);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgOptionNameMustPrecedeEqualsSign,
                                                       segmentName);
            string segmentHeader = $"{SegmentHeaderCode} {segmentName} {optionString}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_OptionNameNotFollowedByEqualsSign_LogsMessage()
        {
            // Arrange
            string segmentName = "Segment1";
            int lineNumber = 1;
            string optionName = "Option1";
            _mh.SetupLocater(_locater, segmentName, lineNumber);
            _nameValidaterExpression = MockHelper.SetupNameValidater(_nameValidater, segmentName, true);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgInvalidFormOfOption,
                                                       segmentName,
                                                       optionName);
            string segmentHeader = $"{SegmentHeaderCode} {segmentName} {optionName}";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_OptionValueMissingAfterEqualsSign_LogsMessage()
        {
            // Arrange
            string segmentName = "Segment1";
            int lineNumber = 1;
            _mh.SetupLocater(_locater, segmentName, lineNumber);
            _nameValidaterExpression = MockHelper.SetupNameValidater(_nameValidater, segmentName, true);
            _loggerExpression = MockHelper.SetupLogger(_logger,
                                                       MsgOptionValueMustFollowEqualsSign,
                                                       segmentName,
                                                       PadSegmentNameOption);
            string segmentHeader = $"{SegmentHeaderCode} {segmentName} {PadSegmentNameOption}=";
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression, Times.Once);
            _locater.VerifySet(_mh.SetCurrentSegmentAction, Times.Once);
            _locater.Verify(_mh.CurrentSegmentExpression, Times.Once);
            _logger.Verify(_loggerExpression, Times.Once);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void ParseSegmentHeader_SegmentNameIsInvalid_LogsMessageAndUsesDefaultName()
        {
            // Arrange
            string invalidSegmentName = "Invalid";
            string defaultSegmentName = $"{DefaultSegmentNamePrefix}1";
            string segmentHeader = $"{SegmentHeaderCode} {invalidSegmentName}";
            int lineNumber = 1;
            _mh.SetupLocater(_locater, defaultSegmentName, lineNumber);
            _mh.SetupDefaultSegmentNameGenerator(_defaultSegmentNameGenerator);
            _nameValidaterExpression = MockHelper.SetupNameValidater(_nameValidater, invalidSegmentName, false);
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
            _nameValidater.Verify(_nameValidaterExpression, Times.Once);
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
            int lineNumber = 1;
            _mh.SetupLocater(_locater, segmentName, lineNumber);
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

        [Fact]
        public void ParseSegmentHeader_ValidSegmentNameWithNoOptions_UsesValidSegmentName()
        {
            // Arrange
            string validSegmentName = "Valid";
            string segmentHeader = $"{SegmentHeaderCode} {validSegmentName}";
            int lineNumber = 1;
            _mh.SetupLocater(_locater, validSegmentName, lineNumber);
            _nameValidaterExpression = MockHelper.SetupNameValidater(_nameValidater, validSegmentName, true);
            SegmentHeaderParser parser = GetSegmentHeaderParser();
            ControlItem expected = new();

            // Act
            ControlItem actual = parser.ParseSegmentHeader(segmentHeader);

            // Assert
            actual
                .Should()
                .Be(expected);
            _nameValidater.Verify(_nameValidaterExpression, Times.Once);
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