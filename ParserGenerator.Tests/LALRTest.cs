using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using ParserGenerator.Demo;
using static ParserGenerator.Demo.Terminal;
using static ParserGenerator.Demo.Nonterminal;
using ParserGenerator.Utility;
using System.IO;

namespace ParserGenerator.Tests
{
    [TestFixture]
    public class LALRTest
    {
        class LALRGrammar
            : LALRGrammar<Terminal, Nonterminal>
        {
            private readonly Production _init;
            private readonly Production _start;
            private readonly Production _access;
            private readonly Production _expression;

            public LALRGrammar()
                : base(Terminal.Unknown, Terminal.Eof, Nonterminal.Init, Nonterminal.Start)
            {
                _init = Productions[Init];
                _start = DefineProduction(Start);
                _access = DefineProduction(Access);
                _expression = DefineProduction(Expression);

                ProductionRule t;

                t = _start %
                    Access / Terminal.Equals / Expression;
                t = _start %
                    Expression;
                t = _access %
                    Star / Expression;
                t = _access %
                    Ident;
                t = _expression %
                    Access;
            }

            private class Lexer : LexerBase
            {
                public Lexer(TextReader reader) : base(reader) { }

                protected override IEnumerable<Token> Lex()
                {
                    yield return new Token(Terminal.Eof);
                }
            }

            public override LexerBase GetLexer(TextReader reader)
            {
                return new Lexer(reader);
            }

            public LRItemSetCollection MockKernels()
            {
                //return new LRItemSetCollection()
                //{
                // I0: S' -> . S
                var rule0 = new LRItemSet(new LRItem(_init.Rules.Single(), 0, isKernel: true).AsSingletonEnumerable());
                // I1: S' -> S .
                var rule1 = new LRItemSet(new LRItem(_init.Rules.Single(), 1).AsSingletonEnumerable());
                // I2: S -> L . = R
                //     R -> L .
                var rule2 = new LRItemSet(new[] {
                    new LRItem(
                        _start.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Access), new Symbol(Terminal.Equals), new Symbol(Expression) }
                            )
                        ), 1
                    ),
                    new LRItem(
                        _expression.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Access) }
                            )
                        ), 1
                    )
                });
                // I3: S -> R .
                var rule3 = new LRItemSet(new[] {
                    new LRItem(
                        _start.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Expression) }
                            )
                        ), 1
                    )
                });
                // I4: L -> * . R
                var rule4 = new LRItemSet(new[] {
                    new LRItem(
                        _access.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Star), new Symbol(Expression) }
                            )
                        ), 1
                    ),
                });
                // I5: L -> id .
                var rule5 = new LRItemSet(new[] {
                    new LRItem(
                        _access.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Ident) }
                            )
                        ), 1
                    ),
                });
                // I6: S -> L = . R
                var rule6 = new LRItemSet(new[] {
                    new LRItem(
                        _start.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Access), new Symbol(Terminal.Equals), new Symbol(Expression) }
                            )
                        ), 2
                    ),
                });
                // I7: L -> * R .
                var rule7 = new LRItemSet(new[] {
                    new LRItem(
                        _access.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Star), new Symbol(Expression) }
                            )
                        ), 2
                    ),
                });
                // I8: R -> L .
                var rule8 = new LRItemSet(new[] {
                    new LRItem(
                        _expression.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Access) }
                            )
                        ), 1
                    ),
                });
                // I9: S -> L = R .
                var rule9 = new LRItemSet(new[] {
                    new LRItem(
                        _start.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Access), new Symbol(Terminal.Equals), new Symbol(Expression) }
                            )
                        ), 3
                    ),
                });

                return new LRItemSetCollection()
                {
                    rule0, rule1, rule2, rule3, rule4, rule5, rule6, rule7, rule8, rule9
                };
            }
        }

        private LALRGrammar _grammar;

        [SetUp]
        public void Init()
        {
            _grammar = new LALRGrammar();
        }

        [Test]
        public void Grammar_DoesHaveCorrectStates()
        {
            var kernels = new HashSet<LALRGrammar.LRItemSet>(_grammar.MockKernels());
            var states = new HashSet<LALRGrammar.LRItemSet>(_grammar.States);

            var intersection = states.Intersect(kernels);

            Assert.That(kernels.Count, Is.EqualTo(states.Count));
            Assert.That(intersection.Count, Is.EqualTo(states.Count));
        }
    }
}
