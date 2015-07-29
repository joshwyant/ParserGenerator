using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRGenerator
{
    public class LR0ItemSet : HashSet<LR0Item>
    {
        public LR0ItemSet(IEnumerable<LR0Item> items)
            : base(items)
        { }

        public int State { get; set; }

        int? hash;
        public override int GetHashCode()
        {
            if (!hash.HasValue)
            {
                hash = 17;
                hash = 23 * hash + Count;
                foreach (var item in this.OrderBy(i => i.GetHashCode()))
                    hash = 23 * hash + item.GetHashCode();
            }
            return hash.Value;
        }

        public override bool Equals(object obj)
        {
            var t = obj as LR0ItemSet;
            
            return t != null && t.Count == Count && t.IsSubsetOf(this);
        }

        /*
        public void Merge(ItemSet set)
        {
            if (!this.Equals(set))
                throw new InvalidOperationException();

            var mylookup = ToDictionary(i => i, i => i);

            foreach (var item in set)
            {
                var myItem = mylookup[item];

                foreach (var lookahead in item.Lookaheads)
                    myItem.Lookaheads.Add(lookahead);
            }
        }
        */
    }
}
