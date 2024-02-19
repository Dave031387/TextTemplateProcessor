namespace TextTemplateProcessor.Core
{
    using static ServiceNames;

    internal static class ServiceParameterNames
    {
        public static string ConsoleReaderParameter = GetParameterName(ConsoleReaderService);
        public static string ConsoleWriterParameter = GetParameterName(ConsoleWriterService);
        public static string DefaultSegmentNameGeneratorParameter = GetParameterName(DefaultSegmentNameGeneratorService);
        public static string FileAndDirectoryServiceParameter = GetParameterName(FileAndDirectoryService);
        public static string IndentProcessorParameter = GetParameterName(IndentProcessorService);
        public static string LocaterParameter = GetParameterName(LocaterService);
        public static string LoggerParameter = GetParameterName(LoggerService);
        public static string MessageWriterParameter = GetParameterName(MessageWriterService);
        public static string NameValidaterParameter = GetParameterName(NameValidaterService);
        public static string PathValidaterParameter = GetParameterName(PathValidaterService);
        public static string SegmentHeaderParserParameter = GetParameterName(SegmentHeaderParserService);
        public static string TemplateLoaderParameter = GetParameterName(TemplateLoaderService);
        public static string TextLineParserParameter = GetParameterName(TextLineParserService);
        public static string TextReaderParameter = GetParameterName(TextReaderService);
        public static string TextTemplateProcessorParameter = GetParameterName(TextTemplateProcessorService);
        public static string TextWriterParameter = GetParameterName(TextWriterService);
        public static string TokenProcessorParameter = GetParameterName(TokenProcessorService);

        private static string GetParameterName(string serviceName)
            => serviceName[1..2].ToLowerInvariant() + serviceName[2..];
    }
}