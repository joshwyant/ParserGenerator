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
            if (items.IsClosed)
                return items;
            
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
        /// Time- and space-optimized closure which stores the list of nonterminals added to a closure.
        /// </summary>
        /// <param name="kernels">The kernels of an item set.</param>
        LRItemSet LR0ComputeClosureNonterminals(LRItemSet itemSet)
        {
            if (itemSet.ClosureProductions != null)
                return itemSet;
            
            // Initialize the set with the next symbol at each marker.
            var nonterminalSet = new HashSet<Nonterminal_T>(itemSet.MarkedSymbols().Nonterminals());
            
            // Create a queue to traverse the nonterminals in the rules.
            var queue = new Queue<Nonterminal_T>(nonterminalSet);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // Get the first nonterminal from rules starting with them.
                foreach (var nonterminal in Productions[current].FirstSymbols().Nonterminals())
                {
                    if (nonterminalSet.Contains(nonterminal)) continue;
                    
                    // We haven't seen this production yet. Let's add it to the closure.
                    queue.Enqueue(nonterminal);
                    nonterminalSet.Add(nonterminal);
                }
            }

            itemSet.ClosureProductions = nonterminalSet;

            return itemSet;
        }

        /// <summary>
        /// Time- and space-optimized goto which returns gotos for all relevant symbols, storing kernels only.
        /// For this function, the input state must have its closure production heads computed 
        /// (with LR0ComputeClosureNonterminals). The output states do not have closure productions
        /// </summary>
        Dictionary<Symbol, LRItemSet> LR0GotoKernels(LRItemSet itemSet)
        {
            if (itemSet.ClosureProductions == null)
                throw new InvalidOperationException();
            
            return itemSet.Kernels
                // Create new items by advancing the marker for the input kernels
                .Where(k => k.Marker < k.Length)
                .Select(k => (symbol: k.Rule.Symbols[k.Marker], item: new LRItem(k.Rule, k.Marker + 1)))
                // Create new items from the nonkernels (using closure production heads)
                // Since we are computing goto for ALL symbols, we advance the marker for all nonkernels.
                // The first item in the rule is the goto symbol, and the marker is advanced past the first item.
                // So a new goto kernel for symbol X, production P would be:
                // P -> X . yz...
                // Then, they are grouped by symbol. So one state (item set) for the nonkernels of the input would be:
                // P -> X . yz...
                // Q -> X . vw...
                // ...
                // The dictionary will contain item sets for all possible goto symbols X, Y, and Z 
                // for the given input state.
                .Concat(itemSet
                    .ClosureProductions
                    .SelectMany(n => Productions[n].Rules)
                    .Where(r => r.Length > 0)
                    .Select(r => (symbol: r.Symbols[0], item: new LRItem(r, 1))))
                // Group by symbol to create an item set per symbol.
                .GroupBy(t => t.symbol, t => t.item)
                .ToDictionary(g => g.Key, g => new LRItemSet(g));
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
        
        protected (LRItemSetCollection states, Dictionary<(int state, Symbol symbol), int> gotos)
        ComputeLR0ItemSetKernelsCollectionAndGotoLookup()
        {
            var states = new LRItemSetCollection();
            var stateLookup = new Dictionary<LRItemSet, LRItemSet>();
            var gotos = new Dictionary<(int, Symbol), int>();
            var workQueue = new Queue<LRItemSet>();

            // Start by adding the start symbol to the item set collection (a kernel item!)
            var startItem = new LRItemSet(new[] {
                new LRItem(Productions[Init].Rules.Single(), 0, isKernel: true)
            });
            LR0ComputeClosureNonterminals(startItem);
            states.StartState = startItem;
            Productions[Init].Rules.Single().IsAccepting = true;
            startItem.Index = states.Count;
            states.Add(startItem);
            stateLookup.Add(startItem, startItem);
            workQueue.Enqueue(startItem);

            // Discover goto item sets for items in the work queue. Each item set is visited only once.
            while (workQueue.Count > 0)
            {
                var itemSet = workQueue.Dequeue();

                var gotoLookup = LR0GotoKernels(itemSet);

                foreach (var symbol in gotoLookup.Keys)
                {
                    if (!stateLookup.TryGetValue(gotoLookup[symbol], out var gotoState))
                    {
                        gotoState = gotoLookup[symbol];
                        LR0ComputeClosureNonterminals(gotoState);
                        gotoState.Index = states.Count;
                        states.Add(gotoState);
                        stateLookup.Add(gotoState, gotoState);
                        workQueue.Enqueue(gotoState);
                    }
                    gotos.Add((itemSet.Index, symbol), gotoState.Index);
                }
            }
            
            return (states, gotos);
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
