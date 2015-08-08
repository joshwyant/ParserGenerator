using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Production init;
            Production start;
            Production access;
            Production expression;

            public LALRGrammar()
                : base(Terminal.Unknown, Terminal.Eof, Nonterminal.Init, Nonterminal.Start)
            {
                init = Productions.Values.Single(p => p.Lhs == Nonterminal.Init);
                start = DefineProduction(Start);
                access = DefineProduction(Access);
                expression = DefineProduction(Expression);

                ProductionRule t;

                t = start %
                    Access / Terminal.Equals / Expression;
                t = start %
                    Expression;
                t = access %
                    Star / Expression;
                t = access %
                    Ident;
                t = expression %
                    Access;
            }

            public class Lexer : LexerBase
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
                var rule0 = new LRItemSet(new LRItem(init.Rules.Single(), 0, isKernel: true).Yield());
                // I1: S' -> S .
                var rule1 = new LRItemSet(new LRItem(init.Rules.Single(), 1).Yield());
                // I2: S -> L . = R
                //     R -> L .
                var rule2 = new LRItemSet(new[] {
                    new LRItem(
                        start.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Access), new Symbol(Terminal.Equals), new Symbol(Expression) }
                            )
                        ), 1
                    ),
                    new LRItem(
                        expression.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Access) }
                            )
                        ), 1
                    )
                });
                // I3: S -> R .
                var rule3 = new LRItemSet(new[] {
                    new LRItem(
                        start.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Expression) }
                            )
                        ), 1
                    )
                });
                // I4: L -> * . R
                var rule4 = new LRItemSet(new[] {
                    new LRItem(
                        access.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Star), new Symbol(Expression) }
                            )
                        ), 1
                    ),
                });
                // I5: L -> id .
                var rule5 = new LRItemSet(new[] {
                    new LRItem(
                        access.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Ident) }
                            )
                        ), 1
                    ),
                });
                // I6: S -> L = . R
                var rule6 = new LRItemSet(new[] {
                    new LRItem(
                        start.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Access), new Symbol(Terminal.Equals), new Symbol(Expression) }
                            )
                        ), 2
                    ),
                });
                // I7: L -> * R .
                var rule7 = new LRItemSet(new[] {
                    new LRItem(
                        access.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Star), new Symbol(Expression) }
                            )
                        ), 2
                    ),
                });
                // I8: R -> L .
                var rule8 = new LRItemSet(new[] {
                    new LRItem(
                        expression.Rules.Single(r => r.Symbols.SequenceEqual(
                            new [] { new Symbol(Access) }
                            )
                        ), 1
                    ),
                });
                // I9: S -> L = R .
                var rule9 = new LRItemSet(new[] {
                    new LRItem(
                        start.Rules.Single(r => r.Symbols.SequenceEqual(
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

        LALRGrammar grammar;

        [SetUp]
        public void Init()
        {
            grammar = new LALRGrammar();
        }

        [Test]
        public void Grammar_DoesHaveCorrectStates()
        {
            var kernels = new HashSet<LALRGrammar.LRItemSet>(grammar.MockKernels());
            var states = new HashSet<LALRGrammar.LRItemSet>(grammar.States);

            var intersection = states.Intersect(kernels);

            Assert.That(kernels.Count, Is.EqualTo(states.Count));
            Assert.That(intersection.Count, Is.EqualTo(states.Count));
        }
    }
}
