namespace TextTemplateProcessor.Core
{
    public class LocaterTests
    {
        private const int LineNumber = 10;
        private const string SegmentName = "Segment1";

        [Fact]
        public void Locater_AfterConstruction_SetsPropertiesToInitialValues()
        {
            // Arrange/Act
            Locater locater = new();

            // Assert
            locater.CurrentSegment
                .Should()
                .Be(string.Empty);
            locater.LineNumber
                .Should()
                .Be(0);
        }

        [Fact]
        public void Location_WhenCalled_ReturnsCorrectLocation()
        {
            // Arrange
            Locater locater = new()
            {
                CurrentSegment = SegmentName,
                LineNumber = LineNumber
            };

            // Act
            (string segmentName, int lineNumber) = locater.Location;

            // Assert
            segmentName
                .Should()
                .Be(SegmentName);
            lineNumber
                .Should()
                .Be(LineNumber);
        }

        [Fact]
        public void Reset_WhenCalled_ResetsTheLocation()
        {
            // Arrange
            Locater locater = new()
            {
                CurrentSegment = SegmentName,
                LineNumber = LineNumber
            };

            // Act
            locater.Reset();

            // Assert
            locater.CurrentSegment
                .Should()
                .BeEmpty();
            locater.LineNumber
                .Should()
                .Be(0);
        }

        [Fact]
        public void ToString_WhenCalled_ReturnsLocationString()
        {
            // Arrange
            Locater locater = new()
            {
                CurrentSegment = SegmentName,
                LineNumber = LineNumber
            };
            string expected = $"{SegmentName}[{LineNumber}]";

            // Act
            string actual = locater.ToString();

            // Assert
            actual
                .Should()
                .Be(expected);
        }
    }
}