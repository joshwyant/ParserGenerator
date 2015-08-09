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

        public static GrammarBase<Terminal_T, Nonterminal_T>.AstNode
            Search<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.AstNode> nodes, GrammarBase<Terminal_T, Nonterminal_T>.Symbol sym, int start = 0)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return nodes.Skip(start).Select(n => n.Search(sym)).FirstOrDefault();
        }

        public static GrammarBase<Terminal_T, Nonterminal_T>.AstNode
            Search<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.AstNode> nodes, int start = 0, params GrammarBase<Terminal_T, Nonterminal_T>.Symbol[] syms)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return nodes.Skip(start).Select(n => n.Search(syms)).FirstOrDefault();
        }

        public static GrammarBase<Terminal_T, Nonterminal_T>.AstNode
            Search<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.AstNode> nodes, params GrammarBase<Terminal_T, Nonterminal_T>.Symbol[] syms)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return nodes.Search(0, syms);
        }

        public static IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.AstNode>
            SearchAll<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.AstNode> nodes, GrammarBase<Terminal_T, Nonterminal_T>.Symbol sym, int start = 0)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return nodes.Skip(start).SelectMany(n => n.SearchAll(sym));
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
