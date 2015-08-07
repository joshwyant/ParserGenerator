using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRGenerator
{
    public enum ActionType
    {
        Shift = 0,
        Reduce = 1,
        Accept = 2,
        Error = 3,
    }
}
