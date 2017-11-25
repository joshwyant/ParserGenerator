using ParserGenerator;

namespace TestLanguage
{
    public class IdentifierToken : GrammarBase<Terminal, Nonterminal>.Token
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
