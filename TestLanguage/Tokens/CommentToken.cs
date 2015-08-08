using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLanguage
{
    public class CommentToken : Grammar.Token
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
