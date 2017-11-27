using ParserGenerator.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator
{
    public abstract partial class LRkGrammar<Terminal_T, Nonterminal_T>
    {
        public class LRItemSet : HashSet<LRItem>
        {
            public LRItemSet(IEnumerable<LRItem> items)
                : base(items)
            { }
            
            public LRItemSet() { }

            public int Index { get; set; } = -1;

            public IEnumerable<LRItem> Kernels { get { return this.Where(i => i.IsKernel); } }

            public void RemoveNonkernels()
            {
                // HashSet<> methods:
                RemoveWhere(i => !i.IsKernel); // Remove items with the marker on the left (except start symbol)
                TrimExcess(); // The whole point of removing the nonkernels is to save memory.
            }

            int? hash;
            public override int GetHashCode()
            {
                if (!hash.HasValue)
                {
                    hash = 17;
                    var count = 0;
                    foreach (var item in Kernels.OrderBy(i => i.GetHashCode()))
                    {
                        count++;
                        hash = 23 * hash + item.GetHashCode();
                    }
                    hash = 23 * hash + count;
                }
                return hash.Value;
            }

            public override bool Equals(object obj)
            {
                var t = obj as LRItemSet;
            
                return t != null && 
                    (ReferenceEquals(this, obj)
                    || Kernels.Count() == Enumerable.Intersect(Kernels, t.Kernels).Count());
            }

            /// <summary>
            /// Merges the given item set by adding items that don't yet exist,
            /// and updating the lookaheads for existing items.
            /// </summary>
            /// <param name="set">The item set to merge into this set.</param>
            /// <returns>Whether any items were added or any lookaheads were updated.</returns>
            public bool Merge(IEnumerable<LRItem> set)
            {
                var added = false;

                var mylookup = this.ToDictionary(i => i, i => i);

                foreach (var item in set)
                {
                    if (mylookup.TryGetValue(item, out var myItem))
                    {
                        if (myItem.Lookaheads.TryUnionWith(item.Lookaheads))
                            added = true;
                    }
                    else
                    {
                        myItem = new LRItem(item.Rule, item.Marker, item.Lookaheads, item.IsKernel);
                        mylookup.Add(myItem, myItem);
                        Add(myItem);
                        added = true;
                    }
                }

                return added;
            }
        
        }
    }
}
