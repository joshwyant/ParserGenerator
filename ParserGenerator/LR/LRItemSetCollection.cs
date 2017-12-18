using System.Collections.Generic;

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
