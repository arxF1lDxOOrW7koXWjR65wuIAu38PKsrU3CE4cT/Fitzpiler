using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitzpiler
{
    class Parser
    {
        Scanner scanner;
        Program program;
        Token current;
        public Parser(Scanner scanner)
        {
            this.scanner = scanner;
            this.program = new Program();
            try
            {
                current = scanner.Pop();
                Consume(KeyWord.PROGRAM);
                this.program.id = current.data;
                Consume(TokenType.ID);

                this.program.vartypes = Declarations();
                this.program.subprograms = new Dictionary<string, Subroutine>();
                foreach(Subroutine s in Subprogram_Declarations(new List<Subroutine>()))
                {
                    this.program.subprograms.Add(s.name, s);
                }
                this.program.statements = Compound_Statement();
                Console.Write(program.ToString());
            }
            catch (ParseFailedException e)
            {
                Console.WriteLine("Tokenization failed:\n\t" + e.Message);
            }
        }

        private List<Subroutine> Subprogram_Declarations(List<Subroutine> subroutines)
        {
            Subroutine subroutine = Subprogram_Head();
            if (subroutine != null) subroutines.Add(subroutine);
            subroutine.vartypes = Declarations();
            subroutine.statements = Compound_Statement();
            return subroutines;
        }

        private Subroutine Subprogram_Head()
        {
            if (current.data == "FUNCTION")
            {
                Consume();
                string funcname = current.data;
                Consume(TokenType.ID);
                if (current.data == "(")
                {
                    Consume(TokenType.STOP);
                    while (current.data != ")")
                    {
                        string argname = current.data;
                        Consume(TokenType.ID);
                    }
                }
                else
                {
                    Consume(TokenType.STOP);
                    switch (current.data)
                    {
                        case "INTEGER":
                            Consume(KeyWord.INTEGER);
                            Consume(TokenType.STOP);
                            return new Subroutine(funcname, null, VarType.INTEGER);
                        case "REAL":
                            Consume(KeyWord.REAL);
                            Consume(TokenType.STOP);
                            return new Subroutine(funcname, null, VarType.INTEGER);
                    }
                }
            }
            return null;
        }
        private Dictionary<string, VarType> Declarations()
        {
            Dictionary<string, VarType> vartypes = new Dictionary<string, VarType>();
            if (current.Match(KeyWord.VAR))
            {
                Consume(KeyWord.VAR);
                do
                {
                    List<string> varnames = new List<string>();
                    do
                    {
                        if (current.data == ",") Consume();
                        varnames.Add(current.data);
                        Consume(TokenType.ID);
                    } while (current.data == ",");
                    Consume(TokenType.STOP);
                    if (current.Match(KeyWord.INTEGER))
                    {
                        VarType vartype = VarType.INTEGER;
                        Consume(KeyWord.INTEGER);
                        if (vartypes.ContainsKey(varnames.Last())) throw new ParseFailedException("Variable already defined");
                        foreach (var v in varnames) vartypes.Add(v, vartype);
                        varnames.Clear();
                    }
                    else if (current.Match(KeyWord.REAL))
                    {
                        VarType vartype = VarType.REAL;
                        Consume(KeyWord.REAL);
                        if (vartypes.ContainsKey(varnames.Last())) throw new ParseFailedException("Variable already defined");
                        foreach (var v in varnames) vartypes.Add(v, vartype);
                        varnames.Clear();
                    }
                    else if (current.Match(KeyWord.ARRAY)) //deal with this later
                    {
                        Consume(KeyWord.ARRAY);
                        Consume();
                        Number begindex = new Number(current.data);
                        Consume(TokenType.NUM);
                        Consume(TokenType.STOP);
                        Number endex = new Number(current.data);
                        Consume(TokenType.NUM);
                        Consume(TokenType.STOP);
                        Consume(KeyWord.OF);
                        if (current.data == "INTEGER")
                        {
                            vartypes.Add(varnames.Last(), VarType.INTEGERARRAY);
                            Consume(KeyWord.INTEGER);
                        }
                        if (current.data == "REAL")
                        {
                            vartypes.Add(varnames.Last(), VarType.REALARRAY);
                            Consume(KeyWord.REAL);
                        }
                    }
                    Consume(TokenType.STOP);
                } while (current.data != "BEGIN" && current.data != "FUNCTION" && current.data != "PROCEDURE");
            }
            return vartypes;
        }

        private List<Statement> Compound_Statement()
        {
            Consume(KeyWord.BEGIN);
            var statements =  Optional_Statements();
            Consume(KeyWord.END);
            return statements;
        }

        private List<Statement> Optional_Statements()
        {
            if (!current.Match(KeyWord.END))
            {
                return Statement_List();
            }
            return null;
        }

        private List<Statement> Statement_List()
        {
            List<Statement> statements = new List<Statement>();
            statements.Add(Statement());
            while ((current.data == ";"))
            {
                Consume(TokenType.STOP);
                var temp = Statement();
                if(temp != null) statements.Add(temp);
            }
            return statements;
        }

        private Statement Statement()
        {
            Statement statement = null;
            if (scanner.Peek().TYPE == TokenType.ASSIGNOP)
            {
                Variable variable = new Variable(current.data);
                if (!this.program.vartypes.ContainsKey(current.data)) throw new ParseFailedException("Undeclaired variable used: " + current.data);
                Consume(TokenType.ID);
                Consume(TokenType.ASSIGNOP);
                Expression expression = Expression();
                Assignment assignment = new Assignment(variable, expression);
                statement = assignment;
            }
            if(scanner.Peek().data == "[")
            {
                string varname = current.data;
                if (!this.program.vartypes.ContainsKey(current.data)) throw new ParseFailedException("Undeclaired variable used: " + current.data);
                else if (this.program.vartypes[current.data] != VarType.INTEGERARRAY && this.program.vartypes[current.data] != VarType.REALARRAY) throw new ParseFailedException("Non-array variable accessed as an array:  " + current.data); 
                Consume(TokenType.ID);
                Consume();
                Expression expression = Expression();
                Variable array = new Variable(varname, expression);
                Consume();
                Consume(TokenType.ASSIGNOP);
                expression = Expression();
                Assignment assignment = new Assignment(array, expression);
                statement = assignment;
            }

            if (current.Match(KeyWord.IF))
            {
                Consume(KeyWord.IF);
                Expression expression = Expression();
                Consume(KeyWord.THEN);
                Statement statement1 = Statement();
                Consume(KeyWord.ELSE);
                Statement statement2 = Statement();
                IfStatement ifstatement = new IfStatement(expression, statement1, statement2);
                statement = ifstatement;
            }
            if (current.Match(KeyWord.WHILE))
            {
                Consume(KeyWord.WHILE);
                Expression expression = Expression();
                Consume(KeyWord.DO);
                Statement statement1 = Statement();
                statement = new WhileStatement(expression, statement1);
            }
            return statement;

        }

        private Expression Expression()
        {
            Expression expression = Term();
            while (current.TYPE == TokenType.ADDOP)
            {
                Op operation = Op.ADD;
                if (current.data == "+") operation = Op.ADD;
                else if (current.data == "-") operation = Op.SUB;
                else if (current.data == "&") operation = Op.AND;
                Consume();
                Expression expression2 = Term();
                Expression newexpression = new Operation(expression, operation, expression2);
                expression = newexpression;
            }
            if (current.TYPE == TokenType.RELOP)
            {
                Op operation = Op.ADD;
                if (current.data == "=") operation = Op.EQ;
                else if (current.data == "<=") operation = Op.LEQ;
                else if (current.data == ">=") operation = Op.GEQ;
                else if (current.data == ">") operation = Op.GRT;
                else if (current.data == "<") operation = Op.LST;
                Consume();
                Expression expression2 = Expression();
                Expression newExpression = new Operation(expression, operation, expression2);
                expression = newExpression;
            }
            return expression;
        }

        private List<Expression> ExpressionList()
        {
            List<Expression> expressionList = new List<Expression>();
            expressionList.Add(Expression());
            if(current.data == ",")
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
                if (current.Match(TokenType.NUM))
                {
                    expression = new Number(current.data);
                    Consume(TokenType.NUM);
                }
                else if (current.data == "-")
                {
                    Consume(TokenType.ADDOP);
                    Expression expression1 = new Number("0");
                    Expression expression2 = Expression();
                    expression = new Operation(expression1, Op.SUB, expression2);
                }
                else if (current.data == "+")
                {
                    Consume(TokenType.ADDOP);
                    expression = Term();
                }
                else if (current.data == "(")
                {
                    Consume(TokenType.STOP);
                    expression = Expression();
                    Consume(TokenType.STOP);
                }
                else if (current.data == "NOT")
                {

                }
                else if (current.Match(TokenType.ID))
                {
                    Variable variable = new Variable(current.data);
                    Consume(TokenType.ID);
                    if (current.data == "[")
                    {
                        Consume();
                        Expression localexpression = Expression();
                        Variable vararray = new Variable(variable.varname, localexpression);
                        expression = vararray;
                    }
                    else if (current.data == "(")
                    {
                        List<Expression> expressionList = ExpressionList();
                        if (!this.program.subprograms.ContainsKey(variable.varname)) throw new ParseFailedException("Use of unregistered subroutine:  " + variable.varname);
                    }
                    else
                    {
                        expression = variable;
                    }
                }

                if (current.Match(TokenType.MULOP))
                {
                    Token token = current;
                    Consume();
                    Expression expression2 = Term();
                    Op operation = token.data == "*" ? Op.MULT : token.data == "/" ? Op.DIVIDE : token.data == "%" ? Op.MOD : Op.AND;
                    expression = new Operation(expression, operation, expression2);
                }
            }
            while (current.Match(TokenType.MULOP));

            return expression;
        }

        public void Consume(KeyWord key)
        {
            if (current.Match(key))
            {
                current = scanner.Pop();
            }
            else
            {
                throw new ParseFailedException("Incorrect Keyword: " + current.data);
            }
        }
        public void Consume(TokenType token)
        {
            if (current.Match(token))
            {
                current = scanner.Pop();
            }
            else
            {
                throw new ParseFailedException("Incorrect syntax: " + current.data);
            }
        }
        public void Consume()
        {
            current = scanner.Pop();
        }

    }    
}
