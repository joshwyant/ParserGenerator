using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ParserGenerator.ActionType;
using System.Linq.Expressions;

namespace ParserGenerator
{
    public abstract partial class Grammar<Terminal_T, Nonterminal_T>
    {
        public abstract class Parser
        {
            public Grammar<Terminal_T, Nonterminal_T> Grammar { get; }
            public LexerBase Lexer { get; }
            public List<string> Errors { get; }  = new List<string>();

            public Parser(Grammar<Terminal_T, Nonterminal_T> g, LexerBase l)
            {
                Grammar = g;
                Lexer = l; // Lazy lex
            }

            public abstract AstNode Parse();
        }
    }
}
