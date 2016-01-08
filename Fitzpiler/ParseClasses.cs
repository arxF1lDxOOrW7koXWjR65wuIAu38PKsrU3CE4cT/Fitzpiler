using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitzpiler
{
    public enum VarType
    {
        INTEGER,
        REAL,
        FUNCTION,
        REALARRAY,
        INTEGERARRAY,
        VOID
    }

    public abstract class Statement
    {
        public virtual string ToString(int level)
        {
            return "\n";
        }

        public virtual IEnumerable<string> GetVars()
        {
            yield return null;
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
        public override IEnumerable<string> GetVars()
        {
            yield return this.variable.varname;
            if (this.variable.accessor != null) foreach (string s in this.variable.accessor.GetVars()) yield return s;
            foreach (string s in this.expression.GetVars()) yield return s;
        }
    }

    class Procedure : Statement
    {
        public string funcname { get; }
        public List<Expression> arguments { get; }
        public Procedure(string funcname, List<Expression> args)
        {
            this.funcname = funcname;
            this.arguments = args;
        }
        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += "PROC." + this.funcname + "\n";
            foreach (Expression arg in arguments) outstr += "|" + arg.ToString(level);
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

        public override IEnumerable<string> GetVars()
        {
            foreach (string s in this.expression.GetVars()) yield return s;
            foreach (string s in this.statement1.GetVars()) yield return s;
            foreach (string s in this.statement2.GetVars()) yield return s;
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
        public override IEnumerable<string> GetVars()
        {
            foreach (string s in this.expression.GetVars()) yield return s;
            foreach (string s in this.statement.GetVars()) yield return s;
        }
    }

    class ReadStatement : Statement
    {
        public Variable variable { get; }
        public ReadStatement(Variable variable)
        {
            this.variable = variable;
        }
        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += "SYS.READ\n";
            outstr += variable.ToString(level + 1);
            return outstr;
        }
        public override IEnumerable<string> GetVars()
        {
            yield return variable.varname;
        }
    }

    class WriteStatement : Statement
    {
        public Expression expression { get; }
        public WriteStatement(Expression expression)
        {
            this.expression = expression;
        }
        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += "SYS.WRITE\n";
            outstr += expression.ToString(level + 1);
            return outstr;
        }
        public override IEnumerable<string> GetVars()
        {
            foreach (string s in expression.GetVars()) yield return s;
        }
    }

    public abstract class Expression
    {
        public virtual string ToString(int level) { return "\n"; }
        public virtual IEnumerable<string> GetVars()
        {
            yield return null;
        }
    }

    class Operation : Expression
    {
        public Expression expression1 { get; }
        public Op operation { get; }
        public Expression expression2 { get; }

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
        public override IEnumerable<string> GetVars()
        {
            foreach (string s in this.expression1.GetVars()) yield return s;
            foreach (string s in this.expression2.GetVars()) yield return s;
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
        public override IEnumerable<string> GetVars()
        {
            yield return this.varname;
        }
    }

    class Function : Expression
    {
        string funcname { get; }
        List<Expression> arguments { get; }

        public Function(string funcname, List<Expression> args)
        {
            this.funcname = funcname;
            this.arguments = args;
        }
        public override string ToString(int level)
        {
            string outstr = "";
            for (int i = 0; i < level; i++) outstr += "-";
            outstr += "FUNC." + this.funcname + "\n";
            foreach (Expression arg in arguments) outstr += "|" + arg.ToString(level);
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


    public class Subroutine : Expression
    {
        public string name { get; }
        public Dictionary<string, VarType> vartypes { get; set; }
        public List<Statement> statements { get; set; }
        public Dictionary<string, VarType> parameters { get; }
        public VarType returnType { get; }
        public Subroutine(string name, Dictionary<string, VarType> args, VarType returnType)
        {
            this.name = name;
            this.parameters = args;
            this.returnType = returnType;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.returnType == VarType.VOID) sb.Append("\nPROCEDURE:\n\t");
            else sb.Append("\nFUNCTION:\n\t");
            sb.Append(this.name + "\n\nReturn Type:\n\t" + this.returnType + "\n\nParameters:");
            foreach (var parameter in this.parameters.Keys) sb.Append("\n\t" + parameter + ":\t" + this.parameters[parameter] + "\n\nVariables:\n");
            if (vartypes != null) foreach (var variable in vartypes.Keys) sb.Append("\t" + variable + ":\t" + vartypes[variable] + "\n");
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

    public class Program
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
            sb.Append("\n\n---------------------------------------------------\n\n");
            return sb.ToString();
        }

        public bool Verify()
        {
            foreach (Subroutine routine in this.subprograms.Values)
            {
                foreach (Statement state in routine.statements)
                {
                    foreach(string s in state.GetVars())
                    {
                        if (s != null && !routine.vartypes.ContainsKey(s)) return false;
                    }
                }
            }

            foreach(Statement state in this.statements)
            {
                foreach (string s in state.GetVars())
                {
                    if (s != null && !this.vartypes.ContainsKey(s)) return false;
                }
            }
            return true;
        }
    }
}
