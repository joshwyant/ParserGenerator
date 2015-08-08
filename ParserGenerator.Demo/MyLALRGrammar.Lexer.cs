using ParserGenerator.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ParserGenerator.Demo.Terminal;

namespace ParserGenerator.Demo
{
    public partial class MyLALRGrammar
    {
        public class Lexer : LexerBase
        {
            Dictionary<string, Terminal> ReservedWords { get; } 
                = new Dictionary<string, Terminal>
            {
                { "if", If }, { "for", For }, { "var", Var }, { "int", Int }, { "float", Float },
                { "bool", Bool }, { "string", Terminal.String }
            };

            public Lexer(TextReader reader) : base(reader) { }

            protected override IEnumerable<Token> Lex()
            {
                while (Reader.HasNext())
                {
                    while (char.IsWhiteSpace(Reader.Peek()))
                    {
                        Reader.Read();
                        continue;
                    }

                    if (!Reader.HasNext()) break;

                    var sb = new StringBuilder();
                
                    var c = Reader.Peek();

                    if (char.IsLetter(c) || c == '_')
                    {
                        while (char.IsLetterOrDigit(Reader.Peek()) || Reader.Peek() == '_')
                            sb.Append(Reader.Read());

                        var ident = sb.ToString();

                        Terminal _out;
                        if (ReservedWords.TryGetValue(ident, out _out))
                            yield return new Token(_out, ident);
                        else
                            yield return new Token(Ident, ident);
                    }
                    else if (char.IsNumber(c))
                    {
                        while (char.IsNumber(Reader.Peek()) || Reader.Peek() == '.')
                            sb.Append(Reader.Read());

                        var num = sb.ToString();

                        yield return new Token(Number, num);
                    }
                    else
                    {
                        var cc = c.ToString(); // lexeme
                        switch (Reader.Read())
                        {
                            case '+':
                                {
                                    if (Reader.Peek() == '=')
                                    {
                                        Reader.Read();
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
                            default: yield return new Token(Terminal.Unknown, cc); break;
                        }
                    }
                }

                yield return new Token(Terminal.Eof);
            }
        }
    }
}
