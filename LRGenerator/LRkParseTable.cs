using System;
using System.Collections.Generic;
using System.IO;
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
                    bw.Write((int)action.Key.Item2);
                    bw.Write(((uint)action.Value.Type << 30) | (uint)action.Value.Number);
                }
                foreach (var @goto in Goto)
                {
                    bw.Write(@goto.Key.Item1);
                    bw.Write((int)@goto.Key.Item2);
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
                    var actionTerminal = (Terminal)br.ReadInt32();
                    var actionKey = new Tuple<int, Terminal>(actionState, actionTerminal);
                    var actionEncoded = br.ReadUInt32();
                    var actionType = (ActionType)(int)(actionEncoded >> 30);
                    var actionNumber = (int)(actionEncoded & 0x3FFFFF);
                    var action = new Action(actionType, actionNumber);
                    Action.Add(actionKey, action);
                }
                while (gotoCount-- > 0)
                {
                    var gotoState = br.ReadInt32();
                    var gotoNonterminal = (Nonterminal)br.ReadInt32();
                    var gotoKey = new Tuple<int, Nonterminal>(gotoState, gotoNonterminal);
                    var gotoRule = br.ReadInt32();
                    Goto.Add(gotoKey, gotoRule);
                }
            }
        }
    }
}
