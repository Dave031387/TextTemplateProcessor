﻿namespace TextTemplateProcessor.TestShared
{
    public enum MethodCall
    {
        ConsoleReader_ReadLine,
        DefaultSegmentNameGenerator_Next,
        DefaultSegmentNameGenerator_Reset,
        FileAndDirectoryService_ClearDirectory,
        FileAndDirectoryService_CombineDirectoryAndFileName,
        FileAndDirectoryService_CreateDirectory,
        FileAndDirectoryService_GetDirectoryName,
        FileAndDirectoryService_GetFileName,
        FileAndDirectoryService_GetFullPath,
        FileAndDirectoryService_GetSolutionDirectory,
        FileAndDirectoryService_ReadTextFile,
        FileAndDirectoryService_WriteTextFile,
        IndentProcessor_IsValidIndentValue,
        IndentProcessor_IsValidTabSizeValue,
        IndentProcessor_Reset,
        IndentProcessor_SetTabSize,
        Locater_CurrentSegment_Getter,
        Locater_CurrentSegment_Setter,
        Locater_LineNumber_Getter,
        Locater_LineNumber_Setter,
        Locater_Reset,
        Logger_Log_Message,
        Logger_Log_FirstMessage,
        Logger_Log_SecondMessage,
        Logger_Log_ThirdMessage,
        Logger_Log_FourthMessage,
        Logger_Log_FifthMessage,
        Logger_SetLogEntryType_Generating,
        Logger_SetLogEntryType_Loading,
        Logger_SetLogEntryType_Reset,
        Logger_SetLogEntryType_Setup,
        Logger_SetLogEntryType_User,
        Logger_SetLogEntryType_Writing,
        Logger_WriteLogEntries,
        MessageWriter_WriteLine,
        MessageWriter_WriteLine_First,
        MessageWriter_WriteLine_Second,
        NameValidater_IsValidName,
        NameValidater_IsValidName_FirstName,
        NameValidater_IsValidName_SecondName,
        NameValidater_IsValidName_ThirdName,
        NameValidater_IsValidName_FourthName,
        PathValidater_ValidateFullPath,
        PathValidater_ValidatePath,
        TemplateLoader_LoadTemplate,
        TextReader_FileName,
        TextReader_FullFilePath,
        TextReader_GetFileName,
        TextReader_GetFullFilePath,
        TextReader_ReadTextFile,
        TextReader_SetFilePath,
        TextWriter_WriteTextFile,
        TokenProcessor_ClearTokens,
        TokenProcessor_SetTokenDelimiters
    }
}
