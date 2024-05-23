namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.TestShared;

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

        [Fact]
        public void CurrentIndent_WhenCalled_CallsIndentProcessorCurrentIndent()
        {
            // Arrange
            InitializeMocks();
            int expected = 4;
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.CurrentIndent)
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
                .Setup(locater => locater.CurrentSegment)
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
                .NotBeSameAs(processor._generatedText);
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
            string textLine1 = "Line 1";
            string textLine2 = "Line 2";
            string textLine3 = "Line 3";
            TextItem textItem1 = new(0, true, false, textLine1);
            TextItem textItem2 = new(1, true, false, textLine2);
            TextItem textItem3 = new(-1, true, false, textLine3);
            _locater
                .SetupProperty(locater => locater.CurrentSegment, "OtherSegment");
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(textLine3)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            ControlItem controlItem = new();
            processor._controlDictionary[segmentName] = controlItem;
            processor._segmentDictionary[segmentName] = new()
            {
                textItem1,
                textItem2,
                textItem3
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = true;
            List<string> expected = new()
            {
                textLine1,
                $"    {textLine2}",
                textLine3
            };

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._controlDictionary[segmentName].IsFirstTime
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
            string tokenName = "Token1";
            string tokenValue = "2";
            string textLine1 = "Line 1";
            string textLine2Before = $"Line <#{tokenName}#>";
            string textLine2After = $"Line {tokenValue}";
            string textLine3 = "Line 3";
            TextItem textItem1 = new(0, true, false, textLine1);
            TextItem textItem2 = new(0, true, false, textLine2Before);
            TextItem textItem3 = new(0, true, false, textLine3);
            Dictionary<string, string> tokenValues = new()
            {
                [tokenName] = tokenValue
            };
            _locater
                .SetupProperty(locater => locater.CurrentSegment, "OtherSegment");
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.LoadTokenValues(tokenValues))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_LoadTokenValues))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine2Before))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(textLine2After)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(textLine3)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.TokenProcessor_LoadTokenValues);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_LoadTokenValues, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            ControlItem controlItem = new();
            processor._controlDictionary[segmentName] = controlItem;
            processor._segmentDictionary[segmentName] = new()
            {
                textItem1,
                textItem2,
                textItem3
            };
            processor.IsTemplateLoaded = true;
            List<string> expected = new()
            {
                textLine1,
                textLine2After,
                textLine3
            };

            // Act
            processor.GenerateSegment(segmentName, tokenValues);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._controlDictionary[segmentName].IsFirstTime
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
            string textLine1 = "Line 1";
            string textLine2 = "Line 2";
            string textLine3 = "Line 3";
            string textLine4 = "Line 4";
            TextItem textItem1 = new(0, true, false, textLine1);
            TextItem textItem2 = new(1, true, false, textLine2);
            TextItem textItem3 = new(-1, true, false, textLine3);
            TextItem textItem4 = new(-1, true, false, textLine4);
            _locater
                .SetupProperty(locater => locater.CurrentSegment, "OtherSegment");
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(firstTimeIndent, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(12)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(textLine3)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem4))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 3))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine4))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 4))
                .Returns(textLine4)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 3, 4);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            ControlItem controlItem = new() { FirstTimeIndent = firstTimeIndent };
            processor._controlDictionary[segmentName] = controlItem;
            processor._segmentDictionary[segmentName] = new()
            {
                textItem1,
                textItem2,
                textItem3,
                textItem4
            };
            List<string> expected = new()
            {
                $"        {textLine1}",
                $"            {textLine2}",
                $"        {textLine3}",
                $"    {textLine4}"
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
            processor._controlDictionary[segmentName].IsFirstTime
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
        public void GenerateSegment_SegmentHasMultipleOptionsAndIsFirstTime_GeneratesTextAndHandlesOptionsCorrectly()
        {
            // Arrange
            InitializeMocks();
            string padSegment = "PadSegment";
            string segmentName = "Segment1";
            int firstTimeIndent = 1;
            int tabSize = 2;
            string textLine1 = "Line 1";
            string textLine2 = "Line 2";
            string textLine3 = "Line 3";
            string padLine = "Pad";
            TextItem textItem1 = new(0, true, false, textLine1);
            TextItem textItem2 = new(1, true, false, textLine2);
            TextItem textItem3 = new(-1, true, false, textLine3);
            TextItem padTextItem = new(1, true, false, padLine);
            _locater
                .SetupProperty(locater => locater.CurrentSegment, "OtherSegment");
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(1, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(textLine3)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.IndentProcessor_SetTabSize);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.Logger_SetLogEntryType_Generating);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            ControlItem controlItem1 = new();
            ControlItem controlItem2 = new()
            {
                FirstTimeIndent = firstTimeIndent,
                PadSegment = padSegment,
                TabSize = tabSize
            };
            processor._controlDictionary[padSegment] = controlItem1;
            processor._controlDictionary[segmentName] = controlItem2;
            processor._segmentDictionary[padSegment] = new()
            {
                padTextItem
            };
            processor._segmentDictionary[segmentName] = new()
            {
                textItem1,
                textItem2,
                textItem3
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;
            List<string> expected = new()
            {
                $"  {textLine1}",
                $"    {textLine2}",
                $"  {textLine3}"
            };

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._controlDictionary[segmentName].IsFirstTime
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
        public void GenerateSegment_SegmentHasMultipleOptionsAndIsNotFirstTime_GeneratesTextAndHandlesOptionsCorrectly()
        {
            // Arrange
            InitializeMocks();
            string padSegment = "PadSegment";
            string segmentName = "Segment1";
            int firstTimeIndent = 1;
            int tabSize = 2;
            string textLine1 = "Line 1";
            string textLine2 = "Line 2";
            string textLine3 = "Line 3";
            string padLine = "Pad";
            TextItem textItem1 = new(0, true, false, textLine1);
            TextItem textItem2 = new(1, true, false, textLine2);
            TextItem textItem3 = new(-1, true, false, textLine3);
            TextItem padTextItem = new(1, true, false, padLine);
            _locater
                .SetupProperty(locater => locater.CurrentSegment, "OtherSegment");
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(3));
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SaveCurrentState())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SaveCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, padSegment, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, padTextItem))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(padLine))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(padLine)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.RestoreCurrentState())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_RestoreCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 3))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 4))
                .Returns(textLine3)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.IndentProcessor_SaveCurrentState);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SaveCurrentState, MethodCallID.Logger_SetLogEntryType_Generating);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SaveCurrentState, MethodCallID.Logger_Log_Message, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_RestoreCurrentState, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_RestoreCurrentState, MethodCallID.Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_SetLogEntryType_Setup, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.IndentProcessor_SetTabSize);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.Logger_SetLogEntryType_Generating);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.IndentProcessor_GetIndent, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 3, 4);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            ControlItem controlItem1 = new();
            ControlItem controlItem2 = new()
            {
                FirstTimeIndent = firstTimeIndent,
                PadSegment = padSegment,
                TabSize = tabSize,
                IsFirstTime = false
            };
            processor._controlDictionary[padSegment] = controlItem1;
            processor._controlDictionary[segmentName] = controlItem2;
            processor._segmentDictionary[padSegment] = new()
            {
                padTextItem
            };
            processor._segmentDictionary[segmentName] = new()
            {
                textItem1,
                textItem2,
                textItem3
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;
            List<string> expected = new()
            {
                $"    {padLine}",
                textLine1,
                $"  {textLine2}",
                textLine3
            };

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._controlDictionary[segmentName].IsFirstTime
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
                .SetupProperty(locater => locater.CurrentSegment);
            _locater
                .SetupProperty(locater => locater.LineNumber);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgSegmentHasNoTextLines, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
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
        public void GenerateSegment_SegmentHasPadOptionAndIsFirstTime_GeneratesTextWithoutPad()
        {
            // Arrange
            InitializeMocks();
            string padSegment = "PadSegment";
            string segmentName = "Segment1";
            string textLine1 = "Line 1";
            string textLine2 = "Line 2";
            string textLine3 = "Line 3";
            string padLine = "Padding";
            TextItem textItem1 = new(1, true, false, textLine1);
            TextItem textItem2 = new(1, true, false, textLine2);
            TextItem textItem3 = new(-2, true, false, textLine3);
            TextItem padTextItem = new(2, true, false, padLine);
            _locater
                .SetupProperty(locater => locater.CurrentSegment, "OtherSegment");
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(textLine3)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            ControlItem controlItem1 = new();
            ControlItem controlItem2 = new() { PadSegment = padSegment };
            processor._controlDictionary[padSegment] = controlItem1;
            processor._controlDictionary[segmentName] = controlItem2;
            processor._segmentDictionary[padSegment] = new()
            {
                padTextItem
            };
            processor._segmentDictionary[segmentName] = new()
            {
                textItem1,
                textItem2,
                textItem3
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;
            List<string> expected = new()
            {
                $"    {textLine1}",
                $"        {textLine2}",
                textLine3
            };

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._controlDictionary[segmentName].IsFirstTime
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
        public void GenerateSegment_SegmentHasPadOptionAndNotFirstTime_GeneratesTextWithCorrectPad()
        {
            // Arrange
            InitializeMocks();
            string padSegment = "PadSegment";
            string segmentName = "Segment1";
            string textLine1 = "Line 1";
            string textLine2 = "Line 2";
            string textLine3 = "Line 3";
            string padLine = "Padding";
            TextItem textItem1 = new(1, true, false, textLine1);
            TextItem textItem2 = new(1, true, false, textLine2);
            TextItem textItem3 = new(-2, true, false, textLine3);
            TextItem padTextItem = new(2, true, false, padLine);
            _locater
                .SetupProperty(locater => locater.CurrentSegment, "OtherSegment");
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(2));
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SaveCurrentState())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SaveCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, padSegment, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, padTextItem))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(padLine))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(padLine)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.RestoreCurrentState())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_RestoreCurrentState))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 3))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 4))
                .Returns(textLine3)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.IndentProcessor_SaveCurrentState);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SaveCurrentState, MethodCallID.Logger_Log_Message, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_RestoreCurrentState, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_RestoreCurrentState, MethodCallID.Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetIndent, 2, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 3, 4);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            ControlItem controlItem1 = new();
            ControlItem controlItem2 = new() { PadSegment = padSegment, IsFirstTime = false };
            processor._controlDictionary[padSegment] = controlItem1;
            processor._controlDictionary[segmentName] = controlItem2;
            processor._segmentDictionary[padSegment] = new()
            {
                padTextItem
            };
            processor._segmentDictionary[segmentName] = new()
            {
                textItem1,
                textItem2,
                textItem3
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;
            List<string> expected = new()
            {
                $"        {padLine}",
                $"    {textLine1}",
                $"        {textLine2}",
                textLine3
            };

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._controlDictionary[segmentName].IsFirstTime
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
            string textLine1 = "Line 1";
            string textLine2 = "Line 2";
            string textLine3 = "Line 3";
            TextItem textItem1 = new(1, true, false, textLine1);
            TextItem textItem2 = new(1, true, false, textLine2);
            TextItem textItem3 = new(-2, true, false, textLine3);
            int tabSize = 2;
            _locater
                .SetupProperty(locater => locater.CurrentSegment, "OtherSegment");
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Exactly(2));
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(2)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(0)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(textLine3)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.Logger_SetLogEntryType_Setup);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.IndentProcessor_SetTabSize);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.Logger_SetLogEntryType_Generating);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_SetTabSize, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            ControlItem controlItem = new() { TabSize = tabSize };
            processor._controlDictionary[segmentName] = controlItem;
            processor._segmentDictionary[segmentName] = new()
            {
                textItem1,
                textItem2,
                textItem3
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;
            List<string> expected = new()
            {
                $"  {textLine1}",
                $"    {textLine2}",
                textLine3
            };

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._controlDictionary[segmentName].IsFirstTime
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
            string textLine1 = "Line 1";
            string textLine2 = "Line 2";
            string textLine3 = "Line 3";
            string textLine4 = "Line 4";
            TextItem textItem1 = new(2, true, false, textLine1);
            TextItem textItem2 = new(0, true, false, textLine2);
            TextItem textItem3 = new(-1, true, false, textLine3);
            TextItem textItem4 = new(2, true, false, textLine4);
            _locater
                .SetupProperty(locater => locater.CurrentSegment, "OtherSegment");
            _locater
                .SetupProperty(locater => locater.LineNumber, 5);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgProcessingSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetFirstTimeIndent(0, textItem1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetFirstTimeIndent))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine1))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 1))
                .Returns(textLine1)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 1))
                .Returns(8)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine2))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 2))
                .Returns(textLine2)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 2))
                .Returns(4)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine3))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 3))
                .Returns(textLine3)
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.GetIndent(textItem4))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_GetIndent, 3))
                .Returns(12)
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ReplaceTokens(textLine4))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ReplaceTokens, 4))
                .Returns(textLine4)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.IndentProcessor_GetFirstTimeIndent);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetFirstTimeIndent, MethodCallID.TokenProcessor_ReplaceTokens, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 1, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 1, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 2, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 2, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ReplaceTokens, MethodCallID.IndentProcessor_GetIndent, 3, 3);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_GetIndent, MethodCallID.TokenProcessor_ReplaceTokens, 3, 4);
            TextTemplateProcessor processor = GetTextTemplateProcessor();
            ControlItem controlItem = new();
            processor._controlDictionary[segmentName] = controlItem;
            processor._segmentDictionary[segmentName] = new()
            {
                textItem1,
                textItem2,
                textItem3,
                textItem4
            };
            processor.IsTemplateLoaded = true;
            processor.IsOutputFileWritten = false;
            List<string> expected = new()
            {
                $"        {textLine1}",
                $"        {textLine2}",
                $"    {textLine3}",
                $"            {textLine4}"
            };

            // Act
            processor.GenerateSegment(segmentName);

            // Assert
            processor.GeneratedText
                .Should()
                .ContainInConsecutiveOrder(expected);
            processor.IsOutputFileWritten
                .Should()
                .BeFalse();
            processor._controlDictionary[segmentName].IsFirstTime
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

        [Theory]
        [InlineData(Whitespace)]
        [InlineData(null)]
        public void GenerateSegment_SegmentNameIsNullOrWhitespace_LogsMessage(string? segmentName)
        {
            // Arrange
            InitializeMocks();
            _locater
                .SetupProperty(locater => locater.LineNumber);
            _locater
                .SetupProperty(locater => locater.CurrentSegment);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgSegmentNameIsNullOrWhitespace, null, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            processor.GenerateSegment(segmentName!);

            // Assert
            processor.GeneratedText
                .Should()
                .BeEmpty();
            _locater.Object.CurrentSegment
                .Should()
                .BeEmpty();
            _locater.Object.LineNumber
                .Should()
                .Be(0);
            VerifyMocks();
        }

        [Fact]
        public void GenerateSegment_TemplateFileNotLoaded_LogsMessage()
        {
            // Arrange
            InitializeMocks();
            string segmentName = "Segment1";
            _locater
                .SetupProperty(locater => locater.LineNumber);
            _locater
                .SetupProperty(locater => locater.CurrentSegment);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgAttemptToGenerateSegmentBeforeItWasLoaded, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

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
                .SetupProperty(locater => locater.LineNumber);
            _locater
                .SetupProperty(locater => locater.CurrentSegment);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Generating))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Generating))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnknownSegmentName, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Generating, MethodCallID.Logger_Log_Message);
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
                .Setup(locater => locater.LineNumber)
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
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(string.Empty)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath))
                .Returns(string.Empty)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_SetFilePath);
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
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName,
                                                       0,
                                                       () => returnedFileName = fileName,
                                                       () => returnedFileName = lastFileName))
                .Returns(() => returnedFileName)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath,
                                                       0,
                                                       () => returnedFilePath = filePath,
                                                       () => returnedFilePath = lastFilePath))
                .Returns(() => returnedFilePath)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_ReadTextFile))
                .Returns(templateLines)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgNextLoadRequestBeforeFirstIsWritten, fileName, lastFileName))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 1))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgLoadingTemplateFile, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message, 2))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(templateLoader => templateLoader.LoadTemplate(templateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_SetFilePath, MethodCallID.Logger_Log_Message, 0, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.TextReader_ReadTextFile, 1);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.DefaultSegmentNameGenerator_Reset, MethodCallID.Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ClearTokens, MethodCallID.Logger_Log_Message, 0, 2);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.TemplateLoader_LoadTemplate, 2);
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
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName,
                                                       0,
                                                       () => returnedFileName = fileName,
                                                       () => returnedFileName = lastFileName))
                .Returns(() => returnedFileName)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath,
                                                       0,
                                                       () => returnedFilePath = filePath,
                                                       () => returnedFilePath = lastFilePath))
                .Returns(() => returnedFilePath)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_ReadTextFile))
                .Returns(templateLines)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgLoadingTemplateFile, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(templateLoader => templateLoader.LoadTemplate(templateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_SetFilePath, MethodCallID.TextReader_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.DefaultSegmentNameGenerator_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ClearTokens, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.TemplateLoader_LoadTemplate);
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
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(fileName)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath))
                .Returns(filePath)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgAttemptToLoadMoreThanOnce, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_SetFilePath, MethodCallID.Logger_Log_Message);
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
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName,
                                                       0,
                                                       () => returnedFileName = fileName,
                                                       () => returnedFileName = string.Empty))
                .Returns(() => returnedFileName)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath,
                                                       0,
                                                       () => returnedFilePath = filePath,
                                                       () => returnedFilePath = string.Empty))
                .Returns(() => returnedFilePath)
                .Verifiable(Times.AtLeast(2));
            _textReader
                .Setup(textReader => textReader.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_ReadTextFile))
                .Returns(Array.Empty<string>())
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTemplateFileIsEmpty, filePath, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_SetFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_SetFilePath, MethodCallID.TextReader_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.DefaultSegmentNameGenerator_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ClearTokens, MethodCallID.Logger_Log_Message);
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
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath))
                .Returns(string.Empty)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTemplateFilePathNotSet, null, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.Logger_Log_Message);
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
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath))
                .Returns(filePath)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_ReadTextFile))
                .Returns(templateLines)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTemplateFileIsEmpty, filePath, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.DefaultSegmentNameGenerator_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ClearTokens, MethodCallID.Logger_Log_Message);
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
                .Setup(textReader => textReader.FullFilePath)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFullFilePath))
                .Returns(filePath)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(fileName)
                .Verifiable(Times.AtLeastOnce);
            _textReader
                .Setup(textReader => textReader.ReadTextFile())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_ReadTextFile))
                .Returns(templateLines)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgLoadingTemplateFile, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _templateLoader
                .Setup(templateLoader => templateLoader.LoadTemplate(templateLines, It.IsAny<Dictionary<string, List<TextItem>>>(), It.IsAny<Dictionary<string, ControlItem>>()))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TemplateLoader_LoadTemplate))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFullFilePath);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFullFilePath, MethodCallID.TextReader_ReadTextFile);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.DefaultSegmentNameGenerator_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_ReadTextFile, MethodCallID.TokenProcessor_ClearTokens);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.DefaultSegmentNameGenerator_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ClearTokens, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_Log_Message, MethodCallID.TemplateLoader_LoadTemplate);
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
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(fileName)
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Loading))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Loading))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgAttemptToLoadMoreThanOnce, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Loading, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.Logger_Log_Message);
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
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgTemplateHasBeenReset, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(fileName)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.DefaultSegmentNameGenerator_Reset, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.TokenProcessor_ClearTokens, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.Logger_Log_Message);
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
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.IndentProcessor_Reset);
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
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextHasBeenReset, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_GetFileName))
                .Returns(fileName)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.TextReader_GetFileName);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextReader_GetFileName, MethodCallID.Logger_Log_Message);
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
                .SetupSet(locater => locater.CurrentSegment = segmentName)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.LineNumber = 0)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_LineNumber_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgUnableToResetSegment, segmentName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_LineNumber_Setter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Setter, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_LineNumber_Setter, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
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
                .SetupSet(locater => locater.CurrentSegment = segmentName1)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Setter))
                .Verifiable(Times.Once);
            _locater
                .SetupSet(locater => locater.LineNumber = 0)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_LineNumber_Setter))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.CurrentSegment)
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_CurrentSegment_Getter))
                .Returns(segmentName1)
                .Verifiable(Times.AtLeastOnce);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgSegmentHasBeenReset, segmentName1, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_CurrentSegment_Setter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_LineNumber_Setter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Setter, MethodCallID.Locater_CurrentSegment_Getter);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_LineNumber_Setter, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_CurrentSegment_Getter, MethodCallID.Logger_Log_Message);
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
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.SetTabSize(tabSize))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_SetTabSize))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.IndentProcessor_SetTabSize);
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
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.SetFilePath(filePath))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.TextReader_SetFilePath);
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
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.SetTokenDelimiters(tokenStart, tokenEnd, escapeChar))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_SetTokenDelimiters))
                .Returns(expected)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.TokenProcessor_SetTokenDelimiters);
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
                .Setup(indentProcessor => indentProcessor.TabSize)
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
                .Setup(textReader => textReader.FullFilePath)
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
                    .Setup(logger => logger.SetLogEntryType(LogEntryType.Setup))
                    .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Setup))
                    .Verifiable(Times.Once);
                _textReader
                    .Setup(textReader => textReader.SetFilePath(_templateFilePath))
                    .Callback(_verifier.GetCallOrderAction(MethodCallID.TextReader_SetFilePath))
                    .Verifiable(Times.Once);
                _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Setup, MethodCallID.TextReader_SetFilePath);
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
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(filePath, generatedText))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextWriter_WriteTextFile))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Writing, MethodCallID.TextWriter_WriteTextFile);
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
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Reset))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Reset))
                .Verifiable(Times.Once);
            _logger
                .Setup(logger => logger.Log(MsgGeneratedTextHasBeenReset, fileName, null))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_Log_Message))
                .Verifiable(Times.Once);
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _textReader
                .Setup(textReader => textReader.FileName)
                .Returns(fileName)
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(filePath, generatedText))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextWriter_WriteTextFile))
                .Returns(true)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Writing, MethodCallID.TextWriter_WriteTextFile);
            _verifier.DefineExpectedCallOrder(MethodCallID.TextWriter_WriteTextFile, MethodCallID.Logger_SetLogEntryType_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.Locater_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Reset, MethodCallID.IndentProcessor_Reset);
            _verifier.DefineExpectedCallOrder(MethodCallID.Locater_Reset, MethodCallID.Logger_Log_Message);
            _verifier.DefineExpectedCallOrder(MethodCallID.IndentProcessor_Reset, MethodCallID.Logger_Log_Message);
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
                .Setup(logger => logger.SetLogEntryType(LogEntryType.Writing))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Logger_SetLogEntryType_Writing))
                .Verifiable(Times.Once);
            _textWriter
                .Setup(textWriter => textWriter.WriteTextFile(filePath, generatedText))
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TextWriter_WriteTextFile))
                .Returns(false)
                .Verifiable(Times.Once);
            _verifier.DefineExpectedCallOrder(MethodCallID.Logger_SetLogEntryType_Writing, MethodCallID.TextWriter_WriteTextFile);
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

        private void SetupResetAll()
        {
            _locater
                .Setup(locater => locater.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.Locater_Reset))
                .Verifiable(Times.Once);
            _defaultSegmentNameGenerator
                .Setup(defaultSegmentNameGenerator => defaultSegmentNameGenerator.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.DefaultSegmentNameGenerator_Reset))
                .Verifiable(Times.Once);
            _indentProcessor
                .Setup(indentProcessor => indentProcessor.Reset())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.IndentProcessor_Reset))
                .Verifiable(Times.Once);
            _tokenProcessor
                .Setup(tokenProcessor => tokenProcessor.ClearTokens())
                .Callback(_verifier.GetCallOrderAction(MethodCallID.TokenProcessor_ClearTokens))
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