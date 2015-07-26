using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LR0Generator
{
    public enum Nonterminal
    {
        Start,
        StatementList,
        SimpleStatement,
        Statement,
        OptionalExpression,
        OptionalSimpleStatement,
        Expression,
        Assignment,
        AssignmentExpression,
        Compare,
        TypeName,
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
