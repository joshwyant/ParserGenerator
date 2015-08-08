using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LRGenerator.Terminal;

namespace LRGenerator
{
    public class Lexer : IPeekable<Token>
    {
        Dictionary<string, Terminal> ReservedWords { get; } 
            = new Dictionary<string, Terminal>
        {
            { "if", If }, { "for", For }, { "var", Var }, { "int", Int }, { "float", Float },
            { "bool", Bool }, { "string", Terminal.String }
        };

        CharacterReader reader;
        IPeekable<Token> peekable;

        public Lexer(TextReader reader)
        {
            this.reader = new CharacterReader(reader);
            this.peekable = Lex().AsPeekable();
        }

        private IEnumerable<Token> Lex()
        {
            while (reader.HasNext())
            {
                while (char.IsWhiteSpace(reader.Peek()))
                {
                    reader.Read();
                    continue;
                }

                if (!reader.HasNext()) break;

                var sb = new StringBuilder();
                
                var c = reader.Peek();

                if (char.IsLetter(c) || c == '_')
                {
                    while (char.IsLetterOrDigit(reader.Peek()) || reader.Peek() == '_')
                        sb.Append(reader.Read());

                    var ident = sb.ToString();

                    Terminal _out;
                    if (ReservedWords.TryGetValue(ident, out _out))
                        yield return new Token(_out, ident);
                    else
                        yield return new Token(Ident, ident);
                }
                else if (char.IsNumber(c))
                {
                    while (char.IsNumber(reader.Peek()) || reader.Peek() == '.')
                        sb.Append(reader.Read());

                    var num = sb.ToString();

                    yield return new Token(Number, num);
                }
                else
                {
                    var cc = c.ToString(); // lexeme
                    switch (reader.Read())
                    {
                        case '+':
                            {
                                if (reader.Peek() == '=')
                                {
                                    reader.Read();
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

        #region IPeekable<Token>
        public Token Read()
        {
            return peekable.Read();
        }

        public Token Peek()
        {
            return peekable.Peek();
        }

        public bool HasNext()
        {
            return peekable.HasNext();
        }

        public IEnumerator<Token> GetEnumerator()
        {
            return peekable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return peekable.GetEnumerator();
        }
        #endregion
    }
}
