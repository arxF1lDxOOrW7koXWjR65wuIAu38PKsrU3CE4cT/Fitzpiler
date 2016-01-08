using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Fitzpiler
{
    class MainProgram
    {
        static void Main(string[] args)
        {
            string inFilePath = args[0];
            string outFilePath = args[1];
            try
            {
                string[] program = File.ReadAllLines(inFilePath);
                Scanner scanner = new Scanner(program);
                Parser parser = new Parser(scanner);
                Generator generator = new Generator(parser.program);
                Console.Write(generator.ToString());
                File.WriteAllText(outFilePath, generator.ToString());
            }
            catch(IOException e)
            {
                Console.WriteLine("Could not open file for reading[" + inFilePath + "]");
            }

        }
    }
}
