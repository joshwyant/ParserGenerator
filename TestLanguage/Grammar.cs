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
                .As(UsingDirectivesOptional, NamespaceDeclarationsOptional);

            DefineProduction (UsingDirectivesOptional)
                .As(UsingDirectives)
                .Optional();

            DefineProduction (UsingDirectives)
                .As(UsingDirectives, UsingDirective)
                .Or(UsingDirective);

            DefineProduction (UsingDirective)
                .As(Using, FullyQualifiedNamespace, Semicolon);

            DefineProduction(FullyQualifiedNamespace)
                .As(FullyQualifiedNamespace, Dot, Identifier)
                .Or(Identifier);

            DefineProduction(NamespaceDeclarationsOptional)
                .As(NamespaceDeclarations)
                .Optional();

            DefineProduction(NamespaceDeclarations)
                .As(NamespaceDeclarations, NamespaceDeclaration)
                .Or(NamespaceDeclaration);

            DefineProduction(NamespaceDeclaration)
                .As(Namespace, FullyQualifiedNamespace, LeftCurlyBrace, CompileUnit, RightCurlyBrace);
        }
    }
}
