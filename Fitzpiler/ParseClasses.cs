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
        public Variable variable { get; }
        public Expression expression { get; }
        public Assignment(Variable variable, Expression expression)
        {
            this.variable = variable;
            this.expression = expression;
        }
        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += "ASSIGNOP\n";
            outstr += variable.ToString(level + 1) + "\n";
            outstr += expression.ToString(level + 1);
            return outstr;
        }
    }

    class IfStatement : Statement
    {

        public IfStatement(Expression expression1, Statement statement1, Statement statement2)
        {
            this.expression = expression1;
            this.statement1 = statement1;
            this.statement2 = statement2;
        }

       public Expression expression { get; }
       public Statement statement1 { get; } 
       public Statement statement2 { get; } 

        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += "IF: \n" + expression.ToString(level + 1) + "\n";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += "THEN: \n";
            outstr += statement1.ToString(level + 1) + "\n";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += "ELSE: \n";
            outstr += statement2.ToString(level + 1) + "\n";
            return outstr;
        }
    }

    class WhileStatement : Statement
    {
        public Expression expression { get; }
        public Statement statement { get; }

        public WhileStatement(Expression expression, Statement statement)
        {
            this.expression = expression;
            this.statement = statement;
        }

        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += "WHILE:\n";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += expression.ToString(level + 1) + "\n";
            outstr += "DO:\n";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += statement.ToString(level + 1) + "\n";
            return outstr;
        }
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
            for (int i = 0; i < level; i++) outstr += "-";
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
            outstr += expression1.ToString(level + 1) + "\n";
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
        public Expression accessor { get; }
        public Variable(string varname, Expression accessor)
        {
            this.varname = varname;
            this.accessor = accessor;
        }
        public Variable(string varname)
        {
            this.varname = varname;
        }
        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += varname;
            if (this.accessor != null) outstr += "\n-ARRAY.ACCESS\n" + this.accessor.ToString(level + 1);
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
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += number;
            return outstr;
        }
    }


    class Subroutine : Expression
    {
        public string name { get; }
        public Dictionary<string, VarType> vartypes { get; set; }
        public List<Statement> statements { get; set; }
        public Dictionary<string, VarType> arguments { get; }
        public VarType returnType { get; }
        public Subroutine(string name, Dictionary<string, VarType> args, VarType returnType)
        {
            this.name = name;
            this.arguments = args;
            this.returnType = returnType;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\nFUNCTION: " + this.name + "\nVariables:\n");
            if (vartypes != null) foreach (var variable in vartypes.Keys) sb.Append("\t" + variable + ": " + vartypes[variable] + "\n");
         //   sb.Append("\nSubprograms:\n");
       //     if (subprograms != null) foreach (var subroutine in subprograms) sb.Append("\t" + subroutine + "\n");
            sb.Append("\nStatements:\n");
            if (statements != null)
            {
                foreach (Statement s in statements)
                {
                    sb.Append(s.ToString(0) + "\n\n");
                }
            }
            return sb.ToString();
        }

        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += "SUBROUTINE_CALL: " + this.name + "\n";
            for (int i = 0; i < level + 1; i++) outstr += "-";
            outstr += "ARGS: \n";

            return outstr;
        }
    }

    class Function
    {
        VarType returntype;
        Dictionary<string, VarType> vartypes;
        List<Statement> statements;
    }
    class Program
    {
        public string id;
        public Dictionary<string, VarType> vartypes = new Dictionary<string, VarType>();
        public Dictionary<string, Subroutine> subprograms = new Dictionary<string, Subroutine>();
        public List<Statement> statements = new List<Statement>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("PROGRAM: " + this.id + "\n---------------------------------------------------\nVariables:\n");
            if (vartypes != null) foreach (var variable in vartypes.Keys) sb.Append("\t" + variable + ": " + vartypes[variable] + "\n");
            sb.Append("---------------------------------------------------\nSubprograms:\n");
            if (subprograms != null) foreach (Subroutine subroutine in subprograms.Values) sb.Append("\t\n" + subroutine.ToString());
            sb.Append("---------------------------------------------------\nStatements:\n");
            if (statements != null)
            {
                foreach (Statement s in statements)
                {
                    sb.Append("\n\n" + s.ToString(0));
                }
            }
            return sb.ToString();
        }
    }
}
