namespace TextTemplateProcessor.Interfaces
{
    using global::TextTemplateProcessor.Core;

    /// <summary>
    /// An interface that defines methods and properties for managing line indentation in a text
    /// file.
    /// </summary>
    internal interface IIndentProcessor
    {
        /// <summary>
        /// Gets the current indent value.
        /// </summary>
        /// <remarks>
        /// The value returned is the number of tabs that will be inserted at the beginning the next
        /// generated text line.
        /// </remarks>
        int CurrentIndent { get; }

        /// <summary>
        /// Gets the current tab size.
        /// </summary>
        /// <remarks>
        /// The value returned is the number of spaces in a single tab.
        /// </remarks>
        int TabSize { get; }

        /// <summary>
        /// Determines the first time indent value for the given text item.
        /// </summary>
        /// <param name="firstTimeOffset">
        /// The first time indent offset value.
        /// </param>
        /// <param name="textItem">
        /// The text item for the current text line being processed.
        /// </param>
        /// <returns>
        /// The calculated indent value for the current text line.
        /// </returns>
        /// <remarks>
        /// If the <paramref name="firstTimeOffset" /> value is zero, then this method will return
        /// the indent value as determined by the <see cref="GetIndent(TextItem)" /> method.
        /// </remarks>
        int GetFirstTimeIndent(int firstTimeOffset, TextItem textItem);

        /// <summary>
        /// Determines the correct indent value for the given text item.
        /// </summary>
        /// <param name="textItem">
        /// The text item for the current text line being processed.
        /// </param>
        /// <returns>
        /// The calculated indent value for the current text line.
        /// </returns>
        int GetIndent(TextItem textItem);

        /// <summary>
        /// Determines whether or not the given string value represents a valid integer indent
        /// value.
        /// </summary>
        /// <param name="stringValue">
        /// The string representation of an indent value.
        /// </param>
        /// <param name="indent">
        /// The integer value that gets parsed from the <paramref name="stringValue" /> parameter.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the <paramref name="stringValue" /> was successfully
        /// converted to a valid indent value. Otherwise, returns <see langword="false" />.
        /// </returns>
        /// <remarks>
        /// The <paramref name="indent" /> parameter will be set to 0 if
        /// <paramref name="stringValue" /> can't be converted to a valid indent value.
        /// </remarks>
        bool IsValidIndentValue(string stringValue, out int indent);

        /// <summary>
        /// Determines whether or not the given string value represents a valid integer tab value.
        /// </summary>
        /// <param name="stringValue">
        /// The string representation of the tab value.
        /// </param>
        /// <param name="tabSize">
        /// The integer value that gets parsed from the <paramref name="stringValue" /> parameter.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the <paramref name="stringValue" /> parameter was
        /// successfully converted to a valid tab size value.
        /// </returns>
        /// <remarks>
        /// The <paramref name="tabSize" /> will be set to 0 if <paramref name="stringValue" />
        /// can't be converted to a valid tab size value.
        /// </remarks>
        bool IsValidTabSizeValue(string stringValue, out int tabSize);

        /// <summary>
        /// Resets the current indent value to zero and sets the tab size to the default value.
        /// </summary>
        void Reset();

        /// <summary>
        /// Restores the current indent and tab size from the saved values.
        /// </summary>
        /// <remarks>
        /// This method exits without doing anything if the current indent and tab size wasn't
        /// previously saved.
        /// </remarks>
        void RestoreCurrentState();

        /// <summary>
        /// Save the current indent and tab size so that they can be restored later.
        /// </summary>
        void SaveCurrentState();

        /// <summary>
        /// Set the tab size to the specified value.
        /// </summary>
        /// <param name="tabSize">
        /// The new value for the tab size.
        /// </param>
        /// <remarks>
        /// The tab size will be constrained to fall within the minimum and maximum values defined
        /// in the <see cref="IndentProcessor" /> class definition.
        /// </remarks>
        void SetTabSize(int tabSize);
    }
}