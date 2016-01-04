using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitzpiler
{
    enum TOKENTYPE
    {
        NUM,
        ID,
        RELOP,
        ADDOP,
        MULOP,
        ASSIGNOP,
        STOP
    }
    enum KEYWORD
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
        ELSE
    }
    
}