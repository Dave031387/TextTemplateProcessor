namespace TextTemplateProcessor.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// An interface that provides methods for parsing tokens in text files and substituting values
    /// for each token.
    /// </summary>
    internal interface ITokenProcessor
    {
        /// <summary>
        /// Clears all tokens from the token dictionary.
        /// </summary>
        void ClearTokens();

        /// <summary>
        /// Searches for valid tokens in the given line of text and adds any tokens found to the
        /// token dictionary.
        /// </summary>
        /// <param name="text">
        /// A line of text possibly containing one or more tokens.
        /// </param>
        /// <remarks>
        /// If the <paramref name="text" /> parameter contains any invalid tokens, the text will be
        /// modified to insert a token escape character ahead of the token start delimiter of each
        /// invalid token.
        /// </remarks>
        void ExtractTokens(ref string text);

        /// <summary>
        /// This method is used to load token substitution values into the Token Dictionary for the
        /// given token names.
        /// </summary>
        /// <param name="tokenValues">
        /// A dictionary of key/value pairs where the key is the token name and the value is the
        /// substitution value to be assigned to that token.
        /// </param>
        /// <remarks>
        /// The token names in the <paramref name="tokenValues" /> dictionary passed into this
        /// method must already exist in the Token Dictionary. Any token names not found will be
        /// ignored.
        /// </remarks>
        void LoadTokenValues(Dictionary<string, string> tokenValues);

        /// <summary>
        /// Replace tokens in the given text line with their corresponding substitution values.
        /// </summary>
        /// <param name="text">
        /// A text string that may contain one or more tokens.
        /// </param>
        /// <returns>
        /// The original <paramref name="text" /> string with all tokens replaced by their
        /// substitution values.
        /// </returns>
        /// <remarks>
        /// The token escape character will be removed from all escaped tokens in the
        /// <paramref name="text" /> string and those tokens will be output without any
        /// substitution.
        /// </remarks>
        string ReplaceTokens(string text);

        /// <summary>
        /// Resets all token substitution values in the Token Dictionary to empty strings.
        /// </summary>
        void ResetTokens();

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
    }
}