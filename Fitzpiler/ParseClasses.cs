using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fitzpiler
{
    public enum VarType
    {
        Integer,
        Real,
        Function,
        Realarray,
        Integerarray,
        Void
    }

    public abstract class Statement
    {
        public virtual string ToString(int level)
        {
            return "\n";
        }

        public virtual IEnumerable<Tuple<string, VarType>> GetVars()
        {
            yield return null;
        }
    }

    internal class Assignment : Statement
    {
        public Assignment(Variable variable, Expression expression)
        {
            this.Variable = variable;
            this.Expression = expression;
        }

        public Variable Variable { get; }
        public Expression Expression { get; }

        public override string ToString(int level)
        {
            var outstr = "";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += "ASSIGNOP\n";
            outstr += Variable.ToString(level + 1) + "\n";
            outstr += Expression.ToString(level + 1);
            return outstr;
        }

        public override IEnumerable<Tuple<string, VarType>> GetVars()
        {
            yield return new Tuple<string, VarType>(Variable.Varname, Variable.Vartype);
            if (Variable.Accessor != null) foreach (var s in Variable.Accessor.GetVars()) yield return s;
            foreach (var s in Expression.GetVars()) yield return s;
        }
    }

    internal class Procedure : Statement
    {
        public Procedure(string funcname, List<Expression> args)
        {
            this.Funcname = funcname;
            Arguments = args;
        }

        public string Funcname { get; }
        public List<Expression> Arguments { get; }

        public override string ToString(int level)
        {
            var outstr = "";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += "PROC." + Funcname + "\n";
            foreach (var arg in Arguments) outstr += "|" + arg.ToString(level);
            return outstr;
        }
    }

    internal class IfStatement : Statement
    {
        public IfStatement(Expression expression1, Statement statement1, Statement statement2)
        {
            Expression = expression1;
            this.Statement1 = statement1;
            this.Statement2 = statement2;
        }

        public Expression Expression { get; }
        public Statement Statement1 { get; }
        public Statement Statement2 { get; }

        public override string ToString(int level)
        {
            var outstr = "";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += "IF: \n" + Expression.ToString(level + 1) + "\n";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += "THEN: \n";
            outstr += Statement1.ToString(level + 1) + "\n";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += "ELSE: \n";
            outstr += Statement2.ToString(level + 1) + "\n";
            return outstr;
        }

        public override IEnumerable<Tuple<string, VarType>> GetVars()
        {
            foreach (var s in Expression.GetVars()) yield return s;
            foreach (var s in Statement1.GetVars()) yield return s;
            foreach (var s in Statement2.GetVars()) yield return s;
        }
    }

    internal class WhileStatement : Statement
    {
        public WhileStatement(Expression expression, Statement statement)
        {
            this.Expression = expression;
            this.Statement = statement;
        }

        public Expression Expression { get; }
        public Statement Statement { get; }

        public override string ToString(int level)
        {
            var outstr = "";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += "WHILE:\n";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += Expression.ToString(level + 1) + "\n";
            outstr += "DO:\n";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += Statement.ToString(level + 1) + "\n";
            return outstr;
        }

        public override IEnumerable<Tuple<string, VarType>> GetVars()
        {
            foreach (var s in Expression.GetVars()) yield return s;
            foreach (var s in Statement.GetVars()) yield return s;
        }
    }

    internal class ReadStatement : Statement
    {
        public ReadStatement(Variable variable)
        {
            this.Variable = variable;
        }

        public Variable Variable { get; }

        public override string ToString(int level)
        {
            var outstr = "";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += "SYS.READ\n";
            outstr += Variable.ToString(level + 1);
            return outstr;
        }

        public override IEnumerable<Tuple<string, VarType>> GetVars()
        {
            yield return Variable.GetVars().First();
        }
    }

    internal class WriteStatement : Statement
    {
        public WriteStatement(Expression expression)
        {
            this.Expression = expression;
        }

        public Expression Expression { get; }

        public override string ToString(int level)
        {
            var outstr = "";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += "SYS.WRITE\n";
            outstr += Expression.ToString(level + 1);
            return outstr;
        }

        public override IEnumerable<Tuple<string, VarType>> GetVars()
        {
            foreach (var s in Expression.GetVars()) yield return s;
        }
    }

    public abstract class Expression
    {
        public virtual string ToString(int level)
        {
            return "\n";
        }

        public virtual IEnumerable<Tuple<string, VarType>> GetVars()
        {
            yield return null;
        }
    }

    internal class Operation : Expression
    {
        public Operation(Expression expression1, Op operation, Expression expression2)
        {
            Expression1 = expression1;
            this.operation = operation;
            Expression2 = expression2;
        }

        public Expression Expression1 { get; }

        public Op operation { get; }

        public Expression Expression2 { get; }

        public override string ToString(int level)
        {
            var outstr = "";
            for (var i = 0; i < level; i++) outstr += "-";
            switch (operation)
            {
                case Op.Add:
                    outstr += "OP.ADD\n";
                    break;
                case Op.Sub:
                    outstr += "OP.SUB\n";
                    break;
                case Op.Mult:
                    outstr += "OP.MULT\n";
                    break;
                case Op.Divide:
                    outstr += "OP.DIVIDE\n";
                    break;
                case Op.Eq:
                    outstr += "RELOP.EQ\n";
                    break;
                case Op.Grt:
                    outstr += "RELOP.GRT\n";
                    break;
                case Op.Lst:
                    outstr += "RELOP.LST\n";
                    break;
                case Op.Geq:
                    outstr += "RELOP.GEQ\n";
                    break;
                case Op.Leq:
                    outstr += "RELOP.LEQ\n";
                    break;
            }
            outstr += Expression1.ToString(level + 1) + "\n";
            outstr += Expression2.ToString(level + 1);
            return outstr;
        }

        public override IEnumerable<Tuple<string, VarType>> GetVars()
        {
            foreach (var s in Expression1.GetVars()) yield return s;
            foreach (var s in Expression2.GetVars()) yield return s;
        }
    }

    internal enum Op
    {
        Add,
        Sub,
        Mult,
        Divide,
        Mod,
        And,
        Or,
        Eq,
        Leq,
        Geq,
        Lst,
        Grt
    }

    internal class Variable : Expression
    {
        public Variable(string varname, VarType vartype, Expression accessor)
        {
            this.Varname = varname;
            this.Vartype = vartype;
            this.Accessor = accessor;
        }

        public Variable(string varname, VarType vartype)
        {
            this.Varname = varname;
            this.Vartype = vartype;
        }

        public string Varname { get; }
        public VarType Vartype { get; }
        public Expression Accessor { get; }

        public override string ToString(int level)
        {
            var outstr = "";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += Varname;
            if (Accessor != null) outstr += "\n-ARRAY.ACCESS\n" + Accessor.ToString(level + 1);
            return outstr;
        }

        public override IEnumerable<Tuple<string, VarType>> GetVars()
        {
            yield return new Tuple<string, VarType>(Varname, Vartype);
        }
    }

    internal class Function : Expression
    {
        public Function(string funcname, List<Expression> args)
        {
            this.Funcname = funcname;
            Arguments = args;
        }

        private string Funcname { get; }
        private List<Expression> Arguments { get; }

        public override string ToString(int level)
        {
            var outstr = "";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += "FUNC." + Funcname + "\n";
            foreach (var arg in Arguments) outstr += "|" + arg.ToString(level);
            return outstr;
        }
    }

    internal class Number : Expression
    {
        public Number(string number)
        {
            this.number = number;
        }

        public string number { get; }

        public override string ToString(int level)
        {
            var outstr = "";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += number;
            return outstr;
        }
    }


    public class Subroutine : Expression
    {
        public Subroutine(string name, Dictionary<string, VarType> args, VarType returnType)
        {
            this.Name = name;
            Parameters = args;
            this.ReturnType = returnType;
        }

        public string Name { get; }
        public Dictionary<string, VarType> Vartypes { get; set; }
        public List<Statement> Statements { get; set; }
        public Dictionary<string, VarType> Parameters { get; }
        public VarType ReturnType { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(ReturnType == VarType.Void ? "\nPROCEDURE:\n\t" : "\nFUNCTION:\n\t");
            sb.Append(Name + "\n\nReturn Type:\n\t" + ReturnType + "\n\nParameters:");
            foreach (var parameter in Parameters.Keys)
                sb.Append("\n\t" + parameter + ":\t" + Parameters[parameter] + "\n\nVariables:\n");
            if (Vartypes != null)
                foreach (var variable in Vartypes.Keys) sb.Append("\t" + variable + ":\t" + Vartypes[variable] + "\n");
            sb.Append("\nStatements:\n");
            if (Statements != null)
            {
                foreach (var s in Statements)
                {
                    sb.Append(s.ToString(0) + "\n\n");
                }
            }
            return sb.ToString();
        }

        public override string ToString(int level)
        {
            var outstr = "";
            for (var i = 0; i < level; i++) outstr += "-";
            outstr += "SUBROUTINE_CALL: " + Name + "\n";
            for (var i = 0; i < level + 1; i++) outstr += "-";
            outstr += "ARGS: \n";

            return outstr;
        }
    }

    public class Program
    {
        public string Id;
        public List<Statement> Statements = new List<Statement>();
        public Dictionary<string, Subroutine> Subprograms = new Dictionary<string, Subroutine>();
        public Dictionary<string, VarType> Vartypes = new Dictionary<string, VarType>();

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("PROGRAM: " + Id + "\n---------------------------------------------------\nVariables:\n");
            if (Vartypes != null)
                foreach (var variable in Vartypes.Keys) sb.Append("\t" + variable + ": " + Vartypes[variable] + "\n");
            sb.Append("---------------------------------------------------\nSubprograms:\n");
            if (Subprograms != null) foreach (var subroutine in Subprograms.Values) sb.Append("\t\n" + subroutine);
            sb.Append("---------------------------------------------------\nStatements:\n");
            if (Statements != null)
            {
                foreach (var s in Statements)
                {
                    sb.Append("\n\n" + s.ToString(0));
                }
            }
            sb.Append("\n\n---------------------------------------------------\n\n");
            return sb.ToString();
        }

        public bool Verify()
        {
            foreach (var routine in Subprograms.Values)
            {
                foreach (var state in routine.Statements)
                {
                    foreach (var s in state.GetVars())
                    {
                        if (s != null && !routine.Vartypes.ContainsKey(s.Item1)) return false;
                    }
                }
            }

            foreach (var state in Statements)
            {
                foreach (var s in state.GetVars())
                {
                    if (s != null && !Vartypes.ContainsKey(s.Item1)) return false;
                }
            }
            return true;
        }
    }
}