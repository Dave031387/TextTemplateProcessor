namespace TextTemplateProcessor.Core
{
    using BasicDI;
    using global::TextTemplateProcessor.Console;
    using global::TextTemplateProcessor.Interfaces;
    using global::TextTemplateProcessor.IO;
    using global::TextTemplateProcessor.Logger;

    internal static class ServiceLocater
    {
        private static IContainer _container = Container.Current;

        static ServiceLocater()
        {
            _container.Bind<IConsoleReader>().To<ConsoleReader>().AsSingleton();
            _container.Bind<IConsoleWriter>().To<ConsoleWriter>().AsSingleton();
            _container.Bind<IDefaultSegmentNameGenerator>().To<DefaultSegmentNameGenerator>().AsSingleton();
            _container.Bind<IFileAndDirectoryService>().To<FileAndDirectoryService>().AsSingleton();
            _container.Bind<IIndentProcessor>().To<IndentProcessor>().AsSingleton();
            _container.Bind<ILocater>().To<Locater>().AsSingleton();
            _container.Bind<ILogger>().To<ConsoleLogger>().AsSingleton();
            _container.Bind<IMessageWriter>().To<MessageWriter>().AsSingleton();
            _container.Bind<INameValidater>().To<NameValidater>().AsSingleton();
            _container.Bind<IPathValidater>().To<PathValidater>().AsSingleton();
            _container.Bind<ISegmentHeaderParser>().To<SegmentHeaderParser>().AsSingleton();
            _container.Bind<ITemplateLoader>().To<TemplateLoader>().AsSingleton();
            _container.Bind<ITextLineParser>().To<TextLineParser>().AsSingleton();
            _container.Bind<ITextReader>().To<TextReader>().AsSingleton();
            _container.Bind<ITextWriter>().To<TextWriter>().AsSingleton();
            _container.Bind<ITokenProcessor>().To<TokenProcessor>().AsSingleton();
        }

        public static T Get<T>() where T : class => _container.Resolve<T>();
    }
}
