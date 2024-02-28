namespace TextTemplateProcessor.Console
{
    public class MessageWriterTests
    {
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
        public void MessageWriter_DefaultConstructor_InitializesAllDependencies()
        {
            // Arrange
            Action action = () => { _ = new MessageWriter(); };

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
            Mock<IConsoleWriter> consoleWriter = new();
            consoleWriter.Setup(x => x.WriteLine(expectedMessage));
            MessageWriter messageWriter = new(consoleWriter.Object);

            // Act
            messageWriter.WriteLine(expectedMessage);

            // Assert
            consoleWriter
                .Verify(x => x.WriteLine(expectedMessage), Times.Once);
        }

        [Fact]
        public void WriteLine_MessageWithOneFormatItem_WritesMessage()
        {
            // Arrange
            string formatString = "This {0} test";
            string formatItem = "is a";
            string expectedMessage = "This is a test";
            Mock<IConsoleWriter> consoleWriter = new();
            consoleWriter.Setup(x => x.WriteLine(expectedMessage));
            MessageWriter messageWriter = new(consoleWriter.Object);

            // Act
            messageWriter.WriteLine(formatString, formatItem);

            // Assert
            consoleWriter
                .Verify(x => x.WriteLine(expectedMessage), Times.Once);
        }

        [Fact]
        public void WriteLine_MessageWithTwoFormatItems_WritesMessage()
        {
            // Arrange
            string formatString = "This {0} {1}";
            string formatItem1 = "is a";
            string formatItem2 = "test";
            string expectedMessage = "This is a test";
            Mock<IConsoleWriter> consoleWriter = new();
            consoleWriter.Setup(x => x.WriteLine(expectedMessage));
            MessageWriter messageWriter = new(consoleWriter.Object);

            // Act
            messageWriter.WriteLine(formatString, formatItem1, formatItem2);

            // Assert
            consoleWriter
                .Verify(x => x.WriteLine(expectedMessage), Times.Once);
        }
    }
}