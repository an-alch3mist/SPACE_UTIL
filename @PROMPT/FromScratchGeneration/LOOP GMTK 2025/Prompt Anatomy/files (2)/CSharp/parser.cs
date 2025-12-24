using System;
using System.Collections.Generic;

namespace LOOPLanguage
{
    /// <summary>
    /// Recursive descent parser for LOOP language
    /// Builds an Abstract Syntax Tree from tokens
    /// </summary>
    public class Parser
    {
        #region Fields
        
        private List<Token> tokens;
        private int current = 0;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Parses tokens into an AST
        /// </summary>
        public List<Stmt> Parse(List<Token> tokenList)
        {
            tokens = tokenList;
            current = 0;
            
            List<Stmt> statements = new List<Stmt>();
            
            while (!IsAtEnd())
            {
                // Skip newlines at top level
                if (Check(TokenType.NEWLINE))
                {
                    Advance();
                    continue;
                }
                
                Stmt stmt = ParseStatement();
                if (stmt != null)
                {
                    statements.Add(stmt);
                }
            }
            
            return statements;
        }
        
        #endregion
        
        #region Statement Parsing
        
        private Stmt ParseStatement()
        {
            try
            {
                // Compound statements
                if (Match(TokenType.IF)) return ParseIfStatement();
                if (Match(TokenType.WHILE)) return ParseWhileStatement();
                if (Match(TokenType.FOR)) return ParseForStatement();
                if (Match(TokenType.DEF)) return ParseFunctionDef();
                if (Match(TokenType.CLASS)) return ParseClassDef();
                
                // Simple statements
                return ParseSimpleStatement();
            }
            catch (ParseError)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ParseError(e.Message, CurrentLine());
            }
        }
        
        private Stmt ParseSimpleStatement()
        {
            Stmt stmt = null;
            
            if (Match(TokenType.RETURN))
            {
                stmt = ParseReturnStatement();
            }
            else if (Match(TokenType.BREAK))
            {
                stmt = new BreakStmt { LineNumber = Previous().LineNumber };
            }
            else if (Match(TokenType.CONTINUE))
            {
                stmt = new ContinueStmt { LineNumber = Previous().LineNumber };
            }
            else if (Match(TokenType.PASS))
            {
                stmt = new PassStmt { LineNumber = Previous().LineNumber };
            }
            else if (Match(TokenType.GLOBAL))
            {
                stmt = ParseGlobalStatement();
            }
            else if (Match(TokenType.IMPORT))
            {
                stmt = ParseImportStatement();
            }
            else
            {
                // Try assignment or expression statement
                stmt = ParseAssignmentOrExpression();
            }
            
            ConsumeNewlineOrEOF();
            return stmt;
        }
        
        private Stmt ParseReturnStatement()
        {
            int line = Previous().LineNumber;
            Expr value = null;
            
            if (!Check(TokenType.NEWLINE) && !IsAtEnd())
            {
                value = ParseExpression();
            }
            
            return new ReturnStmt(value) { LineNumber = line };
        }
        
        private Stmt ParseGlobalStatement()
        {
            int line = Previous().LineNumber;
            List<string> variables = new List<string>();
            
            do
            {
                Token varToken = Consume(TokenType.IDENTIFIER, "Expected variable name after 'global'");
                variables.Add(varToken.Lexeme);
            } while (Match(TokenType.COMMA));
            
            return new GlobalStmt(variables) { LineNumber = line };
        }
        
        private Stmt ParseImportStatement()
        {
            int line = Previous().LineNumber;
            Token enumName = Consume(TokenType.IDENTIFIER, "Expected enum name after 'import'");
            
            string memberName = null;
            if (Match(TokenType.DOT))
            {
                Token member = Consume(TokenType.IDENTIFIER, "Expected member name after '.'");
                memberName = member.Lexeme;
            }
            
            return new ImportStmt(enumName.Lexeme, memberName) { LineNumber = line };
        }
        
        private Stmt ParseAssignmentOrExpression()
        {
            int line = CurrentLine();
            Expr expr = ParseExpression();
            
            // Check for assignment
            if (expr is VariableExpr varExpr && IsAssignmentOperator())
            {
                Token opToken = Advance();
                string op = opToken.Lexeme;
                Expr value = ParseExpression();
                
                return new AssignmentStmt(varExpr.Name, value, op) { LineNumber = line };
            }
            
            return new ExpressionStmt(expr) { LineNumber = line };
        }
        
