namespace TextTemplateProcessor
{
    public class TokenProcessorTests
    {
        private readonly Mock<ILocater> _locater = new();
        private readonly Mock<ILogger> _logger = new();
        private readonly Mock<INameValidater> _nameValidater = new();

        public TokenProcessorTests()
        {
            _locater.Reset();
            _logger.Reset();
            _nameValidater.Reset();
        }

        [Fact]
        public void TokenProcessor_ConstructWithNullLocater_ThrowsException()
        {
            // Arrange
            Action action = () => { TokenProcessor processor = new(_logger.Object, null!, _nameValidater.Object); };
            string expected = GetNullDependencyMessage(
                nameof(TokenProcessor),
                nameof(ILocater),
                "locater");

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TokenProcessor_ConstructWithNullLogger_ThrowsException()
        {
            // Arrange
            Action action = () => { TokenProcessor processor = new(null!, _locater.Object, _nameValidater.Object); };
            string expected = GetNullDependencyMessage(
                nameof(TokenProcessor),
                nameof(ILogger),
                "logger");

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TokenProcessor_ConstructWithNullNameValidater_ThrowsException()
        {
            // Arrange
            Action action = () => { TokenProcessor processor = new(_logger.Object, _locater.Object, null!); };
            string expected = GetNullDependencyMessage(
                nameof(TokenProcessor),
                nameof(INameValidater),
                "nameValidater");

            // Act/Assert
            action
                .Should()
                .Throw<ArgumentNullException>()
                .WithMessage(expected);
        }

        [Fact]
        public void TokenProcessor_ConstructWithValidDependencies_InitializesEmptyTokenDictionary()
        {
            // Arrange/Act
            TokenProcessor processor = new(_logger.Object, _locater.Object, _nameValidater.Object);

            // Assert
            processor.TokenDictionary
                .Should()
                .NotBeNull();
            processor.TokenDictionary
                .Should()
                .BeEmpty();
            _locater
                .VerifyNoOtherCalls();
            _logger
                .VerifyNoOtherCalls();
            _nameValidater
                .VerifyNoOtherCalls();
        }
    }
}