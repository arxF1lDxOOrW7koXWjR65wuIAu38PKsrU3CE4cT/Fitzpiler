using System;
using System.Text;

namespace Fitzpiler
{
    public class Tokenizer
    {
        private int _bufcount;
        private readonly char[] _charbuf = new char[32]; //max size of 32 for any one token 
        private readonly string _program;
        private int _ptr;

        public Tokenizer(string[] program)
        {
            var sb = new StringBuilder();
            foreach (var s in program) sb.Append(s + '\n');
            this._program = sb.ToString();
        }

        public Token Next()
        {
            top:
            try
            {
                if (_ptr + 1 > _program.Length) return null;
                var c = _program[_ptr++];
                while (c == ' ' || c == '\n' || c == '\t')
                {
                    c = _program[_ptr++];
                }
                Token t = null;
                if (c > 64 && c < 123 && c != 93 && c != 91) // Letter
                {
                    do
                    {
                        if (c == 91 || c == 93)
                        {
                            _ptr--;
                            break;
                        }
                        _charbuf[_bufcount++] = c;
                        c = _program.Length > _ptr ? _program[_ptr++] : (char) 0;
                    } while ((c > 64 && c < 123) || c >= 48 && c <= 57);
                    var s = BufToString(_charbuf);
                    Array.Clear(_charbuf, 0, 32);
                    _bufcount = 0;
                    t = new Token(TokenType.Id, s);
                    if (c == '(' || c == ',' || c == ';' || c == ')' || c == '[')
                    {
                        _ptr--;
                    }
                }
                else if (c >= 48 && c <= 57) // Number
                {
                    do
                    {
                        _charbuf[_bufcount++] = c;
                        c = _program.Length > _ptr ? _program[_ptr++] : (char) 0;
                    } while (c >= 48 && c <= 57);
                    var s = BufToString(_charbuf);
                    Array.Clear(_charbuf, 0, 32);
                    _bufcount = 0;
                    t = new Token(TokenType.Num, s);
                    _ptr--;
                }
                else if (c == 123) //Begin comment block
                {
                    var commentCount = 0;
                    var commentEnd = 0;
                    for (var i = _ptr; i < _program.Length; i++)
                    {
                        if (_program[i] == '}' && commentCount == 0)
                        {
                            commentEnd = i;
                            break;
                        }
                        if (_program[i] == '{') commentCount++;
                        else if (_program[i] == '}') commentCount--;
                    }
                    if (commentEnd == 0) throw new ParseFailedException("Unclosed comment");
                    _ptr = commentEnd;
                    _ptr++;
                    goto top;
                }
                else if (c == 60 || c == 62) // Greater than or less than character
                {
                    if (_program[_ptr] == 61)
                    {
                        t = new Token(TokenType.Relop, _program.Substring(_ptr - 1, 2));
                        _ptr += 2;
                    }
                    else
                    {
                        t = new Token(TokenType.Relop, c.ToString());
                        _ptr++;
                        
                    }
                }
                else if (c == 61) //Equal sign
                {
                    t = new Token(TokenType.Relop, c.ToString());
                    _ptr++;
                    
                }
                else if (c == 43 || c == 45 || c == 124) // ADDOPs
                {
                    t = new Token(TokenType.Addop, c.ToString());
                    _ptr++;
                }
                else if (c == 42 || c == 47 || c == 38) // MULOPs
                {
                    t = new Token(TokenType.Mulop, c.ToString());
                    _ptr++;
                }
                else if (c == 58 && _program[_ptr] == 61) // ASSIGNOP
                {
                    t = new Token(TokenType.Assignop, ":=");
                    _ptr++;
                }
                else if (c == 59)
                {
                    t = new Token(TokenType.Stop, ";");
                    _ptr++;
                }
                else if (c == 58 && _program[_ptr + 1] != 61)
                {
                    t = new Token(TokenType.Stop, ":");
                    _ptr++;
                }
                else if (c == 46)
                {
                    t = new Token(TokenType.Stop, ".");
                    // ptr++;
                }
                else if (c == 91)
                {
                    t = new Token(TokenType.Stop, "[");
                }
                else if (c == 93)
                {
                    t = new Token(TokenType.Stop, "]");
                    _ptr++;
                }
                else if (c == '(')
                {
                    t = new Token(TokenType.Stop, "(");
                }
                else if (c == ')')
                {
                    t = new Token(TokenType.Stop, ")");
                }
                else if (c == ',')
                {
                    t = new Token(TokenType.Stop, ",");
                }
                return t;
            }
            catch (IndexOutOfRangeException e)
            {
                throw new ParseFailedException("Unexpected character", e);
            }
        }

        private static string BufToString(char[] str)
        {
            var sb = new StringBuilder();
            foreach (var c in str) if (c != 0) sb.Append(c);
            return sb.ToString();
        }
    }

    public class Token
    {
        public string Data;
        public TokenType Type;

        public Token(TokenType type, string data)
        {
            Type = type;
            this.Data = data;
        }

        public override string ToString()
        {
            if (Type == TokenType.Addop) return "ADDOP: " + Data;
            if (Type == TokenType.Relop) return "RELOP: " + Data;
            if (Type == TokenType.Mulop) return "MULOP: " + Data;
            if (Type == TokenType.Id) return "ID: " + Data;
            if (Type == TokenType.Num) return "NUM: " + Data;
            if (Type == TokenType.Stop) return "STOP:";
            return "ASSIGNOP: " + Data;
        }

        public bool Match(KeyWord keyword)
        {
            var str = Data;
            if (keyword == KeyWord.Array && str == "ARRAY") return true;
            if (keyword == KeyWord.Begin && str == "BEGIN") return true;
            if (keyword == KeyWord.End && str == "END") return true;
            if (keyword == KeyWord.Real && str == "REAL") return true;
            if (keyword == KeyWord.Integer && str == "INTEGER") return true;
            if (keyword == KeyWord.Not && str == "NOT") return true;
            if (keyword == KeyWord.Of && str == "OF") return true;
            if (keyword == KeyWord.If && str == "IF") return true;
            if (keyword == KeyWord.Then && str == "THEN") return true;
            if (keyword == KeyWord.Else && str == "ELSE") return true;
            if (keyword == KeyWord.While && str == "WHILE") return true;
            if (keyword == KeyWord.Do && str == "DO") return true;
            if (keyword == KeyWord.Read && str == "READ") return true;
            if (keyword == KeyWord.Write && str == "WRITE") return true;
            if (keyword == KeyWord.Function && str == "FUNCTION") return true;
            if (keyword == KeyWord.Procedure && str == "PROCEDURE") return true;
            if (keyword == KeyWord.Program && str == "PROGRAM") return true;
            if (keyword == KeyWord.Var && str == "VAR") return true;
            return false;
        }

        public bool Match(TokenType tokentype)
        {
            if (tokentype == Type) return true;
            return false;
        }
    }
}