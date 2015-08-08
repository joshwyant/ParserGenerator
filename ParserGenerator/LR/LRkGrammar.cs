using ParserGenerator.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator
{
    public abstract partial class LRkGrammar<Terminal_T, Nonterminal_T>
        : GrammarBase<Terminal_T, Nonterminal_T>
        where Terminal_T: struct, IComparable, IConvertible
        where Nonterminal_T: struct, IComparable, IConvertible
    {

        protected LRkGrammar(Terminal_T unknown, Terminal_T eof, Nonterminal_T init, Nonterminal_T start)
            : base(unknown, eof, init, start)
        {
        }

        #region Public Properties
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
        public ParsingTable ParseTable
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
        private LRItemSetCollection states;
        private ParsingTable parseTable;
        private Dictionary<Tuple<int, Symbol>, int> gotoSymbol;
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
        protected internal LRItemSetCollection ComputeLR0ItemSetCollection()
        {
            // Here's our big collection of item sets
            var states = new LRItemSetCollection();

            // Start by adding the start symbol to the item set collection (a kernel item!)
            var startItem = new LRItemSet(new[] {
                new LRItem(Productions[Init].Rules.Single(), 0, isKernel: true)
            });
            states.StartState = LR0Closure(startItem);
            states.Add(states.StartState);
            Productions[Init].Rules.Single().IsAccepting = true;

            // Keep looping until nothing more is added to the big collection.
            bool changed;
            do
            {
                changed = false;
                var toAdd = new List<LRItemSet>();
                
                // For each item set already in the collection...
                foreach (var itemset in states)
                {
                    var lhs = itemset.Kernels.First().Rule.Production.Lhs;
                    var marker = itemset.Kernels.First().Marker;
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
        protected internal Dictionary<Tuple<int, Symbol>, int> ComputeLR0GotoLookup(LRItemSetCollection collection)
        {
            var gotos = new Dictionary<Tuple<int, Symbol>, int>();

            var stateLookup = collection.ToDictionary(s => s, s => s);

            // Now compute the goto table. Iterate over all items, once.
            foreach (var itemset in collection)
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

        #region Protected Methods
        public override Parser GetParser(TextReader s)
        {
            return new LRParser(this, GetLexer(s));
        }
        #endregion

        #region Abstract Methods
        protected abstract LRItemSetCollection ComputeItemSetCollection();
        protected abstract ParsingTable ComputeParseTable();
        protected abstract Dictionary<Tuple<int, Symbol>, int> ComputeGotoLookup();
        #endregion
    }
}
