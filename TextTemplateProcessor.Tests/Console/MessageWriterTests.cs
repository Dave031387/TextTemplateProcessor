namespace TextTemplateProcessor.Console
{
    public class MessageWriterTests
    {
        private readonly Mock<IConsoleWriter> _consoleWriter = new();

        [Fact]
        public void MessageWriter_ConstructUsingNullConsoleWriterObject_ThrowsException()
        {
            // Arrange
            InitializeMocks();
            Action action = () => { _ = new MessageWriter(null!); };
            string expected = GetNullDependencyMessage(ClassNames.MessageWriterClass,
                                                       ServiceNames.ConsoleWriterService,
                                                       ServiceParameterNames.ConsoleWriterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void MessageWriter_ConstructUsingValidDependency_ShouldNotThrow()
        {
            // Arrange
            InitializeMocks();
            Action action = () => GetMessageWriter();

            // Act/Assert
            action
                .Should()
                .NotThrow();
            MocksVerifyNoOtherCalls();
        }

        [Fact]
        public void WriteLine_MessageWithNoFormatItems_WritesMessage()
        {
            // Arrange
            InitializeMocks();
            string expectedMessage = "This is a test";
            _consoleWriter
                .Setup(consoleWriter => consoleWriter.WriteLine(expectedMessage))
                .Verifiable(Times.Once);
            MessageWriter messageWriter = GetMessageWriter();

            // Act
            messageWriter.WriteLine(expectedMessage);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void WriteLine_MessageWithOneFormatItem_WritesMessage()
        {
            // Arrange
            InitializeMocks();
            string formatString = "This {0} test";
            string formatItem = "is a";
            string expectedMessage = "This is a test";
            _consoleWriter
                .Setup(consoleWriter => consoleWriter.WriteLine(expectedMessage))
                .Verifiable(Times.Once);
            MessageWriter messageWriter = new(_consoleWriter.Object);

            // Act
            messageWriter.WriteLine(formatString, formatItem);

            // Assert
            VerifyMocks();
        }

        [Fact]
        public void WriteLine_MessageWithTwoFormatItems_WritesMessage()
        {
            // Arrange
            InitializeMocks();
            string formatString = "This {0} {1}";
            string formatItem1 = "is a";
            string formatItem2 = "test";
            string expectedMessage = "This is a test";
            _consoleWriter
                .Setup(consoleWriter => consoleWriter.WriteLine(expectedMessage))
                .Verifiable(Times.Once);
            MessageWriter messageWriter = new(_consoleWriter.Object);

            // Act
            messageWriter.WriteLine(formatString, formatItem1, formatItem2);

            // Assert
            VerifyMocks();
        }

        private MessageWriter GetMessageWriter() => new(_consoleWriter.Object);

        private void InitializeMocks() => _consoleWriter.Reset();

        private void MocksVerifyNoOtherCalls() => _consoleWriter.VerifyNoOtherCalls();

        private void VerifyMocks()
        {
            if (_consoleWriter.Setups.Any())
            {
                _consoleWriter.Verify();
            }

            MocksVerifyNoOtherCalls();
        }
    }
}