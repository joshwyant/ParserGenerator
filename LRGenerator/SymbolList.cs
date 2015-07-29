using System.Collections.Generic;
using System.Linq;

namespace LRGenerator
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