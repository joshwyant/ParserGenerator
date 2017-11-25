using ParserGenerator;

namespace TestLanguage
{
    public class CharLiteralToken : GrammarBase<Terminal, Nonterminal>.Token
    {
        public string Value { get; set; }

        public bool IsWellFormed { get; set; }

        public bool IsTerminated { get; set; }

        public CharLiteralToken(string value, string lexeme, bool isTerminated = true, bool isWellFormed = true)
            : base(Terminal.CharLiteral, lexeme)
        {
            IsTerminated = isTerminated;
            IsWellFormed = isWellFormed;
            Value = value;
        }
    }
}
