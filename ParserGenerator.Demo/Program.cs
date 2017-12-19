using System;
using System.IO;
using System.Linq;

namespace ParserGenerator.Demo
{
    class Program
    {
        public static void Main()
        {
            // Demo(new MySLRGrammar(), "var t = x + 3; if (y) { t += 1; }");

            Demo(
                new MyLALRGrammar(), 
                "if (5) { g<a, b>(c); } List<int> numbers = 3;"
            );

            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void Demo(LRkGrammar<Terminal, Nonterminal> grammar, string toParse)
        {
            Console.WriteLine($"{grammar.GetType().Name}:");
            Console.WriteLine();

            var t = grammar.States;

            var hasConflicts = false;

            var slr = grammar as SLRGrammar<Terminal, Nonterminal>;
            (SLRGrammar<Terminal, Nonterminal>.LRItem, SLRGrammar<Terminal, Nonterminal>.Symbol)[] srConflicts = null;
            SLRGrammar<Terminal, Nonterminal>.LRItemSet[] rrConflicts = null;

            if (slr != null)
            {
                srConflicts = slr.ShiftReduceConflicts();
                rrConflicts = slr.ReduceReduceConflicts();
                hasConflicts = srConflicts.Length > 0 || rrConflicts.Length > 0;
            }

            if (!hasConflicts)
            {
                while (!string.IsNullOrEmpty(toParse))
                {
                    var p = grammar.GetParser(new StringReader(toParse));
                    var ast = p.Parse(); // Could just call grammar.Parse(string), but we want to see the errors.

                    if (p.Errors.Count > 0)
                    {
                        foreach (var error in p.Errors)
                            Console.WriteLine(error);
                    }
                    else
                    {
                        var sw = new StringWriter();
                        ast.Print(sw);
                        Console.WriteLine(sw);
                    }

                    Console.Write(" > ");
                    toParse = Console.ReadLine();
                }

                Console.WriteLine();
            }
            else
            {
                if (srConflicts?.Length > 0)
                {
                    Console.WriteLine("Shift/Reduce conflicts (would favor shift):");

                    foreach (var conflict in srConflicts)
                    {
                        Console.WriteLine($"  {conflict}");
                    }
                    Console.WriteLine();
                }
                if (rrConflicts?.Length > 0)
                {
                    Console.WriteLine("Reduce/Reduce conflicts (critical):");

                    for (var i = 0; i < rrConflicts.Length; i++)
                    {
                        Console.WriteLine($"=> {i}");

                        var conflictSet = rrConflicts[i].OrderBy(c => c.Marker).ToArray();

                        Console.WriteLine("  Nonkernel:");
                        foreach (var item in conflictSet.Where(it => !it.IsKernel))
                        {
                            Console.WriteLine($"    {item}");
                        }

                        Console.WriteLine("  Kernel:");
                        foreach (var item in conflictSet.Where(it => it.IsKernel))
                        {
                            Console.WriteLine($"    {item}");
                        }
                    }
                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }
    }
}
