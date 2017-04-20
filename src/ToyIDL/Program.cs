using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ToyIDL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string[] tests = Directory.GetFiles("../../test/", "*.tidl", SearchOption.AllDirectories);
            foreach (string test in tests)
            {
                try
                {
                    Parser parser = new Parser(File.ReadAllText(test));
                    Console.WriteLine($"{test} parsed successfully");
                    Console.WriteLine("------------------------");
                    Console.WriteLine(parser.Interface);
                    Console.WriteLine("------------------------");
                }
                catch (IdlParserException e)
                {
                    Console.WriteLine($"{test} parse error - {e.Message}");
                }
            }
        }
    }
}
