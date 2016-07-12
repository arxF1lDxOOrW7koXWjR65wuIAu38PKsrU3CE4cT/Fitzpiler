using System;
using System.Collections.Generic;
using System.Linq;

namespace Fitzpiler
{
    public class Parser
    {
        private Token _current;
        private readonly Scanner _scanner;

        public Parser(Scanner scanner)
        {
            this._scanner = scanner;
            Program = new Program();
            try
            {
                _current = scanner.Pop();
                Consume(KeyWord.Program);
                Program.Id = _current.Data;
                Consume(TokenType.Id);

                Program.Vartypes = Declarations();
                Program.Subprograms = new Dictionary<string, Subroutine>();
                foreach (var s in Subprogram_Declarations(new List<Subroutine>()))
                {
                    Program.Subprograms.Add(s.Name, s);
                }
                Program.Statements = Compound_Statement();

                var parseSuccessful = Program.Verify();
                if (!parseSuccessful)
                {
                    throw new ParseFailedException("Variable used without declaration");
                }
            }
            catch (ParseFailedException e)
            {
                Console.WriteLine("Parse failed:\n\t" + e.Message);
            }
        }

        public Program Program { get; }

        private List<Subroutine> Subprogram_Declarations(List<Subroutine> subroutines)
        {
            while (_current.Data == "FUNCTION" || _current.Data == "PROCEDURE")
            {
                var subroutine = Subprogram_Head();
                if (subroutine != null) subroutines.Add(subroutine);
                if (_current.Type == TokenType.Stop) Consume();
                if (subroutine == null) continue;
                subroutine.Vartypes = Declarations();
                if (_current.Type == TokenType.Stop) Consume();
                subroutine.Statements = Compound_Statement();
            }
            return subroutines;
        }

        private Subroutine Subprogram_Head()
        {
            var func = _current.Data == "FUNCTION";

            var parameters = new Dictionary<string, VarType>();
            Consume();
            var funcname = _current.Data;
            Consume(TokenType.Id);
            if (_current.Data == "(")
            {
                Consume(TokenType.Stop);
                do
                {
                    var argname = _current.Data;
                    Consume(TokenType.Id);
                    Consume(TokenType.Stop);
                    switch (_current.Data)
                    {
                        case "INTEGER":
                            Consume(KeyWord.Integer);
                            parameters.Add(argname, VarType.Integer);
                            break;

                        case "REAL":
                            Consume(KeyWord.Real);
                            parameters.Add(argname, VarType.Real);
                            break;
                        case "ARRAY":
                            Consume(KeyWord.Array);
                            Consume();
                            var begindex = new Number(_current.Data);
                            Consume(TokenType.Num);
                            Consume();
                            var endex = new Number(_current.Data);
                            Consume(TokenType.Num); //should probably do something with these at some point
                            Consume();
                            Consume(KeyWord.Of);
                            if (_current.Data == "INTEGER")
                            {
                                Consume(KeyWord.Integer);
                                parameters.Add(argname, VarType.Integerarray);
                            }
                            else if (_current.Data == "REAL")
                            {
                                Consume(KeyWord.Real);
                                parameters.Add(argname, VarType.Realarray);
                            }
                            break;
                    }
                } while (_current.Data != ")");
                Consume(TokenType.Stop);
            }

            if (func)
            {
                Consume(TokenType.Stop);
                switch (_current.Data)
                {
                    case "INTEGER":
                        Consume(KeyWord.Integer);
                        Consume(TokenType.Stop);
                        return new Subroutine(funcname, parameters, VarType.Integer);
                    case "REAL":
                        Consume(KeyWord.Real);
                        Consume(TokenType.Stop);
                        return new Subroutine(funcname, parameters, VarType.Integer);
                    case "ARRAY":
                        Consume(KeyWord.Array);
                        Consume();
                        var begindex = new Number(_current.Data);
                        Consume(TokenType.Num);
                        Consume();
                        var endex = new Number(_current.Data);
                        Consume(TokenType.Num); //should probably do something with these at some point
                        Consume();
                        Consume(KeyWord.Of);
                        if (_current.Data == "INTEGER")
                        {
                            Consume(KeyWord.Integer);
                            return new Subroutine(funcname, parameters, VarType.Integerarray);
                        }
                        if (_current.Data == "REAL")
                        {
                            Consume(KeyWord.Real);
                            return new Subroutine(funcname, parameters, VarType.Realarray);
                        }
                        break;
                }
            }
            else
            {
                return new Subroutine(funcname, parameters, VarType.Void);
            }
            if (_current.Match(TokenType.Stop)) Consume();
            return null;
        }

