using System.Collections.Generic;
using System.Linq;

namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        public class ProductionRule
        {
            public Production Production { get; }
            private readonly List<Symbol> _list;
            public IReadOnlyList<Symbol> Symbols => _list;
            public int Length => Symbols.Count;
            public bool IsAccepting { get; set; }
            public int Index { get; set; }

            internal ProductionRule(Production p, IEnumerable<Symbol> startingSymbols)
            {
                Production = p;
                _list = new SymbolList(startingSymbols);
            }

            public static ProductionRule operator/(ProductionRule r, Symbol s)
            {
                if (s != null)
                    r._list.Add(s);

                r.RecalcHash();
                return r;
            }

            public override int GetHashCode()
            {
                if (!_hash.HasValue)
                    RecalcHash();

                return _hash.Value;
            }

            private int? _hash;
            private void RecalcHash()
            {
                unchecked
                {
                    _hash = 17;
                    _hash = _hash * 23 + Production.Lhs.GetHashCode();
                    foreach (var sym in Symbols)
                        _hash = _hash * 23 + sym.GetHashCode();
                }
            }

            public override bool Equals(object obj)
            {
                return obj is ProductionRule r && r.Production.Equals(Production) && r.Symbols.SequenceEqual(Symbols);
            }

            public override string ToString()
                => $"{Production} -> {Symbols}";
        }
    }
}
