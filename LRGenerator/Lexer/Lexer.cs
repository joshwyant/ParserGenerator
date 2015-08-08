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
    public abstract partial class Grammar<Terminal_T, Nonterminal_T>
    {
        public abstract class Lexer : IPeekable<Token>
        {
            protected IPeekable<char> Reader { get; }
            IPeekable<Token> peekable;

            public Lexer(Stream s)
            {
                this.Reader = new CharacterReader(new StreamReader(s));
                this.peekable = Lex().AsPeekable();
            }

            public Lexer(string s)
            {
                this.Reader = new CharacterReader(new StringReader(s));
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