        private bool IsAssignmentOperator()
        {
            return Check(TokenType.EQUAL) || Check(TokenType.PLUS_EQUAL) ||
                   Check(TokenType.MINUS_EQUAL) || Check(TokenType.STAR_EQUAL) ||
                   Check(TokenType.SLASH_EQUAL);
        }
        
        private Stmt ParseIfStatement()
        {
            int line = Previous().LineNumber;
            Expr condition = ParseExpression();
            Consume(TokenType.COLON, "Expected ':' after if condition");
            List<Stmt> thenBranch = ParseSuite();
            
            List<Stmt> elseBranch = null;
            
            // Handle elif/else
            while (Match(TokenType.ELIF))
            {
                Expr elifCondition = ParseExpression();
                Consume(TokenType.COLON, "Expected ':' after elif condition");
                List<Stmt> elifBody = ParseSuite();
                
                // Convert elif to nested if
                if (elseBranch == null)
                {
                    elseBranch = new List<Stmt>();
                }
                elseBranch.Add(new IfStmt(elifCondition, elifBody) { LineNumber = line });
            }
            
            if (Match(TokenType.ELSE))
            {
                Consume(TokenType.COLON, "Expected ':' after else");
                
                if (elseBranch == null)
                {
                    elseBranch = ParseSuite();
                }
                else
                {
                    // There were elif clauses, so we need to attach else to the last one
                    List<Stmt> finalElse = ParseSuite();
                    IfStmt lastIf = FindLastIfInChain(elseBranch);
                    if (lastIf != null)
                    {
                        lastIf.ElseBranch = finalElse;
                    }
                }
            }
            
            return new IfStmt(condition, thenBranch, elseBranch) { LineNumber = line };
        }
        
        private IfStmt FindLastIfInChain(List<Stmt> stmts)
        {
            if (stmts == null || stmts.Count == 0) return null;
            
            for (int i = stmts.Count - 1; i >= 0; i--)
            {
                if (stmts[i] is IfStmt ifStmt)
                {
                    if (ifStmt.ElseBranch != null && ifStmt.ElseBranch.Count > 0)
                    {
                        IfStmt nested = FindLastIfInChain(ifStmt.ElseBranch);
                        if (nested != null) return nested;
                    }
                    return ifStmt;
                }
            }
            
            return null;
        }
        
        private Stmt ParseWhileStatement()
        {
            int line = Previous().LineNumber;
            Expr condition = ParseExpression();
            Consume(TokenType.COLON, "Expected ':' after while condition");
            List<Stmt> body = ParseSuite();
            
            return new WhileStmt(condition, body) { LineNumber = line };
        }
        
        private Stmt ParseForStatement()
        {
            int line = Previous().LineNumber;
            Token varToken = Consume(TokenType.IDENTIFIER, "Expected variable name in for loop");
            Consume(TokenType.IN, "Expected 'in' in for loop");
            Expr iterable = ParseExpression();
            Consume(TokenType.COLON, "Expected ':' after for clause");
            List<Stmt> body = ParseSuite();
            
            return new ForStmt(varToken.Lexeme, iterable, body) { LineNumber = line };
        }
        