        private Dictionary<string, VarType> Declarations()
        {
            var vartypes = new Dictionary<string, VarType>();
            if (_current.Match(KeyWord.Var))
            {
                Consume(KeyWord.Var);
                do
                {
                    var varnames = new List<string>();
                    do
                    {
                        if (_current.Data == ",") Consume();
                        varnames.Add(_current.Data);
                        Consume(TokenType.Id);
                    } while (_current.Data == ",");
                    Consume(TokenType.Stop);
                    if (_current.Match(KeyWord.Integer))
                    {
                        var vartype = VarType.Integer;
                        Consume(KeyWord.Integer);
                        if (vartypes.ContainsKey(varnames.Last()))
                            throw new ParseFailedException("Variable already defined");
                        foreach (var v in varnames) vartypes.Add(v, vartype);
                        varnames.Clear();
                    }
                    else if (_current.Match(KeyWord.Real))
                    {
                        var vartype = VarType.Real;
                        Consume(KeyWord.Real);
                        if (vartypes.ContainsKey(varnames.Last()))
                            throw new ParseFailedException("Variable already defined");
                        foreach (var v in varnames) vartypes.Add(v, vartype);
                        varnames.Clear();
                    }
                    else if (_current.Match(KeyWord.Array)) //deal with this later
                    {
                        Consume(KeyWord.Array);
                        Consume();
                        var begindex = new Number(_current.Data);
                        Consume(TokenType.Num);
                        Consume(TokenType.Stop);
                        var endex = new Number(_current.Data);
                        Consume(TokenType.Num);
                        Consume(TokenType.Stop);
                        Consume(KeyWord.Of);
                        if (_current.Data == "INTEGER")
                        {
                            vartypes.Add(varnames.Last(), VarType.Integerarray);
                            Consume(KeyWord.Integer);
                        }
                        if (_current.Data == "REAL")
                        {
                            vartypes.Add(varnames.Last(), VarType.Realarray);
                            Consume(KeyWord.Real);
                        }
                    }
                    if (_current.Type == TokenType.Stop) Consume();
                } while (_current.Data != "BEGIN" && _current.Data != "FUNCTION" && _current.Data != "PROCEDURE");
            }
            return vartypes;
        }

        private List<Statement> Compound_Statement()
        {
            Consume(KeyWord.Begin);
            var statements = Optional_Statements();
            Consume(KeyWord.End);
            return statements;
        }

        private List<Statement> Optional_Statements()
        {
            if (!_current.Match(KeyWord.End))
            {
                return Statement_List();
            }
            return null;
        }

        private List<Statement> Statement_List()
        {
            var statements = new List<Statement> {Statement()};
            while (_current.Data == ";")
            {
                Consume(TokenType.Stop);
                var temp = Statement();
                if (temp != null) statements.Add(temp);
            }
            return statements;
        }

        private Statement Statement()
        {
            Statement statement = null;
            if (_scanner.Peek().Type == TokenType.Assignop)
            {
                var variable = new Variable(_current.Data, Program.Vartypes[_current.Data]);
                Consume(TokenType.Id);
                Consume(TokenType.Assignop);
                var expression = Expression();
                var assignment = new Assignment(variable, expression);
                statement = assignment;
            }
            else if (_scanner.Peek().Data == "[")
            {
                var varname = _current.Data;
                Consume(TokenType.Id);
                Consume();
                var expression = Expression();
                var array = new Variable(varname, VarType.Integer, expression); //fix this
                Consume();
                Consume(TokenType.Assignop);
                expression = Expression();
                var assignment = new Assignment(array, expression);
                statement = assignment;
            }

            else if (_current.Match(KeyWord.If))
            {
                Consume(KeyWord.If);
                var expression = Expression();
                Consume(KeyWord.Then);
                var statement1 = Statement();
                Consume(KeyWord.Else);
                var statement2 = Statement();
                var ifstatement = new IfStatement(expression, statement1, statement2);
                statement = ifstatement;
            }
            else if (_current.Match(KeyWord.While))
            {
                Consume(KeyWord.While);
                var expression = Expression();
                Consume(KeyWord.Do);
                var statement1 = Statement();
                statement = new WhileStatement(expression, statement1);
            }
            else if (_current.Match(KeyWord.Read))
            {
                Consume(KeyWord.Read);
                Consume(TokenType.Stop);
                var variable = new Variable(_current.Data, VarType.Integer);
                Consume(TokenType.Id);
                Consume(TokenType.Stop);
                statement = new ReadStatement(variable);
            }
            else if (_current.Match(KeyWord.Write))
            {
                Consume(KeyWord.Write);
                Consume(TokenType.Stop);
                var expression = Expression();
                Consume(TokenType.Stop);
                statement = new WriteStatement(expression);
            }
            else if (Program.Subprograms.ContainsKey(_current.Data))
            {
                var funcname = _current.Data;
                Consume(TokenType.Id);
                Consume(TokenType.Stop);
                var arguments = ExpressionList();
                Consume(TokenType.Stop);
                statement = new Procedure(funcname, arguments);
            }
            return statement;
        }

