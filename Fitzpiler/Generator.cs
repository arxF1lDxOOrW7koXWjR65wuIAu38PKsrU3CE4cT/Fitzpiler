using System.Collections.Generic;
using System.Text;

namespace Fitzpiler
{
    internal class Generator
    {
/*
        private int _labels = 0;
        private int _maxstack = 0;
*/
        private readonly Program _program;
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly Dictionary<string, int> _vars = new Dictionary<string, int>();

        public Generator(Program program)
        {
            this._program = program;
            Header();
            Locals();
            _sb.Append("\n");
            Statements(_program.Statements);
            _sb.Append("ret\n}\n");
        }

        public new string ToString()
        {
            return _sb.ToString();
        }

        public void Header()
        {
            _sb.Append(".assembly extern mscorlib {} \n");
            _sb.Append(".assembly " + _program.Id + "{}\n\n");
            _sb.Append(".method static public void main() il managed\n{\n\n");
            _sb.Append(".entrypoint\n.maxstack 8\n");
        }

        public void Locals(Subroutine s)
        {
        }

        public void Locals()
        {
            _sb.Append(".locals init (\n");
            var count = 0;
            foreach (var varname in _program.Vartypes.Keys)
            {
                if (_program.Vartypes[varname] == VarType.Integer) _sb.Append("[" + count + "] int32 " + varname + ",\n");
                if (_program.Vartypes[varname] == VarType.Real) _sb.Append("[" + count + "] float32 " + varname + ",\n");
                _vars.Add(varname, count++);
            }
            _sb.Append(")\n");
            _sb.Remove(_sb.Length - 4, 2);
        }

        public string Statements(List<Statement> statements)
        {
            var sb = new StringBuilder();
            foreach (var s in statements)
            {
                if (s is Assignment)
                {
                    var assignment = s as Assignment;
                    sb.Append(Expression(assignment.Expression));
                    sb.Append("stloc." + _vars[assignment.Variable.Varname] + "\n");
                }
                else if (s is ReadStatement)
                {
                    var readstatement = s as ReadStatement;
                }
                else if (s is WriteStatement)
                {
                    var writestatement = s as WriteStatement;
                    //   VarType expressiontype = Expression(writestatement.expression);
                    //   if (expressiontype == VarType.INTEGER)
                    //   {
                    //sb.Append("box [mscorlib]System.Int32\n");
                    sb.Append("call  void[mscorlib] System.Console::WriteLine(int32)\n");
                    //  }
                    // else if(expressiontype == VarType.REAL)
                    // {
                    //    sb.Append("call  void[mscorlib] System.Console::WriteLine(float32)\n");
                    // }

                    // sb.Append("call  void[mscorlib] System.Console::WriteLine(object)\n");
                }
                else if (s is IfStatement)
                {
                    //var ifstatement = s as IfStatement;
                    //  VarType exp1 = Expression(ifstatement.expression);

                    //var ifop = (ifstatement.Expression as Operation).operation;
                    //switch (ifop)
                    //{
                    //    // case Op.EQ:
                    //}
                }
            }
            return sb.ToString();
        }

        public string Expression(Expression expression)
        {
            var sb = new StringBuilder();
            var operation1 = expression as Operation;
            if (operation1 != null)
            {
                var operation = operation1;
                sb.Append(Expression(operation.Expression1));
                sb.Append(Expression(operation.Expression2));
                switch (operation.operation)
                {
                    case Op.Add:
                        sb.Append("add\n");
                        break;
                    case Op.Sub:
                        sb.Append("sub\n");
                        break;
                    case Op.Mult:
                        sb.Append("mul\n");
                        break;
                    case Op.Divide:
                        sb.Append("div\n");
                        break;
                    case Op.Mod:
                        sb.Append("rem\n");
                        break;
                    case Op.Or:
                        sb.Append("or\n");
                        break;
                    case Op.And:
                        sb.Append("or\n");
                        break;
                    //needs XOR
                }
                //if (exp1 == VarType.INTEGER && exp2 == VarType.INTEGER)
                //{
                //    return VarType.INTEGER;
                //}
                //else
                //{
                //    return VarType.REAL;
                //}
            }
            var variable1 = expression as Variable;
            if (variable1 != null)
            {
                var variable = variable1;
                sb.Append("ldloc " + _vars[variable.Varname] + "\n");
                // return this.program.vartypes[variable.varname];
            }
            if (!(expression is Number)) return sb.ToString();
            var number = (Number) expression;
            if (number.number.Contains("."))
            {
                sb.Append("ldc.r4 " + number.number + "\n");
            }
            else
            {
                sb.Append("ldc.i4 " + number.number + "\n");
            }
            //  return VarType.VOID;
            return sb.ToString();
        }
    }
}