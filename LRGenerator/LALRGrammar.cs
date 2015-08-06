//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static LRGenerator.Terminal;
//using static LRGenerator.Nonterminal;

//namespace LRGenerator
//{
//    public class LALRGrammar : LRkGrammar
//    {
//        protected LRItemSet Closure(LRItemSet items)
//        {
//            // Initialize the return set to the item set
//            LRItemSet newset = new LRItemSet(items);

//            // Keep looping until no more items were added in this iteration
//            bool changed;
//            do
//            {
//                changed = false;
//                var toAdd = new HashSet<LRItem>();
//                // For each item in the set
//                foreach (var item in newset)
//                {
//                    if (item.Marker < item.Length)
//                    {
//                        var nextSymbol = item.Rule.Symbols[item.Marker];
//                        if (!nextSymbol.IsTerminal)
//                        {
//                            var hashset = new HashSet<Terminal>();

//                            foreach (var lookahead in item.Lookaheads)
//                            {
//                                // For each rule of the production past the marker for this item
//                                foreach (var rule in Productions[nextSymbol.Nonterminal].Rules)
//                                {
//                                    var followingSymbols = item.Rule.Symbols.Skip(item.Marker + 1);

//                                    hashset.UnionWith(FirstOfSet(followingSymbols, lookahead));

//                                    // Create a new nonkernel item
//                                    var newitem = new LRItem(rule, 0);
//                                    toAdd.Add(newitem);
//                                }
//                            }

//                            if (hashset.Count > 0)
//                                toAdd.Add(new LRItem())
//                        }
//                    }
//                }

//                // Try to add the closure to the item set
//                if (toAdd.Count > 0 && newset.TryUnionWith(toAdd))
//                    changed = true;

//            } while (changed);

//            return newset;
//        }

//        protected LRItemSet Goto(LRItemSet items, Symbol s)
//        {
//            // Start with an empty set.
//            var newset = new LRItemSet(new LRItem[] { });

//            // For each given item
//            foreach (var item in items)
//            {
//                // If for this item we can advance the marker over the given grammar symbol,
//                //   then we can go ahead and add this new item to the set.
//                if (item.Marker < item.Length && item.Rule.Symbols[item.Marker].Equals(s))
//                    newset.Add(new LRItem(item.Rule, item.Marker + 1));
//            }

//            // "Goto" is going to be the closure of this new set.
//            return Closure(newset);
//        } 

//        LRItemSetCollection ComputeLR0ItemSetKernels()
//        {
//            var set = ComputeLR0ItemSetCollection();
//            set.RemoveNonkernels();
//            return set;
//        }

//        LRItemSet GetGotosAndLookaheads(LRItemSet kernelSet, Symbol X)
//        {
//            var newSet = new Dictionary<LRItem, LRItem>();

//            foreach (var item in kernelSet)
//            {
//                var J = Closure(new LRItemSet(new[] { new LRItem(item.Rule, item.Marker, new HashSet<Terminal>(new[] { Unknown }), true) }));

//                foreach (var j in J)
//                {
//                    if (j.Marker < j.Length && j.Rule.Symbols[j.Marker] == X)
//                        newSet.Add(j);
//                }
//            }

//            var itemSet = new LRItemSet(newSet);
//            itemSet.RemoveNonkernels();
//            return itemSet;
//        }

//        protected void ComputeItemSetCollection()
//        {
//            var lr0kernels = ComputeLR0ItemSetKernels();

//            var lr0kernelLookup = lr0kernels.ToDictionary(i => i, i => i);

//            // Initialize spontaneously generated lookaheads
//            foreach (var set in lr0kernels)
//            {
//                foreach (var item in set)
//                {

//                }
//            }

//            // Here's our big collection of item sets
//            States = new LRItemSetCollection();

//            // Start by adding the start symbol to the item set collection (a kernel item!)
//            var startItem = new LRItemSet(new[] {
//                new LRItem(Productions[Start].Rules.Single(), 0, true)
//            });
//            StartState = Closure(startItem);
//            States.Add(StartState);

//            // Keep looping until nothing more is added to the big collection.
//            bool changed;
//            do
//            {
//                changed = false;
//                // Keep track of itemsets to add (we can't modify collection in foreach!)
//                var toAdd = new LRItemSetCollection();

//                // For each item set already in the collection...
//                foreach (var itemset in States)
//                {
//                    // For every grammar symbol...
//                    foreach (var sym in Symbols)
//                    {
//                        // Calculate GOTO.
//                        var _goto = Goto(itemset, sym);

//                        // If there are any GOTOs and we haven't already added them...
//                        if (_goto.Any())
//                        {
//                            // We're going to add this itemset.
//                            if (!States.Contains(_goto))
//                                toAdd.Add(_goto);

//                            // Also add a transition.
//                            var transitionKey = new Tuple<LRItemSet, Symbol>(itemset, sym);
//                            if (!GotoLookup.ContainsKey(transitionKey))
//                            {
//                                GotoLookup.Add(transitionKey, _goto);
//                                changed = true;
//                            }

//                        }
//                    }
//                }

//                // Add these new itemsets to the collection
//                if (toAdd.Count > 0)
//                {
//                    var startIndex = States.Count;
//                    States.AddRange(toAdd);
//                    for (var i = startIndex; i < States.Count; i++)
//                        States[i].State = i;
//                }

//            } while (changed);
//        }
//    }
//}
