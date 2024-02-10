namespace TextTemplateProcessor
{
    public class NameValidaterTests
    {
        [Theory]
        [InlineData("123ABC", "Name must not begin with a numeric")]
        [InlineData("_Nope", "Name must not begin with an underscore")]
        [InlineData("A,bc", "Name must not contain commas")]
        [InlineData("A-bc", "Name must not contain hyphens")]
        [InlineData("Abc:", "Name must not contain colons")]
        [InlineData("A(BC)", "Name must not contain parenthesis")]
        [InlineData("Ab=c", "Name must not contain an equals sign")]
        [InlineData("ABC?", "Name must not contain a question mark")]
        [InlineData("A@bc", "Name must not contain an at sign")]
        [InlineData("ab*c", "Name must not contain an asterisk")]
        [InlineData("ab$c", "Name must not contain a dollar sign")]
        [InlineData("a&bc", "Name must not contain an ampersand")]
        [InlineData("a.bc", "Name must not contain a period")]
        public void IsValidName_NameIsNotValid_ReturnsFalse(string name, string reason)
        {
            // Arrange
            NameValidater nameValidater = new();

            // Act
            bool isValid = nameValidater.IsValidName(name);

            // Assert
            isValid
                .Should()
                .BeFalse(reason);
        }

        [Fact]
        public void IsValidName_NameIsNull_ReturnsFalse()
        {
            // Arrange
            NameValidater nameValidater = new();

            // Act
            bool isValid = nameValidater.IsValidName(null);

            // Assert
            isValid
                .Should()
                .BeFalse();
        }

        [Theory]
        [InlineData("A")]
        [InlineData("z")]
        [InlineData("Z_")]
        [InlineData("a1")]
        [InlineData("Good_Bye")]
        [InlineData("F150")]
        [InlineData("B_36CR")]
        [InlineData("M__1x")]
        [InlineData("A_b_c_")]
        public void IsValidName_NameIsValid_ReturnsTrue(string name)
        {
            // Arrange
            NameValidater nameValidater = new();

            // Act
            bool isValid = nameValidater.IsValidName(name);

            // Assert
            isValid
                .Should()
                .BeTrue();
        }
    }
}