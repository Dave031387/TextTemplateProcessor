namespace TextTemplateProcessor.Interfaces
{
    internal interface IServiceLocater
    {
        T Get<T>();
    }
}