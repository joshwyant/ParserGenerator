using System.Collections;
using System.Collections.Generic;

namespace ParserGenerator.Utility
{
    public static class PeekableExtensions
    {
        public static IPeekable<T> AsPeekable<T>(this IEnumerable<T> enumerable)
        {
            return new Peekable<T>(enumerable);
        }

        private class Peekable<T> : IPeekable<T>
        {
            private readonly IEnumerator<T> _enumerator;

            public Peekable(IEnumerable<T> enumerable)
            {
                _enumerator = enumerable.GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enumerator(this);
            }

            public bool HasNext()
            {
                Peek();
                return _hasPeek;
            }

            private T _peeked;
            private bool _hasPeek;
            public T Peek()
            {
                if (!_hasPeek)
                {
                    _peeked = Read();
                    _hasPeek = _didMove;
                }

                return _peeked;
            }

            private T _current;
            private bool _didMove;
            public T Read()
            {
                _didMove = true;
                if (_hasPeek)
                {
                    _current = _peeked;
                }
                else
                {
                    if (!_enumerator.MoveNext())
                        _didMove = false;

                    _current = _didMove ? _enumerator.Current : default(T);
                }

                _hasPeek = false;
                return _current;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private class Enumerator : IEnumerator<T>
            {
                private readonly Peekable<T> _peekable;

                public Enumerator(Peekable<T> peekable)
                {
                    _peekable = peekable;
                }

                public T Current => _peekable._current;

                object IEnumerator.Current => Current;

                public void Dispose() => _peekable._enumerator.Dispose();

                public bool MoveNext()
                {
                    _peekable.Read();
                    return _peekable._didMove;
                }

                public void Reset() => _peekable._enumerator.Reset();
            }
        }
    }
}
