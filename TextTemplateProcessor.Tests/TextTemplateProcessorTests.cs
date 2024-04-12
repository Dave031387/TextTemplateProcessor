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
        private readonly MethodCallOrderVerifier _verifier = new();
        private int _currentIndent;
        private int _currentTabSize;
        private int _textLineCounter;

        [Fact]
        public void CurrentIndent_WhenCalled_CallsIndentProcessorCurrentIndent()
        {
            // Arrange
            InitializeMocks();
            int expected = 4;
            _indentProcessor
                .Setup(x => x.CurrentIndent)
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
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgAttemptToGenerateSegmentBeforeItWasLoaded, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Generating, MethodCall.Logger_Log_Message);
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
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgUnknownSegmentName, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Generating, MethodCall.Logger_Log_Message);
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
        public void LoadTemplateWithFilePath_InvalidFilePath_LogsMessage()
        {
            // Arrange
            string filePath = NextAbsoluteFilePath;
            string[] generatedText = new[] { "Line 1", "Line 2" };
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            InitializeMocks();
            _textReader
                .Setup(x => x.FileName)
                .Returns(string.Empty)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFileName,
                                                       null,
                                                       true))
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(x => x.FullFilePath)
                .Returns(string.Empty)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFullFilePath,
                                                       null,
                                                       true))
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(x => x.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFileName, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFullFilePath, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_SetFilePath);
            processor._generatedText.AddRange(generatedText);
            processor.IsTemplateLoaded = true;

            // Act
            processor.LoadTemplate(filePath);

            // Assert
            processor.IsTemplateLoaded
                .Should()
                .BeFalse();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._generatedText
                .Should()
                .ContainInConsecutiveOrder(generatedText);
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplateWithFilePath_LastGeneratedTextNotWritten_LogsMessageAndLoadsTemplate()
        {
            // Arrange
            string directoryPath = NextAbsoluteName;
            string lastFileName = NextFileName;
            string lastFilePath = $"{directoryPath}{Sep}{lastFileName}";
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            string[] templateLines = new[] { "Line 1", "Line 2" };
            string returnedFileName = lastFileName;
            string returnedFilePath = lastFilePath;
            TextTemplateProcessor processor = GetTextTemplateProcessor(filePath);
            InitializeMocks();
            SetupResetAll();
            _textReader
                .Setup(x => x.FileName)
                .Returns(() => returnedFileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFileName,
                                                       null,
                                                       true,
                                                       () => returnedFileName = fileName))
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(x => x.FullFilePath)
                .Returns(() => returnedFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFullFilePath,
                                                       null,
                                                       true,
                                                       () => returnedFilePath = filePath))
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(x => x.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(x => x.ReadTextFile())
                .Returns(templateLines)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_ReadTextFile))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgNextLoadRequestBeforeFirstIsWritten, fileName, lastFileName))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_FirstMessage))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgLoadingTemplateFile, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_SecondMessage))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(x => x.LoadTemplate(templateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFileName, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFullFilePath, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_SetFilePath, MethodCall.Logger_Log_FirstMessage);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_FirstMessage, MethodCall.TextReader_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_Log_SecondMessage);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_Log_SecondMessage);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_Log_SecondMessage);
            _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_Log_SecondMessage);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_SecondMessage, MethodCall.TemplateLoader_LoadTemplate);
            processor._generatedText.AddRange(new[] { "Line A", "Line B" });
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;

            // Act
            processor.LoadTemplate(filePath);

            // Assert
            processor._generatedText
                .Should()
                .BeEmpty();
            processor.IsTemplateLoaded
                .Should()
                .BeTrue();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplateWithFilePath_LastGeneratedTextWasWritten_LoadsTemplate()
        {
            // Arrange
            string directoryPath = NextAbsoluteName;
            string lastFileName = NextFileName;
            string lastFilePath = $"{directoryPath}{Sep}{lastFileName}";
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            string[] templateLines = new[] { "Line 1", "Line 2" };
            string returnedFileName = lastFileName;
            string returnedFilePath = lastFilePath;
            TextTemplateProcessor processor = GetTextTemplateProcessor(filePath);
            InitializeMocks();
            SetupResetAll();
            _textReader
                .Setup(x => x.FileName)
                .Returns(() => returnedFileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFileName,
                                                       null,
                                                       true,
                                                       () => returnedFileName = fileName))
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(x => x.FullFilePath)
                .Returns(() => returnedFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFullFilePath,
                                                       null,
                                                       true,
                                                       () => returnedFilePath = filePath))
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(x => x.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(x => x.ReadTextFile())
                .Returns(templateLines)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_ReadTextFile))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgLoadingTemplateFile, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(x => x.LoadTemplate(templateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFileName, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFullFilePath, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_SetFilePath, MethodCall.TextReader_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.TemplateLoader_LoadTemplate);
            processor._generatedText.AddRange(new[] { "Line A", "Line B" });
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = true;

            // Act
            processor.LoadTemplate(filePath);

            // Assert
            processor._generatedText
                .Should()
                .BeEmpty();
            processor.IsTemplateLoaded
                .Should()
                .BeTrue();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplateWithFilePath_TemplateAlreadyLoaded_LogsMessage()
        {
            // Arrange
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            string[] generatedText = new[] { "Line 1", "Line 2" };
            TextTemplateProcessor processor = GetTextTemplateProcessor(filePath);
            InitializeMocks();
            _textReader
                .Setup(x => x.FileName)
                .Returns(fileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFileName, null, true))
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(x => x.FullFilePath)
                .Returns(filePath)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFullFilePath, null, true))
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(x => x.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgAttemptToLoadMoreThanOnce, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFileName, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFullFilePath, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_SetFilePath, MethodCall.Logger_Log_Message);
            processor._generatedText.AddRange(generatedText);
            processor.IsTemplateLoaded = true;

            // Act
            processor.LoadTemplate(filePath);

            // Assert
            processor.IsTemplateLoaded
                .Should()
                .BeTrue();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._generatedText
                .Should()
                .ContainInConsecutiveOrder(generatedText);
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplateWithFilePath_TemplateFileIsEmpty_LogsMessage()
        {
            // Arrange
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            string returnedFileName = string.Empty;
            string returnedFilePath = string.Empty;
            TextTemplateProcessor processor = GetTextTemplateProcessor(filePath);
            InitializeMocks();
            SetupResetAll();
            _textReader
                .Setup(x => x.FileName)
                .Returns(() => returnedFileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFileName,
                                                       null,
                                                       true,
                                                       () => returnedFileName = fileName))
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(x => x.FullFilePath)
                .Returns(() => returnedFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFullFilePath,
                                                       null,
                                                       true,
                                                       () => returnedFilePath = filePath))
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(x => x.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(x => x.ReadTextFile())
                .Returns(Array.Empty<string>())
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_ReadTextFile))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgTemplateFileIsEmpty, filePath, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFileName, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFullFilePath, MethodCall.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_SetFilePath, MethodCall.TextReader_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_Log_Message);
            processor._generatedText.AddRange(new[] { "Line 1", "Line 2" });
            processor.IsTemplateLoaded = false;
            processor.IsOutputFileWritten = false;

            // Act
            processor.LoadTemplate(filePath);

            // Assert
            processor._generatedText
                .Should()
                .BeEmpty();
            processor.IsTemplateLoaded
                .Should()
                .BeFalse();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplateWithoutFilePath_FilePathNotSet_LogsMessage()
        {
            // Arrange
            string[] generatedText = new[] { "Line 1", "Line 2" };
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            InitializeMocks();
            _textReader
                .Setup(x => x.FullFilePath)
                .Returns(string.Empty)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFullFilePath))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgTemplateFilePathNotSet, null, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFullFilePath, MethodCall.Logger_Log_Message);
            processor._generatedText.AddRange(generatedText);

            // Act
            processor.LoadTemplate();

            // Assert
            processor.IsTemplateLoaded
                .Should()
                .BeFalse();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._generatedText
                .Should()
                .ContainInConsecutiveOrder(generatedText);
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplateWithoutFilePath_TemplateFileIsEmpty_LogsMessage()
        {
            // Arrange
            string filePath = NextAbsoluteFilePath;
            string[] templateLines = Array.Empty<string>();
            TextTemplateProcessor processor = GetTextTemplateProcessor(filePath);
            InitializeMocks();
            SetupResetAll();
            _textReader
                .Setup(x => x.FullFilePath)
                .Returns(filePath)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFullFilePath, null, true))
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(x => x.ReadTextFile())
                .Returns(templateLines)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_ReadTextFile))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgTemplateFileIsEmpty, filePath, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFullFilePath, MethodCall.TextReader_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_Log_Message);
            processor._generatedText.AddRange(new[] { "Line 1", "Line 2" });

            // Act
            processor.LoadTemplate();

            // Assert
            processor._generatedText
                .Should()
                .BeEmpty();
            processor.IsTemplateLoaded
                .Should()
                .BeFalse();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplateWithoutFilePath_TemplateFileIsNotEmpty_LoadsTemplateFile()
        {
            // Arrange
            string fileName = NextFileName;
            string directoryPath = NextAbsoluteName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            string[] templateLines = new[] { "Line 1", "Line 2" };
            TextTemplateProcessor processor = GetTextTemplateProcessor(filePath);
            InitializeMocks();
            SetupResetAll();
            _textReader
                .Setup(x => x.FullFilePath)
                .Returns(filePath)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFullFilePath, null, true))
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(x => x.FileName)
                .Returns(fileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFileName, null, true))
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(x => x.ReadTextFile())
                .Returns(templateLines)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_ReadTextFile))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgLoadingTemplateFile, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(x => x.LoadTemplate(templateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFullFilePath, MethodCall.TextReader_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_ReadTextFile, MethodCall.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFileName, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_Log_Message, MethodCall.TemplateLoader_LoadTemplate);
            processor._generatedText.AddRange(new[] { "Line A", "Line B" });

            // Act
            processor.LoadTemplate();

            // Assert
            processor._generatedText
                .Should()
                .BeEmpty();
            processor.IsTemplateLoaded
                .Should()
                .BeTrue();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void LoadTemplateWithoutFilePath_TemplateIsAlreadyLoaded_LogsMessage()
        {
            // Arrange
            string fileName = NextFileName;
            string directoryPath = NextAbsoluteName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            string[] generatedText = new[] { "Line 1", "Line 2" };
            TextTemplateProcessor processor = GetTextTemplateProcessor(filePath);
            InitializeMocks();
            _textReader
                .Setup(x => x.FileName)
                .Returns(fileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFileName))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgAttemptToLoadMoreThanOnce, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Loading, MethodCall.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFileName, MethodCall.Logger_Log_Message);
            processor._generatedText.AddRange(generatedText);
            processor.IsTemplateLoaded = true;

            // Act
            processor.LoadTemplate();

            // Assert
            processor.IsTemplateLoaded
                .Should()
                .BeTrue();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._generatedText
                .Should()
                .ContainInConsecutiveOrder(generatedText);
            VerifyMocks();
        }

        [Fact]
        public void ResetAll_ShouldDisplayMessageIsFalse_ResetsAllButDoesNotLogMessage()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            string textLine1 = "Line 1";
            string textLine2 = "Line 2";
            SetupResetAll();
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._generatedText.AddRange(new[] { textLine1, textLine2 });
            processor._controlDictionary[segmentName] = new();
            processor._segmentDictionary[segmentName] = new()
            {
                new(0, true, false, textLine1),
                new(0, true, false, textLine2)
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = true;

            // Act
            processor.ResetAll(false);

            // Assert
            processor._generatedText
                .Should()
                .BeEmpty();
            processor._controlDictionary
                .Should()
                .BeEmpty();
            processor._segmentDictionary
                .Should()
                .BeEmpty();
            processor.IsTemplateLoaded
                .Should()
                .BeFalse();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void ResetAll_ShouldDisplayMessageIsTrue_ResetsAllAndLogsMessage()
        {
            // Arrange
            InitializeMocks();
            string fileName = NextFileName;
            string segmentName = "Segment1";
            string textLine1 = "Line 1";
            string textLine2 = "Line 2";
            SetupResetAll();
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgTemplateHasBeenReset, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _textReader
                .Setup(x => x.FileName)
                .Returns(fileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFileName))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.DefaultSegmentNameGenerator_Reset, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.TokenProcessor_ClearTokens, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFileName, MethodCall.Logger_Log_Message);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._generatedText.AddRange(new[] { textLine1, textLine2 });
            processor._controlDictionary[segmentName] = new();
            processor._segmentDictionary[segmentName] = new()
            {
                new(0, true, false, textLine1),
                new(0, true, false, textLine2)
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = true;

            // Act
            processor.ResetAll();

            // Assert
            processor._generatedText
                .Should()
                .BeEmpty();
            processor._controlDictionary
                .Should()
                .BeEmpty();
            processor._segmentDictionary
                .Should()
                .BeEmpty();
            processor.IsTemplateLoaded
                .Should()
                .BeFalse();
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            VerifyMocks();
        }

        [Fact]
        public void ResetGeneratedText_ShouldDisplayMessageIsFalse_ResetsGeneratedTextBotDoesNotLogMessage()
        {
            // Arrange
            InitializeMocks();
            string segmentName1 = "Segment1";
            string segmentName2 = "Segment2";
            string segmentName3 = "Segment3";
            string padSegmentName = "Pad";
            int firstTimeIndent = 1;
            int tabSize = 2;
            ControlItem controlItem1 = new()
            {
                IsFirstTime = false,
                PadSegment = padSegmentName
            };
            ControlItem controlItem2 = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = firstTimeIndent
            };
            ControlItem controlItem3 = new()
            {
                IsFirstTime = false,
                TabSize = tabSize
            };
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(x => x.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.IndentProcessor_Reset);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._generatedText.AddRange(new[] { "Line 1", "Line 2" });
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = true;
            processor._controlDictionary[segmentName1] = controlItem1;
            processor._controlDictionary[segmentName2] = controlItem2;
            processor._controlDictionary[segmentName3] = controlItem3;
            Dictionary<string, ControlItem> expected = new()
            {
                [segmentName1] = new ControlItem() { IsFirstTime = true, PadSegment = padSegmentName },
                [segmentName2] = new ControlItem() { IsFirstTime = true, FirstTimeIndent = firstTimeIndent },
                [segmentName3] = new ControlItem() { IsFirstTime = true, TabSize = tabSize }
            };

            // Act
            processor.ResetGeneratedText(false);

            // Assert
            processor._controlDictionary
                .Should()
                .HaveSameCount(expected);
            processor._controlDictionary
                .Should()
                .BeEquivalentTo(expected);
            processor._generatedText
                .Should()
                .BeEmpty();
            processor.IsTemplateLoaded
                .Should()
                .BeTrue();
            processor.IsOutputFileWritten
                .Should()
                .BeTrue();
            VerifyMocks();
        }

        [Fact]
        public void ResetGeneratedText_ShouldDisplayMessageIsTrue_ResetsGeneratedTextAndLogsMessage()
        {
            // Arrange
            InitializeMocks();
            string fileName = NextFileName;
            string segmentName1 = "Segment1";
            string segmentName2 = "Segment2";
            string segmentName3 = "Segment3";
            string padSegmentName = "Pad";
            int firstTimeIndent = 1;
            int tabSize = 2;
            ControlItem controlItem1 = new()
            {
                IsFirstTime = false,
                PadSegment = padSegmentName
            };
            ControlItem controlItem2 = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = firstTimeIndent
            };
            ControlItem controlItem3 = new()
            {
                IsFirstTime = false,
                TabSize = tabSize
            };
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgGeneratedTextHasBeenReset, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(x => x.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(x => x.FileName)
                .Returns(fileName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_GetFileName))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.TextReader_GetFileName, MethodCall.Logger_Log_Message);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._generatedText.AddRange(new[] { "Line 1", "Line 2" });
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = true;
            processor._controlDictionary[segmentName1] = controlItem1;
            processor._controlDictionary[segmentName2] = controlItem2;
            processor._controlDictionary[segmentName3] = controlItem3;
            Dictionary<string, ControlItem> expected = new()
            {
                [segmentName1] = new ControlItem() { IsFirstTime = true, PadSegment = padSegmentName },
                [segmentName2] = new ControlItem() { IsFirstTime = true, FirstTimeIndent = firstTimeIndent },
                [segmentName3] = new ControlItem() { IsFirstTime = true, TabSize = tabSize }
            };

            // Act
            processor.ResetGeneratedText();

            // Assert
            processor._controlDictionary
                .Should()
                .HaveSameCount(expected);
            processor._controlDictionary
                .Should()
                .BeEquivalentTo(expected);
            processor._generatedText
                .Should()
                .BeEmpty();
            processor.IsTemplateLoaded
                .Should()
                .BeTrue();
            processor.IsOutputFileWritten
                .Should()
                .BeTrue();
            VerifyMocks();
        }

        [Theory]
        [InlineData("Segment1")]
        [InlineData("")]
        [InlineData(null)]
        public void ResetSegment_SegmentNotFound_LogsMessage(string? name)
        {
            // Arrange
            InitializeMocks();
            string segmentName = name is null ? string.Empty : name;
            _locater
                .SetupSet(x => x.CurrentSegment = segmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.LineNumber = 0)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_LineNumber_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(segmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter, null, true))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgUnableToResetSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.Locater_LineNumber_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_LineNumber_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            processor.ResetSegment(name!);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void ResetSegment_ValidSegmentName_ResetsSegment()
        {
            // Arrange
            InitializeMocks();
            string segmentName1 = "Segment1";
            ControlItem controlItem1 = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = 1,
                TabSize = 2,
                PadSegment = "Pad"
            };
            ControlItem expected1 = new()
            {
                IsFirstTime = true,
                FirstTimeIndent = 1,
                TabSize = 2,
                PadSegment = "Pad"
            };
            string segmentName2 = "Segment2";
            ControlItem controlItem2 = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = 0,
                TabSize = 0,
                PadSegment = ""
            };
            ControlItem expected2 = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = 0,
                TabSize = 0,
                PadSegment = ""
            };
            _locater
                .SetupSet(x => x.CurrentSegment = segmentName1)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(x => x.LineNumber = 0)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_LineNumber_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.CurrentSegment)
                .Returns(segmentName1)
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_CurrentSegment_Getter, null, true))
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgSegmentHasBeenReset, segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.Locater_LineNumber_Setter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Setter, MethodCall.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_LineNumber_Setter, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_CurrentSegment_Getter, MethodCall.Logger_Log_Message);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._controlDictionary[segmentName1] = controlItem1;
            processor._controlDictionary[segmentName2] = controlItem2;

            // Act
            processor.ResetSegment(segmentName1);

            // Assert
            processor._controlDictionary[segmentName1]
                .Should()
                .Be(expected1);
            processor._controlDictionary[segmentName2]
                .Should()
                .Be(expected2);
            VerifyMocks();
        }

        [Fact]
        public void SetTabSize_WhenCalled_InvokesIndentProcessorSetTabSize()
        {
            // Arrange
            InitializeMocks();
            int tabSize = 2;
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(x => x.SetTabSize(tabSize))
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.IndentProcessor_SetTabSize);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            processor.SetTabSize(tabSize);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void SetTemplateFilePath_WhenCalled_InvokesTextReaderSetFilePath()
        {
            // Arrange
            InitializeMocks();
            string filePath = NextAbsoluteFilePath;
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _textReader
                .Setup(x => x.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.TextReader_SetFilePath);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            processor.SetTemplateFilePath(filePath);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void SetTokenDelimiters_WhenCalled_InvokesTokenProcessorSetTokenDelimiters()
        {
            // Arrange
            InitializeMocks();
            string tokenStart = "{{";
            string tokenEnd = "}}";
            char escapeChar = '\\';
            bool expected = true;
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(x => x.SetTokenDelimiters(tokenStart, tokenEnd, escapeChar))
                .Returns(expected)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_SetTokenDelimiters))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.TokenProcessor_SetTokenDelimiters);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            bool actual = processor.SetTokenDelimiters(tokenStart, tokenEnd, TokenEscapeChar);

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
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullIndentProcessor_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullLocater_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullLogger_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullTemplateLoader_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullTextReader_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullTextWriter_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingNullTokenProcessor_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();
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
            MocksVerifyNoOtherCalls();
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
                    .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Setup))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(x => x.SetFilePath(_templateFilePath))
                    .Callback(_verifier.GetCallOrderAction(MethodCall.TextReader_SetFilePath))
                    .Verifiable(Times.Once);
                _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Setup, MethodCall.TextReader_SetFilePath);
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

        [Fact]
        public void WriteGeneratedTextToFile_SuccessfulWriteAndResetGeneratedTextIsFalse_WritesToFile()
        {
            // Arrange
            InitializeMocks();
            string filePath = NextAbsoluteFilePath;
            List<string> generatedText = new()
            {
                "Line 1",
                "Line 2"
            };
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _textWriter
                .Setup(x => x.WriteTextFile(filePath, generatedText))
                .Returns(true)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextWriter_WriteTextFile))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Writing, MethodCall.TextWriter_WriteTextFile);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._generatedText.AddRange(generatedText);

            // Act
            processor.WriteGeneratedTextToFile(filePath, false);

            // Assert
            processor.IsOutputFileWritten
                .Should()
                .BeTrue();
            processor._generatedText
                .Should()
                .BeEquivalentTo(generatedText);
            VerifyMocks();
        }

        [Fact]
        public void WriteGeneratedTextToFile_SuccessfulWriteAndResetGeneratedTextIsTrue_WritesToFileAndResetsGeneratedText()
        {
            // Arrange
            InitializeMocks();
            string directoryPath = NextAbsoluteName;
            string fileName = NextFileName;
            string filePath = $"{directoryPath}{Sep}{fileName}";
            string segmentName = "Segment1";
            ControlItem controlItem = new()
            {
                IsFirstTime = false,
                FirstTimeIndent = 2,
                TabSize = 2,
                PadSegment = "Pad"
            };
            ControlItem expected = new()
            {
                IsFirstTime = true,
                FirstTimeIndent = 2,
                TabSize = 2,
                PadSegment = "Pad"
            };
            List<string> generatedText = new()
            {
                "Line 1",
                "Line 2"
            };
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(x => x.Log(MsgGeneratedTextHasBeenReset, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_Log_Message))
                .Verifiable(Times.Once);
            _locater
                .Setup(x => x.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(x => x.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(x => x.FileName)
                .Returns(fileName)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(x => x.WriteTextFile(filePath, generatedText))
                .Returns(true)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextWriter_WriteTextFile))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Writing, MethodCall.TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(MethodCall.TextWriter_WriteTextFile, MethodCall.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Reset, MethodCall.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCall.Locater_Reset, MethodCall.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCall.IndentProcessor_Reset, MethodCall.Logger_Log_Message);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            processor._generatedText.AddRange(generatedText);
            processor._controlDictionary[segmentName] = controlItem;

            // Act
            processor.WriteGeneratedTextToFile(filePath);

            // Assert
            processor.IsOutputFileWritten
                .Should()
                .BeTrue();
            processor._generatedText
                .Should()
                .BeEmpty();
            processor._controlDictionary[segmentName]
                .Should()
                .Be(expected);
            VerifyMocks();
        }

        [Fact]
        public void WriteGeneratedTextToFile_UnsuccessfulWrite_DoesNothing()
        {
            // Arrange
            InitializeMocks();
            string filePath = NextAbsoluteFilePath;
            List<string> generatedText = new();
            _logger
                .Setup(x => x.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCall.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _textWriter
                .Setup(x => x.WriteTextFile(filePath, generatedText))
                .Returns(false)
                .Callback(_verifier.GetCallOrderAction(MethodCall.TextWriter_WriteTextFile))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCall.Logger_SetLogEntryType_Writing, MethodCall.TextWriter_WriteTextFile);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            processor.WriteGeneratedTextToFile(filePath, false);

            // Assert
            processor.IsOutputFileWritten
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
            _verifier.Reset();
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

            if (_locater.Setups.Any() is false)
            {
                _locater
                    .SetupProperty(x => x.LineNumber);
                _locater
                    .SetupProperty(x => x.CurrentSegment);
            }

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

        private void SetupResetAll()
        {
            _locater
                .Setup(x => x.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.Locater_Reset))
                .Verifiable(Times.Once);
            _defaultSegmentNameGenerator
                .Setup(x => x.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.DefaultSegmentNameGenerator_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(x => x.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCall.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(x => x.ClearTokens())
                .Callback(_verifier.GetCallOrderAction(MethodCall.TokenProcessor_ClearTokens))
                .Verifiable(Times.Once);
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
            _verifier.Verify();
        }
    }
}