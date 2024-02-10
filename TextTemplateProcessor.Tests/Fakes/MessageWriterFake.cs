namespace TextTemplateProcessor.Fakes
{
    using System.Collections.Generic;

    internal class MessageWriterFake : IMessageWriter
    {
        public List<string> Buffer { get; } = new();

        public void Reset() => Buffer.Clear();

        public void WriteLine(string message)
        {
            message
                .Should()
                .NotBeNull();
            Buffer.Add(message);
        }

        public void WriteLine(string message, string arg)
            => WriteLine(string.Format(message, arg));

        public void WriteLine(string message, string arg1, string arg2)
            => WriteLine(string.Format(message, arg1, arg2));
    }
}