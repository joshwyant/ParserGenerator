using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LR0Generator.Terminal;

namespace LR0Generator
{
    public class Lexer
    {
        const char Eof = '\xFFFF';

        public TextReader Reader { get; }

        public Lexer(TextReader reader)
        {
            Reader = reader;
        }

        char? peek;
        char peekc() { return (peek ?? (peek = (char)Reader.Peek())).Value; }
        char readc() { peek = null; return (char)Reader.Read(); }

        Dictionary<string, Terminal> ReservedWords { get; } 
            = new Dictionary<string, Terminal>
        {
            { "if", If }, { "for", For }, { "var", Var }, { "int", Int }, { "float", Float },
            { "bool", Bool }, { "string", Terminal.String }
        };

        Token peekToken;
        public Token Peek()
        {
            if (peekToken == null)
                peekToken = Read();

            return peekToken;
        }

        IEnumerator<Token> enumerator;
        public Token Read()
        {
            var token = peekToken;

            if (token == null)
            {
                if (enumerator == null)
                    enumerator = Lex().GetEnumerator();

                enumerator.MoveNext();
                token = enumerator.Current;
            }

            peekToken = null;

            return token;
        }

        public IEnumerable<Token> Lex()
        {
            while (peekc() != Eof)
            {
                while (char.IsWhiteSpace(peekc()))
                {
                    readc();
                    continue;
                }

                if (peekc() == Eof) break;

                var sb = new StringBuilder();
                
                var c = peekc();

                if (char.IsLetter(c) || c == '_')
                {
                    while (char.IsLetterOrDigit(peekc()) || peekc() == '_')
                        sb.Append(readc());

                    var ident = sb.ToString();

                    Terminal _out;
                    if (ReservedWords.TryGetValue(ident, out _out))
                        yield return new Token(_out, ident);
                    else
                        yield return new Token(Ident, ident);
                }
                else if (char.IsNumber(c))
                {
                    while (char.IsNumber(peekc()) || peekc() == '.')
                        sb.Append(readc());

                    var num = sb.ToString();

                    yield return new Token(Number, num);
                }
                else
                {
                    var cc = c.ToString(); // lexeme
                    switch (readc())
                    {
                        case '+':
                            {
                                if (peekc() == '=')
                                {
                                    readc();
                                    yield return new Token(PlusEquals, "+=");
                                    break;
                                }
                                yield return new Token(Plus, cc);
                                break;
                            }
                        case '-': yield return new Token(Minus, cc); break;
                        case '*': yield return new Token(Star, cc); break;
                        case '/': yield return new Token(Slash, cc); break;
                        case '(': yield return new Token(LeftParen, cc); break;
                        case ')': yield return new Token(RightParen, cc); break;
                        case ';': yield return new Token(Semicolon, cc); break;
                        case ',': yield return new Token(Comma, cc); break;
                        case '=': yield return new Token(Terminal.Equals, cc); break;
                        case '{': yield return new Token(LeftBrace, cc); break;
                        case '}': yield return new Token(RightBrace, cc); break;
                        case '<': yield return new Token(LeftAngle, cc); break;
                        case '>': yield return new Token(RightAngle, cc); break;
                        default: yield return new Token(Unknown, cc); break;
                    }
                }
            }

            yield return new Token(Terminal.Eof);
        }
    }
}
