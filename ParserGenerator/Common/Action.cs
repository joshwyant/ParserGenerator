using System.Collections.Generic;
using System.Linq;

namespace ParserGenerator
{
    public class Action
    {
        public ActionType Type { get; }
        public int Number => Numbers.Single();
        public List<int> Numbers { get; } = new List<int>();
        public bool IsForkedAction => Numbers.Count > 1;

        public Action(ActionType type, int num)
        {
            Type = type;
            Numbers.Add(num);
        }
        
        public Action(ActionType type, IEnumerable<int> nums)
        {
            Type = type;
            Numbers.AddRange(nums);
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
                    return string.Join(" ", Numbers.Select(n => $"r{n}"));
                default:
                    return Type.ToString();
            }
        }
    }
}
