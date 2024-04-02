namespace TextTemplateProcessor
{
    public class TextTemplateProcessorTests
    {
        private static readonly string _templateDirectoryPath = $"{VolumeRoot}{Sep}test";
        private static readonly string _templateFileName = "template.txt";
        private static readonly string _templateFilePath = $"{_templateDirectoryPath}{Sep}{_templateFileName}";
        private readonly Mock<IDefaultSegmentNameGenerator> _defaultSegmentNameGenerator = new();
        private readonly Mock<IIndentProcessor> _indentProcessor = new();
        private readonly Mock<ILocater> _locater = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<ITemplateLoader> _templateLoader = new();
        private readonly Mock<ITextReader> _textReader = new();
        private readonly Mock<ITextWriter> _textWriter = new();
        private readonly Mock<ITokenProcessor> _tokenProcessor = new();
        private int _currentIndent;
        private int _currentTabSize;
        private int _textLineCounter;

        [Fact]
        public void CurrentIndent_WhenCalled_CallsIndentProcessorCurrentIndent()
        {
            // Arrange
            InitializeMocks();
            int expected = 4;
            _indentProcessor.Setup(x => x.CurrentIndent)
                .Returns(expected)
                .Verifiable(Times.Once);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            int actual = processor.CurrentIndent;

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void CurrentSegment_Getter_CallsLocaterCurrentSegmentGetter()
        {
            // Arrange
            InitializeMocks();
            string expected = "Segment1";
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(expected)
                .Verifiable(Times.Once);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            string actual = processor.CurrentSegment;

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void GeneratedText_Getter_ReturnsCopyOfGeneratedTextBuffer()
        {
            // Arrange
            InitializeMocks();
            List<string> expected = new()
            {
                "Line 1",
                "Line 2",
                "Line 3"
            };
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._generatedText.AddRange(expected);

            // Act
            List<string> actual = processor.GeneratedText.ToList();

            // Assert
            actual
                .Should()
                .NotBeNullOrEmpty();
            actual
                .Should()
                .NotBeSameAs(expected);
            actual
                .Should()
                .HaveSameCount(expected);
            actual
                .Should()
                .ContainInConsecutiveOrder(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void GenerateSegment_IsOutputFileWrittenFlagIsTrue_GeneratesTextAndSetsFlagToFalse()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            ControlItem controlItem = new();
            SetupGenerateSegment(segmentName);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._controlDictionary[segmentName] = controlItem;
            List<string> expected = new()
            {
                SetupGenerateTextLine(processor, segmentName, 0),
                SetupGenerateTextLine(processor, segmentName, 1),
                SetupGenerateTextLine(processor, segmentName, -1)
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = true;

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(segmentName);
            _locater.Object.LineNumber
                .Should()
                .Be(expected.Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_PassInTokenValueDictionary_CallsLoadTokenValuesOnTokenProcessor()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            ControlItem controlItem = new();
            Dictionary<string, string> tokenValues = new()
            {
                ["Token1"] = "Token value 1"
            };
            SetupGenerateSegment(segmentName, null, tokenValues);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._controlDictionary[segmentName] = controlItem;
            List<string> expected = new()
            {
                SetupGenerateTextLine(processor, segmentName, 0),
                SetupGenerateTextLine(processor, segmentName, 0),
                SetupGenerateTextLine(processor, segmentName, 0)
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;

            // Act
            processor.GenerateSegment(segmentName, tokenValues);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(segmentName);
            _locater.Object.LineNumber
                .Should()
                .Be(expected.Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasFirstTimeIndentOption_GeneratesTextWithCorrectFirstTimeIndent()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            int firstTimeIndent = 2;
            ControlItem controlItem = new() { FirstTimeIndent = firstTimeIndent };
            SetupGenerateSegment(segmentName);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._controlDictionary[segmentName] = controlItem;
            List<string> expected = new()
            {
                SetupGenerateTextLine(processor, segmentName, 0, firstTimeIndent),
                SetupGenerateTextLine(processor, segmentName, 1),
                SetupGenerateTextLine(processor, segmentName, -1),
                SetupGenerateTextLine(processor, segmentName, -1)
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(segmentName);
            _locater.Object.LineNumber
                .Should()
                .Be(expected.Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasMultipleOptions_GeneratesTextAndHandlesOptionsCorrectly()
        {
            // Arrange
            InitializeMocks();
            string padSegment = "PadSegment";
            string segmentName = "Segment1";
            int firstTimeIndent = 1;
            int tabSize = 2;
            ControlItem controlItem1 = new();
            ControlItem controlItem2 = new()
            {
                FirstTimeIndent = firstTimeIndent,
                PadSegment = padSegment,
                TabSize = tabSize
            };
            SetupGenerateSegment(padSegment);
            SetupGenerateSegment(segmentName, padSegment, null, tabSize);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._controlDictionary[padSegment] = controlItem1;
            processor._controlDictionary[segmentName] = controlItem2;
            List<string> expected = new()
            {
                SetupGenerateTextLine(processor, padSegment, 1),
                SetupGenerateTextLine(processor, padSegment, 0),
                SetupGenerateTextLine(processor, padSegment, 1)
            };
            _currentIndent = 0;
            expected.AddRange(new[]
            {
                SetupGenerateTextLine(processor, segmentName, 0, firstTimeIndent),
                SetupGenerateTextLine(processor, segmentName, 1),
                SetupGenerateTextLine(processor, segmentName, -2),
                SetupGenerateTextLine(processor, segmentName, 0)
            });
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(segmentName);
            _locater.Object.LineNumber
                .Should()
                .Be(processor._segmentDictionary[segmentName].Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasNoTextLines_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            _locater
                .SetupProperty(x => x.CurrentSegment);
            _locater
                .SetupProperty(x => x.LineNumber);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgSegmentHasNoTextLines, segmentName, null))
                .Verifiable(Times.Once);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor.IsTemplateLoaded = true;
            processor._controlDictionary[segmentName] = new();

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .BeEmpty();
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasPadOption_GeneratesTextWithCorrectPad()
        {
            // Arrange
            InitializeMocks();
            string padSegment = "PadSegment";
            string segmentName = "Segment1";
            ControlItem controlItem1 = new();
            ControlItem controlItem2 = new() { PadSegment = padSegment };
            SetupGenerateSegment(padSegment);
            SetupGenerateSegment(segmentName, padSegment);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._controlDictionary[padSegment] = controlItem1;
            processor._controlDictionary[segmentName] = controlItem2;
            List<string> expected = new()
            {
                SetupGenerateTextLine(processor, padSegment, 2),
                SetupGenerateTextLine(processor, padSegment, 0),
                SetupGenerateTextLine(processor, padSegment, 0)
            };
            _currentIndent = 0;
            expected.AddRange(new[]
            {
                SetupGenerateTextLine(processor, segmentName, 1),
                SetupGenerateTextLine(processor, segmentName, 1),
                SetupGenerateTextLine(processor, segmentName, -2),
                SetupGenerateTextLine(processor, segmentName, 0)
            });
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(segmentName);
            _locater.Object.LineNumber
                .Should()
                .Be(processor._segmentDictionary[segmentName].Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasTabOption_GeneratesTextWithCorrectTabSetting()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            int tabSize = 2;
            ControlItem controlItem = new() { TabSize = tabSize };
            SetupGenerateSegment(segmentName, null, null, tabSize);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._controlDictionary[segmentName] = controlItem;
            List<string> expected = new()
            {
                SetupGenerateTextLine(processor, segmentName, 1),
                SetupGenerateTextLine(processor, segmentName, 1),
                SetupGenerateTextLine(processor, segmentName, -2),
                SetupGenerateTextLine(processor, segmentName, 0)
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(segmentName);
            _locater.Object.LineNumber
                .Should()
                .Be(expected.Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_SegmentHasTextLinesAndDefaultSegmentOptions_GeneratesText()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            ControlItem controlItem = new();
            SetupGenerateSegment(segmentName);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._controlDictionary[segmentName] = controlItem;
            List<string> expected = new()
            {
                SetupGenerateTextLine(processor, segmentName, 2),
                SetupGenerateTextLine(processor, segmentName, 0),
                SetupGenerateTextLine(processor, segmentName, -1),
                SetupGenerateTextLine(processor, segmentName, 2)
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            _locater.Object.CurrentSegment
                .Should()
                .Be(segmentName);
            _locater.Object.LineNumber
                .Should()
                .Be(expected.Count);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_TemplateFileNotLoaded_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            _locater
                .SetupProperty(x => x.LineNumber);
            _locater
                .SetupProperty(x => x.CurrentSegment);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgAttemptToGenerateSegmentBeforeItWasLoaded, segmentName, null))
                .Verifiable(Times.Once);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor.IsTemplateLoaded = false;

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .BeEmpty();
            _locater.Object.CurrentSegment
                .Should()
                .Be(segmentName);
            _locater.Object.LineNumber
                .Should()
                .Be(0);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_UnknownSegmentName_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            _locater
                .SetupProperty(x => x.LineNumber);
            _locater
                .SetupProperty(x => x.CurrentSegment);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgUnknownSegmentName, segmentName, null))
                .Verifiable(Times.Once);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor.IsTemplateLoaded = true;

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .BeEmpty();
            _locater.Object.CurrentSegment
                .Should()
                .Be(segmentName);
            _locater.Object.LineNumber
                .Should()
                .Be(0);
            VerifyMocks();
        }

        [Fact]
        public void LineNumber_Getter_CallsLocaterLineNumberGetter()
        {
            // Arrange
            InitializeMocks();
            int expected = 9;
            _locater
                .Setup(x => x.LineNumber)
                .Returns(expected)
                .Verifiable(Times.Once);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            int actual = processor.LineNumber;

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void TabSize_Getter_CallsIndentProcessorTabSizeGetter()
        {
            // Arrange
            InitializeMocks();
            int expected = 3;
            _indentProcessor
                .Setup(x => x.TabSize)
                .Returns(expected)
                .Verifiable(Times.Once);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            int actual = processor.TabSize;

            // Assert
            actual
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void TemplateFilePath_Getter_CallsTextReaderFullFilePathGetter()
        {
            // Arrange
            InitializeMocks();
            _textReader
                .Setup(x => x.FullFilePath)
                .Returns(_templateFilePath)
                .Verifiable(Times.Once);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            string actual = processor.TemplateFilePath;

            // Assert
            actual
                .Should()
                .Be(_templateFilePath);
            VerifyMocks();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullDefaultSegmentNameGenerator_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            Action action;
            TextTemplateProcessor processor;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.DefaultSegmentNameGeneratorService,
                                                       ServiceParameterNames.DefaultSegmentNameGeneratorParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    processor = new(_templateFilePath,
                                    _logger.Object,
                                    null!,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                })
                : (() =>
                {
                    processor = new(_logger.Object,
                                    null!,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullIndentProcessor_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            Action action;
            TextTemplateProcessor processor;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.IndentProcessorService,
                                                       ServiceParameterNames.IndentProcessorParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    processor = new(_templateFilePath,
                                    _logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    null!,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                })
                : (() =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    null!,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullLocater_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            Action action;
            TextTemplateProcessor processor;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.LocaterService,
                                                       ServiceParameterNames.LocaterParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    processor = new(_templateFilePath,
                                    _logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    null!,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                })
                : (() =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    null!,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullLogger_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            Action action;
            TextTemplateProcessor processor;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.LoggerService,
                                                       ServiceParameterNames.LoggerParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    processor = new(_templateFilePath,
                                    null!,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                })
                : (() =>
                {
                    processor = new(null!,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullTemplateLoader_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            Action action;
            TextTemplateProcessor processor;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.TemplateLoaderService,
                                                       ServiceParameterNames.TemplateLoaderParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    processor = new(_templateFilePath,
                                    _logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    null!,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                })
                : (() =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    null!,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullTextReader_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            Action action;
            TextTemplateProcessor processor;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.TextReaderService,
                                                       ServiceParameterNames.TextReaderParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    processor = new(_templateFilePath,
                                    _logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    null!,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                })
                : (() =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    null!,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullTextWriter_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            Action action;
            TextTemplateProcessor processor;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.TextWriterService,
                                                       ServiceParameterNames.TextWriterParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    processor = new(_templateFilePath,
                                    _logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    null!,
                                    _tokenProcessor.Object);
                })
                : (() =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    null!,
                                    _tokenProcessor.Object);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullTokenProcessor_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            Action action;
            TextTemplateProcessor processor;
            string expected = GetNullDependencyMessage(ClassNames.TextTemplateProcessorClass,
                                                       ServiceNames.TokenProcessorService,
                                                       ServiceParameterNames.TokenProcessorParameter);
            action = useTemplateFilePath
                ? (() =>
                {
                    processor = new(_templateFilePath,
                                    _logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    null!);
                })
                : (() =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    null!);
                });

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingValidDependencies_InitializesProperties(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();

            if (useTemplateFilePath)
            {
                _logger
                    .Setup(x => x.SetLogEntryType(LogEntryType.Setup))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(x => x.SetFilePath(_templateFilePath))
                    .Verifiable(Times.Once);
            }

            // Act
            TextTemplateProcessor processor = useTemplateFilePath ? GetTextTemplateProcessor(_templateFilePath) : GetTextTemplateProcessor();

            // Assert
            processor._controlDictionary
                .Should()
                .NotBeNull();
            processor._controlDictionary
                .Should()
                .BeEmpty();
            processor._segmentDictionary
                .Should()
                .NotBeNull();
            processor._segmentDictionary
                .Should()
                .BeEmpty();
            processor._generatedText
                .Should()
                .NotBeNull();
            processor._generatedText
                .Should()
                .BeEmpty();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor.IsTemplateLoaded
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        private TextTemplateProcessor GetTextTemplateProcessor(string? templateFilePath = null)
        {
            return templateFilePath is null
                ? new TextTemplateProcessor(_logger.Object,
                                            _defaultSegmentNameGenerator.Object,
                                            _indentProcessor.Object,
                                            _locater.Object,
                                            _templateLoader.Object,
                                            _textReader.Object,
                                            _textWriter.Object,
                                            _tokenProcessor.Object)
                : new TextTemplateProcessor(templateFilePath,
                                            _logger.Object,
                                            _defaultSegmentNameGenerator.Object,
                                            _indentProcessor.Object,
                                            _locater.Object,
                                            _templateLoader.Object,
                                            _textReader.Object,
                                            _textWriter.Object,
                                            _tokenProcessor.Object);
        }

        private void InitializeMocks()
        {
            _defaultSegmentNameGenerator.Reset();
            _indentProcessor.Reset();
            _locater.Reset();
            _logger.Reset();
            _templateLoader.Reset();
            _textReader.Reset();
            _textWriter.Reset();
            _tokenProcessor.Reset();
            _currentIndent = 0;
            _currentTabSize = DefaultTabSize;
            _textLineCounter = 0;
        }

        private void MocksVerifyNoOtherCalls()
        {
            _defaultSegmentNameGenerator.VerifyNoOtherCalls();
            _indentProcessor.VerifyNoOtherCalls();
            _locater.VerifyNoOtherCalls();
            _logger.VerifyNoOtherCalls();
            _templateLoader.VerifyNoOtherCalls();
            _textReader.VerifyNoOtherCalls();
            _textWriter.VerifyNoOtherCalls();
            _tokenProcessor.VerifyNoOtherCalls();
        }

        private void SetupGenerateSegment(string segmentName,
                                          string? padSegment = null,
                                          Dictionary<string, string>? tokenValues = null,
                                          int tabSize = 0)
        {
            int expectedGeneratingCount = 1;

            _locater
                .SetupProperty(x => x.LineNumber);
            _locater
                .SetupProperty(x => x.CurrentSegment);
            _logger
                .Setup(x => x.Log(MsgProcessingSegment, segmentName, null))
                .Verifiable(Times.Once);

            if (tokenValues is not null)
            {
                _tokenProcessor
                    .Setup(x => x.LoadTokenValues(tokenValues))
                    .Verifiable(Times.Once);
            }

            if (padSegment is not null)
            {
                _logger
                    .Setup(x => x.Log(MsgProcessingSegment, padSegment, null))
                    .Verifiable(Times.Once);
                _indentProcessor
                    .Setup(x => x.SaveCurrentState())
                    .Verifiable(Times.Once);
                _indentProcessor
                    .Setup(x => x.RestoreCurrentState())
                    .Verifiable(Times.Once);
                expectedGeneratingCount++;
            }

            if (tabSize > 0)
            {
                _logger
                    .Setup(x => x.SetLogEntryType(LogEntryType.Setup))
                    .Verifiable(Times.Once);
                _indentProcessor
                    .Setup(x => x.SetTabSize(tabSize))
                    .Verifiable(Times.Once);
                expectedGeneratingCount++;
                _currentTabSize = tabSize;
            }

            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Generating))
                    .Verifiable(Times.Exactly(expectedGeneratingCount));
        }

        private string SetupGenerateTextLine(TextTemplateProcessor processor,
                                             string segmentName,
                                             int indent,
                                             int firstTimeIndent = 0)
        {
            string text = $"Line {++_textLineCounter}";
            TextItem textItem = new(indent, true, false, text);

            if (processor._segmentDictionary.ContainsKey(segmentName))
            {
                processor._segmentDictionary[segmentName].Add(textItem);
                _currentIndent += indent * _currentTabSize;
                _indentProcessor
                    .Setup(x => x.GetIndent(textItem))
                    .Returns(_currentIndent)
                    .Verifiable(Times.Once);
            }
            else
            {
                processor._segmentDictionary[segmentName] = new() { textItem };

                if (firstTimeIndent > 0)
                {
                    _currentIndent += firstTimeIndent * _currentTabSize;
                }
                else
                {
                    _currentIndent += indent * _currentTabSize;
                }

                _indentProcessor
                    .Setup(x => x.GetFirstTimeIndent(firstTimeIndent, textItem))
                    .Returns(_currentIndent)
                    .Verifiable(Times.Once);
            }

            _tokenProcessor
                .Setup(x => x.ReplaceTokens(text))
                .Returns(text)
                .Verifiable(Times.Once);

            string pad = new(' ', _currentIndent);
            return pad + text;
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

            if (_templateLoader.Setups.Any())
            {
                _templateLoader.Verify();
            }

            if (_textReader.Setups.Any())
            {
                _textReader.Verify();
            }

            if (_textWriter.Setups.Any())
            {
                _textWriter.Verify();
            }

            if (_tokenProcessor.Setups.Any())
            {
                _tokenProcessor.Verify();
            }

            MocksVerifyNoOtherCalls();
        }
    }
}