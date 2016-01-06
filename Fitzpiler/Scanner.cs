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
        private Stack<Token> doubleQueue = new Stack<Token>(); //Second stack may not be necessary
        private Token current;
        public Tokenizer tokenizer { get; private set; }
        public Scanner(string [] program)
        {
            this.program = program;
            try
            {
                tokenizer = new Tokenizer(this.program);
                Token t;
                while ((t = tokenizer.Next()) != null)
                {
                    queue.Enqueue(t);
                 //   Console.WriteLine(t);
                }
            }
            catch(ParseFailedException e)
            {
                Console.WriteLine("Tokenization failed:\n\t" +  e.Message);
            }
        }
        public Token Pop()
        {
            current = queue.Dequeue();
            doubleQueue.Push(current);
            return current;
        }
        public Token Peek()
        {
            return queue.Peek();
        }
        public void Rewind() //Remove if unused, may not work anyways
        {
            current = doubleQueue.Pop();
            queue.Enqueue(current);
        }
    }
}
