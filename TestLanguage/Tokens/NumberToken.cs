using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLanguage
{
    public class NumberToken : Grammar.Token
    {
        public bool IsHexadecimal { get; }
        public bool IsUnsigned { get; }
        public bool IsFloating { get; }
        public char TypeSuffix { get; }
        public string NumberPart { get; }
        public bool IsTypeSpecified { get; }

        public NumberToken(string lexeme, string numberPart, char typeSuffix = '\0', bool isHexadecimal = false, bool isUnsigned = false, bool isFloating = false, bool isTypeSpecified = false)
            : base(Terminal.Number, lexeme)
        {
            IsHexadecimal = IsHexadecimal;
            IsUnsigned = isUnsigned;
            IsFloating = isFloating;
            TypeSuffix = typeSuffix;
            NumberPart = numberPart;
            IsTypeSpecified = isTypeSpecified;
        }

        public int GetIntValue()
        {
            return int.Parse(NumberPart);
        }

        public uint GetUIntValue()
        {
            return uint.Parse(NumberPart);
        }

        public long GetLongValue()
        {
            return long.Parse(NumberPart);
        }

        public ulong GetULongValue()
        {
            return ulong.Parse(NumberPart);
        }

        public float GetFloatValue()
        {
            return float.Parse(NumberPart);
        }

        public double GetDoubleValue()
        {
            return double.Parse(NumberPart);
        }

        public decimal GetDecimalValue()
        {
            return decimal.Parse(NumberPart);
        }
    }
}
