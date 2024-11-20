namespace TextTemplateProcessor.TestShared
{
    /// <summary>
    /// The <see cref="Sequence{T}" /> class gives an alternative to the Moq SetupSequence method
    /// for returning a series of values from a mock object.
    /// </summary>
    /// <typeparam name="T">
    /// The class type or value type that is to be returned from this sequence.
    /// </typeparam>
    /// <param name="values">
    /// The set of values that will be returned in order from this sequence.
    /// </param>
    /// <param name="maxCalls">
    /// The maximum number of calls permitted for this sequence.
    /// </param>
    public class Sequence<T>(T[] values, int maxCalls = int.MaxValue)
    {
        private readonly int _maxCalls = maxCalls;
        private readonly int _maxIndex = values.Length - 1;
        private readonly T[] _values = values;
        private int _index = -1;
        private int _totalCalls = 0;

        public T Get()
        {
            _totalCalls
                .Should()
                .BeLessThan(_maxCalls, $"total calls on the sequence should be less than {_maxCalls}");

            _totalCalls++;
            return _index < _maxIndex ? _values[_index] : _values[_maxIndex];
        }

        public T GetNext()
        {
            Next();
            return Get();
        }

        public void Next()
        {
            if (_index < _maxIndex)
            {
                _index++;
            }
        }
    }
}