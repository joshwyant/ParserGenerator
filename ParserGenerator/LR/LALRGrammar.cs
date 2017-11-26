using ParserGenerator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        protected LRItemSet LR1Closure(LRItemSet items)
        {
            // Initialize the return set to the item set
            LRItemSet newset = new LRItemSet(items);

            // Keep looping until no more items were added in this iteration
            bool changed;
            do
            {
                changed = false;
                var toAdd = new List<LRItem>();
                // For each item in the set
                foreach (var item in newset)
                {
                    if (item.Marker < item.Length)
                    {
                        var nextSymbol = item.Rule.Symbols[item.Marker];
                        if (!nextSymbol.IsTerminal)
                        {
                            // Get all the possible lookaheads past this symbol
                            var newLookaheads = new HashSet<Terminal_T>();
                            foreach (var lookahead in item.Lookaheads)
                            {
                                var followingSymbols = item.Rule.Symbols
                                    .Skip(item.Marker + 1)
                                    .Concat(new Symbol(lookahead).Yield());

                                newLookaheads.UnionWith(FirstOf(followingSymbols));
                            }

                            if (newLookaheads.Any())
                            {
                                // For each rule of the production past the marker for this item
                                foreach (var rule in Productions[nextSymbol.Nonterminal].Rules)
                                {
                                    // Create a new nonkernel item
                                    var newitem = new LRItem(rule, 0, newLookaheads);
                                    toAdd.Add(newitem);
                                }
                            }
                        }
                    }
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
            var newset = new LRItemSet(new LRItem[] { });

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
            var itemLookup = collection.ToDictionary(c => c);

            // Determine propagation of lookaheads, and initialze lookaheads based on spontaneous generation
            // (page 270-275, dragon book 2nd ed.)
            collection.StartState.Single().Lookaheads.Add(Eof);
            var rulePropagations = new Dictionary<Tuple<int, LRItem>, HashSet<Tuple<int, LRItem>>>();

            foreach (var itemset in collection)
            {
                foreach (var kernelitem in itemset)
                {
                    var dummyLookahead = new HashSet<Terminal_T>(Unknown.Yield());
                    var dummyItem = new LRItem(kernelitem.Rule, kernelitem.Marker, dummyLookahead);
                    var j = LR1Closure(new LRItemSet(dummyItem.Yield()));

                    foreach (var sym in Symbols)
                    {
                        var gotoKey = new Tuple<int, Symbol>(itemset.Index, sym);
                        LRItemSet gotoState;
                        int gotoidx;
                        if (gotoSymbol.TryGetValue(gotoKey, out gotoidx))
                        {
                            gotoState = collection[gotoidx];
                            foreach (var b in j.Where(bb => bb.Marker < bb.Length && bb.Rule.Symbols[bb.Marker].Equals(sym)))
                            {
                                var newItem = new LRItem(b.Rule, b.Marker + 1);
                                var gotoItem = gotoState.SingleOrDefault(i => i.Equals(newItem));

                                var itemKey = new Tuple<int, LRItem>(itemset.Index, kernelitem);

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

                        HashSet<Tuple<int, LRItem>> propagated;
                        if (rulePropagations.TryGetValue(itemKey, out propagated))
                        {
                            foreach (var key in propagated)
                            {
                                if (key.Item2.Lookaheads.TryUnionWith(item.Lookaheads))
                                    changed = true;
                            }
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
            var table = new ParsingTable();
            table.StartState = States.StartState.Index;

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
                                int @goto;
                                if (GotoSymbol.TryGetValue(new Tuple<int, Symbol>(state.Index, sym), out @goto))
                                {
                                    table.Action[key] = new Action(Shift, @goto);
                                }
                            }
                            else if (item.Length == item.Marker)
                            {
                                if (item.Rule.IsAccepting)
                                {
                                    if (sym.Terminal.CompareTo(Eof) == 0)
                                        table.Action[key] = new Action(ActionType.Accept);
                                }
                                else if (item.Lookaheads.Contains(sym.Terminal) && item.Rule.Production.Lhs.CompareTo(Init) != 0)
                                    table.Action[key] = new Action(Reduce, item.Rule.Index);

                                // else don't add, will be error by default
                            }
                        }
                    }
                    else // Nonterminal
                    {
                        int @goto;
                        if (GotoSymbol.TryGetValue(new Tuple<int, Symbol>(state.Index, sym), out @goto))
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
    }
}
