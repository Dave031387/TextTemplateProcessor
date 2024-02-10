namespace TextTemplateProcessor.Interfaces
{
    /// <summary>
    /// An interface that defines methods for generating default segment names.
    /// </summary>
    internal interface IDefaultSegmentNameGenerator
    {
        /// <summary>
        /// Gets the next default segment name.
        /// </summary>
        /// <remarks>
        /// The default name is comprised of the string "DefaultSegment" with an auto-incremented
        /// integer appended to the end. <br /> The first default name would be "DefaultSegment1",
        /// the second "DefaultSegment2", and so on.
        /// </remarks>
        string Next { get; }

        /// <summary>
        /// Resets the default segment name counter back to zero.
        /// </summary>
        void Reset();
    }
}