using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitzpiler
{
    class Generator
    {
        private Program program;
        private StringBuilder sb = new StringBuilder();
        private Dictionary<string, int> vars = new Dictionary<string, int>();
        private int maxstack = 0;

        public Generator(Program program)
        {
            this.program = program;
            Header();
            Locals();
            sb.Append("\n");
            Statements(this.program.statements);
            sb.Append("ret\n}\n");
        }

        public new string ToString()
        {
            return sb.ToString();
        }

        public void Header()
        {
            sb.Append(".assembly extern mscorlib {} \n");
            sb.Append(".assembly " + this.program.id + "{}\n\n");
            sb.Append(".method static public void main() il managed\n{\n\n");
            sb.Append(".entrypoint\n.maxstack 8\n");
        }

        public void Locals(Subroutine s)
        {

        }

        public void Locals()
        {
            sb.Append(".locals init (\n");
            int count = 0;
            foreach(string varname in this.program.vartypes.Keys)
            {
                if (this.program.vartypes[varname] == VarType.INTEGER) sb.Append("[" + count + "] int32 " + varname + ",\n");
                if (this.program.vartypes[varname] == VarType.REAL) sb.Append("[" + count + "] float32 " + varname + ",\n");
                vars.Add(varname, count++);
            }
            sb.Append(")\n");
            sb.Remove(sb.Length - 4, 2);
        }

        public void Statements(List<Statement> statements)
        {
            foreach (Statement s in statements)
            {
                if (s is Assignment)
                {
                    Assignment assignment = s as Assignment;
                    Expression(assignment.expression);
                    sb.Append("stloc." + vars[assignment.variable.varname] + "\n");
                }
                if (s is ReadStatement)
                {
                    ReadStatement readstatement = s as ReadStatement;
                }
                if (s is WriteStatement)
                {
                    WriteStatement writestatement = s as WriteStatement;
                    Expression(writestatement.expression);
                    sb.Append("box [mscorlib]System.Int32\n");
                    sb.Append("call  void[mscorlib] System.Console::WriteLine(object)\n");
                }
            }
        }
        public void Expression(Expression expression)
        {
            if(expression is Operation)
            {
                Operation operation = expression as Operation;
                Expression(operation.expression1);
                Expression(operation.expression2);
                switch (operation.operation)
                {
                    case Op.ADD:
                        sb.Append("add\n");
                        break;
                    case Op.SUB:
                        sb.Append("sub\n");
                        break;
                    case Op.MULT:
                        sb.Append("mul\n");
                        break;
                    case Op.DIVIDE:
                        sb.Append("div\n");
                        break;
                    case Op.MOD:
                        sb.Append("rem\n");
                        break;
                    case Op.OR:
                        sb.Append("or\n");
                        break;
                    case Op.AND:
                        sb.Append("or\n");
                        break;
                    //needs XOR
                }
            }
            if(expression is Variable)
            {
                Variable variable = expression as Variable;
                sb.Append("ldloc " + vars[variable.varname] + "\n");
            }
            if(expression is Number)
            {
                Number number = expression as Number;
                sb.Append("ldc.i4 " + number.number + "\n");
            }
        }
    }
}
