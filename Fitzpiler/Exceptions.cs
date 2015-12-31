using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitzpiler
{
    public class ParseFailedException : Exception
    {
        public ParseFailedException() { }
        public ParseFailedException(string message) : base(message) { }

        public ParseFailedException(string message, Exception inner) : base(message, inner) { }
    }
}
