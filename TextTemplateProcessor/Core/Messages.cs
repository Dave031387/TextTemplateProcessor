﻿namespace TextTemplateProcessor.Core
{
    /// <summary>
    /// The <see cref="Messages" /> static class defines all the message strings that are used for
    /// exceptions, log entries, etc, in the <see cref="TextTemplateProcessor" /> class library.
    /// </summary>
    internal static class Messages
    {
        internal const string MsgAttemptingToReadFile = "Attempting to read text template file:\n{0}";
        internal const string MsgAttemptToGenerateSegmentBeforeItWasLoaded = "An attempt was made to generate segment \"{0}\" before the template was loaded.";
        internal const string MsgAttemptToLoadMoreThanOnce = "Attempted to load template file \"{0}\" more than once. Repeat loads will be ignored.";
        internal const string MsgClearTheOutputDirectory = "\nCONFIRM: Do you want to clear the contents of the following directory?\n{0}";
        internal const string MsgContinuationPrompt = "Press [ENTER] to continue...";
        internal const string MsgDependencyIsNull = "The {1} dependency of the {0} object should not be null.";
        internal const string MsgDirectoryNotFound = "The specified directory was not found. Directory path: ";
        internal const string MsgDirectoryPathIsEmptyOrWhitespace = "The directory path must not be empty or contain only whitespace.";
        internal const string MsgErrorWhenClearingOutputDirectory = "An unexpected error occurred while trying to clear the output directory. {0}";
        internal const string MsgErrorWhenCreatingOutputDirectory = "An unexpected error occurred when creating the output directory. {0}";
        internal const string MsgErrorWhenLocatingSolutionDirectory = "An unexpected error occurred while trying to locate the solution directory. {0}";
        internal const string MsgErrorWhileConstructingFilePath = "An error occurred when trying to construct the output file path. {0}";
        internal const string MsgErrorWhileReadingTemplateFile = "An error occurred while reading the template file. {0}";
        internal const string MsgErrorWhileReadingUserResponse = "An unexpected error occurred while reading the user response. {0}";
        internal const string MsgFileNotFound = "The specified file was not found. Full file path: ";
        internal const string MsgFilePathIsEmptyOrWhitespace = "The file path must not be empty or contain only whitespace.";
        internal const string MsgFileSuccessfullyRead = "The text template file has been successfully read.";
        internal const string MsgFirstTimeIndentHasBeenTruncated = "The calculated first time indent for segment \"{0}\" went negative. It will be set to zero.";
        internal const string MsgFirstTimeIndentIsInvalid = "The First Time Indent for segment \"{0}\" must be a number between -9 and 9. The value found was \"{1}\"";
        internal const string MsgFirstTimeIndentSetToZero = "Found a First Time Indent option value of zero for segment \"{0}\". This value disables the First Time Indent processing.";
        internal const string MsgFoundDuplicateOptionNameOnHeaderLine = "The option \"{1}\" appears more than once for segment \"{0}\". Only the first occurrence will be used.";
        internal const string MsgFoundDuplicateSegmentName = "Segment name \"{0}\" appears more than once in the template file. Default name \"{1}\" will be used in place of the duplicate.";
        internal const string MsgFoundSolutionDirectoryPath = "The solution directory path was determined to be: {0}";
        internal const string MsgFourthCharacterMustBeBlank = "The fourth character of each template line should be blank:\n{0}\n   ^";
        internal const string MsgFullPathCannotBeDetermined = "The full path can't be determined because the solution directory path is unknown.";
        internal const string MsgGeneratedTextHasBeenReset = "The generated text cache for template file \"{0}\" has been reset.";
        internal const string MsgGeneratedTextIsEmpty = "The generated text is empty. Unable to write to output file \"{0}\"";
        internal const string MsgGeneratedTextIsNull = "Unable to write to the output file because the generated text is null.";
        internal const string MsgIndentValueMustBeValidNumber = "The indent value \"{0}\" is not a valid integer value.";
        internal const string MsgIndentValueOutOfRange = "The FTI option value must be a number between -9 and 9. The value given was {0}.";
        internal const string MsgInvalidControlCode = "The following template line doesn't begin with a valid control code:\n{0}\n^^^";
        internal const string MsgInvalidDirectoryCharacters = "The directory path contains invalid characters.";
        internal const string MsgInvalidFileNameCharacters = "The file name contains invalid characters.";
        internal const string MsgInvalidFormOfOption = "Segment options must follow the form \"option=value\" with no intervening spaces. Found \"{1}\" on the \"{0}\" segment header.";
        internal const string MsgInvalidPadSegmentName = "\"{1}\" is not a valid name for the PAD option for segment \"{0}\". It will be ignored.";
        internal const string MsgInvalidSegmentName = "\"{0}\" is not a valid segment name. The default name \"{1}\" will be used instead.";
        internal const string MsgInvalidTabSizeOption = "The Tab Size option for segment \"{0}\" was invalid and will be ignored.";
        internal const string MsgLeftIndentHasBeenTruncated = "The calculated line indent for segment \"{0}\" went negative. It will be set to zero.";
        internal const string MsgLoadingTemplateFile = "Loading template file \"{0}\"";
        internal const string MsgMinimumLineLengthInTemplateFileIs3 = "All lines in the template file must be at least 3 characters long.";
        internal const string MsgMissingDirectoryPath = "The specified file path doesn't contain a valid directory path.";
        internal const string MsgMissingFileName = "The file name is missing from the file path.";
        internal const string MsgMissingInitialSegmentHeader = "The template file is missing the initial segment header. The default segment \"{0}\" will be used.";
        internal const string MsgMissingTokenName = "Found token start and end delimiters with no token name between them. The token will be ignored.";
        internal const string MsgMultipleLevelsOfPadSegments = "Pad segment \"{1}\" specified for segment \"{0}\" also contains a pad segment. Multiple levels of pad segments are not allowed.";
        internal const string MsgNextLoadRequestBeforeFirstIsWritten = "Template file \"{0}\" is being loaded before any output was written for template file \"{1}\"";
        internal const string MsgNoTextLinesFollowingSegmentHeader = "The header line for segment \"{0}\" must be followed by one or more valid text lines. The segment will be ignored.";
        internal const string MsgNullDirectoryPath = "The directory path must not be null.";
        internal const string MsgNullFilePath = "The file path must not be null.";
        internal const string MsgOptionNameMustPrecedeEqualsSign = "An option name must appear immediately before the equals sign with no intervening spaces in the \"{0}\" segment header.";
        internal const string MsgOptionValueMustFollowEqualsSign = "The value for option \"{1}\" must appear immediately after the equals sign with no intervening spaces in the \"{0}\" segment header.";
        internal const string MsgOutputDirectoryCleared = "The output directory has been cleared.";
        internal const string MsgOutputDirectoryNotSet = "The output file can't be written because the output directory hasn't been set.";
        internal const string MsgPadSegmentMustBeDefinedEarlier = "The PAD segment name \"{1}\" referenced by segment \"{0}\" must be defined earlier in the template file. It will be ignored.";
        internal const string MsgPadSegmentNameSameAsSegmentHeaderName = "The PAD segment name and segment header name for segment \"{0}\" are identical. The PAD segment name will be ignored.";
        internal const string MsgPathIsNotRooted = "Expected a rooted path, but found \"{0}\"";
        internal const string MsgProcessingSegment = "Processing segment \"{0}\"...";
        internal const string MsgRootPathIsNull = "The root directory path must not be null.";
        internal const string MsgSegmentHasBeenAdded = "Segment \"{0}\" has been added to the control dictionary.";
        internal const string MsgSegmentHasBeenReset = "Segment \"{0}\" has been reset.";
        internal const string MsgSegmentHasNoTextLines = "Tried to generate segment \"{0}\" but the segment has no text lines.";
        internal const string MsgSegmentNameIsNullOrWhitespace = "The segment name passed into the GenerateSegment method was null, empty or whitespace.";
        internal const string MsgSegmentNameMustStartInColumn5 = "The segment name must start in column 5 of the segment header line. The default name \"{0}\" will be used instead.\n{1}\n    ^";
        internal const string MsgTabSizeTooLarge = "The requested tab size is too large. The maximum value \"{0}\" will be used.";
        internal const string MsgTabSizeTooSmall = "The requested tab size is too small. The minimum value \"{0}\" will be used.";
        internal const string MsgTabSizeValueMustBeValidNumber = "The tab size value \"{0}\" is not a valid integer value.";
        internal const string MsgTabSizeValueOutOfRange = "The tab size must be an integer between 1 and 9, but the specified value was \"{0}\".";
        internal const string MsgTemplateFileIsEmpty = "This template file is empty: {0}";
        internal const string MsgTemplateFilePathNotSet = "Unable to load the template file because a valid file path has not been set.";
        internal const string MsgTemplateHasBeenReset = "The environment for template file \"{0}\" has been reset.";
        internal const string MsgTokenDictionaryContainsInvalidTokenName = "The token dictionary contained an invalid token name \"{1}\" for segment \"{0}\".";
        internal const string MsgTokenDictionaryIsEmpty = "An empty token dictionary was supplied for segment \"{0}\".";
        internal const string MsgTokenDictionaryIsNull = "A null token dictionary was supplied for segment \"{0}\".";
        internal const string MsgTokenEndAndTokenEscapeAreSame = "The token end delimiter \"{0}\" must not be the same as the same as the token escape character \"{1}\".";
        internal const string MsgTokenEndDelimiterIsEmpty = "The token end delimiter must not be empty or whitespace.";
        internal const string MsgTokenEndDelimiterIsNull = "The token end delimiter must not be null.";
        internal const string MsgTokenHasInvalidName = "Found a token with an invalid name: \"{0}\". It will be ignored.";
        internal const string MsgTokenMissingEndDelimiter = "Found a token start delimiter with no matching end delimiter. The token will be ignored.";
        internal const string MsgTokenNameNotFound = "The token name \"{1}\" in segment {0} wasn't found in the token dictionary. It will be output as is.";
        internal const string MsgTokenStartAndTokenEndAreSame = "The token start delimiter \"{0}\" must not be the same as the same as the token end delimiter \"{1}\".";
        internal const string MsgTokenStartAndTokenEscapeAreSame = "The token start delimiter \"{0}\" must not be the same as the same as the token escape character \"{1}\".";
        internal const string MsgTokenStartDelimiterIsEmpty = "The token start delimiter must not be empty or whitespace.";
        internal const string MsgTokenStartDelimiterIsNull = "The token start delimiter must not be null.";
        internal const string MsgTokenStartDelimiterWarning = "Ending the token start delimiter with '-', '+' or '=' may cause confusion and lead to unexpected errors.";
        internal const string MsgTokenValueIsEmpty = "Found token \"{1}\" with no assigned value while generating segment \"{0}\".";
        internal const string MsgTokenWithEmptyValue = "Token \"{1}\" was passed in with an empty value for segment \"{0}\".";
        internal const string MsgTokenWithNullValue = "Token \"{1}\" was passed in with a null value for segment \"{0}\".";
        internal const string MsgUnableToCreateOutputDirectory = "Encountered an error when trying to create the output directory path.\n{0}";
        internal const string MsgUnableToLoadTemplateFile = "Encountered an error when trying to load the template file.\n{0}";
        internal const string MsgUnableToLocateSolutionDirectory = "The directory containing the solution file could not be found.";
        internal const string MsgUnableToResetSegment = "Unable to reset segment \"{0}\" because of a null or unknown segment name.";
        internal const string MsgUnableToSetTemplateFilePath = "Unable to set the template file path. {0}";
        internal const string MsgUnableToWriteFile = "Unable to write to output file. {0}";
        internal const string MsgUnknownSegmentName = "A request was made to generate segment \"{0}\" but that segment wasn't found in the template file.";
        internal const string MsgUnknownSegmentOptionFound = "An unknown segment option \"{1}\" was found on segment \"{0}\". It will be ignored.";
        internal const string MsgUnknownTokenName = "An unknown token name \"{1}\" was supplied for segment \"{0}\". It will be ignored.";
        internal const string MsgWritingTextFile = "Writing generated text to file \"{0}\"";
        internal const string MsgYesNoPrompt = "Enter Y (yes) or N (no)...";
    }
}