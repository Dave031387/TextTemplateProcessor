namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Interfaces;

    /// <summary>
    /// The <see cref="DefaultSegmentNameGenerator" /> class is a class used for generating unique
    /// default segment names for segments that either don't specify a name or that have a name that
    /// matches a previous segment in the same text template file.
    /// </summary>
    internal class DefaultSegmentNameGenerator : IDefaultSegmentNameGenerator
    {
        private const string DefaultSegmentNamePrefix = "DefaultSegment";
        private int _defaultSegmentCounter = 0;

        /// <summary>
        /// Default constructor that creates an instance of the
        /// <see cref="DefaultSegmentNameGenerator" /> class.
        /// </summary>
        public DefaultSegmentNameGenerator()
        {
        }

        /// <summary>
        /// Gets the next default segment name.
        /// </summary>
        /// <remarks>
        /// The default name is comprised of the string "DefaultSegment" with an auto-incremented
        /// integer appended to the end. <br /> The first default name would be "DefaultSegment1",
        /// the second "DefaultSegment2", and so on.
        /// </remarks>
        public string Next => $"{DefaultSegmentNamePrefix}{++_defaultSegmentCounter}";

        /// <summary>
        /// Resets the default segment name counter back to zero.
        /// </summary>
        public void Reset() => _defaultSegmentCounter = 0;
    }
}