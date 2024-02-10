namespace TextTemplateProcessor
{
    public class DefaultSegmentNameGeneratorTests
    {
        private const string DefaultPrefix = "DefaultSegment";

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void Next_ConsecutiveCalls_GeneratesConsecutiveDefaultNames(int count)
        {
            // Arrange
            DefaultSegmentNameGenerator nameGenerator = new();
            string expected = $"{DefaultPrefix}{count}";

            for (int i = 1; i < count; i++)
            {
                _ = nameGenerator.Next;
            }

            // Act
            string actual = nameGenerator.Next;

            // Assert
            actual
                .Should()
                .Be(expected);
        }

        [Fact]
        public void Reset_AfterCallsToNext_ResetsCounter()
        {
            // Arrange
            DefaultSegmentNameGenerator nameGenerator = new();
            _ = nameGenerator.Next;
            _ = nameGenerator.Next;
            _ = nameGenerator.Next;
            _ = nameGenerator.Next;
            string expected = $"{DefaultPrefix}1";

            // Act
            nameGenerator.Reset();
            string actual = nameGenerator.Next;

            // Assert
            actual
                .Should()
                .Be(expected);
        }
    }
}