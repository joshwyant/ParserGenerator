using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using ParserGenerator;
using static TestLanguage.Terminal;
using System.Globalization;

namespace TestLanguage
{
    public partial class Grammar
    {
        public override LexerBase GetLexer(TextReader reader)
        {
            return new Lexer(reader);
        }

        public class Lexer : LexerBase
        {
            public static Dictionary<string, Terminal> ReservedWords { get; } 
                = new Dictionary<string, Terminal>
            {
                { "asm", Asm },
                { "naked", Naked },
                { "null", Null },
                { "true", True },
                { "false", False },
                { "const", Const },
                { "object", Terminal.Object },
                { "void", Terminal.Void },
                { "byte", Terminal.Byte },
                { "bool", Bool },
                { "char", Terminal.Char },
                { "string", Terminal.String },
                { "short", Short },
                { "int", Int },
                { "uint", UInt },
                { "long", Long },
                { "ulong", ULong },
                { "float", Float },
                { "double", Terminal.Double },
                { "decimal", Terminal.Decimal },
                { "using", Using },
                { "namespace", Namespace },
                { "public", Public },
                { "private", Private },
                { "protected", Protected },
                { "static", Static },
                { "class", Class },
                { "struct", Struct },
                { "for", For },
                { "while", While },
                { "do", Do },
                { "foreach", ForEach },
                { "break", Break },
                { "continue", Continue },
                { "switch", Switch },
                { "default", Default },
                { "case", Case },
                { "try", Try },
                { "catch", Catch },
                { "finally", Finally },
                { "throw", Throw },
                { "new", New },
                { "typeof", Typeof },
                { "goto", Goto },
                { "return", Return },
                { "virtual", Virtual },
                { "interface", Interface },
                { "delegate", Terminal.Delegate },
                { "if", If },
                { "else", Else },
                { "enum", Terminal.Enum },
                { "lock", Lock },
                { "var", Var }
            };

            public Lexer(TextReader reader) : base(reader) { }

            int startline;
            int startcol;
            int line;
            int col;
            StringBuilder lexemeBuilder = new StringBuilder();
            
            char read()
            {
                if (Reader.HasNext())
                {
                    col++;
                    var c = Reader.Read();
                    lexemeBuilder.Append(c);
                    return c;
                }
                else return '\xFFFF';
            }

            protected override IEnumerable<Token> Lex()
            {
                return LexWithoutTrivia();
            }

            private IEnumerable<Token> LexWithoutTrivia()
            {
                return LexAll().Where(t => 
                    t.Terminal != Terminal.Whitespace && 
                    t.Terminal != Terminal.Comment
                );
            }

