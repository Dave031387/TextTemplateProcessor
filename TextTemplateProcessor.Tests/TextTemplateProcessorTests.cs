namespace TextTemplateProcessor
{
    using System.Linq.Expressions;

    public class TextTemplateProcessorTests
    {
        private static readonly string _templateDirectoryPath = $"{VolumeRoot}{Sep}test";
        private static readonly string _templateFileName = "template.txt";
        private static readonly string _templateFilePath = $"{_templateDirectoryPath}{Sep}{_templateFileName}";
        private readonly Expression<Func<IIndentProcessor, int>> _currentIndentExpression = x => x.CurrentIndent;
        private readonly Mock<IDefaultSegmentNameGenerator> _defaultSegmentNameGenerator = new();
        private readonly Mock<IIndentProcessor> _indentProcessor = new();
        private readonly Mock<ILocater> _locater = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly MockHelper _mh = new();
        private readonly Mock<ITemplateLoader> _templateLoader = new();
        private readonly Mock<ITextReader> _textReader = new();
        private readonly Mock<ITextWriter> _textWriter = new();
        private readonly Mock<ITokenProcessor> _tokenProcessor = new();

        [Fact]
        public void CurrentIndent_WhenCalled_CallsIndentProcessorCurrentIndent()
        {
            // Arrange
            InitializeMocks();
            int expected = 4;
            _indentProcessor.Setup(_currentIndentExpression).Returns(expected);
            TextTemplateProcessor processor = GetTextTemplateProcessor();

            // Act
            int actual = processor.CurrentIndent;

            // Assert
            actual
                .Should()
                .Be(expected);
            _indentProcessor.Verify(_currentIndentExpression, Times.Once);
            MocksVerifyNoOtherCalls();
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
            if (useTemplateFilePath)
            {
                action = () =>
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
                };
            }
            else
            {
                action = () =>
                {
                    processor = new(_logger.Object,
                                    null!,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                };
            }

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
            if (useTemplateFilePath)
            {
                action = () =>
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
                };
            }
            else
            {
                action = () =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    null!,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                };
            }

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
            if (useTemplateFilePath)
            {
                action = () =>
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
                };
            }
            else
            {
                action = () =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    null!,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                };
            }

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
            if (useTemplateFilePath)
            {
                action = () =>
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
                };
            }
            else
            {
                action = () =>
                {
                    processor = new(null!,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                };
            }

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
            if (useTemplateFilePath)
            {
                action = () =>
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
                };
            }
            else
            {
                action = () =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    null!,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                };
            }

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
            if (useTemplateFilePath)
            {
                action = () =>
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
                };
            }
            else
            {
                action = () =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    null!,
                                    _textWriter.Object,
                                    _tokenProcessor.Object);
                };
            }

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
            if (useTemplateFilePath)
            {
                action = () =>
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
                };
            }
            else
            {
                action = () =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    null!,
                                    _tokenProcessor.Object);
                };
            }

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
            if (useTemplateFilePath)
            {
                action = () =>
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
                };
            }
            else
            {
                action = () =>
                {
                    processor = new(_logger.Object,
                                    _defaultSegmentNameGenerator.Object,
                                    _indentProcessor.Object,
                                    _locater.Object,
                                    _templateLoader.Object,
                                    _textReader.Object,
                                    _textWriter.Object,
                                    null!);
                };
            }

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TextTemplateProcessor_ConstructUsingValidDependencies_ThrowsException(bool useTemplateFilePath)
        {
            // Arrange
            InitializeMocks();

            if (useTemplateFilePath)
            {
                _mh.SetupLogger(_logger, LogEntryType.Setup);
                _mh.SetupTextReader(_textReader, _templateDirectoryPath, _templateFileName);
            }

            TextTemplateProcessor processor;
            Action action = useTemplateFilePath
                ? (() => processor = GetTextTemplateProcessor(_templateFilePath))
                : (() => processor = GetTextTemplateProcessor());

            // Act/Assert
            action
                .Should()
                .NotThrow();

            if (useTemplateFilePath)
            {
                _logger.Verify(_mh.SetLogEntryTypeExpression, Times.Once());
                _textReader.Verify(_mh.SetFilePathAction, Times.Once());
            }

            MocksVerifyNoOtherCalls();
        }

        private TextTemplateProcessor GetTextTemplateProcessor(string? templateFilePath = null)
        {
            if (templateFilePath is null)
            {
                return new TextTemplateProcessor(_logger.Object,
                                                 _defaultSegmentNameGenerator.Object,
                                                 _indentProcessor.Object,
                                                 _locater.Object,
                                                 _templateLoader.Object,
                                                 _textReader.Object,
                                                 _textWriter.Object,
                                                 _tokenProcessor.Object);
            }
            else
            {
                return new TextTemplateProcessor(templateFilePath,
                                                 _logger.Object,
                                                 _defaultSegmentNameGenerator.Object,
                                                 _indentProcessor.Object,
                                                 _locater.Object,
                                                 _templateLoader.Object,
                                                 _textReader.Object,
                                                 _textWriter.Object,
                                                 _tokenProcessor.Object);
            }
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
    }
}