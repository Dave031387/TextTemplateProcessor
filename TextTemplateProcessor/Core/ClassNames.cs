namespace TextTemplateProcessor.Core
{
    using TextTemplateProcessor.Console;
    using TextTemplateProcessor.IO;
    using TextTemplateProcessor.Logger;

    internal static class ClassNames
    {
        public static string ConsoleLoggerClass = nameof(ConsoleLogger);
        public static string ConsoleReaderClass = nameof(ConsoleReader);
        public static string ConsoleWriterClass = nameof(ConsoleWriter);
        public static string DefaultSegmentNameGeneratorClass = nameof(DefaultSegmentNameGenerator);
        public static string FileAndDirectoryServiceClass = nameof(FileAndDirectoryService);
        public static string IndentProcessorClass = nameof(IndentProcessor);
        public static string LocaterClass = nameof(Locater);
        public static string MessageWriterClass = nameof(MessageWriter);
        public static string NameValidaterClass = nameof(NameValidater);
        public static string PathValidaterClass = nameof(PathValidater);
        public static string SegmentHeaderParserClass = nameof(SegmentHeaderParser);
        public static string TemplateLoaderClass = nameof(TemplateLoader);
        public static string TextLineParserClass = nameof(TextLineParser);
        public static string TextReaderClass = nameof(TextReader);
        public static string TextTemplateConsoleBaseClass = nameof(TextTemplateConsoleBase);
        public static string TextWriterClass = nameof(TextWriter);
        public static string TokenProcessorClass = nameof(TokenProcessor);
    }
}