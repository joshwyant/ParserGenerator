using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRGenerator
{
    public class LRkParseTable
    {
        public Dictionary<Tuple<int, Nonterminal>, int> Goto { get; } = new Dictionary<Tuple<int, Nonterminal>, int>();
        public Dictionary<Tuple<int, Terminal>, Action> Action { get; } = new Dictionary<Tuple<int, Terminal>, Action>();
        public int StartState { get; set; }
    }
}
