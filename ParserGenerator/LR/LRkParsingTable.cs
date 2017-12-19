using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ParserGenerator
{
    public abstract partial class LRkGrammar<Terminal_T, Nonterminal_T>
    {
        public class ParsingTable
        {
            public Dictionary<(int state, Nonterminal_T nonterminal), int> Goto { get; } = new Dictionary<(int, Nonterminal_T), int>();
            public Dictionary<(int state, Terminal_T terminal), Action> Action { get; } = new Dictionary<(int, Terminal_T), Action>();
            public int StartState { get; set; }

            public void Save(Stream s)
            {
                using (var bw = new BinaryWriter(s))
                {
                    bw.Write(StartState);
                    bw.Write(Action.Count);
                    bw.Write(Goto.Count);
                    foreach (var action in Action)
                    {
                        bw.Write(action.Key.state);
                        bw.Write(action.Key.terminal.ToInt32(CultureInfo.InvariantCulture));
                        bw.Write(((uint)action.Value.Type << 30) | (uint)action.Value.Numbers.Count);
                        foreach (var n in action.Value.Numbers)
                            bw.Write(n);
                    }
                    foreach (var @goto in Goto)
                    {
                        bw.Write(@goto.Key.state);
                        bw.Write(@goto.Key.nonterminal.ToInt32(CultureInfo.InvariantCulture));
                        bw.Write(@goto.Value);
                    }
                }
            }

            public void Load(Stream s)
            {
                using (var br = new BinaryReader(s))
                {
                    StartState = br.ReadInt32();
                    var actionCount = br.ReadInt32();
                    var gotoCount = br.ReadInt32();
                    while (actionCount-- > 0)
                    {
                        var actionState = br.ReadInt32();
                        var actionTerminal = (Terminal_T)(object)br.ReadInt32(); // Only way to convert to enum of that type
                        var actionKey = (actionState, actionTerminal);
                        var actionEncoded = br.ReadUInt32();
                        var actionType = (ActionType)(int)(actionEncoded >> 30);
                        var actionNumberCount = (int)(actionEncoded & 0x3FFFFF);
                        var action = new Action(actionType, Enumerable.Range(0, actionNumberCount).Select(i => br.ReadInt32()));
                        Action.Add(actionKey, action);
                    }
                    while (gotoCount-- > 0)
                    {
                        var gotoKey = (br.ReadInt32(), (Nonterminal_T)(object)br.ReadInt32());
                        var gotoRule = br.ReadInt32();
                        Goto.Add(gotoKey, gotoRule);
                    }
                }
            }
        }
    }
}
