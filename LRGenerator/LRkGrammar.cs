using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LRGenerator.Terminal;
using static LRGenerator.Nonterminal;

namespace LRGenerator
{
    public abstract class LRkGrammar
    {
        #region Public Properties
        public Dictionary<Nonterminal, Production> Productions { get; } = new Dictionary<Nonterminal, Production>();
        public List<ProductionRule> IndexedProductions
        {
            get
            {
                if (indexedProductions == null)
                    FlattenProductions();

                return indexedProductions;
            }
        }
        public LRItemSetCollection States
        {
            get
            {
                if (states == null)
                    states = ComputeItemSetCollection();

                return states;
            }
        }
        public Dictionary<Tuple<int, Symbol>, int> GotoSymbol
        {
            get
            {
                if (gotoSymbol == null)
                    gotoSymbol = ComputeGotoLookup();

                return gotoSymbol;
            }
        }
        public LRkParseTable ParseTable
        {
            get
            {
                FlattenProductions();

                if (parseTable == null)
                    parseTable = ComputeParseTable();

                return parseTable;
            }
        }
        #endregion

        #region Private Fields
        private Dictionary<Symbol, HashSet<Terminal>> first;
        private Dictionary<Symbol, HashSet<Terminal>> follow;
        private Dictionary<Symbol, bool> nullable;
        private Symbol[] symbols;
        private LRItemSetCollection states;
        private List<ProductionRule> indexedProductions;
        private LRkParseTable parseTable;
        private Dictionary<Tuple<int, Symbol>, int> gotoSymbol;
        #endregion

        #region Protected Properties
        protected Dictionary<Symbol, HashSet<Terminal>> First
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
        protected Dictionary<Symbol, HashSet<Terminal>> Follow
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
        #endregion

        #region Protected Internal Methods
        protected internal LRItemSet LR0Closure(LRItemSet items)
        {
            // Initialize the return set to the item set
            LRItemSet newset = new LRItemSet(items);

            // Keep looping until no more items were added in this iteration
            bool changed;
            do
            {
                changed = false;
                var toAdd = new HashSet<LRItem>();
                // For each item in the set
                foreach (var item in newset)
                {
                    if (item.Marker < item.Length)
                    {
                        var nextSymbol = item.Rule.Symbols[item.Marker];
                        if (!nextSymbol.IsTerminal)
                        {
                            // For each rule of the production past the marker for this item
                            foreach (var rule in Productions[nextSymbol.Nonterminal].Rules)
                            {
                                // Create a new nonkernel item
                                var newitem = new LRItem(rule, 0);
                                toAdd.Add(newitem);
                            }
                        }
                    }
                }

                // Try to add the closure to the item set
                if (toAdd.Count > 0 && newset.TryUnionWith(toAdd))
                    changed = true;

            } while (changed);

            return newset;
        }
        protected internal LRItemSet LR0Goto(LRItemSet items, Symbol s)
        {
            return LR0Closure(
                new LRItemSet(
                    items
                    .Where(i => i.Marker < i.Length && i.Rule.Symbols[i.Marker].Equals(s))
                    .Select(i => new LRItem(i.Rule, i.Marker + 1))
                )
            );
        }
        protected internal IEnumerable<Terminal> FirstOf(IEnumerable<Symbol> symbols)
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
        protected internal LRItemSetCollection ComputeLR0ItemSetCollection()
        {
            // Here's our big collection of item sets
            var states = new LRItemSetCollection();

            // Start by adding the start symbol to the item set collection (a kernel item!)
            var startItem = new LRItemSet(new[] {
                new LRItem(Productions[Start].Rules.Single(), 0, true)
            });
            states.StartState = LR0Closure(startItem);
            states.Add(states.StartState);

            // Keep looping until nothing more is added to the big collection.
            bool changed;
            do
            {
                changed = false;
                var toAdd = new List<LRItemSet>();
                
                // For each item set already in the collection...
                foreach (var itemset in states)
                {
                    // For every grammar symbol...
                    foreach (var sym in Symbols)
                    {
                        // Calculate GOTO.
                        var @goto = LR0Goto(itemset, sym);

                        // If there are any gotos...
                        if (@goto.Any())
                        {
                            // We're going to add this itemset.
                            if (!states.Contains(@goto))
                            {
                                toAdd.Add(@goto);
                                changed = true;
                            }
                        }
                    }
                }

                if (toAdd.Any())
                    states.AddRange(toAdd.Distinct());

            } while (changed);

            for (var i = 0; i < states.Count; i++)
                states[i].Index = i;

            return states;
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
        protected internal Dictionary<Tuple<int, Symbol>, int> ComputeLR0GotoLookup()
        {
            var gotos = new Dictionary<Tuple<int, Symbol>, int>();

            var stateLookup = States.ToDictionary(s => s, s => s);

            // Now compute the goto table. Iterate over all items, once.
            foreach (var itemset in States)
            {
                foreach (var sym in Symbols)
                {
                    // Calculate GOTO dynamically.
                    var @goto = LR0Goto(itemset, sym);

                    // If there are any gotos...
                    if (@goto.Any())
                    {
                        var key = new Tuple<int, Symbol>(itemset.Index, sym);

                        if (!gotos.ContainsKey(key))
                        {
                            // Match the dynamic goto with an actual state in our collection.
                            LRItemSet existingGoto = null;

                            if (stateLookup.TryGetValue(@goto, out existingGoto))
                            {
                                // Add the goto from state <itemset> to state <existingGoto> to the lookup for the Goto function.
                                gotos[key] = existingGoto.Index;
                            }

                        }
                    }
                }
            }

            return gotos;
        }
        #endregion

        #region Abstract Methods
        protected abstract LRItemSetCollection ComputeItemSetCollection();
        protected abstract LRkParseTable ComputeParseTable();
        protected abstract Dictionary<Tuple<int, Symbol>, int> ComputeGotoLookup();
        #endregion

        #region Private Methods
        private void ComputeFirstAndFollows()
        {
            // Get terminals and nonterminals from enumerations
            var terminals = (Terminal[])Enum.GetValues(typeof(Terminal));
            var nonterminals = (Nonterminal[])Enum.GetValues(typeof(Nonterminal));

            // Pull together terminals and nonterminals
            Symbols = new Symbol[terminals.Length + nonterminals.Length];

            terminals.Select(t => new Symbol(t))
                .Concat(nonterminals.Select(n => new Symbol(n)))
                .CopyToArray(Symbols);

            // Initialize First and Follow sets, and Nullable set which is used to compute them.
            First = Symbols.ToDictionary(k => k, v => new HashSet<Terminal>());
            Follow = Symbols.ToDictionary(k => k, v => new HashSet<Terminal>());
            Nullable = Symbols.ToDictionary(k => k, v => false);

            // Set the FIRST sets of terminals to themselves
            foreach (var k in Symbols.Where(tnt => tnt.IsTerminal))
                First[k].Add(k.Terminal);

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
