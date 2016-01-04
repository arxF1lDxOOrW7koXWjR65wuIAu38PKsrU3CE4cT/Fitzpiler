using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitzpiler
{
    enum VarType // Realarray should not be used
    {
        INTEGER,
        REAL,
        FUNCTION,
        REALARRAY,
        INTEGERARRAY
    }

    abstract class Statement { }

    class Assignment : Statement
    {
        public string varname { get; }
        public Expression expression { get; }
        public Assignment(string varname, Expression expression)
        {
            this.varname = varname;
            this.expression = expression;
        }
    }

    class IfStatement : Statement
    {
        private Expression expression1;
        private Statement statement11;
        private Statement statement21;

        public IfStatement(Expression expression1, Statement statement11, Statement statement21)
        {
            this.expression1 = expression1;
            this.statement11 = statement11;
            this.statement21 = statement21;
        }

        public Expression expression { get; }
       public Statement statement1 { get; } 
       public Statement statement2 { get; } 
    }

    class WhileStatement : Statement
    {
        Expression expression;
        Statement statement;
    }

    abstract class Expression
    {

    }

    abstract class Subprogram { }

    class Function
    {
        Dictionary<string, VarType> vartypes;
        List<Statement> statements;
    }

}
