using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParserGenerator.Demo
{
    public enum Nonterminal
    {
        Init,
        Start,
        StatementList,
        SimpleStatement,
        Statement,
        OptionalExpression,
        OptionalSimpleStatement,
        Expression,
        Access,
        Assignment,
        AssignmentExpression,
        Compare,
        TypeName,
        SimpleName,
        TypeParameters,
        TypeParameterList,
        Call,
        Parameters,
        ParameterList,
        AssignmentType,
        Term,
        Factor,
    }
}
