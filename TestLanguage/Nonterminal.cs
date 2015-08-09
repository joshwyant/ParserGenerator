using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLanguage
{
    public enum Nonterminal
    {
        Init,
        Start,
        CompileUnit,
        UsingDirectives,
        UsingDirective,
        FullyQualifiedNamespace,
        TypeOrNamespaceDeclarations,
        TypeOrNamespaceDeclaration,
        NamespaceDeclaration,
        NamespaceBody,
        TypeDeclaration,
        ClassDeclaration,
        MemberSpecifiers,
        MemberSpecifier,
        AccessSpecifier,
        SimpleTypeName,
        SimpleType,
        TypeName,
        TypeArgumentList,
        TypeArguments,
        Inheritance,
        InheritanceTypes,
        ClassBody,
        MemberDeclarations,
        MemberDeclaration,
        MethodDeclaration,
        ParameterDefinitions,
        ParameterDefinition,
        MethodBody,
        MethodBodyInternal,
    }
}
