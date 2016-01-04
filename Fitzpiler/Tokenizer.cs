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
        public int line { get; private set; } = 0;
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
                while (c == ' ' || c == '\n')
                {
                    if (c == '\n') line++;
                    c = program[ptr++];
                }
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
                    t = new Token(TOKENTYPE.ID, s);
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
                else if (c == 58 && program[ptr] == 61) // ASSIGNOP
                {
                    t = new Token(TOKENTYPE.ASSIGNOP, ":=");
                    ptr++;
                }
                else if(c == 59)
                {
                    t = new Token(TOKENTYPE.STOP, ";");
                    ptr++;
                }
                else if (c == 58 && program[ptr + 1] != 61)
                {
                    t = new Token(TOKENTYPE.STOP, ":");
                    ptr++;
                }
                else if (c == 46)
                {
                    t = new Token(TOKENTYPE.STOP, "."); 
                    ptr++;
                }
                else if (c == 91)
                {
                    t = new Token(TOKENTYPE.STOP, "[");
                    ptr++;
                }
                else if (c == 93)
                {
                    t = new Token(TOKENTYPE.STOP, "]");
                    ptr++;
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
        public bool Match(KEYWORD keyword)
        {
            var str = this.data;
            if (keyword == KEYWORD.ARRAY && str == "ARRAY") return true;
            if (keyword == KEYWORD.BEGIN && str == "BEGIN") return true;
            if (keyword == KEYWORD.END && str == "END") return true;
            if (keyword == KEYWORD.REAL && str == "REAL") return true;
            if (keyword == KEYWORD.INTEGER && str == "INTEGER") return true;
            if (keyword == KEYWORD.NOT && str == "NOT") return true;
            if (keyword == KEYWORD.IF && str == "IF") return true;
            if (keyword == KEYWORD.WHILE && str == "WHILE") return true;
            if (keyword == KEYWORD.READ && str == "READ") return true;
            if (keyword == KEYWORD.WRITE && str == "WRITE") return true;
            if (keyword == KEYWORD.FUNCTION && str == "FUNCTION") return true;
            if (keyword == KEYWORD.PROCEDURE && str == "PROCEDURE") return true;
            if (keyword == KEYWORD.PROGRAM && str == "PROGRAM") return true;
            if (keyword == KEYWORD.VAR && str == "VAR") return true;
            return false;
        }
        public bool Match(TOKENTYPE tokentype)
        {
            if (tokentype == this.TYPE) return true;
            return false;
        }
        
    }
}
