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
            private IEnumerator<T> enumerator;

            public Peekable(IEnumerable<T> enumerable)
            {
                enumerator = enumerable.GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enumerator(this);
            }

            public bool HasNext()
            {
                Peek();
                return hasPeek;
            }

            T peeked;
            bool hasPeek;
            public T Peek()
            {
                if (!hasPeek)
                {
                    peeked = Read();
                    hasPeek = didMove;
                }

                return peeked;
            }

            T current;
            bool didMove;
            public T Read()
            {
                didMove = true;
                if (hasPeek)
                {
                    current = peeked;
                }
                else
                {
                    if (!enumerator.MoveNext())
                        didMove = false;

                    if (didMove)
                        current = enumerator.Current;
                    else
                        current = default(T);
                }

                hasPeek = false;
                return current;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public class Enumerator : IEnumerator<T>
            {
                private Peekable<T> peekable;

                public Enumerator(Peekable<T> peekable)
                {
                    this.peekable = peekable;
                }

                public T Current
                {
                    get
                    {
                        return peekable.current;
                    }
                }

                object IEnumerator.Current
                {
                    get
                    {
                        return Current;
                    }
                }

                public void Dispose()
                {
                    peekable.enumerator.Dispose();
                }

                public bool MoveNext()
                {
                    peekable.Read();
                    return peekable.didMove;
                }

                public void Reset()
                {
                    peekable.enumerator.Reset();
                }
            }
        }
    }
}
