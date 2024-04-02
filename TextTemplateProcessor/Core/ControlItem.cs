namespace TextTemplateProcessor.Core
{
    /// <summary>
    /// The <see cref="ControlItem" /> class is used to control the formatting or a given segment in
    /// a text template file.
    /// </summary>
    internal class ControlItem
    {
        /// <summary>
        /// Creates a new instance of <see cref="ControlItem" /> with properties initialized to
        /// default values.
        /// </summary>
        internal ControlItem()
        {
            IsFirstTime = true;
            FirstTimeIndent = 0;
            PadSegment = string.Empty;
            TabSize = 0;
        }

        /// <summary>
        /// Gets an integer value indicating how many tab stops the first line of a segment should
        /// be indented the first time the segment is processed.
        /// </summary>
        internal int FirstTimeIndent { get; set; }

        /// <summary>
        /// Gets a boolean value indicating whether this is the first time the given segment is
        /// being processed.
        /// </summary>
        internal bool IsFirstTime { get; set; }

        /// <summary>
        /// Gets the name of the segment that should be inserted ahead of the given segment on the
        /// second and subsequent times the given segment is processed.
        /// </summary>
        /// <remarks>
        /// This property will be an empty string if nothing should be inserted ahead of the given
        /// segment.
        /// </remarks>
        internal string PadSegment { get; set; }

        /// <summary>
        /// Returns a boolean value indicating whether or not the pad segment should be inserted the
        /// next time the given segment is processed.
        /// </summary>
        internal bool ShouldGeneratePadSegment => string.IsNullOrEmpty(PadSegment) is false && IsFirstTime;

        /// <summary>
        /// Gets or sets the tab size for the segment.
        /// </summary>
        internal int TabSize { get; set; }

        /// <summary>
        /// Determines whether or not the specified object is equal the current
        /// <see cref="ControlItem" /> object.
        /// </summary>
        /// <param name="obj">
        /// The object to be compared with the current <see cref="ControlItem" /> object.
        /// </param>
        /// <returns>
        /// Returns <see langword="true" /> if the specified object is a <see cref="ControlItem" />
        /// object and its property values match the property values on the current
        /// <see cref="ControlItem" /> object.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return obj is not null && (obj is ControlItem controlItem
                ? FirstTimeIndent == controlItem.FirstTimeIndent
                    && IsFirstTime == controlItem.IsFirstTime
                    && PadSegment == controlItem.PadSegment
                    && TabSize == controlItem.TabSize
                : base.Equals(obj));
        }

        /// <summary>
        /// Generates a hash code for the current <see cref="ControlItem" /> object.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code value.
        /// </returns>
        public override int GetHashCode()
            => FirstTimeIndent.GetHashCode()
            ^ IsFirstTime.GetHashCode()
            ^ PadSegment.GetHashCode()
            ^ TabSize.GetHashCode();

        /// <summary>
        /// Generates a <see langword="string" /> representation of the current
        /// <see cref="ControlItem" /> object.
        /// </summary>
        /// <returns>
        /// The string representation of the current <see cref="ControlItem" /> object.
        /// </returns>
        public override string ToString()
            => $"Is first time: {IsFirstTime} "
            + $"/ FTI: {FirstTimeIndent} "
            + $"/ PAD: {PadSegment} "
            + $"/ TAB: {TabSize}";
    }
}