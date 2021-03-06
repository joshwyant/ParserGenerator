﻿using ParserGenerator;

namespace TestLanguage
{
    public class StringLiteralToken : GrammarBase<Terminal, Nonterminal>.Token
    {
        public string Value { get; set; }

        public bool IsWellFormed { get; set; }

        public bool IsTerminated { get; set; }

        public StringLiteralToken(string value, string lexeme, bool isTerminated = true, bool isWellFormed = true)
            : base(Terminal.StringLiteral, lexeme)
        {
            IsTerminated = isTerminated;
            IsWellFormed = isWellFormed;
            Value = value;
        }
    }
}
