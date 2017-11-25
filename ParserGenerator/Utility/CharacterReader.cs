using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.Utility
{
    /*
        The whole idea here is to allow the reader to peek. 
        TextReader.Peek doesn't work if it's a StreamReader and the underlying stream can't seek.
        */
    public class CharacterReader : IPeekable<char>
    {
        TextReader reader;
        IPeekable<char> peekable;

        public CharacterReader(TextReader original)
        {
            this.reader = original;
            this.peekable = this.AsPeekable();
        }

        public bool HasNext()
        {
            return peekable.HasNext();
        }

        public char Peek()
        {
            return peekable.Peek();
        }

        public char Read()
        {
            return peekable.Read();
        }

        public IEnumerator<char> GetEnumerator()
        {
            return new Enumerator(reader);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public class Enumerator : IEnumerator<char>
        {
            public TextReader reader;
            public Enumerator(TextReader original)
            {
                reader = original;
            }

            char current;
            public char Current
            {
                get
                {
                    return current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return current;
                }
            }

            public void Dispose()
            {
                reader.Dispose();
            }

            public bool MoveNext()
            {
                var read = reader.Read();
                current = (char)read;
                return read != -1;
            }

            public void Reset()
            {
                // Let's not do anything here....
            }
    }
    }
}
