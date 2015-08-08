using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TestLanguage.Terminal;
using static TestLanguage.Nonterminal;
using ParserGenerator;

namespace TestLanguage.Tests
{
    [TestFixture]
    public class LanguageTests
    {
        [Test]
        public void Language_DoesProduceCorrectTokens()
        {
            string test = null;

            Func<Grammar.Token[]> testit = () =>
                new Grammar.Lexer(new StringReader(test)).ToArray();

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
            string test;
            var grammar = new Grammar();
            Func<string, Grammar.Parser> getParser = str => grammar.GetParser(new StringReader(str));

            test = @"
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
    }
}
