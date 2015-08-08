using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ParserGenerator.Demo.Terminal;
using static ParserGenerator.Demo.Nonterminal;

namespace ParserGenerator.Demo
{
    public partial class MyLALRGrammar : LALRGrammar<Terminal, Nonterminal>
    {
        public MyLALRGrammar()
            : base(Terminal.Unknown, Terminal.Eof, Nonterminal.Init, Nonterminal.Start)
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
            var simpleName = DefineProduction(SimpleName);
            var typeParams = DefineProduction(TypeParameters);
            var typeParamList = DefineProduction(TypeParameterList);
            var call = DefineProduction(Call);
            var param = DefineProduction(Parameters);
            var paramList = DefineProduction(ParameterList);
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
            t = typeName % SimpleName;
            t = typeName % SimpleName / TypeParameters;
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
                    % AssignmentType / SimpleName / Terminal.Equals / AssignmentExpression;
            t = assignment
                    % AssignmentType / SimpleName / PlusEquals / AssignmentExpression;
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
                    % SimpleName / Terminal.Equals / AssignmentExpression;
            t = assignExpr
                    % SimpleName / PlusEquals / AssignmentExpression;
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
                    % SimpleName;
            t = simpleName
                    % Ident;
            t = factor
                    % Number;
            //t = factor % Call;
        }
    }
}
