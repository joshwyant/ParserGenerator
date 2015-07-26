using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LR0Generator
{
    public class AstNode
    {
        public AstNode(Symbol s)
        {
            Symbol = s;
            Children = new AstNode[0];
        }

        public AstNode(Symbol s, AstNode[] children)
            : this(s)
        {
            Children = children;
        }

        public Symbol Symbol { get; }
        public AstNode[] Children { get; set; }

        public override string ToString()
        {
            return Children == null || Children.Length == 0
                ? Symbol.ToString()
                : string.Format("{0} => {1}", Symbol, string.Join(" ", Children.Select(c => c.Symbol)));
        }

        public void Print(TextWriter w)
        {
            if (Children == null || Children.Length == 0)
            {
                w.Write(Symbol.Token?.ToString()??Symbol.ToString());
            }
            else
            {
                foreach (var c in Children)
                    c.Print(w);
            }
        }
    }
}
