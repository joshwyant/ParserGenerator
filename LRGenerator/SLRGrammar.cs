using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LRGenerator.Terminal;
using static LRGenerator.Nonterminal;

namespace LRGenerator
{
    public class SLRGrammar : Grammar
    {
        protected override void ComputeFirstAndFollows()
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

        protected override LR0ItemSet Closure(LR0ItemSet items)
        {
            // Initialize the return set to the item set
            LR0ItemSet newset = new LR0ItemSet(items);

            // Keep looping until no more items were added in this iteration
            bool changed;
            do
            {
                changed = false;
                var toAdd = new HashSet<LR0Item>();
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
                                var newitem = new LR0Item(rule, 0);
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

        protected override LR0ItemSet Goto(LR0ItemSet items, Symbol s)
        {
            // Start with an empty set.
            var newset = new LR0ItemSet(new LR0Item[] { });

            // For each given item
            foreach (var item in items)
            {
                // If for this item we can advance the marker over the given grammar symbol,
                //   then we can go ahead and add this new item to the set.
                if (item.Marker < item.Length && item.Rule.Symbols[item.Marker].Equals(s))
                    newset.Add(new LR0Item(item.Rule, item.Marker + 1));
            }

            // "Goto" is going to be the closure of this new set.
            return Closure(newset);
        } 

        protected override void ComputeItemSetCollection()
        {
            // Here's our big collection of item sets
            States = new LR0ItemSetCollection();

            // Start by adding the start symbol to the item set collection (a kernel item!)
            var startItem = new LR0ItemSet(new[] {
                new LR0Item(Productions[Start].Rules.Single(), 0, true)
            });
            StartState = Closure(startItem);
            States.Add(StartState);

            // Keep looping until nothing more is added to the big collection.
            bool changed;
            do
            {
                changed = false;
                // Keep track of itemsets to add (we can't modify collection in foreach!)
                var toAdd = new LR0ItemSetCollection();

                // For each item set already in the collection...
                foreach (var itemset in States)
                {
                    // For every grammar symbol...
                    foreach (var sym in Symbols)
                    {
                        // Calculate GOTO.
                        var _goto = Goto(itemset, sym);

                        // If there are any GOTOs and we haven't already added them...
                        if (_goto.Any())
                        {
                            // We're going to add this itemset.
                            if (!States.Contains(_goto))
                                toAdd.Add(_goto);

                            // Also add a transition.
                            var transitionKey = new Tuple<LR0ItemSet, Symbol>(itemset, sym);
                            if (!Transitions.ContainsKey(transitionKey))
                            {
                                Transitions.Add(transitionKey, _goto);
                                changed = true;
                            }

                        }
                    }
                }

                // Add these new itemsets to the collection
                if (toAdd.Count > 0)
                {
                    var startIndex = States.Count;
                    States.AddRange(toAdd);
                    for (var i = startIndex; i < States.Count; i++)
                        States[i].State = i;
                }

            } while (changed);
        }
    }
}
