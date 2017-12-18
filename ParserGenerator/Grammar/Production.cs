using ParserGenerator.Utility;
using System;
using System.Collections.Generic;

namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        public class Production
        {
            public Nonterminal_T Lhs { get; }
            public List<ProductionRule> Rules { get; }
            private readonly List<ProductionRule> _list;

            internal Production(Nonterminal_T lhs)
            {
                Lhs = lhs;
                Rules = _list = new List<ProductionRule>();
            }

            public static ProductionRule operator%(Production p, Symbol s)
            {
                var r = new ProductionRule(p, s == null ? new Symbol[0] : new[] { s });
                p._list.Add(r);
                return r;
            }

            public Production Or(params Symbol[] symbols)
            {
                if (_list.Count == 0)
                    throw new InvalidOperationException();

                _list.Add(new ProductionRule(this, symbols));
                return this;
            }

            public Production As(params Symbol[] symbols)
            {
                if (_list.Count != 0)
                    throw new InvalidOperationException();

                _list.Add(new ProductionRule(this, symbols));
                return this;
            }

            public Production AsAny(params Symbol[] symbols)
            {
                if (_list.Count != 0)
                    throw new InvalidOperationException();

                foreach (var symbol in symbols)
                {
                    _list.Add(new ProductionRule(this, symbol.AsSingletonEnumerable()));
                }
                return this;
            }

            public Production OrAny(params Symbol[] symbols)
            {
                if (_list.Count == 0)
                    throw new InvalidOperationException();

                foreach (var symbol in symbols)
                {
                    _list.Add(new ProductionRule(this, symbol.AsSingletonEnumerable()));
                }
                return this;
            }

            public Production Optional()
            {
                _list.Add(new ProductionRule(this, new Symbol[0]));
                return this;
            }

            public override int GetHashCode()
            {
                return Lhs.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return ReferenceEquals(obj, this);
            }

            public override string ToString()
            {
                return Lhs.ToString();
            }
        }
    }
}
