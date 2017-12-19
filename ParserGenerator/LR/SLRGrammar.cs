using System;
using System.Collections.Generic;
using System.Linq;
using static ParserGenerator.ActionType;

namespace ParserGenerator
{
    public abstract class SLRGrammar<Terminal_T, Nonterminal_T> 
        : LRkGrammar<Terminal_T, Nonterminal_T>
        where Terminal_T : struct, IComparable, IConvertible
        where Nonterminal_T : struct, IComparable, IConvertible
    {
        protected SLRGrammar(Terminal_T unknown, Terminal_T eof, Nonterminal_T init, Nonterminal_T start)
            : base(unknown, eof, init, start)
        { }

        public LRItemSet[] ReduceReduceConflicts()
        {
            return States
                .Where(rs => rs.Count(s => s.Length == s.Marker) > 1)
                .Select(rs => new LRItemSet(rs.Where(s => s.Length == s.Marker || !s.IsKernel)))
                .Distinct()
                .ToArray();
        }

        public (LRItem item, Symbol symbol)[] ShiftReduceConflicts()
        {
            var toReturn = new List<(LRItem, Symbol)>();
            foreach (var s in States)
            {
                foreach (var i in s)
                {
                    if (i.Length == i.Marker
                        && !i.Rule.Production.Rules.Any(r => r.IsAccepting))
                    {
                        foreach (var sym in Follow[i.Rule.Production.Lhs])
                        {
                            var transitionKey = (state: s.Index, symbol: sym);
                            if (GotoSymbol.ContainsKey(transitionKey))
                            {
                                toReturn.Add((i, sym));
                            }
                        }
                    }
                }
            }
            return toReturn.Distinct().ToArray();

        }

        protected override LRItemSetCollection ComputeItemSetCollection()
        {
            return ComputeLR0ItemSetCollection();
        }

        /// <summary>
        /// Page 253, algorithm 4.46, dragon book 2nd ed.
        /// </summary>
        /// <returns></returns>
        protected override ParsingTable ComputeParseTable()
        {
            var table = new ParsingTable {StartState = States.StartState.Index};

            foreach (var state in States)
            {
                foreach (var sym in Symbols)
                {
                    if (sym.IsTerminal)
                    {
                        var key = (state.Index, sym.Terminal);

                        foreach (var item in state)
                        {
                            // Preferring shift over reduce here (though undefined for SLR)
                            if (item.Marker < item.Length && item.Rule.Symbols[item.Marker].Equals(sym))
                            {
                                if (GotoSymbol.TryGetValue((state.Index, sym), out var @goto))
                                {
                                    table.Action[key] = new Action(Shift, @goto);
                                }
                            }
                            else if (item.Length == item.Marker)
                            {
                                if (item.Rule.IsAccepting)
                                {
                                    if (sym.Terminal.CompareTo(Eof) == 0)
                                        table.Action[key] = new Action(Accept);
                                }
                                else if (Follow[item.Rule.Production.Lhs].Contains(sym.Terminal))
                                    table.Action[key] = new Action(Reduce, item.Rule.Index);

                                // else don't add, will be error by default
                            }
                        }
                    }
                    else // Nonterminal
                    {
                        if (GotoSymbol.TryGetValue((state.Index, sym), out var @goto))
                        {
                            table.Goto[(state.Index, sym.Nonterminal)] = @goto;
                        }
                    }
                }
            }

            return table;
        }

        protected override Dictionary<(int state, Symbol symbol), int> ComputeGotoLookup()
        {
            return ComputeLR0GotoLookup(States);
        }
    }
}
