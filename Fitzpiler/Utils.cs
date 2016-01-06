using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitzpiler
{
    enum TokenType
    {
        NUM,
        ID,
        RELOP,
        ADDOP,
        MULOP,
        ASSIGNOP,
        STOP
    }
    enum KeyWord
    {
        PROGRAM,
        VAR,
        ARRAY,
        INTEGER,
        REAL,
        FUNCTION,
        PROCEDURE,
        BEGIN,
        END,
        IF,
        WHILE,
        READ,
        WRITE,
        NOT,
        THEN,
        ELSE,
        DO,
        OF
    }
    
}