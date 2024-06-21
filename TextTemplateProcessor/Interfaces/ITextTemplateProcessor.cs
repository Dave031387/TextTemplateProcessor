namespace TextTemplateProcessor.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// This is an interface that provides properties and methods for processing text template
    /// files.
    /// </summary>
    public interface ITextTemplateProcessor
    {
        /// <summary>
        /// Gets the current indent value.
        /// </summary>
        /// <remarks>
        /// The indent value is the number of tabs that will get inserted at the beginning of the
        /// next generated text line.
        /// </remarks>
        int CurrentIndent { get; }

        /// <summary>
        /// Gets the name of the current segment that is being processed in the text template file.
        /// </summary>
        string CurrentSegment { get; }

        /// <summary>
        /// Gets the collection of strings that make up the generated text buffer.
        /// </summary>
        /// <remarks>
        /// Note that this method returns a copy of the generated text buffer. Any changes made to
        /// the collection returned from this method will have absolutely no effect on the actual
        /// generated text buffer that is maintained by the <see cref="TextTemplateProcessor" />
        /// class object.
        /// </remarks>
        IEnumerable<string> GeneratedText { get; }

        /// <summary>
        /// Gets a boolean value that is <see langword="true" /> if the generated text buffer has
        /// been written to the output file.
        /// </summary>
        bool IsOutputFileWritten { get; }

        /// <summary>
        /// Gets a boolean value that is <see langword="true" /> if a template file has been loaded
        /// into the text template processor.
        /// </summary>
        bool IsTemplateLoaded { get; }

        /// <summary>
        /// Gets the ordinal number of the current text line that is being processed within the
        /// current segment of the text template file.
        /// </summary>
        int LineNumber { get; }

        /// <summary>
        /// Gets the current tab size.
        /// </summary>
        /// <remarks>
        /// The tab size is the number of space characters that make up one tab.
        /// </remarks>
        int TabSize { get; }

        /// <summary>
        /// Gets the name of the text template file that is loaded into memory.
        /// </summary>
        /// <remarks>
        /// This will return an empty string if a template file hasn't yet been loaded.
        /// </remarks>
        string TemplateFileName { get; }

        /// <summary>
        /// Gets the full file path of the text template file that is loaded in memory.
        /// </summary>
        /// <remarks>
        /// This will return an empty string if a template file hasn't yet been loaded.
        /// </remarks>
        string TemplateFilePath { get; }

        /// <summary>
        /// Generates the text lines for the given segment in the text template file.
        /// </summary>
        /// <param name="segmentName">
        /// The name of the segment to be generated.
        /// </param>
        /// <param name="tokenDictionary">
        /// An optional dictionary of key/value pairs where the key is a token name and the value is
        /// the substitution value for that token.
        /// </param>
        /// <remarks>
        /// Each text line will indented according to the indent controls and all tokens will be
        /// replaced with their respective substitution values.
        /// </remarks>
        void GenerateSegment(string segmentName, Dictionary<string, string>? tokenDictionary = null);

        /// <summary>
        /// Loads a text template file into memory to be processed.
        /// </summary>
        /// <remarks>
        /// The file path of the text template file must have previously been loaded either via the
        /// constructor that takes a file path as an argument, or via the
        /// <see cref="SetTemplateFilePath(string)" /> method.
        /// </remarks>
        void LoadTemplate();

        /// <summary>
        /// Sets the template file path to the given value and then loads the template file into
        /// memory to be processed.
        /// </summary>
        /// <param name="filePath">
        /// The file path of the text template file that is to be processed.
        /// </param>
        void LoadTemplate(string filePath);

        /// <summary>
        /// Resets the text template processor back to its initial state
        /// </summary>
        /// <param name="shouldDisplayMessage">
        /// An optional boolean value indicating whether or not a message should be logged after the
        /// state has been reset.
        /// <para> The default is <see langword="true" /> (message will be logged). </para>
        /// </param>
        void ResetAll(bool shouldDisplayMessage = true);

        /// <summary>
        /// Clears the generated text buffer and resets the locater and indent processor.
        /// </summary>
        /// <param name="shouldDisplayMessage">
        /// An optional boolean value indicating whether or not a message should be logged after the
        /// buffer has been cleared.
        /// <para> The default is <see langword="true" /> (message will be logged). </para>
        /// </param>
        void ResetGeneratedText(bool shouldDisplayMessage = true);

        /// <summary>
        /// Resets the current line number of the given segment to zero and resets the "is first
        /// time" control flag for the segment to <see langword="true" />.
        /// </summary>
        /// <param name="segmentName">
        /// The name of the segment to be reset.
        /// </param>
        /// <remarks>
        /// The <see cref="CurrentSegment" /> property will be set to the given segment name.
        /// </remarks>
        void ResetSegment(string segmentName);

        /// <summary>
        /// Resets the token start and token end delimiters and the token escape character to their
        /// default values.
        /// </summary>
        void ResetTokenDelimiters();

        /// <summary>
        /// Sets the tab size to be used when generating text lines.
        /// </summary>
        /// <param name="tabSize">
        /// The new tab size value.
        /// </param>
        void SetTabSize(int tabSize);

        /// <summary>
        /// Sets the template file path to the given value.
        /// </summary>
        /// <param name="templateFilePath">
        /// The file path of an existing text template file.
        /// </param>
        void SetTemplateFilePath(string templateFilePath);

        /// <summary>
        /// Sets the token start and token end delimiters and the token escape character to the
        /// specified values.
        /// </summary>
        /// <param name="tokenStart">
        /// The new token start delimiter string.
        /// </param>
        /// <param name="tokenEnd">
        /// The new token end delimiter string.
        /// </param>
        /// <param name="tokenEscapeChar">
        /// The new token escape character.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the delimiter values were successfully changed. Otherwise,
        /// returns <see langword="false" />.
        /// </returns>
        bool SetTokenDelimiters(string tokenStart, string tokenEnd, char tokenEscapeChar);

        /// <summary>
        /// Writes the generated text buffer to the given output file path and optionally clears the
        /// buffer.
        /// </summary>
        /// <param name="filePath">
        /// The file path where the generated text buffer is to be written.
        /// </param>
        /// <param name="resetGeneratedText">
        /// An optional boolean value indicating whether or not the generated text buffer should be
        /// cleared after it is written.
        /// <para> The default is <see langword="true" /> (the buffer will be cleared). </para>
        /// </param>
        /// <remarks>
        /// The output file will be created if it doesn't already exist. Otherwise, the existing
        /// file will be overwritten.
        /// </remarks>
        void WriteGeneratedTextToFile(string filePath, bool resetGeneratedText = true);
    }
}