using ParserGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static TestLanguage.Terminal;
using static TestLanguage.Nonterminal;

namespace TestLanguage
{
    public partial class Grammar
        : LALRGrammar<Terminal, Nonterminal>
    {
        public Grammar()
            : base(Terminal.Unknown, Terminal.Eof, Nonterminal.Init, Nonterminal.Start)
        {
            // Todo: Implement the grammar here
        }
    }
}
