using ParserGenerator;
using static TestLanguage.Terminal;
using static TestLanguage.Nonterminal;

namespace TestLanguage
{
    public partial class Grammar
        : LALRGrammar<Terminal, Nonterminal>
    {
        public Grammar()
            : base(Terminal.Unknown, Terminal.Eof, Nonterminal.Init, Nonterminal.Start)
        {
            DefineProduction (Start)
                .As(CompileUnit);

            DefineProduction (CompileUnit)
                .As(UsingDirectives, TypeOrNamespaceDeclarations);

            DefineProduction (UsingDirectives)
                .As(UsingDirectives, UsingDirective)
                .Or(UsingDirective)
                .Optional();

            DefineProduction (UsingDirective)
                .As(Using, FullyQualifiedNamespace, Semicolon); // Expand later to include aliases

            DefineProduction(FullyQualifiedNamespace)
                .As(FullyQualifiedNamespace, Dot, Identifier)
                .Or(Identifier);

            DefineProduction(TypeOrNamespaceDeclarations)
                .As(TypeOrNamespaceDeclarations, TypeOrNamespaceDeclaration)
                .Or(TypeOrNamespaceDeclaration)
                .Optional();

            DefineProduction(NamespaceDeclaration)
                .As(Namespace, FullyQualifiedNamespace, LeftCurlyBrace, NamespaceBody, RightCurlyBrace);

            DefineProduction(NamespaceBody)
                .As(UsingDirectives, TypeOrNamespaceDeclarations);

            DefineProduction(TypeOrNamespaceDeclaration)
                .As(MemberSpecifiers, TypeDeclaration)
                .Or(NamespaceDeclaration);

            DefineProduction(TypeDeclaration)
                .AsAny(ClassDeclaration); // To be expanded to structs, enums, etc. later

            DefineProduction(ClassDeclaration)
                .As(Class, SimpleTypeName, Inheritance, LeftCurlyBrace, ClassBody, RightCurlyBrace);

            DefineProduction(MemberSpecifiers)
                .As(MemberSpecifiers, MemberSpecifier)
                .Or(MemberSpecifier)
                .Optional();

            DefineProduction(MemberSpecifier)
                //.AsAny(AccessSpecifier); // Add the others (sealed, etc.) later

            //DefineProduction(AccessSpecifier)
                .AsAny(Public, Private, Protected, Internal, Static, Abstract);

            DefineProduction(SimpleTypeName)
                .As(Identifier, TypeArgumentList);

            DefineProduction(TypeArgumentList)
                .As(LessThan, TypeArguments, GreaterThan)
                .Optional();

            DefineProduction(TypeArguments)
                .As(TypeArguments, Comma, TypeName)
                .Or(TypeName);

            DefineProduction(TypeName)
                .As(TypeName, Dot, SimpleTypeName)
                .Or(SimpleTypeName)
                .OrAny(Object, Void, Byte, Bool, 
                    Char, String, Short, UShort, 
                    Int, UInt, Long, ULong, Float, Double, Decimal);

            DefineProduction(Inheritance)
                .As(Colon, InheritanceTypes)
                .Optional();

            DefineProduction(InheritanceTypes)
                .As(InheritanceTypes, Comma, TypeName)
                .Or(TypeName);

            DefineProduction(ClassBody)
                .As(MemberDeclarations);

            DefineProduction(MemberDeclarations)
                .As(MemberDeclarations, MemberDeclaration)
                .Or(MemberDeclaration)
                .Optional();

            DefineProduction(MemberDeclaration)
                .As(MemberSpecifiers, ClassDeclaration) // Expand later (fields, properties, nested classes, etc.)
                .Or(MemberSpecifiers, MethodDeclaration);

            DefineProduction(MethodDeclaration)
                .As(TypeName, Identifier, TypeArgumentList, LeftParenthesis, ParameterDefinitions, RightParenthesis, MethodBody);

            DefineProduction(ParameterDefinitions)
                .As(ParameterDefinitions, Comma, ParameterDefinition)
                .Or(ParameterDefinition)
                .Optional();

            DefineProduction(ParameterDefinition)
                .As(TypeName, Identifier);

            DefineProduction(MethodBody)
                .As(LeftCurlyBrace, MethodBodyInternal, RightCurlyBrace)
                .Or(Semicolon);

            DefineProduction(MethodBodyInternal)
                .Optional(); // To expand later
        }
    }
}
