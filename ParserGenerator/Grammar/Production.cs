using ParserGenerator.Utility;
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
            List<ProductionRule> list;

            internal Production(Nonterminal_T lhs)
            {
                Lhs = lhs;
                Rules = list = new List<ProductionRule>();
            }

            public static ProductionRule operator%(Production p, Symbol s)
            {
                var r = new ProductionRule(p, s == null ? new Symbol[0] : new[] { s });
                p.list.Add(r);
                return r;
            }

            public Production Or(params Symbol[] symbols)
            {
                if (list.Count == 0)
                    throw new InvalidOperationException();

                list.Add(new ProductionRule(this, symbols));
                return this;
            }

            public Production As(params Symbol[] symbols)
            {
                if (list.Count != 0)
                    throw new InvalidOperationException();

                list.Add(new ProductionRule(this, symbols));
                return this;
            }

            public Production AsAny(params Symbol[] symbols)
            {
                if (list.Count != 0)
                    throw new InvalidOperationException();

                foreach (var symbol in symbols)
                {
                    list.Add(new ProductionRule(this, symbol.AsSingletonEnumerable()));
                }
                return this;
            }

            public Production OrAny(params Symbol[] symbols)
            {
                if (list.Count == 0)
                    throw new InvalidOperationException();

                foreach (var symbol in symbols)
                {
                    list.Add(new ProductionRule(this, symbol.AsSingletonEnumerable()));
                }
                return this;
            }

            public Production Optional()
            {
                list.Add(new ProductionRule(this, new Symbol[0]));
                return this;
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
