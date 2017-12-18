using System.Collections.Generic;

namespace ParserGenerator.Utility
{
    public interface IPeekable<T> : IEnumerable<T>
    {
        T Read();
        T Peek();
        bool HasNext();
    }
}
