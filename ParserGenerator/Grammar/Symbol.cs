namespace ParserGenerator
{
    public abstract partial class GrammarBase<Terminal_T, Nonterminal_T>
    {
        public class Symbol
        {
            public Terminal_T Terminal { get; }
            public Nonterminal_T Nonterminal { get; }
            public Token Token { get; }
            public bool IsTerminal { get; }

            public Symbol(Token t)
                : this(t.Terminal)
            {
                Token = t;
            }

            public Symbol(Terminal_T t)
            {
                IsTerminal = true;
                Terminal = t;
            }

            public Symbol(Nonterminal_T t)
            {
                IsTerminal = false;
                Nonterminal = t;
            }

            private int? _hash;
            public override int GetHashCode()
            {
                if (!_hash.HasValue)
                {
                    _hash = 17;
                    _hash = _hash * 23 + IsTerminal.GetHashCode();
                    _hash = _hash * 23 + Terminal.GetHashCode();
                    _hash = _hash * 23 + Nonterminal.GetHashCode();
                }

                return _hash.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is Symbol s && s.IsTerminal == IsTerminal &&
                    (IsTerminal ?
                        s.Terminal.CompareTo(Terminal) == 0 : 
                        s.Nonterminal.CompareTo(Nonterminal) == 0);
            }

            public static implicit operator Symbol(Terminal_T t)
            {
                return new Symbol(t);
            }

            public static implicit operator Symbol(Nonterminal_T t)
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
}
