namespace TextTemplateProcessor.Core
{
    using global::TextTemplateProcessor.Console;
    using global::TextTemplateProcessor.Interfaces;
    using global::TextTemplateProcessor.IO;
    using global::TextTemplateProcessor.Logger;
    using Ninject;

    internal class ServiceLocater
    {
        private static readonly IServiceLocater _serviceLocater;

        static ServiceLocater() => _serviceLocater = new DefaultServiceLocater();

        public static IServiceLocater Current => _serviceLocater;

        private class DefaultServiceLocater : IServiceLocater, IDisposable
        {
            private readonly IKernel _kernel;

            public DefaultServiceLocater()
            {
                _kernel = new StandardKernel();
                LoadBindings();
            }

            public T Get<T>() => _kernel.Get<T>();

            private void LoadBindings()
            {
                _kernel.Bind<IConsoleReader>().To<ConsoleReader>().InSingletonScope();
                _kernel.Bind<IConsoleWriter>().To<ConsoleWriter>().InSingletonScope();
                _kernel.Bind<IDefaultSegmentNameGenerator>().To<DefaultSegmentNameGenerator>().InSingletonScope();
                _kernel.Bind<IFileAndDirectoryService>().To<FileAndDirectoryService>().InSingletonScope();
                _kernel.Bind<IIndentProcessor>().To<IndentProcessor>().InSingletonScope();
                _kernel.Bind<IMessageWriter>().To<MessageWriter>().InSingletonScope();
                _kernel.Bind<ILocater>().To<Locater>().InSingletonScope();
                _kernel.Bind<ILogger>().To<ConsoleLogger>().InSingletonScope();
                _kernel.Bind<INameValidater>().To<NameValidater>().InSingletonScope();
                _kernel.Bind<IPathValidater>().To<PathValidater>().InSingletonScope();
                _kernel.Bind<ISegmentHeaderParser>().To<SegmentHeaderParser>().InSingletonScope();
                _kernel.Bind<ITemplateLoader>().To<TemplateLoader>().InSingletonScope();
                _kernel.Bind<ITextLineParser>().To<TextLineParser>().InSingletonScope();
                _kernel.Bind<ITextReader>().To<TextReader>().InSingletonScope();
                _kernel.Bind<ITextWriter>().To<TextWriter>().InSingletonScope();
                _kernel.Bind<ITokenProcessor>().To<TokenProcessor>().InSingletonScope();
            }

            public void Dispose() => _kernel.Dispose();
        }
    }
}