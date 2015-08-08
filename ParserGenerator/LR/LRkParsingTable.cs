using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator
{
    public abstract partial class LRkGrammar<Terminal_T, Nonterminal_T>
    {
        public class ParsingTable
        {
            public Dictionary<Tuple<int, Nonterminal_T>, int> Goto { get; } = new Dictionary<Tuple<int, Nonterminal_T>, int>();
            public Dictionary<Tuple<int, Terminal_T>, Action> Action { get; } = new Dictionary<Tuple<int, Terminal_T>, Action>();
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
                        bw.Write(action.Key.Item1);
                        bw.Write(action.Key.Item2.ToInt32(CultureInfo.InvariantCulture));
                        bw.Write(((uint)action.Value.Type << 30) | (uint)action.Value.Number);
                    }
                    foreach (var @goto in Goto)
                    {
                        bw.Write(@goto.Key.Item1);
                        bw.Write(@goto.Key.Item2.ToInt32(CultureInfo.InvariantCulture));
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
                        var actionKey = new Tuple<int, Terminal_T>(actionState, actionTerminal);
                        var actionEncoded = br.ReadUInt32();
                        var actionType = (ActionType)(int)(actionEncoded >> 30);
                        var actionNumber = (int)(actionEncoded & 0x3FFFFF);
                        var action = new Action(actionType, actionNumber);
                        Action.Add(actionKey, action);
                    }
                    while (gotoCount-- > 0)
                    {
                        var gotoState = br.ReadInt32();
                        var gotoNonterminal = (Nonterminal_T)(object)br.ReadInt32();
                        var gotoKey = new Tuple<int, Nonterminal_T>(gotoState, gotoNonterminal);
                        var gotoRule = br.ReadInt32();
                        Goto.Add(gotoKey, gotoRule);
                    }
                }
            }
        }
    }
}
