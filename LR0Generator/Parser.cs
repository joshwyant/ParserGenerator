using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LR0Generator.Terminal;
using static LR0Generator.Nonterminal;

namespace LR0Generator
{
    public class Parser
    {
        internal Grammar Grammar { get; }
        Lexer Lexer { get; }
        Stack<Tuple<AstNode, LR0ItemSet>> Stack { get; } = new Stack<Tuple<AstNode, LR0ItemSet>>();
        public List<string> Errors { get; }  = new List<string>();

        public Parser(Grammar g, string s)
        {
            Grammar = g;
            Lexer = new Lexer(new StringReader(s));
        }

        public AstNode ParseAst()
        {
            AstNode toReturn = null;

            var currentState = Grammar.StartState;

            // Push the start state onto the stack
            Stack.Push(new Tuple<AstNode, LR0ItemSet>(new AstNode(Start), Grammar.StartState));

            bool didReduceByAccepting = false;
            do
            {
                var t = Lexer.Peek();

                var transitionKey = new Tuple<LR0ItemSet, Symbol>(currentState, t);

                if (Grammar.Transitions.ContainsKey(transitionKey))
                {
                    // Shift!

                    // Advance state
                    currentState = Grammar.Transitions[transitionKey];
                    // Shift onto stack
                    Stack.Push(new Tuple<AstNode, LR0ItemSet>(new AstNode(Lexer.Read()), currentState));
                }
                else
                {
                    // Reduce!

                    // Get the item we are supposed to reduce by in this state (marker is at the end).
                    var item = currentState.SingleOrDefault(i => i.Length == i.Marker);

                    if (item == null)
                    {
                        Lexer.Read();

                        var tokenStr = string.IsNullOrEmpty(t.Lexeme) ? t.Terminal.ToString() : t.Lexeme;

                        Errors.Add($"Unexpected symbol: {tokenStr}");

                        if (t.Terminal == Terminal.Eof)
                            didReduceByAccepting = true;

                        continue;
                    }

                    // What production are we reducing to?
                    var reduceLhs = item.Rule.Production.Lhs;

                    // Now create an array for the symbols:
                    var symbols = new AstNode[item.Length];

                    // Pop the thing off the stack
                    for (var i = item.Length - 1; i >= 0; i--)
                    {
                        symbols[i] = Stack.Pop().Item1;
                    }
                    
                    // Create a new Ast node
                    var reducedNode = new AstNode(reduceLhs, symbols);

                    // We're done!!
                    if (item.Rule.IsAccepting)
                    {
                        didReduceByAccepting = true;
                        toReturn = reducedNode;
                    }
                    else
                    {
                        // Get the state at the top of the stack
                        var topState = Stack.Peek().Item2;

                        // Get the next transition key based on the item we're reducing by
                        transitionKey = new Tuple<LR0ItemSet, Symbol>(topState, reduceLhs);
                        var newState = Grammar.Transitions[transitionKey];

                        // Push that onto the stack
                        Stack.Push(new Tuple<AstNode, LR0ItemSet>(reducedNode, newState));

                        // Transition to the top state
                        currentState = newState;
                    }
                }
            } while (!didReduceByAccepting);

            return toReturn;
        }
    }
}
