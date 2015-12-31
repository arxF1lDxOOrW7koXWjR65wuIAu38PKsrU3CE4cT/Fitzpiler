using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Fitzpiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string filepath = args[0];
            try
            {
                string[] program = File.ReadAllLines(filepath);
                Scanner scanner = new Scanner(program);
            }
            catch(IOException e)
            {
                Console.WriteLine("Could not open file for reading[" + filepath + "]");
            }

        }
    }
}
