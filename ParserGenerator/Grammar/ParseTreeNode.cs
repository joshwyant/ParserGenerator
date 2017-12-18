using ParserGenerator.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        public class ParseTreeNode
        {
            public ParseTreeNode(Symbol s)
            {
                Symbol = s;
                Children = new ParseTreeNode[0];
            }

            public ParseTreeNode(Symbol s, ParseTreeNode[] children)
                : this(s)
            {
                Children = children ?? new ParseTreeNode[0];
            }

            public Symbol Symbol { get; }
            public ParseTreeNode[] Children { get; }

            public bool IsEmpty => !Symbol.IsTerminal && Children.Length == 0;

            public override string ToString()
            {
                return Children == null || Children.Length == 0
                    ? Symbol.ToString()
                    : string.Format("{0} => {1}", Symbol, string.Join(" ", Children.Select(c => c.Symbol)));
            }

            public void Print(TextWriter w)
            {
                if (Children == null || Children.Length == 0)
                {
                    w.Write(Symbol.Token?.ToString()??Symbol.ToString());
                }
                else
                {
                    for (var i = 0; i < Children.Length; i++)
                    {
                        var c = Children[i];
                        if (i != 0)
                        {
                            w.Write(" ");
                        }
                        c.Print(w);
                    }
                }
            }

            private IEnumerable<Token> FlattenToEnumerable()
            {
                if (Symbol != null && Symbol.IsTerminal)
                {
                    return Symbol.Token.AsSingletonEnumerable();
                }

                return Children.SelectMany(c => c.FlattenToEnumerable());
            }

            public IPeekable<Token> Flatten()
            {
                return FlattenToEnumerable().AsPeekable();
            }

            public ParseTreeNode Search(int start, params Symbol[] types)
            {
                if (types.Contains(Symbol))
                    return this;

                return Children
                    .Skip(start)
                    .Select(c => c.Search(0, types))
                    .FirstOrDefault(found => found != null);
            }

            public ParseTreeNode Search(Symbol type, int start = 0)
            {
                if (type.Equals(Symbol))
                    return this;

                return Children
                    .Skip(start)
                    .Select(c => c.Search(type, start))
                    .FirstOrDefault(found => found != null);
            }

            public IEnumerable<ParseTreeNode> SearchAll(Symbol type, int start = 0)
            {
                if (type.Equals(Symbol))
                {
                    yield return this;
                    yield break;
                }

                foreach (var c in Children.Skip(start))
                {
                    foreach (var found in c.SearchAll(type))
                    {
                        yield return found;
                    }
                }
            }

            public ParseTreeNode Search(params Symbol[] types) => Search(0, types);

            private int? _hash;
            public override int GetHashCode()
            {
                if (!_hash.HasValue)
                {
                    _hash = 17;
                    _hash = _hash * 23 + Symbol.GetHashCode();
                    foreach (var child in Children)
                    {
                        _hash = _hash * 23 + child.GetHashCode();
                    }
                }

                return _hash.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is ParseTreeNode s && s.Symbol == Symbol && Children.SequenceEqual(s.Children);
            }
        }

    }
}
