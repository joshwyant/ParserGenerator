using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ParserGenerator.Utility
{
    /*
        The whole idea here is to allow the reader to peek. 
        TextReader.Peek doesn't work if it's a StreamReader and the underlying stream can't seek.
    */
    public class CharacterReader : IPeekable<char>
    {
        private readonly TextReader _reader;
        private readonly IPeekable<char> _peekable;

        public CharacterReader(TextReader original)
        {
            _reader = original;
            _peekable = this.AsPeekable();
        }

        public bool HasNext() => _peekable.HasNext();

        public char Peek() => _peekable.Peek();

        public char Read() => _peekable.Read();

        public IEnumerator<char> GetEnumerator() => new Enumerator(_reader);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class Enumerator : IEnumerator<char>
        {
            private readonly TextReader _reader;
            public Enumerator(TextReader original)
            {
                _reader = original;
            }

            public char Current { get; private set; }
            object IEnumerator.Current => Current;

            public void Dispose() => _reader.Dispose();

            public bool MoveNext()
            {
                var read = _reader.Read();
                Current = (char)read;
                return read != -1;
            }

            public void Reset()
            {
                // Let's not do anything here....
            }
    }
    }
}
