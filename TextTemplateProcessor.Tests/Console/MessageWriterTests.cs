namespace TextTemplateProcessor.Console
{
    public class MessageWriterTests
    {
        private readonly Mock<IConsoleWriter> _consoleWriter = new();
        private readonly MockHelper _mh = new();

        [Fact]
        public void MessageWriter_ConstructUsingNullConsoleWriterObject_ThrowsException()
        {
            // Arrange
            Action action = () => { _ = new MessageWriter(null!); };
            string expected = GetNullDependencyMessage(ClassNames.MessageWriterClass,
                                                       ServiceNames.ConsoleWriterService,
                                                       ServiceParameterNames.ConsoleWriterParameter);

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void MessageWriter_ConstructUsingValidDependency_ShouldNotThrow()
        {
            // Arrange
            Action action = () => GetMessageWriter();

            // Act/Assert
            action
                .Should()
                .NotThrow();
        }

        [Fact]
        public void WriteLine_MessageWithNoFormatItems_WritesMessage()
        {
            // Arrange
            string expectedMessage = "This is a test";
            _mh.SetupConsoleWriter(_consoleWriter, expectedMessage);
            MessageWriter messageWriter = GetMessageWriter();

            // Act
            messageWriter.WriteLine(expectedMessage);

            // Assert
            _consoleWriter.Verify(_mh.ConsoleWriterExpression, Times.Once);
        }

        [Fact]
        public void WriteLine_MessageWithOneFormatItem_WritesMessage()
        {
            // Arrange
            string formatString = "This {0} test";
            string formatItem = "is a";
            string expectedMessage = "This is a test";
            _mh.SetupConsoleWriter(_consoleWriter, expectedMessage);
            MessageWriter messageWriter = new(_consoleWriter.Object);

            // Act
            messageWriter.WriteLine(formatString, formatItem);

            // Assert
            _consoleWriter.Verify(_mh.ConsoleWriterExpression, Times.Once);
        }

        [Fact]
        public void WriteLine_MessageWithTwoFormatItems_WritesMessage()
        {
            // Arrange
            string formatString = "This {0} {1}";
            string formatItem1 = "is a";
            string formatItem2 = "test";
            string expectedMessage = "This is a test";
            _mh.SetupConsoleWriter(_consoleWriter, expectedMessage);
            MessageWriter messageWriter = new(_consoleWriter.Object);

            // Act
            messageWriter.WriteLine(formatString, formatItem1, formatItem2);

            // Assert
            _consoleWriter.Verify(_mh.ConsoleWriterExpression, Times.Once);
        }

        private MessageWriter GetMessageWriter() => new(_consoleWriter.Object);
    }
}