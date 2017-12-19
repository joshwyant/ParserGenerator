using System;
using System.Collections.Generic;
using System.Linq;
using static ParserGenerator.ActionType;

namespace ParserGenerator
{
    public abstract partial class LRkGrammar<Terminal_T, Nonterminal_T>
    {
        public class LRParser : Parser
        {
            private Stack<(ParseTreeNode node, int state)> Stack { get; } = new Stack<(ParseTreeNode node, int state)>();

            public LRParser(LRkGrammar<Terminal_T, Nonterminal_T> g, LexerBase l)
                : base(g, l) { }

            public ParseTreeNode ParseGLR()
            {
                throw new NotImplementedException();
            }
            
            public override ParseTreeNode Parse()
            {
                var grammar = Grammar as LRkGrammar<Terminal_T, Nonterminal_T>;

                var table = grammar.ParseTable;
                var currentState = table.StartState;

                // Push the start state onto the stack
                Stack.Push((new ParseTreeNode(Grammar.Start), currentState));

                foreach (var t in Lexer)
                {
                    // Reduce any number of times for a given token. Always advance to the next token for no reduction.
                    bool reduced;
                    do {
                        reduced = false;

                        table.Action.TryGetValue((currentState, t.Terminal), out var action);
                    
                        // Get the action type. If action is null, default to the 'Error' action
                        switch (action?.Type ?? Error)
                        {
                            case Shift:
                                // Shift N
                                currentState = action.Number;
                                Stack.Push((new ParseTreeNode(t), currentState));
                                break;
                            case Reduce:
                                // Reduce by rule N
                                var rule = Grammar.IndexedProductions[action.Number];
                                var reduceLhs = rule.Production.Lhs;
    
                                // Now create an array for the symbols:
                                var symbols = new ParseTreeNode[rule.Length];
    
                                // Pop the thing off the stack
                                for (var i = rule.Length - 1; i >= 0; i--)
                                    symbols[i] = Stack.Pop().node;
    
                                // Create a new Ast node
                                var reducedNode = new ParseTreeNode(reduceLhs, symbols);
                            
                                // Get the state at the top of the stack
                                var topState = Stack.Peek().state;
    
                                // Get the next transition key based on the item we're reducing by
                                // It should exist in the goto table, we should never try to reduce when it doesn't make sense.
                                var newState = table.Goto[(topState, reduceLhs)];
    
                                // Push that onto the stack
                                Stack.Push((reducedNode, newState));
    
                                // Transition to the top state
                                currentState = newState;
                            
                                // Keep reducing before moving to the next token
                                reduced = true;
                                break;
                            case Accept:
                                return Stack.Pop().node;
                            case Error:
                                var tokenStr = string.IsNullOrEmpty(t.Lexeme) ? t.Terminal.ToString() : t.Lexeme;
    
                                Errors.Add($"Unexpected symbol: {tokenStr}");
    
                                if (t.Terminal.CompareTo(Grammar.Eof) == 0)
                                    // Just return whatever's on the stack
                                    return new ParseTreeNode(Grammar.Unknown, Stack.Skip(1).Select(s => s.node).ToArray());
    
                                break;
                        }
                    } while (reduced);
                }
                return null;
            }
        }
    }
}
