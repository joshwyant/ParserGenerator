using ParserGenerator.Utility;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        public abstract class LexerBase : IPeekable<Token>
        {
            protected IPeekable<char> Reader { get; }
            private readonly IPeekable<Token> _peekable;

            public LexerBase(TextReader reader)
            {
                Reader = new CharacterReader(reader);
                _peekable = Lex().AsPeekable();
            }

            protected abstract IEnumerable<Token> Lex();

            #region IPeekable<Token>
            public Token Read()
            {
                return _peekable.Read();
            }

            public Token Peek()
            {
                return _peekable.Peek();
            }

            public bool HasNext()
            {
                return _peekable.HasNext();
            }

            public IEnumerator<Token> GetEnumerator()
            {
                return _peekable.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _peekable.GetEnumerator();
            }
            #endregion
        }
    }
}