        private Expression Expression()
        {
            var expression = Term();
            while (_current.Type == TokenType.Addop)
            {
                var operation = Op.Add;
                if (_current.Data == "+") operation = Op.Add;
                else if (_current.Data == "-") operation = Op.Sub;
                else if (_current.Data == "&") operation = Op.And;
                Consume();
                var expression2 = Term();
                Expression newexpression = new Operation(expression, operation, expression2);
                expression = newexpression;
            }
            if (_current.Type == TokenType.Relop)
            {
                var operation = Op.Add;
                if (_current.Data == "=") operation = Op.Eq;
                else if (_current.Data == "<=") operation = Op.Leq;
                else if (_current.Data == ">=") operation = Op.Geq;
                else if (_current.Data == ">") operation = Op.Grt;
                else if (_current.Data == "<") operation = Op.Lst;
                Consume();
                var expression2 = Expression();
                Expression newExpression = new Operation(expression, operation, expression2);
                expression = newExpression;
            }
            return expression;
        }

        private List<Expression> ExpressionList()
        {
            var expressionList = new List<Expression> {Expression()};
            if (_current.Data == ",")
            {
                expressionList.Add(Expression());
            }
            return expressionList;
        }

        private Expression Term()
        {
            Expression expression = null;
            do
            {
                if (_current.Match(TokenType.Num))
                {
                    var number = _current.Data;
                    Consume(TokenType.Num);
                    if (_current.Data == ".")
                    {
                        Consume();
                        number += "." + _current.Data;
                        Consume(TokenType.Num);
                    }
                    if (_current.Data == "E")
                    {
                    }
                    expression = new Number(number);
                }
                else if (_current.Data == "-")
                {
                    Consume(TokenType.Addop);
                    Expression expression1 = new Number("0");
                    var expression2 = Expression();
                    expression = new Operation(expression1, Op.Sub, expression2);
                }
                else if (_current.Data == "+")
                {
                    Consume(TokenType.Addop);
                    expression = Term();
                }
                else if (_current.Data == "(")
                {
                    Consume(TokenType.Stop);
                    expression = Expression();
                    Consume(TokenType.Stop);
                }
                else if (_current.Data == "NOT")
                {
                }
                else if (_current.Match(TokenType.Id))
                {
                    var variable = new Variable(_current.Data, Program.Vartypes[_current.Data]);
                    Consume(TokenType.Id);
                    if (_current.Data == "[")
                    {
                        Consume();
                        var localexpression = Expression();
                        var vararray = Program.Vartypes[variable.Varname] == VarType.Integer
                            ? new Variable(variable.Varname, VarType.Integerarray, localexpression)
                            : new Variable(variable.Varname, VarType.Realarray, localexpression);
                        expression = vararray;
                    }
                    else if (_current.Data == "(")
                    {
                        Consume();
                        var expressionList = ExpressionList();
                        // move this validation to the verifier
                        if (!Program.Subprograms.ContainsKey(variable.Varname))
                            throw new ParseFailedException("Use of unregistered subroutine:  " + variable.Varname);
                        expression = new Function(variable.Varname, expressionList);
                        Consume();
                    }
                    else
                    {
                        expression = variable;
                    }
                }

                if (_current.Match(TokenType.Mulop))
                {
                    var token = _current;
                    Consume();
                    var expression2 = Term();
                    var operation = token.Data == "*"
                        ? Op.Mult
                        : token.Data == "/" ? Op.Divide : token.Data == "%" ? Op.Mod : Op.And;
                    expression = new Operation(expression, operation, expression2);
                }
            } while (_current.Match(TokenType.Mulop));

            return expression;
        }

        public void Consume(KeyWord key)
        {
            if (_current.Match(key))
            {
                _current = _scanner.Pop();
            }
            else
            {
                throw new ParseFailedException("Incorrect Keyword: " + _current.Data);
            }
        }

        public void Consume(TokenType token)
        {
            if (_current.Match(token))
            {
                _current = _scanner.Pop();
            }
            else
            {
                throw new ParseFailedException("Incorrect syntax: " + _current.Data);
            }
        }

        public void Consume()
        {
            _current = _scanner.Pop();
        }
    }
}