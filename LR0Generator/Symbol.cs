using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LR0Generator
{
    public class Symbol
    {
        public Terminal Terminal { get; }
        public Nonterminal Nonterminal { get; }
        public Token Token { get; }
        public bool IsTerminal { get; }

        public Symbol(Token t)
            : this(t.Terminal)
        {
            Token = t;
        }

        public Symbol(Terminal t)
        {
            IsTerminal = true;
            Terminal = t;
        }

        public Symbol(Nonterminal t)
        {
            IsTerminal = false;
            Nonterminal = t;
        }

        int? hash;
        public override int GetHashCode()
        {
            if (!hash.HasValue)
            {
                hash = 17;
                hash = hash * 23 + IsTerminal.GetHashCode();
                hash = hash * 23 + Terminal.GetHashCode();
                hash = hash * 23 + Nonterminal.GetHashCode();
            }

            return hash.Value;
        }

        public override bool Equals(object obj)
        {
            var s = obj as Symbol;

            return s != null && s.IsTerminal == IsTerminal &&
                (IsTerminal ?
                    s.Terminal == Terminal : 
                    s.Nonterminal == Nonterminal);
        }

        public static implicit operator Symbol(Terminal t)
        {
            return new Symbol(t);
        }

        public static implicit operator Symbol(Nonterminal t)
        {
            return new Symbol(t);
        }

        public static implicit operator Symbol(Token t)
        {
            return new Symbol(t);
        }

        public override string ToString()
            => IsTerminal ? $"{Terminal}" : $"<{Nonterminal}>";
    }
}
