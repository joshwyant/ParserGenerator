using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LR0Generator
{
    class Program
    {
        public static void Main()
        {
            var p = new Parser(new MyGrammar(), "if (x) { for ( var i = 0; i < 10; i += 1) a = 3; } a = y");

            var ast = p.ParseAst();

            var sw = new StringWriter();

            ast.Print(sw);

            Console.WriteLine(sw.ToString());

            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
