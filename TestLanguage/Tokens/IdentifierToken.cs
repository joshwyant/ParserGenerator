using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLanguage
{
    public class IdentifierToken : Grammar.Token
    {
        public string Name { get; }

        public IdentifierToken(string name) : base(Terminal.Identifier, name)
        {
            Name = name;
        }

        public IdentifierToken(string name, string lexeme = null) : base(Terminal.Identifier, lexeme)
        {
            Name = name;
        }
    }
}
