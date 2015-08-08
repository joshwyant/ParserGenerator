using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator
{
    public abstract partial class LRkGrammar<Terminal_T, Nonterminal_T>
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
}
