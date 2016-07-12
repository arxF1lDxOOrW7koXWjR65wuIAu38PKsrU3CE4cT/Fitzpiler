using System;
using System.IO;

namespace Fitzpiler
{
    internal class MainProgram
    {
        private static void Main(string[] args)
        {
            var inFilePath = args[0];
            var outFilePath = args[1];
            try
            {
                var program = File.ReadAllLines(inFilePath);
                var scanner = new Scanner(program);
                var parser = new Parser(scanner);
                var generator = new Gen2Gen(parser.Program);
                Console.Write(generator.ToString());
                File.WriteAllText(outFilePath, generator.ToString());
            }
            catch (IOException)
            {
                Console.WriteLine("Could not open file for reading[" + inFilePath + "]");
            }
        }
    }
}