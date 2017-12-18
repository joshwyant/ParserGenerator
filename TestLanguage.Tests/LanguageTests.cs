using NUnit.Framework;
using System.IO;
using System.Linq;
using static TestLanguage.Terminal;
using static TestLanguage.Nonterminal;
using ParserGenerator;

namespace TestLanguage.Tests
{
    [TestFixture]
    public class LanguageTests
    {
        Grammar grammar;

        [SetUp]
        public void Init()
        {
            grammar = new Grammar();
        }

        Grammar.Parser getParser(string str)
        {
            return grammar.GetParser(new StringReader(str));
        }

        [Test]
        public void Language_DoesProduceCorrectTokens()
        {
            string test = null;

            Grammar.Token[] testit() => new Grammar.Lexer(new StringReader(test)).ToArray();

            test = "using namespace @namespace;";

            var expected = new[] { Using, Namespace, Identifier, Semicolon, Eof };

            var result = testit();
            var resultTerminals = result.Select(r => r.Terminal).ToArray();
            
            Assert.That(resultTerminals, Is.EquivalentTo(expected));
            Assert.That(result[2], Is.AssignableFrom<IdentifierToken>());
            Assert.That((result[2] as IdentifierToken).Name, Is.EqualTo("namespace"));
            Assert.That(result[2].Lexeme, Is.EqualTo("@namespace"));
            Assert.That(result.AsLexemes(), Is.EquivalentTo(new[] { "using", "namespace", "@namespace", ";", null }));

            test = "34.3f";
            result = testit();
            Assert.That(result[0], Is.TypeOf<NumberToken>());
            var number = result[0] as NumberToken;
            Assert.That(number.GetFloatValue(), Is.EqualTo(34.3f));
            Assert.That(number.IsTypeSpecified, Is.True);
            Assert.That(number.TypeSuffix, Is.EqualTo('f'));
        }

        [Test]
        public void Language_Namespaces()
        {
            string test = @"
using System;
using System.Collections.Generic;

namespace A {
}
namespace B.C {}";
            
            var p = getParser(test);
            var ast = p.Parse();

            Assert.That(p.HasErrors, Is.False);

            var usingdecls = ast.SearchAll(UsingDirective).ToArray();

            Assert.That(usingdecls.Length, Is.EqualTo(2));
            Assert.That(
                usingdecls[0].SearchAll(Identifier).AsLexemes(),
                Is.EquivalentTo(new[] { "System" })
            );
            Assert.That(
                usingdecls[1].SearchAll(Identifier).AsLexemes(),
                Is.EquivalentTo(new[] { "System", "Collections", "Generic" })
            );

            var ns = ast.SearchAll(NamespaceDeclaration).ToArray();

            Assert.That(ns.Length, Is.EqualTo(2));
            Assert.That(
                ns[0].Search(FullyQualifiedNamespace).SearchAll(Identifier).AsLexemes(),
                Is.EquivalentTo(new[] { "A" })
            );
            Assert.That(
                ns[1].Search(FullyQualifiedNamespace).SearchAll(Identifier).AsLexemes(),
                Is.EquivalentTo(new[] { "B", "C" })
            );
        }

        [Test]
        public void Language_Classes()
        {
            string test = @"
namespace ABC {
    public class A {
        static class AB<T<S,R>> : b { }
    }

    class b : List<int> {
        
    }
}
class inRootNamespace{}";
            var p = getParser(test);
            var ast = p.Parse();

            Assert.That(p.HasErrors, Is.False);

            var classes = ast.SearchAll(ClassDeclaration).ToArray();

            Assert.That(classes.Length, Is.EqualTo(3));

            Assert.That(classes[0].Search(Inheritance).IsEmpty, Is.True);
            Assert.That(classes[1].Search(Inheritance).IsEmpty, Is.False);
            Assert.That(classes[1].Search(Inheritance).Search(TypeName).AsLexemes(), Is.EquivalentTo(new[] { "List", "<", "int", ">" }));

            var nestedClass = classes[0].Search(ClassBody).Search(ClassDeclaration);
            Assert.That(nestedClass, Is.Not.Null);
            Assert.That(nestedClass.Search(SimpleTypeName).AsLexemes().First(), Is.EqualTo("AB"));
            Assert.That(nestedClass.Search(TypeArgumentList).IsEmpty, Is.False);
            Assert.That(nestedClass.Search(TypeArgumentList).AsLexemes(), Is.EquivalentTo(new[] { "<", "T", "<", "S", ",", "R", ">", ">" }));
            Assert.That(nestedClass.Search(Inheritance).Search(TypeName).AsLexemes().Single(), Is.EqualTo("b"));
        }

        [Test]
        public void Language_ClassMembers()
        {
            string test = @"
namespace ns {
  class c {
    int main(string args, int count) { }
    public abstract void DoSomething<T<S,R>>();
  }
}";

            var p = getParser(test);
            var ast = p.Parse();

            Assert.That(p.HasErrors, Is.False);
            
        }
    }
}