            private IEnumerable<Token> LexAll()
            {
                line = 1;
                col = 1;

                // Scan away until there are no characters in the stream!
                while (Reader.HasNext())
                {
                    // Set up specific information for this token
                    startline = line;
                    startcol = col;
                    lexemeBuilder.Clear();

                    // Scan whitespace
                    while (char.IsWhiteSpace(Reader.Peek()))
                    {
                        if (!HandleNewline())
                        {
                            read();
                        }

                    }
                    if (lexemeBuilder.Length > 0)
                    {
                        yield return new Token(Whitespace, lexemeBuilder.ToString());
                        continue;
                    }

                    // Scan division and comments.
                    if (Reader.Peek() == '/')
                    {
                        read();
                        switch (Reader.Peek())
                        {
                            // Single-line comment
                            case '/':

                                while (Reader.HasNext() && Reader.Peek() != '\r' && Reader.Peek() != '\n')
                                {
                                    read();
                                }
                                yield return new CommentToken(lexemeBuilder.ToString());
                                break;

                            // Multiline comment
                            case '*':
                                read();
                                bool commentClosed = false;
                                while (Reader.HasNext())
                                {
                                    HandleNewline();

                                    // Make sure we didn't reach the end of the stream.
                                    if (!Reader.HasNext())
                                        break;

                                    // Scan through and try to find the end of the comment.
                                    var lastc = read();
                                    if (lastc == '*' && Reader.HasNext())
                                    {
                                        if (Reader.Peek() == '/')
                                        {
                                            commentClosed = true;
                                            read();
                                            break;
                                        }
                                    }
                                }
                                yield return new CommentToken
                                (
                                    lexemeBuilder.ToString(),
                                    isMultiline: true,
                                    isClosedProperly: commentClosed
                                );
                                break;

                            // Division assignment
                            case '=':
                                read();
                                yield return new Token(DivisionAssignment);
                                break;

                            // Division
                            default:
                                yield return new Token(Divide);
                                break;
                        }

                        // Scan the next token.
                        continue;
                    }

                    // Recognize identifiers and strings starting with an @ symbol (verbatim specifier).
                    var verbatim = ('@' == Reader.Peek());
                    if (verbatim)
                    {
                        // Swallow it up
                        read();

                        // Make sure it precedes a string or identifier. If not, return our invalid "Verbatim" token.
                        if (!((Reader.Peek() >= 'a' && Reader.Peek() <= 'z') || (Reader.Peek() >= 'A' && Reader.Peek() <= 'Z') || Reader.Peek() == '_' || Reader.Peek() == '\"'))
                        {
                            yield return new Token(Terminal.Unknown);
                            continue;
                        }
                    }

                    // Does it start an identifier?
                    if ((Reader.Peek() >= 'a' && Reader.Peek() <= 'z') || (Reader.Peek() >= 'A' && Reader.Peek() <= 'Z') || Reader.Peek() == '_')
                    {
                        var nameBuilder = new StringBuilder();
                        // Scan it up!
                        do
                        {
                            nameBuilder.Append(read());
                        }
                        while (Reader.HasNext() && ((Reader.Peek() >= 'a' && Reader.Peek() <= 'z') || (Reader.Peek() >= 'A' && Reader.Peek() <= 'Z') || (Reader.Peek() >= '0' && Reader.Peek() <= '9') || Reader.Peek() == '_'));


                        if (verbatim)
                        {
                            yield return new IdentifierToken(nameBuilder.ToString(), lexemeBuilder.ToString());
                        }
                        else
                        {
                            Terminal keyword;
                            var name = nameBuilder.ToString();
                            if (!ReservedWords.TryGetValue(name, out keyword))
                                yield return new IdentifierToken(name);
                            else
                                yield return new Token(keyword);
                        }
                    }
                    // Does it start a number then? (Or is it a dot?)
                    else if ((Reader.Peek() >= '0' && Reader.Peek() <= '9') || Reader.Peek() == '.')
                    {
                        // The type suffix of the number
                        char typeSuffix = (char)0;

                        bool fraction = false; // Is this number a floating point number? (Are we ready to start scanning the fractional part?)
                        bool unsigned = false;
                        bool typeSpecified = false;

                        if (Reader.Peek() == '.')
                        {
                            read();

                            // If the next character is not a number, then we just have a dot.
                            if (Reader.HasNext() || !(Reader.Peek() >= '0' && Reader.Peek() <= '9'))
                            {
                                yield return new Token(Dot);
                                continue; // Begin scanning the next token.
                            }

                            // This is a floating point number; make sure we start by scanning the fractional part.
                            fraction = true;
                        }

                        // Create string builders to keep track of the numeric part of the number.
                        StringBuilder numberPart = new StringBuilder();

                        // Now, see if we have a hexadecimal number.
                        if (!fraction && Reader.Peek() == '0')
                        {
                            read();

                            // See if the next character is an 'x'.
                            if (Reader.Peek() == 'x' || Reader.Peek() == 'X')
                            {
                                read();

                                // Now, scan all the hexadeximal digits.
                                while (Reader.HasNext() && ((Reader.Peek() >= '0' && Reader.Peek() <= '9') || (Reader.Peek() >= 'A' && Reader.Peek() <= 'F') || (Reader.Peek() >= 'a' && Reader.Peek() <= 'f')))
                                {
                                    numberPart.Append(read());
                                }

                                // See if it is a type suffix. We can have U, L, UL, or LU (ignoring case). c holds the next character.
                                for (int i = 0; i < 2; i++)
                                {
                                    if (!unsigned && (Reader.Peek() == 'u' || Reader.Peek() == 'U'))
                                    {
                                        read();
                                        unsigned = true; // We don't allow "UU"
                                    }
                                    else if (!typeSpecified && (Reader.Peek() == 'l' || Reader.Peek() == 'L'))
                                    {
                                        typeSpecified = true; // We won't allow "LL"
                                        typeSuffix = read();
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                // Return the new hexadecimal token.
                                yield return new NumberToken
                                (
                                    lexemeBuilder.ToString(),
                                    numberPart: numberPart.ToString(),
                                    isHexadecimal: true,
                                    isFloating: false,
                                    isUnsigned: unsigned,
                                    typeSuffix: typeSuffix,
                                    isTypeSpecified: typeSpecified
                                );
                                continue;
                            }
                            // If this is not a hexadecimal number but it just starts with 0, scan the rest of the integer part of the number.
                            else
                            {
                                // Add the '0' to the number part because this is not a hexadecimal number.
                                numberPart.Append('0');

                                while (Reader.HasNext() && (Reader.Peek() >= '0' && Reader.Peek() <= '9'))
                                {
                                    numberPart.Append(read());
                                }
                            }
                        }

                        // Scan the integer part of the number if we are not yet ready to scan the fractional part.
                        if (!fraction)
                        {
                            while (Reader.HasNext() && (Reader.Peek() >= '0' && Reader.Peek() <= '9'))
                            {
                                numberPart.Append(read());
                            }
                        }

                        // Signal the OK to scan the fractional part
                        if (Reader.Peek() == '.')
                        {
                            fraction = true;
                        }

                        if (fraction)
                        {
                            // Yay! We can finally scan the fractional part now.

                            // Add the dot to our number.
                            numberPart.Append(read());

                            // Scan the fractional part.
                            while (Reader.HasNext() && (Reader.Peek() >= '0' && Reader.Peek() <= '9'))
                            {
                                numberPart.Append(read());
                            }
                        }

                        // try to scan the exponent.
                        if (Reader.Peek() == 'e' || Reader.Peek() == 'E')
                        {
                            fraction = true;

                            numberPart.Append(read());

                            if (Reader.Peek() == '+' || Reader.Peek() == '-')
                            {
                                numberPart.Append(read());
                            }

                            while (Reader.HasNext() && (Reader.Peek() >= '0' && Reader.Peek() <= '9'))
                            {
                                numberPart.Append(read());
                            }
                        }

                        // Get number suffixes
                        var lowerc = char.ToLowerInvariant(Reader.Peek());
                        if (lowerc == 'f' || lowerc == 'd' || lowerc == 'm')
                        {
                            typeSpecified = true;
                            typeSuffix = read();
                            fraction = true;
                        }

                        if (!fraction)
                        {
                            // See if it is a type suffix. We can have U, L, UL, or LU (ignoring case). c holds the next character.
                            for (int i = 0; i < 2; i++)
                            {
                                if (!unsigned && (Reader.Peek() == 'u' || Reader.Peek() == 'U'))
                                {
                                    read();
                                    unsigned = true; // We don't allow "UU"
                                }
                                else if (!typeSpecified && (Reader.Peek() == 'l' || Reader.Peek() == 'L'))
                                {
                                    typeSpecified = true; // We won't allow "LL"
                                    typeSuffix = read();
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        // Return the token
                        if (fraction)
                        {
                            yield return new NumberToken
                            (
                                lexemeBuilder.ToString(),
                                numberPart: numberPart.ToString(),
                                isFloating: true,
                                isTypeSpecified: typeSpecified,
                                typeSuffix: typeSuffix
                            );
                        }
                        else
                        {
                            yield return new NumberToken
                            (
                                lexemeBuilder.ToString(),
                                numberPart: numberPart.ToString(),
                                isTypeSpecified: typeSpecified,
                                typeSuffix: typeSuffix,
                                isUnsigned: unsigned
                            );
                        }
                    }
                    // Single characters
                    else if (Reader.Peek() == '\'' || Reader.Peek() == '\"')
                    {
                        bool terminated = false;
                        bool errors = false;

                        var terminator = read();

                        StringBuilder str = new StringBuilder();

                        while (Reader.HasNext() && Reader.Peek() != terminator && (verbatim || (Reader.Peek() != '\r' && Reader.Peek() != '\n')))
                        {
                            if (verbatim)
                            {
                                if (Reader.Peek() == '\r' || Reader.Peek() == '\n')
                                {
                                    // Handle \r\n correctly by counting it as a single line ending.
                                    var cr = ('\r' == Reader.Peek());
                                    str.Append(read());
                                    if (cr && (Reader.Peek() == '\n'))
                                    {
                                        str.Append(read());
                                    }
                                    line++;
                                    col = 1;
                                }
                                else
                                {
                                    str.Append(read());
                                }
                            }
                            else
                            {
                                if (Reader.Peek() == '\\')
                                {
                                    read();
                                    if (!Reader.HasNext())
                                        errors = true;
                                    else
                                    {
                                        var c = read();
                                        switch (c)
                                        {
                                            case '\\':
                                                str.Append('\\');
                                                break;
                                            case '\"':
                                                str.Append('\"');
                                                break;
                                            case '\'':
                                                str.Append('\'');
                                                break;
                                            case '\0':
                                                str.Append('\0');
                                                break;
                                            case 'r':
                                                str.Append('\r');
                                                break;
                                            case 'n':
                                                str.Append('\n');
                                                break;
                                            case 'x':
                                            case 'u':
                                            case 'U':
                                                StringBuilder hex = new StringBuilder();
                                                while (Reader.HasNext() && ((Reader.Peek() >= '0' && Reader.Peek() <= '9') || (Reader.Peek() >= 'A' && Reader.Peek() <= 'F') || (Reader.Peek() >= 'a' && Reader.Peek() <= 'f')))
                                                {
                                                    hex.Append(read());
                                                    if (hex.Length == 8 || (c == 'u' && hex.Length == 4))
                                                        break;
                                                }
                                                if (hex.Length == 0 || (c == 'u' && hex.Length != 4) || (c == 'U' && hex.Length != 8))
                                                {
                                                    errors = true;
                                                }
                                                else
                                                {
                                                    int val;

                                                    if (int.TryParse(hex.ToString(), NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo, out val))
                                                    {
                                                        try
                                                        {
                                                            str.Append(char.ConvertFromUtf32(val));
                                                        }
                                                        catch
                                                        {
                                                            errors = true;
                                                        }
                                                    }
                                                    else
                                                        errors = true;
                                                }
                                                break;
                                            case 'a':
                                                str.Append('\a');
                                                break;
                                            case 'b':
                                                str.Append('\b');
                                                break;
                                            case 't':
                                                str.Append('\t');
                                                break;
                                            case 'f':
                                                str.Append('\f');
                                                break;
                                            case 'v':
                                                str.Append('\v');
                                                break;
                                            default:
                                                errors = true;
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    str.Append(read());
                                }
                            }
                        }

                        if (Reader.Peek() == terminator)
                        {
                            terminated = true;
                            read();
                        }

                        if (terminator == '\"')
                        {
                            yield return new StringLiteralToken
                            (
                                str.ToString(),
                                lexemeBuilder.ToString(),
                                isTerminated: terminated,
                                isWellFormed: !errors
                            );
                        }
                        else
                        {
                            yield return new CharLiteralToken
                            (
                                str.ToString(),
                                lexemeBuilder.ToString(),
                                isTerminated: terminated,
                                isWellFormed: !errors
                            );
                        }
                    }
                    // Characters that could also have an = sign (or possibly repeat)
                    else if (
                        Reader.Peek() == '!' ||
                        Reader.Peek() == '%' ||
                        Reader.Peek() == '*' ||
                        Reader.Peek() == '^' ||
                        Reader.Peek() == '&' ||
                        Reader.Peek() == '|' ||
                        Reader.Peek() == '+' ||
                        Reader.Peek() == '-' ||
                        Reader.Peek() == '=' ||
                        Reader.Peek() == '<' ||
                        Reader.Peek() == '>') // Slash is handled differently
                    {
                        var c = read();
                        var doubled = false;
                        var equalsSign = false;

                        // All the characters that can be doubled
                        if (
                            c == '&' ||
                            c == '|' ||
                            c == '+' ||
                            c == '-' ||
                            c == '=' ||
                            c == '<' ||
                            c == '>')
                        {
                            if (c == Reader.Peek())
                            {
                                read();
                                doubled = true;
                            }

                            if (Reader.Peek() == '=' && ((c == '<') || (c == '>')))
                            {
                                read();
                                if (c == '<')
                                    yield return new Token(ShiftLeftAssignment);
                                else
                                    yield return new Token(ShiftRightAssignment);
                                continue;
                            }
                        }

                        // See if an equals sign follows
                        if (!doubled && (Reader.Peek() == '='))
                        {
                            equalsSign = true;
                            read();
                        }

                        // output the correct token
                        switch (c)
                        {
                            case '&':
                                yield return new Token(doubled ? LogicalAnd : equalsSign ? AndAssignment : BitwiseAnd);
                                break;
                            case '|':
                                yield return new Token(doubled ? LogicalOr : equalsSign ? OrAssignment : BitwiseOr);
                                break;
                            case '+':
                                yield return new Token(doubled ? Increment : equalsSign ? AdditionAssignment : Add);
                                break;
                            case '-':
                                yield return new Token(doubled ? Decrement : equalsSign ? SubtractionAssignment : Subtract);
                                break;
                            case '=':
                                yield return new Token(doubled ? Equality : equalsSign ? Equality : Assignment);
                                break;
                            case '<':
                                yield return new Token(doubled ? ShiftLeft : equalsSign ? LessThanOrEqual : LessThan);
                                break;
                            case '>':
                                yield return new Token(doubled ? ShiftRight : equalsSign ? GreaterThanOrEqual : GreaterThan);
                                break;
                            case '!':
                                yield return new Token(equalsSign ? NotEqual : Not);
                                break;
                            case '%':
                                yield return new Token(equalsSign ? ModAssignment : Mod);
                                break;
                            case '*':
                                yield return new Token(equalsSign ? MultiplicationAssignment : Multiply);
                                break;
                            case '^':
                                yield return new Token(equalsSign ? XorAssignment : BitwiseXor);
                                break;
                        }
                    }
                    // End-of-file
                    else if (!Reader.HasNext())
                    {
                        yield return new Token(Terminal.Eof);
                        yield break; // That's it
                    }
                    // All other single characters
                    else
                    {
                        var c = read();
                        switch (c)
                        {
                            case '~':
                                yield return new Token(BitwiseNegate);
                                break;
                            case '(':
                                yield return new Token(LeftParenthesis);
                                break;
                            case ')':
                                yield return new Token(RightParenthesis);
                                break;
                            case '{':
                                yield return new Token(LeftCurlyBrace);
                                break;
                            case '}':
                                yield return new Token(RightCurlyBrace);
                                break;
                            case '[':
                                yield return new Token(LeftSquareBracket);
                                break;
                            case ']':
                                yield return new Token(RightSquareBracket);
                                break;
                            case ':':
                                yield return new Token(Colon);
                                break;
                            case ';':
                                yield return new Token(Semicolon);
                                break;
                            case ',':
                                yield return new Token(Comma);
                                break;
                            case '?':
                                yield return new Token(QuestionMark);
                                break;
                            default:
                                yield return new Token(Terminal.Unknown);
                                break;
                        }
                    }
                }

                yield return new Token(Terminal.Eof);
            }

            private bool HandleNewline()
            {
                bool newline = false;
                // Handle newlines properly.
                if (Reader.Peek() == '\r' || Reader.Peek() == '\n')
                {
                    newline = true;
                    // Handle \r\n correctly by counting it as a single line ending.
                    var cr = ('\r' == Reader.Peek());
                    read();
                    if (cr && (Reader.Peek() == '\n'))
                    {
                        read();
                    }
                    line++;
                    col = 1;
                }

                return newline;
            }
        }
    }
}
