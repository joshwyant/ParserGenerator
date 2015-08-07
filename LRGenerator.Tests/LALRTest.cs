using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LRGenerator.Terminal;
using static LRGenerator.Nonterminal;

namespace LRGenerator.Tests
{
    [TestFixture]
    public class LALRTest
    {
        LALRGrammar grammar;
        Production init;
        Production start;
        Production access;
        Production expression;

        [SetUp]
        public void Init()
        {
            grammar = new LALRGrammar();

            init = grammar.Productions.Values.Single(p => p.Lhs == Nonterminal.Init);
            start = grammar.DefineProduction(Start);
            access = grammar.DefineProduction(Access);
            expression = grammar.DefineProduction(Expression);

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

        LRItemSetCollection MockKernels()
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

        [Test]
        public void Grammar_DoesHaveCorrectStates()
        {
            var kernels = new HashSet<LRItemSet>(MockKernels());
            var states = new HashSet<LRItemSet>(grammar.States);

            var intersection = states.Intersect(kernels);

            Assert.That(kernels.Count, Is.EqualTo(states.Count));
            Assert.That(intersection.Count, Is.EqualTo(states.Count));
        }
    }
}
