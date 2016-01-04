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
                Consume(KEYWORD.PROGRAM);
                this.program.id = current.data;
                Consume(TOKENTYPE.ID);

                Declarations();
                Compound_Statement();

            }
            catch(ParseFailedException e)
            {
                Console.WriteLine("Tokenization failed: line " + scanner.tokenizer.line + "\n\t" + e.Message);
            }
        }
        private void Declarations()
        {
            if(current.Match(KEYWORD.VAR))
            {
                Consume(KEYWORD.VAR);
                do
                {
                    string varname = current.data;
                    Consume(TOKENTYPE.ID);
                    Consume(TOKENTYPE.STOP);
                    if (current.Match(KEYWORD.INTEGER))
                    {
                        VarType vartype = VarType.INTEGER;
                        Consume(KEYWORD.INTEGER);
                        if (program.vartypes.ContainsKey(varname)) throw new ParseFailedException("Variable already defined"); 
                        program.vartypes.Add(varname, vartype);
                    }
                    else if (current.Match(KEYWORD.REAL))
                    {
                        VarType vartype = VarType.REAL;
                        Consume(KEYWORD.REAL);
                        if (program.vartypes.ContainsKey(varname)) throw new ParseFailedException("Variable already defined");
                        program.vartypes.Add(varname, vartype);
                    }
                    else if (current.Match(KEYWORD.ARRAY)) //deal with this later
                    {

                    }
                } while (current.Match(TOKENTYPE.ID) && scanner.Peek().data == ":");
                    
            }
        }

        private void Compound_Statement()
        {
            Consume(KEYWORD.BEGIN);
            Optional_Statements();
            Consume(KEYWORD.END);
        }

        private void Optional_Statements()
        {
            if(!current.Match(KEYWORD.END))
            {
                Statement_List();
            }
        }

        private List<Statement> Statement_List()
        {
            List<Statement> statements = new List<Statement>();
            statements.Add(Statement());
            while (!(current.data == ";"))
            {
                Consume(TOKENTYPE.STOP);
                statements.Add(Statement());
            }
            return statements;
        }

        private Statement Statement()
        {
            if(scanner.Peek().TYPE == TOKENTYPE.ASSIGNOP)
            {
                string varname = current.data;
                Consume(TOKENTYPE.ID);
                Consume(TOKENTYPE.ASSIGNOP);
                Expression expression = Expression();
                Assignment assignment = new Assignment(varname, expression);
                return assignment;
            }

            if(current.Match(KEYWORD.IF))
            {
                Consume(KEYWORD.IF);
                Expression expression = Expression();
                Consume(KEYWORD.THEN);
                Statement statement1 = Statement();
                Consume(KEYWORD.ELSE);
                Statement statement2 = Statement();
                IfStatement ifstatement = new IfStatement(expression, statement1, statement2);
                return ifstatement;
            }
            return null;
        }

        private Expression Expression()
        {
            return null;
        }
        public void Consume(KEYWORD key)
        {
            if(current.Match(key))
            {
                current = scanner.Pop();
            }
            else
            {
                throw new ParseFailedException("Incorrect Keyword: " + current.data);
            }
        }
        public void Consume(TOKENTYPE token)
        {
            if(current.Match(token))
            {
                current = scanner.Pop();
            }
            else
            {
                throw new ParseFailedException("Incorrect syntax: " + current.data);
            }
        }
    }


    class Program
    {
        public string id;
        public Dictionary<string, VarType> vartypes = new Dictionary<string, VarType>();
        public Dictionary<string, Subprogram> subprograms = new Dictionary<string, Subprogram>();
        public List<Statement> statements = new List<Statement>();
    }

    
}
