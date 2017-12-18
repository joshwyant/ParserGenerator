using System.Collections.Generic;

namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        private class SymbolList : List<Symbol>
        {
            public SymbolList(IEnumerable<Symbol> symbols)
                : base(symbols)
            { }

            public override string ToString()
                => string.Join(" ", this);
        }
    }
}