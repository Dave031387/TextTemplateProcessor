namespace TextTemplateProcessor.TestShared
{
    public static class MethodCallTokens
    {
        public static readonly MethodCallToken ConsoleReader_ReadLine = new(nameof(ConsoleReader_ReadLine));
        public static readonly MethodCallToken DefaultSegmentNameGenerator_Next = new(nameof(DefaultSegmentNameGenerator_Next));
        public static readonly MethodCallToken DefaultSegmentNameGenerator_Reset = new(nameof(DefaultSegmentNameGenerator_Reset));
        public static readonly MethodCallToken FileAndDirectoryService_ClearDirectory = new(nameof(FileAndDirectoryService_ClearDirectory));
        public static readonly MethodCallToken FileAndDirectoryService_CombineDirectoryAndFileName = new(nameof(FileAndDirectoryService_CombineDirectoryAndFileName));
        public static readonly MethodCallToken FileAndDirectoryService_CreateDirectory = new(nameof(FileAndDirectoryService_CreateDirectory));
        public static readonly MethodCallToken FileAndDirectoryService_GetDirectoryName = new(nameof(FileAndDirectoryService_GetDirectoryName));
        public static readonly MethodCallToken FileAndDirectoryService_GetFileName = new(nameof(FileAndDirectoryService_GetFileName));
        public static readonly MethodCallToken FileAndDirectoryService_GetFullPath = new(nameof(FileAndDirectoryService_GetFullPath));
        public static readonly MethodCallToken FileAndDirectoryService_GetSolutionDirectory = new(nameof(FileAndDirectoryService_GetSolutionDirectory));
        public static readonly MethodCallToken FileAndDirectoryService_ReadTextFile = new(nameof(FileAndDirectoryService_ReadTextFile));
        public static readonly MethodCallToken FileAndDirectoryService_WriteTextFile = new(nameof(FileAndDirectoryService_WriteTextFile));
        public static readonly MethodCallToken IndentProcessor_GetFirstTimeIndent = new(nameof(IndentProcessor_GetFirstTimeIndent));
        public static readonly MethodCallToken IndentProcessor_GetIndent = new(nameof(IndentProcessor_GetIndent));
        public static readonly MethodCallToken IndentProcessor_GetTabSize = new(nameof(IndentProcessor_GetTabSize));
        public static readonly MethodCallToken IndentProcessor_IsValidIndentValue = new(nameof(IndentProcessor_IsValidIndentValue));
        public static readonly MethodCallToken IndentProcessor_IsValidTabSizeValue = new(nameof(IndentProcessor_IsValidTabSizeValue));
        public static readonly MethodCallToken IndentProcessor_Reset = new(nameof(IndentProcessor_Reset));
        public static readonly MethodCallToken IndentProcessor_RestoreCurrentState = new(nameof(IndentProcessor_RestoreCurrentState));
        public static readonly MethodCallToken IndentProcessor_SaveCurrentState = new(nameof(IndentProcessor_SaveCurrentState));
        public static readonly MethodCallToken IndentProcessor_SetTabSize = new(nameof(IndentProcessor_SetTabSize));
        public static readonly MethodCallToken Locater_CurrentSegment_Getter = new(nameof(Locater_CurrentSegment_Getter));
        public static readonly MethodCallToken Locater_CurrentSegment_Setter = new(nameof(Locater_CurrentSegment_Setter));
        public static readonly MethodCallToken Locater_LineNumber_Getter = new(nameof(Locater_LineNumber_Getter));
        public static readonly MethodCallToken Locater_LineNumber_Setter = new(nameof(Locater_LineNumber_Setter));
        public static readonly MethodCallToken Locater_Reset = new(nameof(Locater_Reset));
        public static readonly MethodCallToken Logger_Clear = new(nameof(Logger_Clear));
        public static readonly MethodCallToken Logger_GetLogEntryType = new(nameof(Logger_GetLogEntryType));
        public static readonly MethodCallToken Logger_LogEntries = new(nameof(Logger_LogEntries));
        public static readonly MethodCallToken Logger_Log_Message = new(nameof(Logger_Log_Message));
        public static readonly MethodCallToken Logger_SetLogEntryType_Generating = new(nameof(Logger_SetLogEntryType_Generating));
        public static readonly MethodCallToken Logger_SetLogEntryType_Loading = new(nameof(Logger_SetLogEntryType_Loading));
        public static readonly MethodCallToken Logger_SetLogEntryType_Reset = new(nameof(Logger_SetLogEntryType_Reset));
        public static readonly MethodCallToken Logger_SetLogEntryType_Setup = new(nameof(Logger_SetLogEntryType_Setup));
        public static readonly MethodCallToken Logger_SetLogEntryType_User = new(nameof(Logger_SetLogEntryType_User));
        public static readonly MethodCallToken Logger_SetLogEntryType_Writing = new(nameof(Logger_SetLogEntryType_Writing));
        public static readonly MethodCallToken Logger_WriteLogEntries = new(nameof(Logger_WriteLogEntries));
        public static readonly MethodCallToken MessageWriter_WriteLine = new(nameof(MessageWriter_WriteLine));
        public static readonly MethodCallToken NameValidater_IsValidName = new(nameof(NameValidater_IsValidName));
        public static readonly MethodCallToken PathValidater_ValidateFullPath = new(nameof(PathValidater_ValidateFullPath));
        public static readonly MethodCallToken PathValidater_ValidatePath = new(nameof(PathValidater_ValidatePath));
        public static readonly MethodCallToken TemplateLoader_LoadTemplate = new(nameof(TemplateLoader_LoadTemplate));
        public static readonly MethodCallToken TextReader_GetFileName = new(nameof(TextReader_GetFileName));
        public static readonly MethodCallToken TextReader_GetFullFilePath = new(nameof(TextReader_GetFullFilePath));
        public static readonly MethodCallToken TextReader_ReadTextFile = new(nameof(TextReader_ReadTextFile));
        public static readonly MethodCallToken TextReader_SetFilePath = new(nameof(TextReader_SetFilePath));
        public static readonly MethodCallToken TextWriter_WriteTextFile = new(nameof(TextWriter_WriteTextFile));
        public static readonly MethodCallToken TokenProcessor_ClearTokens = new(nameof(TokenProcessor_ClearTokens));
        public static readonly MethodCallToken TokenProcessor_LoadTokenValues = new(nameof(TokenProcessor_LoadTokenValues));
        public static readonly MethodCallToken TokenProcessor_ReplaceTokens = new(nameof(TokenProcessor_ReplaceTokens));
        public static readonly MethodCallToken TokenProcessor_ResetTokenDelimiters = new(nameof(TokenProcessor_ResetTokenDelimiters));
        public static readonly MethodCallToken TokenProcessor_SetTokenDelimiters = new(nameof(TokenProcessor_SetTokenDelimiters));
    }
}
