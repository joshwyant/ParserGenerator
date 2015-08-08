using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LRGenerator.Terminal;
using static LRGenerator.ActionType;
using System.Linq.Expressions;

namespace LRGenerator
{
    public class LRkParser
    {
        internal LRkGrammar Grammar { get; }
        IEnumerable<Token> Tokens { get; }
        Stack<Tuple<AstNode, int>> Stack { get; } = new Stack<Tuple<AstNode, int>>();
        public List<string> Errors { get; }  = new List<string>();

        public LRkParser(LRkGrammar g, string s)
        {
            Grammar = g;
            Tokens = new Lexer(new StringReader(s)); // Lazy lex
        }

        public LRkParser(LRkGrammar g, Stream s)
        {
            Grammar = g;
            Tokens = new Lexer(new StreamReader(s));
        }

        public AstNode ParseAst()
        {
            var table = Grammar.ParseTable;
            var currentState = table.StartState;

            // Push the start state onto the stack
            Stack.Push(new Tuple<AstNode, int>(new AstNode(Nonterminal.Start), currentState));

            foreach (var t in Tokens)
            {
                // Reduce any number of times for a given token. Always advance to the next token for no reduction.
                bool reduced;
                do {
                    reduced = false;
                
                    Action action = null;
    
                    table.Action.TryGetValue(new Tuple<int, Terminal>(currentState, t.Terminal), out action);
                    
                    // Get the action type. If action is null, default to the 'Error' action
                    switch (action?.Type ?? Error)
                    {
                        case Shift:
                            // Shift N
                            currentState = action.Number;
                            Stack.Push(new Tuple<AstNode, int>(new AstNode(t), currentState));
                            break;
                        case Reduce:
                            // Reduce by rule N
                            var rule = Grammar.IndexedProductions[action.Number];
                            var reduceLhs = rule.Production.Lhs;
    
                            // Now create an array for the symbols:
                            var symbols = new AstNode[rule.Length];
    
                            // Pop the thing off the stack
                            for (var i = rule.Length - 1; i >= 0; i--)
                                symbols[i] = Stack.Pop().Item1;
    
                            // Create a new Ast node
                            var reducedNode = new AstNode(reduceLhs, symbols);
                            
                            // Get the state at the top of the stack
                            var topState = Stack.Peek().Item2;
    
                            // Get the next transition key based on the item we're reducing by
                            // It should exist in the goto table, we should never try to reduce when it doesn't make sense.
                            var newState = table.Goto[new Tuple<int, Nonterminal>(topState, reduceLhs)];
    
                            // Push that onto the stack
                            Stack.Push(new Tuple<AstNode, int>(reducedNode, newState));
    
                            // Transition to the top state
                            currentState = newState;
                            
                            // Keep reducing before moving to the next token
                            reduced = true;
                            break;
                        case Accept:
                            return Stack.Pop().Item1;
                        case Error:
                            var tokenStr = string.IsNullOrEmpty(t.Lexeme) ? t.Terminal.ToString() : t.Lexeme;
    
                            Errors.Add($"Unexpected symbol: {tokenStr}");
    
                            if (t.Terminal == Terminal.Eof)
                                // Just return whatever's on the stack
                                return new AstNode(Unknown, Stack.Skip(1).Select(s => s.Item1).ToArray());
    
                            break;
                    }
                } while (reduced);
            }
            return null;
        }

        //private static Func<LRkParser, AstNode> CompileParser(LRkParseTable table)
        //{
        //    var parserParam = Expression.Parameter(typeof(LRkParser), "parser");
        //    var stack = Expression.Property(parserParam, "Stack");
        //    var stackPush = Expression.Call(stack, typeof(Stack<Tuple<AstNode, int>>).GetMethod("Push"));
        //    var newStackObj = Expression.New(typeof(Tuple<AstNode, int>).GetConstructors().Single(c => c.GetParameters().Count() == 2));
        //    var newAstNode = Expression.New(typeof(AstNode));
        //    var parserGrammar = Expression.Property(parserParam, "Grammar");
        //    var parserLexer = Expression.Property(parserParam, "Lexer");
        //    var lexerRead = Expression.Call(parserLexer, typeof(Lexer).GetMethod("Read"));
        //    var currentState = Expression.Variable(typeof(int));
        //    var currentToken = Expression.Variable(typeof(Token));

        //    var program = Expression.Block(
        //        Expression.Assign(currentState, Expression.Constant(table.StartState)),
        //        Expression.Assign(currentToken, lexerRead),
        //        Expression.Loop(
        //            Expression.Switch(
        //                currentState,
        //                new[]
        //                {
        //                    Expression.
        //                }
        //            )
        //        )
        //    );
        //}
    }
}
