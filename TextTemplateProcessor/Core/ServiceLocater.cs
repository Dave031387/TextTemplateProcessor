namespace TextTemplateProcessor.Core
{
    using BasicIoC;
    using global::TextTemplateProcessor.Console;
    using global::TextTemplateProcessor.Interfaces;
    using global::TextTemplateProcessor.IO;
    using global::TextTemplateProcessor.Logger;

    internal class ServiceLocater : IServiceLocater
    {
        private readonly Container _container;

        private static readonly Lazy<ServiceLocater> _lazy
            = new(() => new ServiceLocater());

        private ServiceLocater()
        {
            _container = Container.Instance;
            _container.RegisterSingleton<IConsoleReader, ConsoleReader>();
            _container.RegisterSingleton<IConsoleWriter, ConsoleWriter>();
            _container.RegisterSingleton<IDefaultSegmentNameGenerator, DefaultSegmentNameGenerator>();
            _container.RegisterSingleton<IFileAndDirectoryService, FileAndDirectoryService>();
            _container.RegisterSingleton<IIndentProcessor, IndentProcessor>();
            _container.RegisterSingleton<IMessageWriter, MessageWriter>();
            _container.RegisterSingleton<ILocater, Locater>();
            _container.RegisterSingleton<ILogger, ConsoleLogger>();
            _container.RegisterSingleton<INameValidater, NameValidater>();
            _container.RegisterSingleton<IPathValidater, PathValidater>();
            _container.RegisterSingleton<ISegmentHeaderParser, SegmentHeaderParser>();
            _container.RegisterSingleton<ITemplateLoader, TemplateLoader>();
            _container.RegisterSingleton<ITextLineParser, TextLineParser>();
            _container.RegisterSingleton<ITextReader, TextReader>();
            _container.RegisterSingleton<ITextWriter, TextWriter>();
            _container.RegisterSingleton<ITokenProcessor, TokenProcessor>();
        }

        public static IServiceLocater Current => _lazy.Value;

        public T Get<T>(string? key = null) where T : class
            => _container.ResolveDependency<T>(key)!;
    }
}