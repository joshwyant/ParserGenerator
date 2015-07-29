using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRGenerator
{
    public class ProductionRule
    {
        public Production Production { get; }
        public IReadOnlyList<Symbol> Symbols { get; }
        public int Length { get { return Symbols.Count; } }
        public bool IsAccepting { get; set; }

        internal ProductionRule(Production p, IEnumerable<Symbol> startingSymbols)
        {
            Production = p;
            Symbols = new SymbolList(startingSymbols);
        }

        public static ProductionRule operator/(ProductionRule r, Symbol s)
        {
            var list = r.Symbols as List<Symbol>;

            if (s != null)
                list.Add(s);

            r.RecalcHash();
            return r;
        }

        public override int GetHashCode()
        {
            if (!hash.HasValue)
                RecalcHash();

            return hash.Value;
        }

        int? hash;
        private void RecalcHash()
        {
            unchecked
            {
                hash = 17;
                hash = hash * 23 + Production.Lhs.GetHashCode();
                foreach (var sym in Symbols)
                    hash = hash * 23 + sym.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            var r = obj as ProductionRule;

            return (r != null && r.Production.Equals(Production) && r.Symbols.SequenceEqual(Symbols));
        }

        public override string ToString()
            => $"{Production} -> {Symbols}";
    }
}
