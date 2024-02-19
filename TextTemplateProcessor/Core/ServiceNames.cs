namespace TextTemplateProcessor.Core
{
    using global::TextTemplateProcessor.Interfaces;

    internal static class ServiceNames
    {
        public static string ConsoleReaderService = nameof(IConsoleReader);
        public static string ConsoleWriterService = nameof(IConsoleWriter);
        public static string DefaultSegmentNameGeneratorService = nameof(IDefaultSegmentNameGenerator);
        public static string FileAndDirectoryService = nameof(IFileAndDirectoryService);
        public static string IndentProcessorService = nameof(IIndentProcessor);
        public static string LocaterService = nameof(ILocater);
        public static string LoggerService = nameof(ILogger);
        public static string MessageWriterService = nameof(IMessageWriter);
        public static string NameValidaterService = nameof(INameValidater);
        public static string PathValidaterService = nameof(IPathValidater);
        public static string SegmentHeaderParserService = nameof(ISegmentHeaderParser);
        public static string TemplateLoaderService = nameof(ITemplateLoader);
        public static string TextLineParserService = nameof(ITextLineParser);
        public static string TextReaderService = nameof(ITextReader);
        public static string TextTemplateProcessorService = nameof(ITextTemplateProcessor);
        public static string TextWriterService = nameof(ITextWriter);
        public static string TokenProcessorService = nameof(ITokenProcessor);
    }
}