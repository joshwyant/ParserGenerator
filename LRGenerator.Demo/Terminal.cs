using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.Demo
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
        Comma,
        LeftBrace,
        RightBrace,
        LeftAngle,
        RightAngle,
        Var,
        Int,
        Float,
        Bool,
        String,
        If,
        For,
        Equals,
        Eof,
    }
}
