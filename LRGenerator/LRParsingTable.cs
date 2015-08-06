//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection.Emit;
//using System.Text;
//using System.Threading.Tasks;
//using static LRGenerator.ActionType;

//namespace LRGenerator
//{
//    public class LRParsingTable
//    {
//        public Dictionary<Tuple<int, int>, Action> Action { get; } = new Dictionary<Tuple<int, int>, Action>();
//        public Dictionary<Tuple<int, int>, int> Goto { get; } = new Dictionary<Tuple<int, int>, int>();
//        public int NumberOfStates { get; set; }
//        public int AcceptingState { get; set; }
//        public int EofToken { get; set; }

//        public void Compile(ILGenerator il)
//        {
//            var stateLabels = new Label[NumberOfStates];

//            Enumerable.Range(0, NumberOfStates).Each(i => 
//                stateLabels[i] = il.DefineLabel()
//            );

//            il.

//            for (int i = 0; i < )
//        }

//        public void Save(Stream s)
//        {
//            using (var bw = new BinaryWriter(s))
//            {

//            }
//        }

//        public void Load(Stream s)
//        {
//            using (var br = new BinaryReader(s))
//            {
//                NumberOfStates = br.ReadInt32();

//                AcceptingState = br.ReadInt32();
//                EofToken = br.ReadInt32();
//                Action.Add(new Tuple<int, int>(AcceptingState, EofToken), new Action(Accept));

//                for (var i = 0; i < NumberOfStates; i++)
//                {
//                    var numActions = br.ReadInt32();
//                    while (numActions-- > 0)
//                    {
//                        var t = br.ReadUInt32();
//                        var isReduce = (t | 0x80000000) != 0;
//                        var state = (int)(t & 0x7fffffff);
//                        Action.Add(new Tuple<int, int>(i, ), new Action(isReduce ? Reduce : Shift, state));
//                    }
//                    var numReduce = br.ReadInt32();
//                    while (numReduce-- > 0)
//                    {

//                    }
//                }
//            }
//        }
//    }
//}
