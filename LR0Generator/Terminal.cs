using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LR0Generator
{
    public enum Terminal
    {
        Unknown,
        LeftParen,
        RightParen,
        Ident,
        Number,
        Plus,
        PlusEquals,
        Minus,
        Star,
        Slash,
        Semicolon,
        LeftBrace,
        RightBrace,
        LeftAngle,
        RightAngle,
        Var,
        If,
        For,
        Equals,
        Eof,
    }
}
