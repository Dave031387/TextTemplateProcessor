namespace TextTemplateProcessor
{
    public class TemplateLoaderTests
    {
        private readonly Dictionary<string, ControlItem> _actualControlDictionary = new();
        private readonly Dictionary<string, List<TextItem>> _actualSegmentDictionary = new();
        private readonly Mock<IDefaultSegmentNameGenerator> _defaultSegmentNameGenerator = new(MockBehavior.Strict);
        private readonly Dictionary<string, ControlItem> _expectedControlDictionary = new();
        private readonly Dictionary<string, List<TextItem>> _expectedSegmentDictionary = new();
        private readonly Mock<ILocater> _locater = new(MockBehavior.Strict);
        private readonly Mock<ILogger> _logger = new(MockBehavior.Strict);
        private readonly Mock<ISegmentHeaderParser> _segmentHeaderParser = new(MockBehavior.Strict);
        private readonly List<string> _templateLines = new();
        private readonly Mock<ITextLineParser> _textLineParser = new(MockBehavior.Strict);
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
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
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void LoadTemplate_SegmentNameIsDuplicate_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            AddSegmentHeaderLineToTemplateFile("Segment1");
            AddTextLineToTemplateFile();
            AddSegmentHeaderLineToTemplateFile("Segment1", true);
            AddTextLineToTemplateFile();

            // Act
            LoadTemplate();

            // Assert
            VerifyDictionaries();
            VerifyMocks();
            MocksVerifyNoOtherCalls();
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
                .SetupSet(x => x.LineNumber = _lineCounter)
                .Verifiable();
            _textLineParser
                .Setup(x => x.IsValidPrefix(commentLine))
                .Returns(true)
                .Verifiable();
            _textLineParser
                .Setup(x => x.IsCommentLine(commentLine))
                .Returns(true)
                .Verifiable();
        }

        private void AddSegmentHeaderLineToTemplateFile(string segmentName,
                                                        bool isDuplicateSegmentName = false,
                                                        bool isSegmentWithTextLines = true,
                                                        string? padSegment = null,
                                                        bool isUnknownPadSegment = false)
        {
            string padOption = padSegment is null ? string.Empty : $" PAD={padSegment}";
            string segmentHeaderLine = $"{SegmentHeaderCode} {segmentName}{padOption}";

            _templateLines.Add(segmentHeaderLine);
            _lineCounter++;
            _locater
                .SetupSet(x => x.LineNumber = _lineCounter)
                .Verifiable();
            _textLineParser
                .Setup(x => x.IsValidPrefix(segmentHeaderLine))
                .Returns(true)
                .Verifiable();
            _textLineParser
                .Setup(x => x.IsCommentLine(segmentHeaderLine))
                .Returns(false)
                .Verifiable();
            _textLineParser
                .Setup(x => x.IsSegmentHeader(segmentHeaderLine))
                .Returns(true)
                .Verifiable();

            if (isDuplicateSegmentName)
            {
                string defaultSegmentName = $"{DefaultSegmentNamePrefix}{++_defaultSegmentNameCounter}";
                _currentSegmentName = defaultSegmentName;
                _logger
                    .Setup(MockHelper.GetLoggerExpression(MsgFoundDuplicateSegmentName,
                                                          segmentName,
                                                          _currentSegmentName))
                    .Verifiable();
            }
            else
            {
                _currentSegmentName = segmentName;
            }

            _logger
                .Setup(MockHelper.GetLoggerExpression(MsgSegmentHasBeenAdded,
                                                      _currentSegmentName))
                .Verifiable();

            bool isValidPadOption = true;

            if (padSegment is null)
            {
                padOption = string.Empty;
                isValidPadOption = false;
            }
            else
            {
                padOption = $" PAD={padSegment}";

                if (isUnknownPadSegment)
                {
                    _logger.Setup(MockHelper.GetLoggerExpression(MsgPadSegmentMustBeDefinedEarlier,
                                                                 segmentName,
                                                                 padSegment))
                        .Verifiable();
                    isValidPadOption = false;
                }
                else if (padSegment == segmentName)
                {
                    _logger.Setup(MockHelper.GetLoggerExpression(MsgPadSegmentNameSameAsSegmentHeaderName,
                                                                 segmentName))
                        .Verifiable();
                    isValidPadOption = false;
                }
            }

            if (isSegmentWithTextLines)
            {
                ControlItem savedControlItem = new();

                if (isValidPadOption)
                {
                    savedControlItem.PadSegment = padSegment!;
                }

                _expectedControlDictionary[_currentSegmentName] = savedControlItem;
            }
            else
            {
                _logger
                    .Setup(MockHelper.GetLoggerExpression(MsgNoTextLinesFollowingSegmentHeader,
                                                          _currentSegmentName))
                    .Verifiable();
            }

            ControlItem returnedControlItem = new()
            {
                PadSegment = padSegment is null ? string.Empty : padSegment
            };

            _segmentHeaderParser
                .Setup(x => x.ParseSegmentHeader(segmentHeaderLine))
                .Returns(returnedControlItem)
                .Verifiable();
        }

        private void AddTextLineToTemplateFile(bool isMissingSegmentHeader = false,
                                               bool isValidTemplateLine = true)
        {
            string text = $"Text line {++_lineCounter}";
            string templateLine = isValidTemplateLine
                ? $"{IndentUnchanged} {text}"
                : $"XXX Invalid {text}";

            _templateLines.Add(templateLine);
            _locater
                .SetupSet(x => x.LineNumber = _lineCounter)
                .Verifiable();

            if (isMissingSegmentHeader)
            {
                string defaultSegmentName = $"{DefaultSegmentNamePrefix}{++_defaultSegmentNameCounter}";
                _currentSegmentName = defaultSegmentName;
                _logger
                    .Setup(MockHelper.GetLoggerExpression(MsgMissingInitialSegmentHeader,
                                                          _currentSegmentName))
                    .Verifiable();
                _expectedControlDictionary[_currentSegmentName] = new();
            }

            if (isValidTemplateLine)
            {
                _textLineParser
                    .Setup(x => x.IsValidPrefix(templateLine))
                    .Returns(true)
                    .Verifiable();
                _textLineParser
                    .Setup(x => x.IsCommentLine(templateLine))
                    .Returns(false)
                    .Verifiable();

                if (templateLine[..3] == Comment)
                {
                    return;
                }

                _textLineParser
                    .Setup(x => x.IsSegmentHeader(templateLine))
                    .Returns(false)
                    .Verifiable();

                TextItem textItem = new(0, true, false, text);

                if (_expectedSegmentDictionary.ContainsKey(_currentSegmentName))
                {
                    _expectedSegmentDictionary[_currentSegmentName].Add(textItem);
                }
                else
                {
                    _expectedSegmentDictionary[_currentSegmentName] = new() { textItem };
                }

                _textLineParser
                    .Setup(x => x.ParseTextLine(templateLine))
                    .Returns(textItem)
                    .Verifiable();
            }
            else
            {
                _textLineParser
                    .Setup(x => x.IsValidPrefix(templateLine))
                    .Returns(false)
                    .Verifiable();
            }
        }

        private string GetCurrentSegment() => _currentSegmentName;

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
                .SetupSequence(x => x.Next)
                .Returns($"{DefaultSegmentNamePrefix}1")
                .Returns($"{DefaultSegmentNamePrefix}2")
                .Returns($"{DefaultSegmentNamePrefix}3")
                .Returns($"{DefaultSegmentNamePrefix}4")
                .Throws<ArgumentException>();
            _locater
                .SetupSet(x => x.CurrentSegment = It.IsAny<string>())
                .Callback<string>(x => SetCurrentSegment(x))
                .Verifiable();
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(() => GetCurrentSegment())
                .Verifiable();
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Parsing))
                .Verifiable();
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

        private void SetCurrentSegment(string segmentName) => _currentSegmentName = segmentName;

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
                    .HaveCount(_expectedControlDictionary.Count);
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
                    .HaveCount(_expectedSegmentDictionary.Count);
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
                _defaultSegmentNameGenerator.Verify(x => x.Next, Times.Exactly(_defaultSegmentNameCounter));
            }

            if (_locater.Setups.Count > 0)
            {
                _locater.Verify();
            }

            if (_logger.Setups.Count > 0)
            {
                _logger.Verify();
            }

            if (_segmentHeaderParser.Setups.Count > 0)
            {
                _segmentHeaderParser.Verify();
            }

            if (_textLineParser.Setups.Count > 0)
            {
                _textLineParser.Verify();
            }
        }
    }
}