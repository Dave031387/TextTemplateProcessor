namespace TextTemplateProcessor.Fakes
{
    using System.Collections.Generic;

    public class ConsoleWriterFake : IConsoleWriter
    {
        public List<string> Buffer { get; } = new();

        public void ClearBuffer() => Buffer.Clear();

        public void WriteLine(string text)
        {
            text
                .Should()
                .NotBeNull();
            Buffer.Add(text);
        }
    }
}