using System.Collections.Generic;
using System.Linq;

namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        internal class SymbolList : List<Symbol>
        {
            public SymbolList(IEnumerable<Symbol> symbols)
                : base(symbols)
            { }

            public override string ToString()
                => string.Join(" ", this);
        }
    }
}