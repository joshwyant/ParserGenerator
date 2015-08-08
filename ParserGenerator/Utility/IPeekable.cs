﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.Utility
{
    public interface IPeekable<T> : IEnumerable<T>
    {
        T Read();
        T Peek();
        bool HasNext();
    }
}