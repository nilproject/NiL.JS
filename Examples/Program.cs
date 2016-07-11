using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExamplesFramework;

namespace Examples
{
    class Program
    {
        private static IList<KeyValuePair<string, IList<Example>>> examples = ExamplesLoader.LoadExamples(typeof(Program).Assembly);

        static void Main(string[] args)
        {
            for (;;)
            {
                var categoryIndex = -1;
                do
                {
                    Console.Clear();
                    for (var i = 0; i < examples.Count; i++)
                    {
                        Console.WriteLine(i + ". " + examples[i].Key);
                    }
                    Console.Write("Select category (index from 0 to " + (examples.Count - 1) + "): ");
                    if (!int.TryParse(Console.ReadLine(), out categoryIndex))
                        categoryIndex = -1;
                }
                while (categoryIndex < 0 || categoryIndex >= examples.Count);

                var exampleIndex = -1;
                do
                {
                    Console.Clear();
                    Console.WriteLine("Category: " + examples[categoryIndex].Key);
                    for (var i = 0; i < examples[categoryIndex].Value.Count; i++)
                    {
                        Console.WriteLine(i + ". " + ExamplesLoader.PrepareExampleName(examples[categoryIndex].Value[i].GetType().Name));
                    }
                    Console.Write("Select example (index from 0 to " + (examples[categoryIndex].Value.Count - 1) + "): ");
                    if (!int.TryParse(Console.ReadLine(), out exampleIndex))
                        exampleIndex = -1;
                }
                while (exampleIndex < 0 || exampleIndex >= examples[categoryIndex].Value.Count);

                Console.Clear();

                examples[categoryIndex].Value[exampleIndex].Run();

                Console.WriteLine("Press any key to return to main menu");
                Console.ReadKey();
            }
        }
    }
}
