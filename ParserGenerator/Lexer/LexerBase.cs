using ParserGenerator.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        public abstract class LexerBase : IPeekable<Token>
        {
            protected IPeekable<char> Reader { get; }
            IPeekable<Token> peekable;

            public LexerBase(TextReader reader)
            {
                this.Reader = new CharacterReader(reader);
                this.peekable = Lex().AsPeekable();
            }

            protected abstract IEnumerable<Token> Lex();

            #region IPeekable<Token>
            public Token Read()
            {
                return peekable.Read();
            }

            public Token Peek()
            {
                return peekable.Peek();
            }

            public bool HasNext()
            {
                return peekable.HasNext();
            }

            public IEnumerator<Token> GetEnumerator()
            {
                return peekable.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return peekable.GetEnumerator();
            }
            #endregion
        }
    }
}
