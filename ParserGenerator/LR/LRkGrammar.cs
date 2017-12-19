using ParserGenerator.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public LRItemSetCollection States => _states ?? (_states = ComputeItemSetCollection());

        public Dictionary<(int state, Symbol symbol), int> GotoSymbol => _gotoSymbol ?? (_gotoSymbol = ComputeGotoLookup());

        public ParsingTable ParseTable
        {
            get
            {
                FlattenProductions();

                return _parseTable ?? (_parseTable = ComputeParseTable());
            }
        }
        #endregion

        #region Private Fields
        private LRItemSetCollection _states;
        private ParsingTable _parseTable;
        private Dictionary<(int state, Symbol symbol), int> _gotoSymbol;
        #endregion
        
        #region Protected Internal Methods
        /// <summary>
        /// The LR0 Closure is the set of all the potential items we can expect to look for in the future at this state.
        /// 
        /// Page 245
        /// </summary>
        /// <param name="items">The items that represents a subset of the LR0 Closure to complete.</param>
        /// <returns>The completed LR0 closure.</returns>
        protected internal LRItemSet LR0Closure(LRItemSet items)
        {
            // Initialize the return set to the item set
            var newset = new LRItemSet(items);

            // Keep looping until no more items were added in this iteration
            bool changed;
            do
            {
                changed = false;
                var toAdd = new HashSet<LRItem>();
                
                // For each item in the set
                foreach (var item in newset)
                {
                    if (item.Marker >= item.Length) continue;
                    
                    var nextSymbol = item.Rule.Symbols[item.Marker];
                    if (nextSymbol.IsTerminal) continue;
                    
                    // For each rule of the production past the marker for this item
                    foreach (var rule in Productions[nextSymbol.Nonterminal].Rules)
                    {
                        // Create a new nonkernel item
                        var newitem = new LRItem(rule, 0);
                        toAdd.Add(newitem);
                    }
                }

                // Try to add the closure to the item set
                if (toAdd.Count > 0 && newset.TryUnionWith(toAdd))
                    changed = true;

            } while (changed);

            return newset;
        }
        /// <summary>
        /// Gives item sets for the given state, which are the additional states with the marker advanced
        /// past the given symbol. Represents the LR0 GOTO set for the given state and symbol, which is a new state
        /// that occurs when the marker advances over the given symbol.
        /// </summary>
        /// <param name="items">The items set which represents a state.</param>
        /// <param name="s">The symbol to advance the marker past.</param>
        /// <returns>Returns the LR0 Goto set, if any.</returns>
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
        
        /// <summary>
        /// Computes the LR0 item set collection, which is the closure of the start state, and all GOTO transitions.
        /// </summary>
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
                    // For every grammar symbol...
                    foreach (var sym in Symbols)
                    {
                        // Calculate GOTO.
                        var @goto = LR0Goto(itemset, sym);

                        // If there are any gotos...
                        if (!@goto.Any()) continue;
                        
                        // We're going to add this itemset.
                        if (states.Contains(@goto)) continue;
                        
                        toAdd.Add(@goto);
                        changed = true;
                    }
                }

                if (toAdd.Any())
                    states.AddRange(toAdd.Distinct());

            } while (changed);

            for (var i = 0; i < states.Count; i++)
                states[i].Index = i;

            return states;
        }
        
        /// <summary>
        /// Gives us the lookup table for determining the next state once we have gobbled up a symbol.
        /// </summary>
        /// <param name="collection">The precomputed list of all the item sets.</param>
        /// <returns>A collection that returns a new state given a current state and a transition symbol.</returns>
        protected internal Dictionary<(int state, Symbol symbol), int> ComputeLR0GotoLookup(LRItemSetCollection collection)
        {
            var gotos = new Dictionary<(int, Symbol), int>();

            var stateLookup = collection.ToDictionary(s => s, s => s);

            // Now compute the goto table. Iterate over all items, once.
            foreach (var itemset in collection)
            {
                foreach (var sym in Symbols)
                {
                    // Calculate GOTO dynamically.
                    var @goto = LR0Goto(itemset, sym);
                    if (!@goto.Any()) continue;
                    
                    var key = (itemset.Index, sym);
                    if (gotos.ContainsKey(key)) continue;
                    
                    // Match the dynamic goto with an actual state in our collection.
                    if (stateLookup.TryGetValue(@goto, out var existingGoto))
                    {
                        // Add the goto from state <itemset> to state <existingGoto> to the lookup for the Goto function.
                        gotos.Add(key, existingGoto.Index);
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
        protected abstract Dictionary<(int state, Symbol symbol), int> ComputeGotoLookup();
        #endregion
    }
}
