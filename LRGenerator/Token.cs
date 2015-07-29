using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRGenerator
{
    public class Token
    {
        public Token(Terminal t)
        {
            Terminal = t;
        }

        public Token(Terminal t, string lexeme)
        {
            Terminal = t;
            Lexeme = lexeme;
        }

        public Terminal Terminal { get; }
        public string Lexeme { get; }

        public override string ToString()
            => Lexeme ?? Terminal.ToString();
    }
}
