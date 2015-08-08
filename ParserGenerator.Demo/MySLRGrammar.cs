using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ParserGenerator.Demo.Terminal;
using static ParserGenerator.Demo.Nonterminal;

namespace ParserGenerator.Demo
{
    public partial class MySLRGrammar : SLRGrammar<Terminal, Nonterminal>
    {
        public MySLRGrammar()
            : base(Terminal.Unknown, Terminal.Eof, Nonterminal.Init, Nonterminal.Start)
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
        }
    }
}
