using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TestLanguage.Terminal;

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
            Assert.That(result[2].Lexeme, Is.EqualTo("@namespace"));

            test = "34.3f";
            result = testit();
            Assert.That(result[0], Is.TypeOf<NumberToken>());
            var number = result[0] as NumberToken;
            Assert.That(number.GetFloatValue(), Is.EqualTo(34.3f));
            Assert.That(number.IsTypeSpecified, Is.True);
            Assert.That(number.TypeSuffix, Is.EqualTo('f'));
        }
    }
}
