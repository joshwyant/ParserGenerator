namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        public class Token
        {
            public Token(Terminal_T t)
            {
                Terminal = t;
            }

            public Token(Terminal_T t, string lexeme)
            {
                Terminal = t;
                Lexeme = lexeme;
            }

            public Terminal_T Terminal { get; }
            public string Lexeme { get; }

            public override string ToString()
                => Lexeme ?? Terminal.ToString();
        }
    }
}
