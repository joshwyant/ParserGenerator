using ParserGenerator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using static ParserGenerator.ActionType;

namespace ParserGenerator
{
    public abstract class LALRGrammar<Terminal_T, Nonterminal_T> 
        : LRkGrammar<Terminal_T, Nonterminal_T>
        where Terminal_T: struct, IComparable, IConvertible
        where Nonterminal_T: struct, IComparable, IConvertible
    {
        protected LALRGrammar(Terminal_T unknown, Terminal_T eof, Nonterminal_T init, Nonterminal_T start)
            : base(unknown, eof, init, start)
        {

        }

        /// <summary>
        /// Computes the LR(1) closure, which includes lookaheads in the item sets.
        /// </summary>
        /// <param name="items">The list of items from which to complete the closure.</param>
        /// <returns>The LR(1) closure.</returns>
        protected LRItemSet LR1Closure(LRItemSet items)
        {
            // Initialize the return set to the item set
            var newset = new LRItemSet(items);

            // Keep looping until no more items were added in this iteration
            bool changed;
            do
            {
                changed = false;
                var toAdd = new List<LRItem>();
                // For each item in the set with a marker before a nonterminal
                foreach (var item in newset.Where(i => i.Marker < i.Length && !i.Rule.Symbols[i.Marker].IsTerminal))
                {
                    var nonterminal = item.Rule.Symbols[item.Marker].Nonterminal;
                    // Get all the possible lookaheads past this symbol
                    var newLookaheads = new HashSet<Terminal_T>();
                    foreach (var lookahead in item.Lookaheads)
                    {
                        var followingSymbols = item.Rule.Symbols
                            .Skip(item.Marker + 1)
                            .Concat(new Symbol(lookahead).AsSingletonEnumerable());

                        newLookaheads.UnionWith(FirstOf(followingSymbols));
                    }

                    if (!newLookaheads.Any()) continue;
                    // For each rule of the production past the marker for this item
                    toAdd.AddRange(Productions[nonterminal].Rules.Select(rule => new LRItem(rule, 0, newLookaheads)));
                }

                // Try to add the closure to the item set
                if (toAdd.Count > 0 && newset.Merge(toAdd))
                    changed = true;

            } while (changed);

            return newset;
        }

        protected LRItemSet LR1Goto(LRItemSet items, Symbol s)
        {
            // Start with an empty set.
            var newset = new LRItemSet();

            // For each given item
            foreach (var item in items)
            {
                // If for this item we can advance the marker over the given grammar symbol,
                //   then we can go ahead and add this new item to the set.
                if (item.Marker < item.Length && item.Rule.Symbols[item.Marker].Equals(s))
                    newset.Add(new LRItem(item.Rule, item.Marker + 1));
            }

            // "Goto" is going to be the closure of this new set.
            return LR1Closure(newset);
        }

        protected override LRItemSetCollection ComputeItemSetCollection()
        {
            var collection = ComputeLR0ItemSetCollection();

            // Must do this before removing the nonkernels.
            var gotoSymbol = ComputeLR0GotoLookup(collection);

            collection.RemoveNonkernels();

            // Determine propagation of lookaheads, and initialze lookaheads based on spontaneous generation
            // (page 270-275, dragon book 2nd ed.)
            collection.StartState.Single().Lookaheads.Add(Eof);
            var rulePropagations = new Dictionary<Tuple<int, LRItem>, HashSet<Tuple<int, LRItem>>>();

            // For all states
            foreach (var itemset in collection)
            {
                // For every kernel item
                foreach (var kernelitem in itemset)
                {
                    var itemKey = new Tuple<int, LRItem>(itemset.Index, kernelitem);
                    
                    // Create an item set with a dummy lookahead.
                    // This dummy item set is based on the current state (current item set).
                    var dummyLookahead = Unknown.AsSingletonEnumerable();
                    var dummyItem = new LRItem(kernelitem.Rule, kernelitem.Marker, dummyLookahead);
                    var dummyItemSet = new LRItemSet(dummyItem.AsSingletonEnumerable());
                    var j = LR1Closure(dummyItemSet);

                    // For every symbol
                    foreach (var sym in Symbols)
                    {
                        // What is the goto key for this state and transition symbol?
                        var gotoKey = new Tuple<int, Symbol>(itemset.Index, sym);
                        
                        // Is there a goto transition for this state and transition symbol?
                        if (!gotoSymbol.TryGetValue(gotoKey, out var gotoidx)) continue;
                        
                        // What would be the next state?
                        var gotoState = collection[gotoidx];
                        
                        // Find the items in the dummy set with the marker before this symbol.
                        foreach (var b in j.Where(bb => bb.Marker < bb.Length && bb.Rule.Symbols[bb.Marker].Equals(sym)))
                        {
                            // Get the item corresponding to the goto state with the marker advanced over the current symbol.
                            var gotoItem = gotoState.Single(i => i.Marker == b.Marker + 1 && ReferenceEquals(i.Rule, b.Rule));

                            // Note if lookaheads are propagated to the next item
                            if (b.Lookaheads.Any(l => l.CompareTo(Unknown) == 0))
                            {
                                if (!rulePropagations.ContainsKey(itemKey))
                                    rulePropagations[itemKey] = new HashSet<Tuple<int, LRItem>>();

                                rulePropagations[itemKey].Add(new Tuple<int, LRItem>(gotoState.Index, gotoItem));
                            }
                                    
                            gotoItem.Lookaheads.UnionWith(
                                b.Lookaheads.Where(l => l.CompareTo(Unknown) != 0)
                            );
                        }
                    }
                }
            }

            bool changed;
            do
            {
                changed = false;

                foreach (var state in collection)
                {
                    foreach (var item in state)
                    {
                        var itemKey = new Tuple<int, LRItem>(state.Index, item);

                        if (!rulePropagations.TryGetValue(itemKey, out var propagated)) continue;
                        foreach (var key in propagated)
                        {
                            if (key.Item2.Lookaheads.TryUnionWith(item.Lookaheads))
                                changed = true;
                        }
                    }
                }
            } while (changed);

            // Close all the kernels
            for (var i = 0; i < collection.Count; i++)
            {
                collection[i] = LR1Closure(collection[i]);
                collection[i].Index = i;
            }

            return collection;
        }

        protected override ParsingTable ComputeParseTable()
        {
            var table = new ParsingTable {StartState = States.StartState.Index};

            foreach (var state in States)
            {
                foreach (var sym in Symbols)
                {
                    if (sym.IsTerminal)
                    {
                        var key = new Tuple<int, Terminal_T>(state.Index, sym.Terminal);

                        foreach (var item in state)
                        {
                            if (item.Marker < item.Length && item.Rule.Symbols[item.Marker].Equals(sym))
                            {
                                if (GotoSymbol.TryGetValue(new Tuple<int, Symbol>(state.Index, sym), out var @goto))
                                {
                                    table.Action[key] = new Action(Shift, @goto);
                                }
                            }
                            else if (item.Length == item.Marker)
                            {
                                if (item.Rule.IsAccepting)
                                {
                                    if (sym.Terminal.CompareTo(Eof) == 0)
                                        table.Action[key] = new Action(Accept);
                                }
                                else if (item.Lookaheads.Contains(sym.Terminal) &&
                                         item.Rule.Production.Lhs.CompareTo(Init) != 0)
                                {
                                    if (table.Action.TryGetValue(key, out var action))
                                    {
                                        // Reduce-reduce conflict.
                                        // Add this rule to the list of rules for this action.
                                        // A GLR parser will try both rules in parallel.
                                        action.Numbers.Add(item.Rule.Index);
                                    }
                                    else
                                    {
                                        table.Action[key] = new Action(Reduce, item.Rule.Index);
                                    }
                                }

                                // else don't add, will be error by default
                            }
                        }
                    }
                    else // Nonterminal
                    {
                        if (GotoSymbol.TryGetValue(new Tuple<int, Symbol>(state.Index, sym), out var @goto))
                        {
                            table.Goto[new Tuple<int, Nonterminal_T>(state.Index, sym.Nonterminal)] = @goto;
                        }
                    }
                }
            }

            return table;
        }

        protected override Dictionary<Tuple<int, Symbol>, int> ComputeGotoLookup()
        {
            var gotos = new Dictionary<Tuple<int, Symbol>, int>();

            var stateLookup = States.ToDictionary(s => s, s => s);

            // Now compute the goto table. Iterate over all items, once.
            foreach (var itemset in States)
            {
                foreach (var sym in Symbols)
                {
                    // Calculate GOTO dynamically.
                    var @goto = LR1Goto(itemset, sym);

                    // If there are any gotos...
                    if (!@goto.Any()) continue;
                    var key = new Tuple<int, Symbol>(itemset.Index, sym);

                    if (gotos.ContainsKey(key)) continue;
                    // Match the dynamic goto with an actual state in our collection.

                    if (stateLookup.TryGetValue(@goto, out var existingGoto))
                    {
                        // Add the goto from state <itemset> to state <existingGoto> to the lookup for the Goto function.
                        gotos[key] = existingGoto.Index;
                    }
                }
            }

            return gotos;
        }
    }
}
