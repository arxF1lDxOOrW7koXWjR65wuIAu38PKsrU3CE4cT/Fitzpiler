using System;
using System.Collections.Generic;

namespace Fitzpiler
{
    public class Scanner
    {
        private string[] program;
        private readonly Queue<Token> _queue = new Queue<Token>();
        private readonly Stack<Token> _doubleQueue = new Stack<Token>(); //Second stack may not be necessary
        private Token _current;
        public Tokenizer Tokenizer { get; }
        public Scanner(string [] program)
        {
            this.program = program;
            try
            {
                Tokenizer = new Tokenizer(this.program);
                Token t;
                while ((t = Tokenizer.Next()) != null)
                {
                    _queue.Enqueue(t);
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
            _current = _queue.Dequeue();
            _doubleQueue.Push(_current);
            return _current;
        }
        public Token Peek()
        {
            return _queue.Peek();
        }
        public void Rewind() //Remove if unused, may not work anyways
        {
            _current = _doubleQueue.Pop();
            _queue.Enqueue(_current);
        }
    }
}
