using ParserGenerator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator
{
    public abstract partial class Grammar<Terminal_T, Nonterminal_T>
        where Terminal_T: struct, IComparable, IConvertible
        where Nonterminal_T: struct, IComparable, IConvertible
    {
        public Terminal_T Unknown { get; }
        public Terminal_T Eof { get; }
        public Nonterminal_T Init { get; }
        public Nonterminal_T Start { get; }

        protected Grammar(Terminal_T unknown, Terminal_T eof, Nonterminal_T init, Nonterminal_T start)
        {
            Unknown = unknown;
            Eof = eof;
            Init = init;
            Start = start;

            var initProduction = DefineProduction(Init);
            var initRules = initProduction.Rules as List<ProductionRule>;
            initRules.Add(new ProductionRule(initProduction, new Symbol(Start).Yield()));
        }

        #region Public Properties
        public Dictionary<Nonterminal_T, Production> Productions { get; } = new Dictionary<Nonterminal_T, Production>();
        public List<ProductionRule> IndexedProductions
        {
            get
            {
                if (indexedProductions == null)
                    FlattenProductions();

                return indexedProductions;
            }
        }
        #endregion

        #region Private Fields
        private Dictionary<Symbol, HashSet<Terminal_T>> first;
        private Dictionary<Symbol, HashSet<Terminal_T>> follow;
        private Dictionary<Symbol, bool> nullable;
        private Symbol[] symbols;
        private List<ProductionRule> indexedProductions;
        #endregion

        #region Protected Properties
        protected Dictionary<Symbol, HashSet<Terminal_T>> First
        {
            get
            {
                if (first == null)
                    ComputeFirstAndFollows();

                return first;
            }
            private set
            {
                first = value;
            }
        }
        protected Dictionary<Symbol, HashSet<Terminal_T>> Follow
        {
            get
            {
                if (follow == null)
                    ComputeFirstAndFollows();

                return follow;
            }
            private set
            {
                follow = value;
            }
        }
        protected Dictionary<Symbol, bool> Nullable
        {
            get
            {
                if (nullable == null)
                    ComputeFirstAndFollows();

                return nullable;
            }
            private set
            {
                nullable = value;
            }
        }
        protected Symbol[] Symbols
        {
            get
            {
                if (symbols == null)
                    ComputeFirstAndFollows();

                return symbols;
            }
            private set
            {
                symbols = value;
            }
        }
        #endregion

        #region Public Methods
        public Production DefineProduction(Nonterminal_T Lhs)
        {
            Production p;

            if (!Productions.TryGetValue(Lhs, out p))
            {
                p = new Production(Lhs);
                Productions.Add(Lhs, p);
            }

            return p;
        }
        #endregion

        #region Protected Internal Methods
        protected internal IEnumerable<Terminal_T> FirstOf(IEnumerable<Symbol> symbols)
        {
            bool prevWasNullable = true;

            return symbols.TakeWhile(s =>
            {
                var takeThisSymbol = prevWasNullable;

                prevWasNullable = Nullable[s];

                return takeThisSymbol;
            })
            .SelectMany(s => First[s])
            .Distinct();
        }
        protected internal void FlattenProductions()
        {
            if (indexedProductions != null)
                return;

            indexedProductions = new List<ProductionRule>();

            foreach (var production in Productions.Values)
            {
                var index = indexedProductions.Count;

                indexedProductions.AddRange(production.Rules);

                for (var i = index; i < indexedProductions.Count; i++)
                {
                    indexedProductions[i].Index = i;
                }
            }
        }
        #endregion
        
        #region Private Methods
        private void ComputeFirstAndFollows()
        {
            // Get terminals and nonterminals from enumerations
            var terminals = (Terminal_T[])Enum.GetValues(typeof(Terminal_T));
            var nonterminals = (Nonterminal_T[])Enum.GetValues(typeof(Nonterminal_T));

            // Pull together terminals and nonterminals
            Symbols = new Symbol[terminals.Length + nonterminals.Length];

            terminals.Select(t => new Symbol(t))
                .Concat(nonterminals.Select(n => new Symbol(n)))
                .CopyToArray(Symbols);

            // Initialize First and Follow sets, and Nullable set which is used to compute them.
            First = Symbols.ToDictionary(k => k, v => new HashSet<Terminal_T>());
            Follow = Symbols.ToDictionary(k => k, v => new HashSet<Terminal_T>());
            Nullable = Symbols.ToDictionary(k => k, v => false);

            // Set the FIRST sets of terminals to themselves
            foreach (var k in Symbols.Where(tnt => tnt.IsTerminal))
                First[k].Add(k.Terminal);

            // Add Eof to Follows[Start]
            Follow[Start].Add(Eof);

            bool changed;

            // Compute First and Follow sets iteratively, looping until they are unchanged
            do
            {
                changed = false;

                // For each production
                foreach (var p in Productions.Values)
                {
                    // let X = lhs of production
                    var X = new Symbol(p.Lhs);

                    // For each rule in production
                    foreach (var r in p.Rules)
                    {
                        // If the rule is empty or all symbols are nullable,
                        // then Nullable[X] = true
                        if (r.Length == 0 || r.Symbols.All(s => Nullable[s]))
                        {
                            if (!Nullable[X])
                                changed = true;

                            Nullable[X] = true;
                        }

                        // Let Y[i..k] = rhs of rule of production
                        var Yik = r.Symbols;

                        // For each symbol in Y
                        for (var i = 0; i < Yik.Count; i++)
                        {
                            var Yi = r.Symbols[i];

                            // If everything in Y following i is nullable,
                            // then First[X] += First[Y[i]]
                            if (i == 0 || Yik.Take(i).All(sym => Nullable[sym]))
                            {
                                if (First[X].TryUnionWith(First[Yi]))
                                    changed = true;
                            }

                            // If everything in Y after i+1 is nullable,
                            // then Follow[Y[i]] += Follow[X]
                            if (i + 1 == Yik.Count || Yik.Skip(i + 1).All(sym => Nullable[sym]))
                            {
                                if (Follow[Yi].TryUnionWith(Follow[X]))
                                    changed = true;
                            }

                            // For each symbol in Y after Y[i]
                            // ...
                            // If everything in Y inbetween i and j are nullable,
                            // then Follow[Y[i]] += First[Y[j]]
                            for (var j = i + 1; j < Yik.Count; j++)
                            {
                                if (i + 1 == j || Yik.Skip(i + 1).Take(j - i - 1).All(sym => Nullable[sym]))
                                {
                                    if (Follow[Yi].TryUnionWith(First[Yik[j]]))
                                        changed = true;
                                }
                            }
                        }
                    }
                }
            } while (changed);
        }
        #endregion
    }
}
