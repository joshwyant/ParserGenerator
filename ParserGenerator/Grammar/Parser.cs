using System.Collections.Generic;

namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        public abstract class Parser
        {
            public GrammarBase<Terminal_T, Nonterminal_T> Grammar { get; }
            public LexerBase Lexer { get; }
            public List<string> Errors { get; }  = new List<string>();
            public bool HasErrors => Errors.Count != 0;

            protected Parser(GrammarBase<Terminal_T, Nonterminal_T> g, LexerBase l)
            {
                Grammar = g;
                Lexer = l; // Lazy lex
            }

            public abstract ParseTreeNode Parse();
        }
    }
}
