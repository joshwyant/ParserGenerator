using ParserGenerator.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator
{
    public static class GrammarExtensions
    {
        public static IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.Token>
            Flatten<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.AstNode> nodes)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return nodes.SelectMany(n => n.Flatten());
        }

        public static IEnumerable<string>
            AsLexemes<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.Token> tokens)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return tokens.Select(t => t.Lexeme);
        }

        public static IEnumerable<string>
            AsLexemes<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.AstNode> nodes)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return nodes.Flatten().AsLexemes();
        }

        public static IEnumerable<string>
            AsLexemes<Terminal_T, Nonterminal_T>
            (this GrammarBase<Terminal_T, Nonterminal_T>.AstNode node)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return node.Flatten().AsLexemes();
        }
    }
}
