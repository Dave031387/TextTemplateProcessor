namespace TextTemplateProcessor.Core
{
    /// <summary>
    /// The <see cref="TextItem" /> record represents a single line of text from a text template
    /// file. <br /> In addition to the text, the record contains information used for controlling
    /// the indentation of the text line when written to the output file.
    /// </summary>
    /// <param name="Indent">
    /// An integer value representing the number of forward tabs (positive values) or back tabs
    /// (negative values) to be applied to this text line when writing it to the output file.
    /// </param>
    /// <param name="IsRelative">
    /// If <see langword="true" />, the specified <paramref name="Indent" /> value will be relative
    /// to the current indent carried forward from the previous text line. <br /> If
    /// <see langword="false" />, the specified <paramref name="Indent" /> value will be absolute
    /// from the beginning of the line written to the output file.
    /// </param>
    /// <param name="IsOneTime">
    /// If <see langword="true" />, the specified <paramref name="Indent" /> value only applies to
    /// the current text line and the current indent value carried forward from the previous text
    /// line is passed on unchanged to the next text line. <br /> If <see langword="false" />, the
    /// specified <paramref name="Indent" /> value adjusts the current indent value carried forward
    /// from the previous text line before being passed on to the next text line.
    /// </param>
    /// <param name="Text">
    /// This is the text from a single line of the text template file, including any optional
    /// placeholder tokens.
    /// </param>
    internal record TextItem(int Indent, bool IsRelative, bool IsOneTime, string Text);
}