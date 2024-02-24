﻿namespace TextTemplateProcessor
{
    using global::TextTemplateProcessor.Core;
    using global::TextTemplateProcessor.Interfaces;
    using static global::TextTemplateProcessor.Core.Messages;

    /// <summary>
    /// The <see cref="IndentProcessor" /> class is used for managing the line indentation for text
    /// lines that are generated by the Text Template Processor.
    /// </summary>
    internal class IndentProcessor : IIndentProcessor
    {
        private const int DefaultTabSize = 4;
        private const int MaxIndentValue = 9;
        private const int MaxTabSize = 9;
        private const int MinIndentValue = -9;
        private const int MinTabSize = 1;
        private readonly ILocater _locater;
        private readonly ILogger _logger;
        private bool _isCurrentIndentSaved = false;
        private int _saveCurrentIndent = 0;
        private string _saveCurrentSegment = string.Empty;
        private int _saveLineNumber = 0;
        private int _saveTabSize = 0;

        /// <summary>
        /// The default constructor that creates an instance of the <see cref="IndentProcessor" />
        /// class.
        /// </summary>
        public IndentProcessor() : this(ServiceLocater.Current.Get<ILogger>(),
                                        ServiceLocater.Current.Get<ILocater>())
        {
        }

        /// <summary>
        /// A constructor that creates an instance of the <see cref="IndentProcessor" /> class and
        /// initializes its dependencies.
        /// </summary>
        /// <param name="logger">
        /// A reference to a logger object used for logging messages.
        /// </param>
        /// <param name="locater">
        /// A reference to a locater object used for keeping track of the current location being
        /// processed in the text template file.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception is thrown if any of the dependencies passed into the constructor are
        /// <see langword="null" />.
        /// </exception>
        internal IndentProcessor(ILogger logger, ILocater locater)
        {
            Utility.NullDependencyCheck(logger,
                                        ClassNames.IndentProcessorClass,
                                        ServiceNames.LoggerService,
                                        ServiceParameterNames.LoggerParameter);

            Utility.NullDependencyCheck(locater,
                                        ClassNames.IndentProcessorClass,
                                        ServiceNames.LocaterService,
                                        ServiceParameterNames.LocaterParameter);

            _logger = logger;
            _locater = locater;
            CurrentIndent = 0;
            TabSize = DefaultTabSize;
        }

        /// <summary>
        /// Gets the current indent value.
        /// </summary>
        /// <remarks>
        /// The value returned is the number of tabs that will be inserted at the beginning the next
        /// generated text line.
        /// </remarks>
        public int CurrentIndent { get; private set; }

        /// <summary>
        /// Gets the current tab size.
        /// </summary>
        /// <remarks>
        /// The value returned is the number of spaces in a single tab.
        /// </remarks>
        public int TabSize { get; private set; }

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
        public int GetFirstTimeIndent(int firstTimeOffset, TextItem textItem)
        {
            int indent = CurrentIndent;

            if (firstTimeOffset is not 0)
            {
                indent += firstTimeOffset * TabSize;

                if (indent < 0)
                {
                    _logger.Log(LogEntryType.Generating,
                                _locater.Location,
                                MsgFirstTimeIndentHasBeenTruncated,
                                _locater.CurrentSegment);
                    indent = 0;
                }

                CurrentIndent = indent;
            }
            else
            {
                indent = GetIndent(textItem);
            }

            return indent;
        }

        /// <summary>
        /// Determines the correct indent value for the given text item.
        /// </summary>
        /// <param name="textItem">
        /// The text item for the current text line being processed.
        /// </param>
        /// <returns>
        /// The calculated indent value for the current text line.
        /// </returns>
        public int GetIndent(TextItem textItem)
        {
            int indent = CurrentIndent;

            if (textItem.IsRelative)
            {
                indent += textItem.Indent * TabSize;
            }
            else
            {
                indent = textItem.Indent * TabSize;
            }

            if (indent < 0)
            {
                _logger.Log(LogEntryType.Generating,
                            _locater.Location,
                            MsgLeftIndentHasBeenTruncated,
                            _locater.CurrentSegment);
                indent = 0;
            }

            if (textItem.IsOneTime is false)
            {
                CurrentIndent = indent;
            }

            return indent;
        }

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
        public bool IsValidIndentValue(string stringValue, out int indent)
        {
            if (int.TryParse(stringValue, out int indentValue))
            {
                if (indentValue is < MinIndentValue or > MaxIndentValue)
                {
                    _logger.Log(LogEntryType.Parsing,
                                _locater.Location,
                                MsgIndentValueOutOfRange,
                                indentValue.ToString());
                }
                else
                {
                    indent = indentValue;
                    return true;
                }
            }
            else
            {
                _logger.Log(LogEntryType.Parsing,
                            _locater.Location,
                            MsgIndentValueMustBeValidNumber,
                            stringValue);
            }

            indent = 0;
            return false;
        }

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
        public bool IsValidTabSizeValue(string stringValue, out int tabSize)
        {
            if (int.TryParse(stringValue, out int tabValue))
            {
                if (tabValue is < MinTabSize or > MaxTabSize)
                {
                    _logger.Log(LogEntryType.Parsing,
                                _locater.Location,
                                MsgTabSizeValueOutOfRange,
                                tabValue.ToString());
                }
                else
                {
                    tabSize = tabValue;
                    return true;
                }
            }
            else
            {
                _logger.Log(LogEntryType.Parsing,
                            _locater.Location,
                            MsgTabSizeValueMustBeValidNumber,
                            stringValue);
            }

            tabSize = 0;
            return false;
        }

        /// <summary>
        /// Resets the current indent value to zero.
        /// </summary>
        public void Reset()
        {
            CurrentIndent = 0;
            TabSize = DefaultTabSize;
        }

        /// <summary>
        /// Restores the current indent, tab size, and location from the saved values.
        /// </summary>
        /// <remarks>
        /// This method exits without doing anything if the current indent, tab size, and location
        /// wasn't previously saved.
        /// </remarks>
        public void RestoreCurrentIndentLocation()
        {
            if (_isCurrentIndentSaved)
            {
                CurrentIndent = _saveCurrentIndent;
                TabSize = _saveTabSize;
                _locater.CurrentSegment = _saveCurrentSegment;
                _locater.LineNumber = _saveLineNumber;
                _isCurrentIndentSaved = false;
            }
        }

        /// <summary>
        /// Save the current indent, tab size, and location so that they can be restored later.
        /// </summary>
        public void SaveCurrentIndentLocation()
        {
            _saveCurrentIndent = CurrentIndent;
            _saveTabSize = TabSize;
            _saveCurrentSegment = _locater.CurrentSegment;
            _saveLineNumber = _locater.LineNumber;
            _isCurrentIndentSaved = true;
        }

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
        public void SetTabSize(int tabSize)
        {
            if (tabSize < MinTabSize)
            {
                _logger.Log(LogEntryType.Setup,
                            MsgTabSizeTooSmall,
                            MinTabSize.ToString());
                TabSize = MinTabSize;
            }
            else if (tabSize > MaxTabSize)
            {
                _logger.Log(LogEntryType.Setup,
                            MsgTabSizeTooLarge,
                            MaxTabSize.ToString());
                TabSize = MaxTabSize;
            }
            else
            {
                TabSize = tabSize;
            }
        }
    }
}