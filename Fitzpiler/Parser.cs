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

                Declarations();
                this.program.statements = Compound_Statement();
                Console.Write(program.ToString());
            }
            catch (ParseFailedException e)
            {
                Console.WriteLine("Tokenization failed: line " + scanner.tokenizer.line + "\n\t" + e.Message);
            }
        }
        private void Declarations()
        {
            if (current.Match(KeyWord.VAR))
            {
                Consume(KeyWord.VAR);
                do
                {
                    string varname = current.data;
                    Consume(TokenType.ID);
                    Consume(TokenType.STOP);
                    if (current.Match(KeyWord.INTEGER))
                    {
                        VarType vartype = VarType.INTEGER;
                        Consume(KeyWord.INTEGER);
                        if (program.vartypes.ContainsKey(varname)) throw new ParseFailedException("Variable already defined");
                        program.vartypes.Add(varname, vartype);
                    }
                    else if (current.Match(KeyWord.REAL))
                    {
                        VarType vartype = VarType.REAL;
                        Consume(KeyWord.REAL);
                        if (program.vartypes.ContainsKey(varname)) throw new ParseFailedException("Variable already defined");
                        program.vartypes.Add(varname, vartype);
                    }
                    else if (current.Match(KeyWord.ARRAY)) //deal with this later
                    {

                    }
                } while (current.Match(TokenType.ID) && scanner.Peek().data == ":");

            }
        }

        private List<Statement> Compound_Statement()
        {
            Consume(KeyWord.BEGIN);
            return Optional_Statements();
            Consume(KeyWord.END);
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
            if (scanner.Peek().TYPE == TokenType.ASSIGNOP)
            {
                string varname = current.data;
                Consume(TokenType.ID);
                Consume(TokenType.ASSIGNOP);
                Expression expression = Expression();
                Assignment assignment = new Assignment(varname, expression);
                return assignment;
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
                return ifstatement;
            }
            return null;
        }

        private Expression Expression()
        {
            Expression expression = Term();
            while (current.TYPE == TokenType.ADDOP)
            {
                Op operation = Op.ADD;
                if (current.data == "+") operation = Op.ADD;
                if (current.data == "-") operation = Op.SUB;
                if (current.data == "&") operation = Op.AND;
                Consume();
                Expression expression2 = Term();
                Expression newexpression = new Operation(expression, operation, expression2);
                expression = newexpression;
            }
            while (current.TYPE == TokenType.RELOP)
            {
                Op operation = Op.ADD;
                if (current.data == "=") operation = Op.EQ;
                else if (current.data == "<=") operation = Op.LEQ;
                else if (current.data == ">=") operation = Op.GEQ;
                else if (current.data == ">") operation = Op.GRT;
                else if (current.data == "<") operation = Op.LST;
                Consume();
                Expression expression2 = Term();
                Expression newexpression = new Operation(expression, operation, expression2);
                expression = newexpression;
            }
            return expression;
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
                        //handle arrays here
                    }
                    else if (current.data == "(")
                    {
                        //Expression_List();
                    }
                    expression = variable;
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

        //private List<Expression> Expression_List()
        //{

        //}
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


    class Program
    {
        public string id;
        public Dictionary<string, VarType> vartypes = new Dictionary<string, VarType>();
        public Dictionary<string, Subprogram> subprograms = new Dictionary<string, Subprogram>();
        public List<Statement> statements = new List<Statement>();

        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("PROGRAM: " + this.id + "\nVariables:\n");
            if(vartypes != null) foreach (var variable in vartypes.Keys) sb.Append("\t" + variable + ": " + vartypes[variable] + "\n");
            sb.Append("Subprograms:\n");
            if(subprograms != null) foreach (var subroutine in subprograms) sb.Append("\t" + subroutine + "\n");
            sb.Append("Statements:\n");
            if (statements != null)
            {
                foreach (Statement s in statements)
                {
                    sb.Append(s.ToString(0)+ "\n");
                }
            }
            return sb.ToString();
        }
    }

    
}
