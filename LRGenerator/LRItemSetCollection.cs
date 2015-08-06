using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRGenerator
{
    public class LRItemSetCollection : List<LRItemSet>
    {
        public LRItemSet StartState { get; set; }

        public void RemoveNonkernels()
        {
            for (var i = 0; i < Count; i++)
            {
                this[i].RemoveNonkernels();
            }
        }
    }
}
