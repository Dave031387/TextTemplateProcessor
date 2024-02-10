namespace TextTemplateProcessor.Interfaces
{
    /// <summary>
    /// An interface that provides a method for validating segment and token name identifiers in a
    /// text template file.
    /// </summary>
    internal interface INameValidater
    {
        /// <summary>
        /// Validates whether the specified identifier is a valid segment name or token name.
        /// </summary>
        /// <param name="identifier">
        /// A <see langword="string" /> value that is either a segment name or token name.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the specified identifier is a valid segment name or token
        /// name.
        /// </returns>
        bool IsValidName(string? identifier);
    }
}