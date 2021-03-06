﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ParserGenerator
{
    public static class GrammarExtensions
    {
        public static IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.Token>
            Flatten<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.ParseTreeNode> nodes)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return nodes.SelectMany(n => n.Flatten());
        }

        public static GrammarBase<Terminal_T, Nonterminal_T>.ParseTreeNode
            Search<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.ParseTreeNode> nodes, GrammarBase<Terminal_T, Nonterminal_T>.Symbol sym, int start = 0)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return nodes.Skip(start).Select(n => n.Search(sym)).FirstOrDefault();
        }

        public static GrammarBase<Terminal_T, Nonterminal_T>.ParseTreeNode
            Search<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.ParseTreeNode> nodes, int start = 0, params GrammarBase<Terminal_T, Nonterminal_T>.Symbol[] syms)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return nodes.Skip(start).Select(n => n.Search(syms)).FirstOrDefault();
        }

        public static GrammarBase<Terminal_T, Nonterminal_T>.ParseTreeNode
            Search<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.ParseTreeNode> nodes, params GrammarBase<Terminal_T, Nonterminal_T>.Symbol[] syms)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return nodes.Search(0, syms);
        }

        public static IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.ParseTreeNode>
            SearchAll<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.ParseTreeNode> nodes, GrammarBase<Terminal_T, Nonterminal_T>.Symbol sym, int start = 0)
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
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.ParseTreeNode> nodes)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return nodes.Flatten().AsLexemes();
        }

        public static IEnumerable<string>
            AsLexemes<Terminal_T, Nonterminal_T>
            (this GrammarBase<Terminal_T, Nonterminal_T>.ParseTreeNode node)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return node.Flatten().AsLexemes();
        }

        public static IEnumerable<Terminal_T> 
            Terminals<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.Symbol> symbols)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return symbols.Where(s => s.IsTerminal).Select(s => s.Terminal).Distinct();
        }

        public static IEnumerable<Nonterminal_T> 
            Nonterminals<Terminal_T, Nonterminal_T>
            (this IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.Symbol> symbols)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return symbols.Where(s => !s.IsTerminal).Select(s => s.Nonterminal).Distinct();
        }
        
        public static IEnumerable<GrammarBase<Terminal_T, Nonterminal_T>.Symbol> 
            FirstSymbols<Terminal_T, Nonterminal_T>
            (this GrammarBase<Terminal_T, Nonterminal_T>.Production production)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return production.Rules.Where(r => r.Length > 0).Select(r => r.Symbols[0]).Distinct();
        }
        
        public static IEnumerable<LRkGrammar<Terminal_T, Nonterminal_T>.Symbol> 
            MarkedSymbols<Terminal_T, Nonterminal_T>
            (this IEnumerable<LRkGrammar<Terminal_T, Nonterminal_T>.LRItem> items)
            where Terminal_T : struct, IComparable, IConvertible
            where Nonterminal_T : struct, IComparable, IConvertible
        {
            return items.Where(i => i.Marker < i.Rule.Length).Select(i => i.Rule.Symbols[i.Marker]);
        }
    }
}
