using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LR0Generator.Terminal;
using static LR0Generator.Nonterminal;

namespace LR0Generator
{
    public class MyGrammar : Grammar
    {
        public MyGrammar()
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

            ProductionRule t;

            t = start
                    % StatementList;
            t.IsAccepting = true;

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
                    % AssignmentExpression;
            t = assignment
                    % Var / Ident / Terminal.Equals / AssignmentExpression;
            t = assignExpr
                    % Var / Ident / PlusEquals / AssignmentExpression;
            t = stmt
                    % SimpleStatement / Semicolon;
            t = stmt
                    % If / LeftParen / AssignmentExpression / RightParen / Statement;
            t = stmt
                    % For / LeftParen / OptionalSimpleStatement / Semicolon / OptionalExpression / Semicolon / OptionalSimpleStatement / RightParen / Statement;
            t = stmt
                    % LeftBrace / StatementList / RightBrace;
            t = stmt
                    % SimpleStatement;
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
                    % Ident;
            t = factor
                    % Number;

            GenerateTables();
        }
    }
}
