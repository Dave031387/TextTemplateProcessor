namespace TextTemplateProcessor
{
    public class TemplateLoaderTests
    {
        private readonly Dictionary<string, ControlItem> _actualControlDictionary = [];
        private readonly Dictionary<string, List<TextItem>> _actualSegmentDictionary = [];
        private readonly Mock<IDefaultSegmentNameGenerator> _defaultSegmentNameGenerator = new();
        private readonly Dictionary<string, ControlItem> _expectedControlDictionary = [];
        private readonly Dictionary<string, List<TextItem>> _expectedSegmentDictionary = [];
        private readonly Mock<ILocater> _locater = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<ISegmentHeaderParser> _segmentHeaderParser = new();
        private readonly List<string> _templateLines = [];
        private readonly Mock<ITextLineParser> _textLineParser = new();
        private string _currentSegmentName = string.Empty;
        private int _defaultSegmentNameCounter = 0;
        private int _lineCounter;

        [Fact]
        public void LoadTemplate_CommentLinesBeforeFirstSegmentHeader_CommentLinesAreIgnored()
        {
            // Arrange
            InitializeMocks();
            AddCommentLineToTemplateFile();
            AddCommentLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment1");
            AddTextLineToTemplateFile();
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_CommentLinesFollowingSegmentHeader_CommentLinesAreIgnored()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1");
            AddCommentLineToTemplateFile();
            AddTextLineToTemplateFile();
            AddCommentLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_FirstTemplateLineIsInvalid_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddTextLineToTemplateFile(true, false);
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_LastSegmentContainsOnlyCommentLines_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1");
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment2", false, false);
            AddCommentLineToTemplateFile();
            AddCommentLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_MissingSegmentHeaderBeforeFirstTextLine_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddTextLineToTemplateFile(true);
            AddSegmentHeaderLineToTemplateFile("Segment1");
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_MultipleIssues_LogsAllMessages()
        {
            // Arrange
            InitializeMocks();
            AddTextLineToTemplateFile(true);
            AddSegmentHeaderLineToTemplateFile("Segment1", false, true, "Segment2", true);
            AddCommentLineToTemplateFile();
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment1", true);
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment2", false, false);

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_NoTextLinesAfterLastSegmentHeader_LogsMessage()
        {
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1");
            AddTextLineToTemplateFile();
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment2", false, false);

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_NoTextLinesBetweenSegmentHeaders_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1", false, false);
            AddSegmentHeaderLineToTemplateFile("Segment2");
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_PadSegmentIsReferencedAfterItIsDefined_PadSegmentIsUsed()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1");
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment2", false, true, "Segment1");
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_PadSegmentIsReferencedBeforeItIsDefined_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1", false, true, "Segment2", true);
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment2");
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_PadSegmentNameMatchesSegmentHeaderName_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1", false, true, "Segment1");
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_SegmentContainsInvalidTemplateLine_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1");
            AddTextLineToTemplateFile();
            AddTextLineToTemplateFile(false, false);
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_SegmentContainsOnlyCommentLines_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1", false, false);
            AddCommentLineToTemplateFile();
            AddCommentLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment2");
            AddTextLineToTemplateFile();
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_SegmentNameIsDuplicate_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1",
                                               false,
                                               true,
                                               null,
                                               false,
                                               false,
                                               2);
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment1",
                                               true,
                                               true,
                                               null,
                                               false,
                                               false,
                                               2);
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_ThreeLevelsOfPadSegments_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1");
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment2", false, true, "Segment1");
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment3", false, true, "Segment2", false, true);
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment4", false, true, "Segment3", false, true);
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplate_TwoLevelsOfPadSegments_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1");
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment2", false, true, "Segment1");
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment3", false, true, "Segment2", false, true);
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
        }

        [Fact]
        public void TemplateLoader_ConstructUsingNullDefaultSegmentNameGenerator_ThrowsException()
        {
            // Arrange
            string expected = GetNullDependencyMessage(ClassNames.TemplateLoaderClass,
                                                       ServiceNames.DefaultSegmentNameGeneratorService,
                                                       ServiceParameterNames.DefaultSegmentNameGeneratorParameter);
            Action action = () =>
            {
                TemplateLoader loader = new(_logger.Object,
                                            null!,
                                            _locater.Object,
                                            _segmentHeaderParser.Object,
                                            _textLineParser.Object);
            };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TemplateLoader_ConstructUsingNullLocater_ThrowsException()
        {
            // Arrange
            string expected = GetNullDependencyMessage(ClassNames.TemplateLoaderClass,
                                                       ServiceNames.LocaterService,
                                                       ServiceParameterNames.LocaterParameter);
            Action action = () =>
            {
                TemplateLoader loader = new(_logger.Object,
                                            _defaultSegmentNameGenerator.Object,
                                            null!,
                                            _segmentHeaderParser.Object,
                                            _textLineParser.Object);
            };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TemplateLoader_ConstructUsingNullLogger_ThrowsException()
        {
            // Arrange
            string expected = GetNullDependencyMessage(ClassNames.TemplateLoaderClass,
                                                       ServiceNames.LoggerService,
                                                       ServiceParameterNames.LoggerParameter);
            Action action = () =>
            {
                TemplateLoader loader = new(null!,
                                            _defaultSegmentNameGenerator.Object,
                                            _locater.Object,
                                            _segmentHeaderParser.Object,
                                            _textLineParser.Object);
            };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TemplateLoader_ConstructUsingNullSegmentHeaderParser_ThrowsException()
        {
            // Arrange
            string expected = GetNullDependencyMessage(ClassNames.TemplateLoaderClass,
                                                       ServiceNames.SegmentHeaderParserService,
                                                       ServiceParameterNames.SegmentHeaderParserParameter);
            Action action = () =>
            {
                TemplateLoader loader = new(_logger.Object,
                                            _defaultSegmentNameGenerator.Object,
                                            _locater.Object,
                                            null!,
                                            _textLineParser.Object);
            };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TemplateLoader_ConstructUsingNullTextLineParser_ThrowsException()
        {
            // Arrange
            string expected = GetNullDependencyMessage(ClassNames.TemplateLoaderClass,
                                                       ServiceNames.TextLineParserService,
                                                       ServiceParameterNames.TextLineParserParameter);
            Action action = () =>
            {
                TemplateLoader loader = new(_logger.Object,
                                            _defaultSegmentNameGenerator.Object,
                                            _locater.Object,
                                            _segmentHeaderParser.Object,
                                            null!);
            };

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        private void AddCommentLineToTemplateFile()
        {
            string commentLine = $"{Comment} Comment line {++_lineCounter}";
            _templateLines.Add(commentLine);
            _locater
                .SetupSet(locater => locater.LineNumber = _lineCounter)
                .Verifiable(Times.Once);
            _textLineParser
                .Setup(textLineParser => textLineParser.IsValidPrefix(commentLine))
                .Returns(true)
                .Verifiable(Times.Once);
            _textLineParser
                .Setup(textLineParser => textLineParser.IsCommentLine(commentLine))
                .Returns(true)
                .Verifiable(Times.Once);
        }

        private void AddSegmentHeaderLineToTemplateFile(string segmentName,
                                                        bool isDuplicateSegmentName = false,
                                                        bool isSegmentWithTextLines = true,
                                                        string? padSegment = null,
                                                        bool isUnknownPadSegment = false,
                                                        bool isMultiplePadLevels = false,
                                                        int callCount = 1)
        {
            string padOption = padSegment is null ? string.Empty : $" PAD={padSegment}";
            string segmentHeaderLine = $"{SegmentHeaderCode} {segmentName}{padOption}";

            _templateLines.Add(segmentHeaderLine);
            _lineCounter++;

            SegmentHeader_SetupCommonMocks(segmentHeaderLine, callCount);
            SegmentHeader_CheckForDuplicateSegmentName(segmentName, isDuplicateSegmentName);
            string expectedPadSegment = SegmentHeader_CheckForValidPadOption(segmentName,
                                                                             padSegment,
                                                                             isUnknownPadSegment,
                                                                             isMultiplePadLevels);
            SegmentHeader_CheckForSegmentWithTextLines(isSegmentWithTextLines, expectedPadSegment);
            SegmentHeader_SetupParseSegmentHeader(segmentHeaderLine, padSegment, callCount);
        }

        private void AddTextLineToTemplateFile(bool isMissingSegmentHeader = false,
                                               bool isValidTextLine = true)
        {
            string text = $"Text line {++_lineCounter}";
            string textLine = isValidTextLine
                ? $"{IndentUnchanged} {text}"
                : $"XXX INVALID {text}";

            _templateLines.Add(textLine);
            _locater
                .SetupSet(locater => locater.LineNumber = _lineCounter)
                .Verifiable(Times.Once);

            TextLine_CheckForMissingSegmentHeader(isMissingSegmentHeader);
            TextLine_CheckForValidTextLine(textLine, isValidTextLine);
        }

        private void InitializeMocks()
        {
            _defaultSegmentNameGenerator.Reset();
            _locater.Reset();
            _logger.Reset();
            _segmentHeaderParser.Reset();
            _textLineParser.Reset();
            _templateLines.Clear();
            _actualSegmentDictionary.Clear();
            _actualControlDictionary.Clear();
            _expectedSegmentDictionary.Clear();
            _expectedControlDictionary.Clear();
            _currentSegmentName = string.Empty;
            _lineCounter = 0;
            _defaultSegmentNameCounter = 0;
            _defaultSegmentNameGenerator
                .SetupSequence(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Next)
                .Returns($"{DefaultSegmentNamePrefix}1")
                .Returns($"{DefaultSegmentNamePrefix}2")
                .Returns($"{DefaultSegmentNamePrefix}3")
                .Returns($"{DefaultSegmentNamePrefix}4")
                .Throws<ArgumentException>();
            _locater
                .SetupProperty(locater => locater.CurrentSegment);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Parsing))
                .Verifiable(Times.Once);
        }

        private void LoadTemplate()
        {
            // Arrange
            TemplateLoader loader = new(_logger.Object,
                                        _defaultSegmentNameGenerator.Object,
                                        _locater.Object,
                                        _segmentHeaderParser.Object,
                                        _textLineParser.Object);

            // Act
            loader.LoadTemplate(_templateLines, _actualSegmentDictionary, _actualControlDictionary);
        }

        private void MocksVerifyNoOtherCalls()
        {
            _defaultSegmentNameGenerator.VerifyNoOtherCalls();
            _locater.VerifyNoOtherCalls();
            _logger.VerifyNoOtherCalls();
            _segmentHeaderParser.VerifyNoOtherCalls();
            _textLineParser.VerifyNoOtherCalls();
        }

        private void SegmentHeader_CheckForDuplicateSegmentName(string segmentName, bool isDuplicateSegmentName)
        {
            if (isDuplicateSegmentName)
            {
                string defaultSegmentName = $"{DefaultSegmentNamePrefix}{++_defaultSegmentNameCounter}";
                _currentSegmentName = defaultSegmentName;
                _logger
                    .Setup(logger => logger.Log(MsgFoundDuplicateSegmentName, segmentName, _currentSegmentName))
                    .Verifiable(Times.Once);
            }
            else
            {
                _currentSegmentName = segmentName;
            }
        }

        private void SegmentHeader_CheckForSegmentWithTextLines(bool isSegmentWithTextLines, string expectedPadSegment)
        {
            if (isSegmentWithTextLines)
            {
                ControlItem savedControlItem = new()
                {
                    PadSegment = expectedPadSegment
                };
                _expectedControlDictionary[_currentSegmentName] = savedControlItem;
            }
            else
            {
                _logger
                    .Setup(logger => logger.Log(MsgNoTextLinesFollowingSegmentHeader, _currentSegmentName, null))
                    .Verifiable(Times.Once);
            }
        }

        private string SegmentHeader_CheckForValidPadOption(string segmentName,
                                                            string? padSegment,
                                                            bool isUnknownPadSegment,
                                                            bool isMultiplePadLevels)
        {
            string expectedPadSegment = string.Empty;

            if (padSegment is null)
            {
            }
            else
            {
                if (isUnknownPadSegment)
                {
                    _logger
                        .Setup(logger => logger.Log(MsgPadSegmentMustBeDefinedEarlier, segmentName, padSegment))
                        .Verifiable(Times.Once);
                }
                else if (padSegment == segmentName)
                {
                    _logger
                        .Setup(logger => logger.Log(MsgPadSegmentNameSameAsSegmentHeaderName, segmentName, null))
                        .Verifiable(Times.Once);
                }
                else if (isMultiplePadLevels)
                {
                    _logger
                        .Setup(logger => logger.Log(MsgMultipleLevelsOfPadSegments, segmentName, padSegment))
                        .Verifiable(Times.Once);
                }
                else
                {
                    expectedPadSegment = padSegment;
                }
            }

            return expectedPadSegment;
        }

        private void SegmentHeader_SetupCommonMocks(string segmentHeaderLine, int callCount)
        {
            _locater
                .SetupSet(locater => locater.LineNumber = _lineCounter)
                .Verifiable(Times.Once);
            _textLineParser
                .Setup(textLineParser => textLineParser.IsValidPrefix(segmentHeaderLine))
                .Returns(true)
                .Verifiable(Times.Exactly(callCount));
            _textLineParser
                .Setup(textLineParser => textLineParser.IsCommentLine(segmentHeaderLine))
                .Returns(false)
                .Verifiable(Times.Exactly(callCount));
            _textLineParser
                .Setup(textLineParser => textLineParser.IsSegmentHeader(segmentHeaderLine))
                .Returns(true)
                .Verifiable(Times.Exactly(callCount));
        }

        private void SegmentHeader_SetupParseSegmentHeader(string segmentHeaderLine, string? padSegment, int callCount)
        {
            ControlItem returnedControlItem = new()
            {
                PadSegment = padSegment is null ? string.Empty : padSegment
            };

            _segmentHeaderParser
                .Setup(segmentHeaderParser => segmentHeaderParser.ParseSegmentHeader(segmentHeaderLine))
                .Returns(returnedControlItem)
                .Verifiable(Times.Exactly(callCount));
            _logger
                .Setup(logger => logger.Log(MsgSegmentHasBeenAdded, _currentSegmentName, null))
                .Verifiable(Times.Once);
        }

        private void TextLine_AddTextItemToSegmentDictionary(string textLine, TextItem textItem)
        {
            if (_expectedSegmentDictionary.TryGetValue(_currentSegmentName, out List<TextItem>? value))
            {
                value.Add(textItem);
            }
            else
            {
                _expectedSegmentDictionary[_currentSegmentName] = [textItem];
            }

            _textLineParser
                .Setup(textLineParser => textLineParser.ParseTextLine(textLine))
                .Returns(textItem)
                .Verifiable(Times.Once);
        }

        private void TextLine_CheckForMissingSegmentHeader(bool isMissingSegmentHeader)
        {
            if (isMissingSegmentHeader)
            {
                string defaultSegmentName = $"{DefaultSegmentNamePrefix}{++_defaultSegmentNameCounter}";
                _currentSegmentName = defaultSegmentName;
                _logger
                    .Setup(logger => logger.Log(MsgMissingInitialSegmentHeader, _currentSegmentName, null))
                    .Verifiable(Times.Once);
                _expectedControlDictionary[_currentSegmentName] = new();
            }
        }

        private void TextLine_CheckForValidTextLine(string textLine, bool isValidTextLine)
        {
            if (isValidTextLine)
            {
                _textLineParser
                    .Setup(textLineParser => textLineParser.IsValidPrefix(textLine))
                    .Returns(true)
                    .Verifiable(Times.Once);
                _textLineParser
                    .Setup(textLineParser => textLineParser.IsCommentLine(textLine))
                    .Returns(false)
                    .Verifiable(Times.Once);
                _textLineParser
                    .Setup(textLineParser => textLineParser.IsSegmentHeader(textLine))
                    .Returns(false)
                .Verifiable(Times.Once);

                TextItem textItem = new(0, true, false, textLine[4..]);

                TextLine_AddTextItemToSegmentDictionary(textLine, textItem);
            }
            else
            {
                _textLineParser
                    .Setup(textLineParser => textLineParser.IsValidPrefix(textLine))
                    .Returns(false)
                    .Verifiable(Times.Once);
            }
        }

        private void VerifyDictionaries()
        {
            if (_expectedControlDictionary.Count == 0)
            {
                _actualControlDictionary
                    .Should()
                        .BeEmpty();
            }
            else
            {
                _actualControlDictionary
                    .Should()
                    .HaveSameCount(_expectedControlDictionary);
                _actualControlDictionary
                    .Should()
                    .ContainKeys(_expectedControlDictionary.Keys);

                foreach (string key in _expectedControlDictionary.Keys)
                {
                    _actualControlDictionary[key]
                        .Should()
                        .Be(_expectedControlDictionary[key]);
                }
            }

            if (_expectedSegmentDictionary.Count == 0)
            {
                _actualSegmentDictionary
                    .Should()
                        .BeEmpty();
            }
            else
            {
                _actualSegmentDictionary
                    .Should()
                    .HaveSameCount(_expectedSegmentDictionary);
                _actualSegmentDictionary
                    .Should()
                    .ContainKeys(_expectedSegmentDictionary.Keys);

                foreach (string key in _expectedSegmentDictionary.Keys)
                {
                    _actualSegmentDictionary[key]
                        .Should()
                        .ContainInConsecutiveOrder(_expectedSegmentDictionary[key]);
                }
            }
        }

        private void VerifyMocks()
        {
            if (_defaultSegmentNameCounter > 0)
            {
                _defaultSegmentNameGenerator
                    .Verify(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Next, Times.Exactly(_defaultSegmentNameCounter));
            }

            if (_locater.Setups.Any())
            {
                _locater.Verify();
            }

            if (_logger.Setups.Any())
            {
                _logger.Verify();
            }

            if (_segmentHeaderParser.Setups.Any())
            {
                _segmentHeaderParser.Verify();
            }

            if (_textLineParser.Setups.Any())
            {
                _textLineParser.Verify();
            }

            MocksVerifyNoOtherCalls();
        }
    }
}