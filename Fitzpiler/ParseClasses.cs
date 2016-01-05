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

    abstract class Statement
    {
        public virtual string ToString(int level)
        {
            return "\n";
        }
    }

    class Assignment : Statement
    {
        public string varname { get; }
        public Expression expression { get; }
        public Assignment(string varname, Expression expression)
        {
            this.varname = varname;
            this.expression = expression;
        }
        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level + 1; i++) outstr += "--";
            outstr += varname + "\n";
            outstr += expression.ToString(level + 1);
            return outstr;
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

        public override string ToString(int level)
        {
            string outstr = "";
            outstr += "IF: " + expression1.ToString(level + 1);
            outstr += "THEN: " + statement1.ToString(level + 1);
            outstr += "ELSE: " + statement2.ToString(level + 1);
            return outstr;
        }
    }

    class WhileStatement : Statement
    {
        Expression expression;
        Statement statement;
    }

    abstract class Expression
    {
        public virtual string ToString(int level) { return "\n"; }
    }

    class Operation : Expression
    {
        Expression expression1;
        Op operation;
        Expression expression2;

        public Operation(Expression expression1, Op operation, Expression expression2)
        {
            this.expression1 = expression1;
            this.operation = operation;
            this.expression2 = expression2;
        }

        public override string ToString(int level)
        {
            string outstr = "";
            outstr += expression1.ToString(level + 1);
            for (int i = 0; i < level + 1; i++) outstr += "--";
            switch (operation)
            {
                case Op.ADD:
                    outstr += "OP.ADD\n";
                    break;
                case Op.SUB:
                    outstr += "OP.SUB\n";
                    break;
                case Op.MULT:
                    outstr += "OP.MULT\n";
                    break;
                case Op.DIVIDE:
                    outstr += "OP.DIVIDE\n";
                    break;
                case Op.EQ:
                    outstr += "RELOP.EQ\n";
                    break;
                case Op.GRT:
                    outstr += "RELOP.GRT\n";
                    break;
                case Op.LST:
                    outstr += "RELOP.LST\n";
                    break;
                case Op.GEQ:
                    outstr += "RELOP.GEQ\n";
                    break;
                case Op.LEQ:
                    outstr += "RELOP.LEQ\n";
                    break;
            }
            outstr += expression2.ToString(level + 1);
            return outstr;
        }
    }

    enum Op
    {
        ADD,
        SUB,
        MULT,
        DIVIDE,
        MOD,
        AND,
        OR,
        EQ,
        LEQ,
        GEQ,
        LST,
        GRT
    }

    class Variable : Expression
    {
        public string varname { get; }
        public Variable(string varname) { this.varname = varname; }
        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level + 1; i++) outstr += "--";
            outstr += varname + "\n";
            return outstr;
        }
    }

    class Number : Expression
    {
        public string number { get; }
        public Number(string number) { this.number = number; }

        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level + 1; i++) outstr += "--";
            outstr += number + "\n";
            return outstr;
        }
    }


    abstract class Subprogram { }

    class Function
    {
        Dictionary<string, VarType> vartypes;
        List<Statement> statements;
    }

}
