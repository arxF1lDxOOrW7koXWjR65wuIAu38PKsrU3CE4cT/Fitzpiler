using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitzpiler
{
    class Scanner
    {
        private string[] program;
        private Queue<Token> queue = new Queue<Token>();
        public Scanner(string [] program)
        {
            this.program = program;
            try
            {
                Tokenizer tokenizer = new Tokenizer(this.program);
                Token t;
                while ((t = tokenizer.Next()) != null)
                {
                    queue.Enqueue(t);
                    Console.WriteLine(t);
                }
            }
            catch(ParseFailedException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
