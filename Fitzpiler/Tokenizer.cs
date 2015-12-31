using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitzpiler
{
    class Tokenizer
    {
        private string program;
        private int ptr = 0;
        private char[] charbuf = new char[32];//max size of 32 for any one token -- probably not used
        private int bufcount = 0;
        public Tokenizer(string[] program)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in program) sb.Append(s + '\n');
            this.program = sb.ToString();
        }

        public Token Next()
        {
            top:
            try
            {
                if (ptr + 1 > program.Length) return null;
                char c = program[ptr++];
                while (c == ' ' || c == '\n') c = program[ptr++];
                Token t = null;
                if (c > 64 && c < 123) // Letter
                {
                    do
                    {
                        charbuf[bufcount++] = c;
                        c = program.Length > ptr ? program[ptr++] : (char)0;
                    } while ((c > 64 && c < 123) || c >= 48 && c <= 57);
                    string s = BufToString(charbuf);
                    Array.Clear(charbuf, 0, 32);
                    bufcount = 0;
                    t = new Token(TOKENTYPE.NUM, s);
                }
                else if (c >= 48 && c <= 57) // Number
                {
                    do
                    {
                        charbuf[bufcount++] = c;
                        c = program.Length > ptr ? program[ptr++] : (char) 0;
                    } while (c >= 48 && c <= 57);
                    string s = BufToString(charbuf);
                    Array.Clear(charbuf, 0, 32);
                    bufcount = 0;
                    t = new Token(TOKENTYPE.NUM, s);
                }
                else if (c == 123) //Begin comment block
                {
                    int commentCount = 0;
                    int commentEnd = 0;
                    for(int i = ptr; i < program.Length; i++)
                    {
                        if (program[i] == '}' && commentCount == 0)
                        {
                            commentEnd = i;
                            break;
                        }
                        else if (program[i] == '{') commentCount++;
                        else if (program[i] == '}') commentCount--;
                    }
                    if (commentEnd == 0) throw new ParseFailedException("Unclosed comment");
                    ptr = commentEnd;
                    ptr++;
                    goto top;
                }
                else if (c == 60 || c == 62) // Greater than or less than character
                {
                    if (program[ptr] == 61)
                    {
                        t = new Token(TOKENTYPE.RELOP, program.Substring(ptr - 1, 2));
                        ptr += 2;
                    }
                    else
                    {
                        t = new Token(TOKENTYPE.RELOP, c.ToString());
                        ptr++; ;
                    }
                }
                else if (c == 61) //Equal sign
                {
                    t = new Token(TOKENTYPE.RELOP, c.ToString());
                    ptr++; ;
                }
                else if (c == 43 || c == 45 || c == 124) // ADDOPs
                {
                    t = new Token(TOKENTYPE.ADDOP, c.ToString());
                    ptr++;
                }
                else if (c == 42 || c == 47 || c == 38)// MULOPs
                {
                    t = new Token(TOKENTYPE.MULOP, c.ToString());
                    ptr++;
                }
                else if (c == 58 && program[ptr + 1] == 61) // ASSIGNOP
                {
                    t = new Token(TOKENTYPE.ASSIGNOP, program.Substring(ptr - 1, 2));
                    ptr++;
                }
                else if(c == 59)
                {
                    t = new Token(TOKENTYPE.STOP, ""); //THIS NEEDS MORE WORK
                }
                return t;

            }
            catch(IndexOutOfRangeException e)
            {
                throw new ParseFailedException("Unexpected character", e);
            }
        }
        private static string BufToString(char[] str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str) if(c != 0) sb.Append(c);
            return sb.ToString();
        }
    }
    class Token
    {
        public TOKENTYPE TYPE;
        public string data;
        public Token(TOKENTYPE type, string data)
        {
            this.TYPE = type;
            this.data = data;
        }
        public override string ToString()
        {
            if (this.TYPE == TOKENTYPE.ADDOP) return "ADDOP: " + this.data;
            if (this.TYPE == TOKENTYPE.RELOP) return "RELOP: " + this.data;
            if (this.TYPE == TOKENTYPE.MULOP) return "MULOP: " + this.data;
            if (this.TYPE == TOKENTYPE.ID) return "ID: " + this.data;
            if (this.TYPE == TOKENTYPE.NUM) return "NUM: " + this.data;
            if (this.TYPE == TOKENTYPE.STOP) return "STOP:";
            return "ASSIGNOP: " + this.data;
        }
    }
}
