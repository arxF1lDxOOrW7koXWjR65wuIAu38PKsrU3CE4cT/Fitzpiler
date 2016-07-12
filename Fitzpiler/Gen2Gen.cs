using System.Collections.Generic;
using System.Text;

namespace Fitzpiler
{
    internal class Gen2Gen
    {
        private int _label;
        private readonly Program _program;
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly Dictionary<string, int> _vars = new Dictionary<string, int>();

        public Gen2Gen(Program program)
        {
            this._program = program;
            _sb.Append(Header());
            _sb.Append(Locals());
            _sb.Append("\n");
            _sb.Append(Statements(_program.Statements));
            _sb.Append("\nret\n}\n");
            foreach (var subroutine in _program.Subprograms.Values)
            {
                _sb.Append(Procedure(subroutine));
            }
        }

        public string Header()
        {
            var ts = new StringBuilder();
            ts.Append(".assembly extern mscorlib {} \n");
            ts.Append(".assembly " + _program.Id + "{}\n\n");
            ts.Append(".method static public void main() il managed\n{\n\n");
            ts.Append(".entrypoint\n.maxstack 8\n");
            return ts.ToString();
        }

        public string Locals()
        {
            var ts = new StringBuilder();
            ts.Append(".locals init (\n");
            var count = 0;
            foreach (var varname in _program.Vartypes.Keys)
            {
                if (_program.Vartypes[varname] == VarType.Integer) ts.Append("[" + count + "] int32 " + varname + ",\n");
                if (_program.Vartypes[varname] == VarType.Real) ts.Append("[" + count + "] float32 " + varname + ",\n");
                _vars.Add(varname, count++);
            }
            ts.Append(")\n");
            ts.Remove(ts.Length - 4, 2);
            return ts.ToString();
        }

        public string Statements(List<Statement> statements)
        {
            var ts = new StringBuilder();
            foreach (var s in statements)
            {
                if (s is Assignment)
                {
                    var assignment = s as Assignment;
                    ts.Append(Expression(assignment.Expression));
                    ts.Append("stloc." + _vars[assignment.Variable.Varname] + "\n");
                }
                else if (s is WriteStatement)
                {
                    var write = s as WriteStatement;
                    ts.Append(Expression(write.Expression));
                    ts.Append("call  void[mscorlib] System.Console::WriteLine(int32)\n");
                }
                else if (s is IfStatement)
                {
                    var ifstatement = s as IfStatement;
                    var op = ifstatement.Expression as Operation;
                    var statement1 = Statements(new List<Statement>(1) {ifstatement.Statement1});
                    var statement2 = Statements(new List<Statement>(1) {ifstatement.Statement2});
                    if (op != null)
                    {
                        ts.Append(Expression(op.Expression1));
                        ts.Append(Expression(op.Expression2));
                        switch (op.operation)
                        {
                            case Op.Eq:
                                ts.Append("beq ");
                                break;
                            case Op.Geq:
                                ts.Append("ble ");
                                break;
                            case Op.Leq:
                                ts.Append("bge ");
                                break;
                            case Op.Grt:
                                ts.Append("blt ");
                                break;
                            case Op.Lst:
                                ts.Append("bgt ");
                                break;
                        }
                        ts.Append("AGEN_" + _label + "\n");
                        ts.Append(statement1);
                        ts.Append("br " + "AGEN_" + (_label + 1) + "\n");
                        ts.Append("AGEN_" + _label + ":\n");
                        ts.Append(statement2);
                        ts.Append("AGEN_" + (_label + 1 + ":\n"));
                        _label++;
                        break;
                    }
                }
                else if (s is WhileStatement)
                {
                    var whilestatement = s as WhileStatement;
                    var op = whilestatement.Expression as Operation;
                    var statement1 = Statements(new List<Statement>(1) {whilestatement.Statement});
                    if (op != null)
                    {
                        ts.Append("AGEN_" + _label + ":\n");
                        ts.Append(Expression(op.Expression1));
                        ts.Append(Expression(op.Expression2));
                        switch (op.operation)
                        {
                            case Op.Eq:
                                ts.Append("beq ");
                                break;
                            case Op.Geq:
                                ts.Append("bge ");
                                break;
                            case Op.Leq:
                                ts.Append("ble ");
                                break;
                            case Op.Grt:
                                ts.Append("bgt ");
                                break;
                            case Op.Lst:
                                ts.Append("blt ");
                                break;
                        }
                        ts.Append("AGEN_" + (_label + 1) + "\n");
                        ts.Append("br AGEN_" + (_label + 2) + "\n");
                        ts.Append("AGEN_" + (_label + 1) + ":\n");
                        ts.Append(statement1);
                        ts.Append("br " + "AGEN_" + _label + "\n");
                        ts.Append("AGEN_" + (_label + 2) + ":\n");
                        _label++;
                    }
                }
                else if (s is ReadStatement)
                {
                    var readstatement = s as ReadStatement;
                }
            }
            return ts.ToString();
        }


        public string Expression(Expression expression)
        {
            var ts = new StringBuilder();
            var operation1 = expression as Operation;
            if (operation1 != null)
            {
                var operation = operation1;
                ts.Append(Expression(operation.Expression1));
                ts.Append(Expression(operation.Expression2));
                switch (operation.operation)
                {
                    case Op.Add:
                        ts.Append("add\n");
                        break;
                    case Op.Sub:
                        ts.Append("sub\n");
                        break;
                    case Op.Mult:
                        ts.Append("mul\n");
                        break;
                    case Op.Divide:
                        ts.Append("div\n");
                        break;
                    case Op.Mod:
                        ts.Append("rem\n");
                        break;
                    case Op.Or:
                        ts.Append("or\n");
                        break;
                    case Op.And:
                        ts.Append("or\n");
                        break;
                    //needs XOR
                }
            }
            var variable1 = expression as Variable;
            if (variable1 != null)
            {
                var variable = variable1;
                ts.Append("ldloc " + _vars[variable.Varname] + "\n");
            }
            if (!(expression is Number)) return ts.ToString();
            var number = (Number) expression;
            if (number.number.Contains("."))
            {
                ts.Append("ldc.r4 " + number.number + "\n");
            }
            else
            {
                ts.Append("ldc.i4 " + number.number + "\n");
            }
            return ts.ToString();
        }

        public string Procedure(Subroutine subroutine)
        {
            var ts = new StringBuilder();
            ts.Append(".method static public void " + subroutine.Name + "(");
            var first = true;
            foreach (var variable in subroutine.Vartypes.Keys)
            {
                if (!first) ts.Append(",");
                first = false;
                if (subroutine.Vartypes[variable] == VarType.Integer) ts.Append("int32 " + variable);
            }
            foreach (var s in subroutine.Statements)
            {
                var statement1 = Statements(new List<Statement>(1) {s});
            }
            ts.Append(") il managed\n{\n.maxstack 8\n");


            return ts.ToString();
        }


        public new string ToString()
        {
            return _sb.ToString();
        }
    }
}