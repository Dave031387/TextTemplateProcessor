namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Interfaces;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The <see cref="NameValidater" /> class is used for validating segment names and token names
    /// in a text template file.
    /// </summary>
    internal partial class NameValidater : INameValidater
    {
        private readonly Regex _valid = ValidNameRegex();

        /// <summary>
        /// Default constructor that creates an instance of the <see cref="NameValidater" /> class.
        /// </summary>
        public NameValidater()
        {
        }

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
        public bool IsValidName(string? identifier) => identifier is not null && _valid.IsMatch(identifier);

        [GeneratedRegex("^([A-Z]|[a-z])+([A-Z]|[a-z]|[0-9]|_)*$")]
        private static partial Regex ValidNameRegex();
    }
}