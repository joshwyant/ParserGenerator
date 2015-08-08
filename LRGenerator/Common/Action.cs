using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator
{
    public class Action
    {
        public ActionType Type { get; }
        public int Number { get; }

        public Action(ActionType type, int num)
        {
            Type = type;
            Number = num;
        }

        public Action(ActionType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ActionType.Shift:
                    return $"s{Number}";
                case ActionType.Reduce:
                    return $"r{Number}";
                default:
                    return Type.ToString();
            }
        }
    }
}
