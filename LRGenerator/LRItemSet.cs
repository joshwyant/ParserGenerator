using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRGenerator
{
    public class LRItemSet : HashSet<LRItem>
    {
        public LRItemSet(IEnumerable<LRItem> items)
            : base(items)
        { }

        public int Index { get; set; } = -1;

        public IEnumerable<LRItem> Kernels { get { return this.Where(i => i.IsKernel); } }

        public void RemoveNonkernels()
        {
            RemoveWhere(i => !i.IsKernel);
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
                || Kernels.SetEquals(t.Kernels);
        }

        
        public bool Merge(IEnumerable<LRItem> set)
        {
            //if (!Equals(set))
            //    throw new InvalidOperationException();

            var added = false;

            var mylookup = this.ToDictionary(i => i, i => i);

            foreach (var item in set)
            {
                LRItem myItem = null;
                if (mylookup.TryGetValue(item, out myItem))
                {
                    if (myItem.Lookaheads.TryUnionWith(item.Lookaheads))
                        added = true;
                }
                else
                {
                    myItem = new LRItem(item.Rule, item.Marker, item.Lookaheads, item.IsKernel);
                    mylookup[myItem] = myItem;
                    this.Add(myItem);
                    added = true;
                }
            }

            return added;
        }
        
    }
}
