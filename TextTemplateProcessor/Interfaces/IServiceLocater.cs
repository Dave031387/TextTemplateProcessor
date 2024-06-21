namespace TextTemplateProcessor.Interfaces
{
    internal interface IServiceLocater
    {
        T Get<T>(string? key = null) where T : class;
    }
}