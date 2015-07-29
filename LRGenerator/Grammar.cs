using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LRGenerator.Terminal;
using static LRGenerator.Nonterminal;

namespace LRGenerator
{
    public abstract class Grammar
    {
        public Dictionary<Nonterminal, Production> Productions { get; } = new Dictionary<Nonterminal, Production>();
        public LR0ItemSet StartState { get; protected set; }
        public Dictionary<Tuple<LR0ItemSet, Symbol>, LR0ItemSet> Transitions { get; } = new Dictionary<Tuple<LR0ItemSet, Symbol>, LR0ItemSet>();
        protected Dictionary<Symbol, HashSet<Terminal>> First { get; set; }
        protected Dictionary<Symbol, HashSet<Terminal>> Follow { get; set; }
        protected Dictionary<Symbol, bool> Nullable { get; set; }
        protected Symbol[] Symbols { get; set; }
        public LR0ItemSetCollection States { get; set; }
        
        public Production DefineProduction(Nonterminal Lhs)
        {
            Production p;

            if (!Productions.TryGetValue(Lhs, out p))
            {
                p = new Production(Lhs);
                Productions.Add(Lhs, p);
            }

            return p;
        }
        
        public void GenerateTables()
        {
            ComputeFirstAndFollows();
            ComputeItemSetCollection();
        }

        protected abstract void ComputeFirstAndFollows();

        protected abstract LR0ItemSet Closure(LR0ItemSet items);

        protected abstract LR0ItemSet Goto(LR0ItemSet items, Symbol s);

        protected abstract void ComputeItemSetCollection();

        public LR0ItemSet[] ReduceReduceConflicts()
        {
            return States
                .Where(rs => rs.Count(s => s.Length == s.Marker) > 1)
                .Select(rs => new LR0ItemSet(rs.Where(s => s.Length == s.Marker || !s.IsKernel)))
                .Distinct()
                .ToArray();
        }

        public Tuple<LR0Item, Symbol>[] ShiftReduceConflicts()
        {
            var toReturn = new List<Tuple<LR0Item, Symbol>>();
            foreach (var s in States)
            {
                foreach (var i in s)
                {
                    if (i.Length == i.Marker
                        && !i.Rule.Production.Rules.Any(r => r.IsAccepting))
                    {
                        foreach (var sym in Follow[i.Rule.Production.Lhs])
                        {
                            var transitionKey = new Tuple<LR0ItemSet, Symbol>(s, sym);
                            if (Transitions.ContainsKey(new Tuple<LR0ItemSet, Symbol>(s, sym)))
                            {
                                toReturn.Add(new Tuple<LR0Item, Symbol>(i, sym));
                            }
                        }
                    }
                }
            }
            return toReturn.Distinct().ToArray();
            
        }
    }
}
