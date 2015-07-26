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
    class Program
    {
        public static void Main()
        {
            Demo(new MyGrammar(), "var t = x + 3; if (y) { t += 1; }");

            Demo(new MyBrokenGrammar(), "f(g<a, b<(c))");

            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void Demo(Grammar grammar, string toParse)
        {
            Console.WriteLine($"{grammar.GetType().Name}:");
            Console.WriteLine();

            var srConflicts = grammar.ShiftReduceConflicts();
            var rrConflicts = grammar.ReduceReduceConflicts();
            var hasConflicts = srConflicts.Length > 0 || rrConflicts.Length > 0;

            if (!hasConflicts)
            {
                while (!string.IsNullOrEmpty(toParse))
                {
                    var p = new Parser(grammar, toParse);
                    var ast = p.ParseAst();

                    if (p.Errors.Count > 0)
                    {
                        foreach (var error in p.Errors)
                            Console.WriteLine(error);
                    }
                    else
                    {
                        var sw = new StringWriter();
                        ast.Print(sw);
                        Console.WriteLine(sw);
                    }

                    Console.Write(" > ");
                    toParse = Console.ReadLine();
                }

                Console.WriteLine();
            }
            else
            {
                if (srConflicts.Length > 0)
                {
                    Console.WriteLine("Shift/Reduce conflicts (would favor shift):");

                    foreach (var conflict in srConflicts)
                    {
                        Console.WriteLine($"  {conflict}");
                    }
                    Console.WriteLine();
                }
                if (rrConflicts.Length > 0)
                {
                    Console.WriteLine("Reduce/Reduce conflicts (critical):");

                    for (var i = 0; i < rrConflicts.Length; i++)
                    {
                        Console.WriteLine($"=> {i}");

                        var conflictSet = rrConflicts[i].OrderBy(c => c.Marker).ToArray();

                        Console.WriteLine("  Nonkernel:");
                        foreach (var item in conflictSet.Where(it => !it.IsKernel))
                        {
                            Console.WriteLine($"    {item}");
                        }

                        Console.WriteLine("  Kernel:");
                        foreach (var item in conflictSet.Where(it => it.IsKernel))
                        {
                            Console.WriteLine($"    {item}");
                        }
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }
    }

    public class MyGrammar : Grammar
    {
        public MyGrammar()
        {
            var start = DefineProduction(Start);
            var stmtList = DefineProduction(StatementList);
            var stmt = DefineProduction(Statement);
            var simpleStmt = DefineProduction(SimpleStatement);
            var expression = DefineProduction(Expression);
            var assignment = DefineProduction(Assignment);
            var assignExpr = DefineProduction(AssignmentExpression);
            var compare = DefineProduction(Compare);
            var term = DefineProduction(Term);
            var factor = DefineProduction(Factor);
            var assignType = DefineProduction(AssignmentType);

            ProductionRule t;

            t = start
                    % StatementList;
            t.IsAccepting = true;

            // 
            // Statements
            t = stmtList
                    % StatementList / Statement;
            t = stmtList
                    % Statement;
            t = simpleStmt
                    % Assignment;
            //
            // Assignments
            t = assignType % Var / Ident;
            t = assignType % Ident;
            t = assignment
                    % AssignmentType / Terminal.Equals / AssignmentExpression;
            t = assignment
                    % AssignmentType / PlusEquals / AssignmentExpression;
            t = stmt
                    % SimpleStatement / Semicolon;
            t = stmt
                    % If / LeftParen / AssignmentExpression / RightParen / Statement;
            t = stmt
                    % LeftBrace / StatementList / RightBrace;
            t = assignExpr
                    % Ident / Terminal.Equals / AssignmentExpression;
            t = assignExpr
                    % Ident / PlusEquals / AssignmentExpression;
            t = assignExpr
                    % Compare;
            t = compare
                    % Compare / LeftAngle / Expression;
            t = compare
                    % Compare / RightAngle / Expression;
            t = compare
                    % Expression;
            t = expression
                    % Expression / Plus / Term;
            t = expression
                    % Expression / Minus / Term;
            t = expression
                    % Term;
            t = term
                    % Term / Star / Factor;
            t = term
                    % Term / Slash / Factor;
            t = term
                    % Factor;
            t = factor
                    % LeftParen / Expression / RightParen;
            t = factor
                    % Ident;
            t = factor
                    % Number;

            GenerateTables();
        }
    }

    public class MyBrokenGrammar : Grammar
    {
        public MyBrokenGrammar()
        {
            var start = DefineProduction(Start);
            var stmtList = DefineProduction(StatementList);
            var stmt = DefineProduction(Statement);
            var simpleStmt = DefineProduction(SimpleStatement);
            var optExpr = DefineProduction(OptionalExpression);
            var optStmt = DefineProduction(OptionalSimpleStatement);
            var expression = DefineProduction(Expression);
            var assignment = DefineProduction(Assignment);
            var assignExpr = DefineProduction(AssignmentExpression);
            var compare = DefineProduction(Compare);
            var term = DefineProduction(Term);
            var factor = DefineProduction(Factor);
            var typeName = DefineProduction(TypeName);
            var typeParams = DefineProduction(TypeParameters);
            var typeParamList = DefineProduction(TypeParameterList);
            var call = DefineProduction(Call);
            var param = DefineProduction(Parameters);
            var paramList = DefineProduction(ParameterList);
            var assignType = DefineProduction(AssignmentType);

            ProductionRule t;

            t = start
                    % StatementList;
            t.IsAccepting = true;

            // 
            // Statements
            t = stmtList
                    % StatementList / Statement;
            t = stmtList
                    % Statement;
            t = stmtList
                    % null;
            t = optStmt
                    % SimpleStatement;
            t = optStmt
                    % null;
            t = simpleStmt
                    % Assignment;
            t = simpleStmt
                    % Call;
            //
            // TypeName
            t = typeName % Int;
            t = typeName % Float;
            t = typeName % Bool;
            t = typeName % Terminal.String;
            t = typeName % Ident;
            t = typeName % Ident / TypeParameters;
            t = typeParams % LeftAngle / TypeParameterList / RightAngle;
            t = typeParamList % TypeParameterList / Comma / TypeName;
            t = typeParamList % TypeName;
            //
            // Call
            t = call % TypeName / Parameters;
            t = param % LeftParen / ParameterList / RightParen;
            t = paramList % ParameterList / Comma / AssignmentExpression;
            t = paramList % AssignmentExpression;
            t = paramList % null;
            //
            // Assignments
            t = assignType % Var;
            t = assignType % TypeName;
            t = assignment
                    % AssignmentType / Ident / Terminal.Equals / AssignmentExpression;
            t = assignment
                    % AssignmentType / Ident / PlusEquals / AssignmentExpression;
            t = stmt
                    % SimpleStatement / Semicolon;
            t = stmt
                    % If / LeftParen / AssignmentExpression / RightParen / Statement;
            t = stmt
                    % For / LeftParen / OptionalSimpleStatement / Semicolon / OptionalExpression / Semicolon / OptionalSimpleStatement / RightParen / Statement;
            t = stmt
                    % LeftBrace / StatementList / RightBrace;
            t = optExpr
                    % AssignmentExpression;
            t = optExpr
                    % null;
            t = assignExpr
                    % Ident / Terminal.Equals / AssignmentExpression;
            t = assignExpr
                    % Ident / PlusEquals / AssignmentExpression;
            t = assignExpr
                    % Compare;
            t = compare
                    % Compare / LeftAngle / Expression;
            t = compare
                    % Compare / RightAngle / Expression;
            t = compare
                    % Expression;
            t = expression
                    % Expression / Plus / Term;
            t = expression
                    % Expression / Minus / Term;
            t = expression
                    % Term;
            t = term
                    % Term / Star / Factor;
            t = term
                    % Term / Slash / Factor;
            t = term
                    % Factor;
            t = factor
                    % LeftParen / Expression / RightParen;
            t = factor
                    % TypeName;
            t = factor
                    % Ident;
            t = factor
                    % Number;
            t = factor % Call;

            GenerateTables();
        }
    }
}
