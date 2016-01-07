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
        private char[] charbuf = new char[32];//max size of 32 for any one token 
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
                while (c == ' ' || c == '\n')
                {
                    c = program[ptr++];
                }
                Token t = null;
                if (c > 64 && c < 123 && c != 93 && c != 91) // Letter
                {
                    do
                    {
                        if (c == 91 || c == 93)
                        {
                            ptr--;
                            break;
                        }
                        charbuf[bufcount++] = c;
                        c = program.Length > ptr ? program[ptr++] : (char)0;
                    } while (((c > 64 && c < 123) || c >= 48 && c <= 57));
                    string s = BufToString(charbuf);
                    Array.Clear(charbuf, 0, 32);
                    bufcount = 0;
                    t = new Token(TokenType.ID, s);
                    if (c == '(' || c == ',' || c== ';' || c == ')')
                    {
                        ptr--;
                    }
                }
                else if (c >= 48 && c <= 57) // Number
                {
                    do
                    {
                        charbuf[bufcount++] = c;
                        c = program.Length > ptr ? program[ptr++] : (char)0;
                    } while (c >= 48 && c <= 57);
                    string s = BufToString(charbuf);
                    Array.Clear(charbuf, 0, 32);
                    bufcount = 0;
                    t = new Token(TokenType.NUM, s);
                    ptr--;
                }
                else if (c == 123) //Begin comment block
                {
                    int commentCount = 0;
                    int commentEnd = 0;
                    for (int i = ptr; i < program.Length; i++)
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
                        t = new Token(TokenType.RELOP, program.Substring(ptr - 1, 2));
                        ptr += 2;
                    }
                    else
                    {
                        t = new Token(TokenType.RELOP, c.ToString());
                        ptr++; ;
                    }
                }
                else if (c == 61) //Equal sign
                {
                    t = new Token(TokenType.RELOP, c.ToString());
                    ptr++; ;
                }
                else if (c == 43 || c == 45 || c == 124) // ADDOPs
                {
                    t = new Token(TokenType.ADDOP, c.ToString());
                    ptr++;
                }
                else if (c == 42 || c == 47 || c == 38)// MULOPs
                {
                    t = new Token(TokenType.MULOP, c.ToString());
                    ptr++;
                }
                else if (c == 58 && program[ptr] == 61) // ASSIGNOP
                {
                    t = new Token(TokenType.ASSIGNOP, ":=");
                    ptr++;
                }
                else if (c == 59)
                {
                    t = new Token(TokenType.STOP, ";");
                    ptr++;
                }
                else if (c == 58 && program[ptr + 1] != 61)
                {
                    t = new Token(TokenType.STOP, ":");
                    ptr++;
                }
                else if (c == 46)
                {
                    t = new Token(TokenType.STOP, ".");
                    ptr++;
                }
                else if (c == 91)
                {
                    t = new Token(TokenType.STOP, "[");
                }
                else if (c == 93)
                {
                    t = new Token(TokenType.STOP, "]");
                    ptr++;
                }
                else if (c == '(')
                {
                    t = new Token(TokenType.STOP, "(");
                }
                else if (c == ')')
                {
                    t = new Token(TokenType.STOP, ")");
                }
                else if (c == ',')
                {
                    t = new Token(TokenType.STOP, ",");
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
        public TokenType TYPE;
        public string data;
        public Token(TokenType type, string data)
        {
            this.TYPE = type;
            this.data = data;
        }
        public override string ToString()
        {
            if (this.TYPE == TokenType.ADDOP) return "ADDOP: " + this.data;
            if (this.TYPE == TokenType.RELOP) return "RELOP: " + this.data;
            if (this.TYPE == TokenType.MULOP) return "MULOP: " + this.data;
            if (this.TYPE == TokenType.ID) return "ID: " + this.data;
            if (this.TYPE == TokenType.NUM) return "NUM: " + this.data;
            if (this.TYPE == TokenType.STOP) return "STOP:";
            return "ASSIGNOP: " + this.data;
        }
        public bool Match(KeyWord keyword)
        {
            var str = this.data;
            if (keyword == KeyWord.ARRAY && str == "ARRAY") return true;
            if (keyword == KeyWord.BEGIN && str == "BEGIN") return true;
            if (keyword == KeyWord.END && str == "END") return true;
            if (keyword == KeyWord.REAL && str == "REAL") return true;
            if (keyword == KeyWord.INTEGER && str == "INTEGER") return true;
            if (keyword == KeyWord.NOT && str == "NOT") return true;
            if (keyword == KeyWord.OF && str == "OF") return true;
            if (keyword == KeyWord.IF && str == "IF") return true;
            if (keyword == KeyWord.THEN && str == "THEN") return true;
            if (keyword == KeyWord.ELSE && str == "ELSE") return true;
            if (keyword == KeyWord.WHILE && str == "WHILE") return true;
            if (keyword == KeyWord.DO && str == "DO") return true;
            if (keyword == KeyWord.READ && str == "READ") return true;
            if (keyword == KeyWord.WRITE && str == "WRITE") return true;
            if (keyword == KeyWord.FUNCTION && str == "FUNCTION") return true;
            if (keyword == KeyWord.PROCEDURE && str == "PROCEDURE") return true;
            if (keyword == KeyWord.PROGRAM && str == "PROGRAM") return true;
            if (keyword == KeyWord.VAR && str == "VAR") return true;
            return false;
        }
        public bool Match(TokenType tokentype)
        {
            if (tokentype == this.TYPE) return true;
            return false;
        }
        
    }
}
