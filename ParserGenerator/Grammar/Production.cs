using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        public class Production
        {
            public Nonterminal_T Lhs { get; }
            public IReadOnlyList<ProductionRule> Rules { get; }

            internal Production(Nonterminal_T lhs)
            {
                Lhs = lhs;
                Rules = new List<ProductionRule>();
            }

            public static ProductionRule operator%(Production p, Symbol s)
            {
                var list = p.Rules as List<ProductionRule>;
                var r = new ProductionRule(p, s == null ? new Symbol[0] : new[] { s });
                list.Add(r);
                return r;
            }

            public override int GetHashCode()
            {
                return Lhs.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return object.ReferenceEquals(obj, this);
            }

            public override string ToString()
            {
                return Lhs.ToString();
            }
        }
    }
}
