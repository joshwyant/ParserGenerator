using ParserGenerator;

namespace TestLanguage
{
    public class CommentToken : GrammarBase<Terminal, Nonterminal>.Token
    {
        public bool IsMultiline { get; }
        public bool IsClosedProperly { get; }
        
        public CommentToken(string lexeme, bool isMultiline = false, bool isClosedProperly = true) 
            : base(Terminal.Comment, lexeme)
        {
            IsMultiline = isMultiline;
            IsClosedProperly = isClosedProperly;
        }
    }
}