        private Stmt ParseFunctionDef()
        {
            int line = Previous().LineNumber;
            Token name = Consume(TokenType.IDENTIFIER, "Expected function name");
            Consume(TokenType.LEFT_PAREN, "Expected '(' after function name");
            
            List<string> parameters = new List<string>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    Token param = Consume(TokenType.IDENTIFIER, "Expected parameter name");
                    parameters.Add(param.Lexeme);
                } while (Match(TokenType.COMMA));
            }
            
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameters");
            Consume(TokenType.COLON, "Expected ':' after function signature");
            List<Stmt> body = ParseSuite();
            
            return new FunctionDefStmt(name.Lexeme, parameters, body) { LineNumber = line };
        }
        
        private Stmt ParseClassDef()
        {
            int line = Previous().LineNumber;
            Token name = Consume(TokenType.IDENTIFIER, "Expected class name");
            Consume(TokenType.COLON, "Expected ':' after class name");
            
            Consume(TokenType.NEWLINE, "Expected newline after ':'");
            Consume(TokenType.INDENT, "Expected indented block after class definition");
            
            List<FunctionDefStmt> methods = new List<FunctionDefStmt>();
            
            while (!Check(TokenType.DEDENT) && !IsAtEnd())
            {
                if (Match(TokenType.NEWLINE))
                {
                    continue;
                }
                
                if (Match(TokenType.DEF))
                {
                    Stmt methodStmt = ParseFunctionDef();
                    if (methodStmt is FunctionDefStmt funcDef)
                    {
                        methods.Add(funcDef);
                    }
                }
                else
                {
                    throw new ParseError("Only method definitions allowed in class body", CurrentLine());
                }
            }
            
            Consume(TokenType.DEDENT, "Expected dedent after class body");
            
            return new ClassDefStmt(name.Lexeme, methods) { LineNumber = line };
        }
        
        private List<Stmt> ParseSuite()
        {
            List<Stmt> statements = new List<Stmt>();
            
            // Check for single-line suite
            if (!Check(TokenType.NEWLINE))
            {
                statements.Add(ParseSimpleStatement());
                return statements;
            }
            
            // Multi-line suite
            Consume(TokenType.NEWLINE, "Expected newline");
            Consume(TokenType.INDENT, "Expected indent");
            
            while (!Check(TokenType.DEDENT) && !IsAtEnd())
            {
                if (Match(TokenType.NEWLINE))
                {
                    continue;
                }
                
                statements.Add(ParseStatement());
            }
            
            Consume(TokenType.DEDENT, "Expected dedent");
            
            return statements;
        }
        
        #endregion
        
        #region Expression Parsing (Precedence Climbing)
        
        private Expr ParseExpression()
        {
            return ParseLambda();
        }
        
        private Expr ParseLambda()
        {
            if (Match(TokenType.LAMBDA))
            {
                int line = Previous().LineNumber;
                List<string> parameters = new List<string>();
                
                // Parse parameters (if any)
                if (!Check(TokenType.COLON))
                {
                    do
                    {
                        Token param = Consume(TokenType.IDENTIFIER, "Expected parameter name");
                        parameters.Add(param.Lexeme);
                    } while (Match(TokenType.COMMA));
                }
                
                Consume(TokenType.COLON, "Expected ':' after lambda parameters");
                Expr body = ParseLogicalOr(); // Parse expression (not ParseLambda to avoid left-recursion)
                
                return new LambdaExpr(parameters, body) { LineNumber = line };
            }
            
            return ParseLogicalOr();
        }
        
        private Expr ParseLogicalOr()
        {
            Expr expr = ParseLogicalAnd();
            
            while (Match(TokenType.OR))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseLogicalAnd();
                expr = new BinaryExpr(expr, op, right) { LineNumber = line };
            }
            
            return expr;
        }
        
        private Expr ParseLogicalAnd()
        {
            Expr expr = ParseLogicalNot();
            
            while (Match(TokenType.AND))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseLogicalNot();
                expr = new BinaryExpr(expr, op, right) { LineNumber = line };
            }
            
            return expr;
        }
        
        private Expr ParseLogicalNot()
        {
            if (Match(TokenType.NOT))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr operand = ParseLogicalNot();
                return new UnaryExpr(op, operand) { LineNumber = line };
            }
            
            return ParseComparison();
        }
        
        private Expr ParseComparison()
        {
            Expr expr = ParseBitwiseOr();
            
            while (Match(TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL, TokenType.LESS,
                        TokenType.GREATER, TokenType.LESS_EQUAL, TokenType.GREATER_EQUAL,
                        TokenType.IN, TokenType.IS))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseBitwiseOr();
                expr = new BinaryExpr(expr, op, right) { LineNumber = line };
            }
            
            return expr;
        }
        
        private Expr ParseBitwiseOr()
        {
            Expr expr = ParseBitwiseXor();
            
            while (Match(TokenType.PIPE))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseBitwiseXor();
                expr = new BinaryExpr(expr, op, right) { LineNumber = line };
            }
            
            return expr;
        }
        
        private Expr ParseBitwiseXor()
        {
            Expr expr = ParseBitwiseAnd();
            
            while (Match(TokenType.CARET))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseBitwiseAnd();
                expr = new BinaryExpr(expr, op, right) { LineNumber = line };
            }
            
            return expr;
        }
        
        private Expr ParseBitwiseAnd()
        {
            Expr expr = ParseShift();
            
            while (Match(TokenType.AMPERSAND))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseShift();
                expr = new BinaryExpr(expr, op, right) { LineNumber = line };
            }
            
            return expr;
        }
        
        private Expr ParseShift()
        {
            Expr expr = ParseAddition();
            
            while (Match(TokenType.LEFT_SHIFT, TokenType.RIGHT_SHIFT))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseAddition();
                expr = new BinaryExpr(expr, op, right) { LineNumber = line };
            }
            
            return expr;
        }
        
        private Expr ParseAddition()
        {
            Expr expr = ParseMultiplication();
            
            while (Match(TokenType.PLUS, TokenType.MINUS))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseMultiplication();
                expr = new BinaryExpr(expr, op, right) { LineNumber = line };
            }
            
            return expr;
        }
        
        private Expr ParseMultiplication()
        {
            Expr expr = ParseExponentiation();
            
            while (Match(TokenType.STAR, TokenType.SLASH, TokenType.PERCENT))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseExponentiation();
                expr = new BinaryExpr(expr, op, right) { LineNumber = line };
            }
            
            return expr;
        }
        
        private Expr ParseExponentiation()
        {
            Expr expr = ParseUnary();
            
            // Right-associative: 2**3**2 = 2**(3**2)
            if (Match(TokenType.DOUBLE_STAR))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr right = ParseExponentiation(); // Recursive for right-associativity
                return new BinaryExpr(expr, op, right) { LineNumber = line };
            }
            
            return expr;
        }
        
        private Expr ParseUnary()
        {
            if (Match(TokenType.MINUS, TokenType.PLUS, TokenType.TILDE, TokenType.NOT))
            {
                int line = Previous().LineNumber;
                TokenType op = Previous().Type;
                Expr operand = ParseUnary();
                return new UnaryExpr(op, operand) { LineNumber = line };
            }
            
            return ParsePrimary();
        }
        
        private Expr ParsePrimary()
        {
            Expr expr = ParseAtom();
            
            // Handle postfix operators (call, index, member access)
            while (true)
            {
                int line = CurrentLine();
                
                if (Match(TokenType.LEFT_PAREN))
                {
                    // Function call
                    List<Expr> arguments = new List<Expr>();
                    
                    if (!Check(TokenType.RIGHT_PAREN))
                    {
                        do
                        {
                            arguments.Add(ParseExpression());
                        } while (Match(TokenType.COMMA));
                    }
                    
                    Consume(TokenType.RIGHT_PAREN, "Expected ')' after arguments");
                    expr = new CallExpr(expr, arguments) { LineNumber = line };
                }
                else if (Match(TokenType.LEFT_BRACKET))
                {
                    // Indexing or slicing
                    Expr index = null;
                    Expr start = null;
                    Expr stop = null;
                    Expr step = null;
                    
                    bool isSlice = false;
                    
                    // Parse start or index
                    if (!Check(TokenType.COLON))
                    {
                        index = ParseExpression();
                        start = index;
                    }
                    
                    // Check for slice
                    if (Match(TokenType.COLON))
                    {
                        isSlice = true;
                        
                        // Parse stop
                        if (!Check(TokenType.COLON) && !Check(TokenType.RIGHT_BRACKET))
                        {
                            stop = ParseExpression();
                        }
                        
                        // Parse step
                        if (Match(TokenType.COLON))
                        {
                            if (!Check(TokenType.RIGHT_BRACKET))
                            {
                                step = ParseExpression();
                            }
                        }
                    }
                    
                    Consume(TokenType.RIGHT_BRACKET, "Expected ']'");
                    
                    if (isSlice)
                    {
                        expr = new SliceExpr(expr, start, stop, step) { LineNumber = line };
                    }
                    else
                    {
                        expr = new IndexExpr(expr, index) { LineNumber = line };
                    }
                }
                else if (Match(TokenType.DOT))
                {
                    // Member access
                    Token member = Consume(TokenType.IDENTIFIER, "Expected member name after '.'");
                    expr = new MemberAccessExpr(expr, member.Lexeme) { LineNumber = line };
                }
                else
                {
                    break;
                }
            }
            
            return expr;
        }
        
        private Expr ParseAtom()
        {
            int line = CurrentLine();
            
            // Literals
            if (Match(TokenType.TRUE))
                return new LiteralExpr(true) { LineNumber = line };
            if (Match(TokenType.FALSE))
                return new LiteralExpr(false) { LineNumber = line };
            if (Match(TokenType.NONE))
                return new LiteralExpr(null) { LineNumber = line };
            
            if (Match(TokenType.NUMBER))
                return new LiteralExpr(Previous().Literal) { LineNumber = line };
            
            if (Match(TokenType.STRING))
                return new LiteralExpr(Previous().Literal) { LineNumber = line };
            
            if (Match(TokenType.IDENTIFIER))
                return new VariableExpr(Previous().Lexeme) { LineNumber = line };
            
            // Parenthesized expression or tuple
            if (Match(TokenType.LEFT_PAREN))
            {
                if (Check(TokenType.RIGHT_PAREN))
                {
                    Advance();
                    return new TupleExpr(new List<Expr>()) { LineNumber = line };
                }
                
                Expr first = ParseExpression();
                
                if (Match(TokenType.COMMA))
                {
                    // Tuple
                    List<Expr> elements = new List<Expr> { first };
                    
                    if (!Check(TokenType.RIGHT_PAREN))
                    {
                        do
                        {
                            elements.Add(ParseExpression());
                        } while (Match(TokenType.COMMA) && !Check(TokenType.RIGHT_PAREN));
                    }
                    
                    Consume(TokenType.RIGHT_PAREN, "Expected ')' after tuple");
                    return new TupleExpr(elements) { LineNumber = line };
                }
                else
                {
                    // Parenthesized expression
                    Consume(TokenType.RIGHT_PAREN, "Expected ')'");
                    return first;
                }
            }
            
            // List literal or list comprehension
            if (Match(TokenType.LEFT_BRACKET))
            {
                if (Check(TokenType.RIGHT_BRACKET))
                {
                    Advance();
                    return new ListExpr(new List<Expr>()) { LineNumber = line };
                }
                
                Expr first = ParseExpression();
                
                // Check for list comprehension
                if (Match(TokenType.FOR))
                {
                    Token varToken = Consume(TokenType.IDENTIFIER, "Expected variable in list comprehension");
                    Consume(TokenType.IN, "Expected 'in' in list comprehension");
                    Expr iterable = ParseExpression();
                    
                    Expr condition = null;
                    if (Match(TokenType.IF))
                    {
                        condition = ParseExpression();
                    }
                    
                    Consume(TokenType.RIGHT_BRACKET, "Expected ']' after list comprehension");
                    return new ListCompExpr(first, varToken.Lexeme, iterable, condition) { LineNumber = line };
                }
                else
                {
                    // Regular list
                    List<Expr> elements = new List<Expr> { first };
                    
                    while (Match(TokenType.COMMA))
                    {
                        if (Check(TokenType.RIGHT_BRACKET)) break;
                        elements.Add(ParseExpression());
                    }
                    
                    Consume(TokenType.RIGHT_BRACKET, "Expected ']'");
                    return new ListExpr(elements) { LineNumber = line };
                }
            }
            
            // Dictionary literal
            if (Match(TokenType.LEFT_BRACE))
            {
                List<Expr> keys = new List<Expr>();
                List<Expr> values = new List<Expr>();
                
                if (!Check(TokenType.RIGHT_BRACE))
                {
                    do
                    {
                        Expr key = ParseExpression();
                        Consume(TokenType.COLON, "Expected ':' in dictionary literal");
                        Expr value = ParseExpression();
                        
                        keys.Add(key);
                        values.Add(value);
                    } while (Match(TokenType.COMMA));
                }
                
                Consume(TokenType.RIGHT_BRACE, "Expected '}'");
                return new DictExpr(keys, values) { LineNumber = line };
            }
            
            throw new ParseError($"Unexpected token: {Peek().Lexeme}", line);
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }
        
        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }
        
        private Token Advance()
        {
            if (!IsAtEnd()) current++;
            return Previous();
        }
        
        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }
        
        private Token Peek()
        {
            return tokens[current];
        }
        
        private Token Previous()
        {
            return tokens[current - 1];
        }
        
        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw new ParseError(message, CurrentLine());
        }
        
        private void ConsumeNewlineOrEOF()
        {
            if (!IsAtEnd() && !Check(TokenType.NEWLINE) && !Check(TokenType.DEDENT))
            {
                throw new ParseError("Expected newline after statement", CurrentLine());
            }
            
            while (Match(TokenType.NEWLINE)) { }
        }
        
        private int CurrentLine()
        {
            return Peek().LineNumber;
        }
        
        #endregion
    }
}
