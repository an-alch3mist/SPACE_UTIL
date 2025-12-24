using System;
using System.Collections.Generic;
using System.Text;

namespace LOOPLanguage
{
    /// <summary>
    /// Tokenizes LOOP source code into a stream of tokens
    /// Handles Python-style indentation with INDENT/DEDENT tokens
    /// </summary>
    public class Lexer
    {
        #region Fields
        
        private string source;
        private List<Token> tokens = new List<Token>();
        private int start = 0;
        private int current = 0;
        private int line = 1;
        private Stack<int> indentStack = new Stack<int>();
        
        private static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>()
        {
            { "if", TokenType.IF },
            { "elif", TokenType.ELIF },
            { "else", TokenType.ELSE },
            { "while", TokenType.WHILE },
            { "for", TokenType.FOR },
            { "def", TokenType.DEF },
            { "return", TokenType.RETURN },
            { "class", TokenType.CLASS },
            { "break", TokenType.BREAK },
            { "continue", TokenType.CONTINUE },
            { "pass", TokenType.PASS },
            { "global", TokenType.GLOBAL },
            { "lambda", TokenType.LAMBDA },
            { "import", TokenType.IMPORT },
            { "and", TokenType.AND },
            { "or", TokenType.OR },
            { "not", TokenType.NOT },
            { "in", TokenType.IN },
            { "is", TokenType.IS },
            { "True", TokenType.TRUE },
            { "False", TokenType.FALSE },
            { "None", TokenType.NONE }
        };
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Tokenizes the input source code
        /// </summary>
        public List<Token> Tokenize(string input)
        {
            source = ValidateAndClean(input);
            tokens = new List<Token>();
            indentStack = new Stack<int>();
            indentStack.Push(0);
            start = 0;
            current = 0;
            line = 1;
            
            bool atLineStart = true;
            
            while (!IsAtEnd())
            {
                // Handle indentation at the start of a line
                if (atLineStart && !IsAtEnd())
                {
                    int indent = CountLeadingSpaces();
                    
                    // Skip empty lines and comment-only lines
                    if (Peek() == '\n' || Peek() == '#' || (Peek() == '/' && PeekNext() == '/'))
                    {
                        if (Peek() == '\n')
                        {
                            Advance();
                            line++;
                        }
                        else
                        {
                            SkipComment();
                        }
                        continue;
                    }
                    
                    ProcessIndentation(indent);
                    atLineStart = false;
                }
                
                start = current;
                ScanToken();
                
                // Check if we just added a NEWLINE token
                if (tokens.Count > 0 && tokens[tokens.Count - 1].Type == TokenType.NEWLINE)
                {
                    atLineStart = true;
                }
            }
            
            // Emit remaining DEDENTs
            while (indentStack.Count > 1)
            {
                indentStack.Pop();
                AddToken(TokenType.DEDENT);
            }
            
            AddToken(TokenType.EOF);
            return tokens;
        }
        
        #endregion
        
        #region Input Validation
        
        /// <summary>
        /// Validates and cleans the input source code
        /// </summary>
        private string ValidateAndClean(string input)
        {
            if (input == null) input = "";
            
            // Normalize line endings
            input = input.Replace("\r\n", "\n");
            input = input.Replace("\r", "\n");
            
            // Convert tabs to 4 spaces
            input = input.Replace("\t", "    ");
            
            // Remove invisible characters
            input = input.Replace("\v", "");
            input = input.Replace("\f", "");
            input = input.Replace("\uFEFF", "");
            
            // Ensure ends with newline
            if (!input.EndsWith("\n"))
            {
                input += "\n";
            }
            
            return input;
        }
        
        #endregion
        
        #region Indentation Handling
        
        /// <summary>
        /// Counts leading spaces at the current position
        /// </summary>
        private int CountLeadingSpaces()
        {
            int count = 0;
            while (!IsAtEnd() && Peek() == ' ')
            {
                count++;
                Advance();
            }
            return count;
        }
        
        /// <summary>
        /// Processes indentation changes and emits INDENT/DEDENT tokens
        /// </summary>
        private void ProcessIndentation(int spaces)
        {
            int currentIndent = indentStack.Peek();
            
            if (spaces > currentIndent)
            {
                // INDENT
                if (spaces % 4 != 0)
                {
                    throw new LexerError("Indentation must be multiple of 4 spaces", line);
                }
                indentStack.Push(spaces);
                AddToken(TokenType.INDENT);
            }
            else if (spaces < currentIndent)
            {
                // DEDENT (possibly multiple)
                while (indentStack.Count > 0 && indentStack.Peek() > spaces)
                {
                    indentStack.Pop();
                    AddToken(TokenType.DEDENT);
                }
                
                // Validate alignment
                if (indentStack.Count == 0 || indentStack.Peek() != spaces)
                {
                    throw new LexerError("Indentation mismatch - dedent does not match any outer indentation level", line);
                }
            }
        }
        
        #endregion
        
        #region Token Scanning
        
        /// <summary>
        /// Scans the next token
        /// </summary>
        private void ScanToken()
        {
            char c = Advance();
            
            switch (c)
            {
                // Whitespace (space handled by indentation, skip others)
                case ' ':
                    break;
                    
                // Newline
                case '\n':
                    AddToken(TokenType.NEWLINE);
                    line++;
                    break;
                    
                // Single-character tokens
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '[': AddToken(TokenType.LEFT_BRACKET); break;
                case ']': AddToken(TokenType.RIGHT_BRACKET); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case ':': AddToken(TokenType.COLON); break;
                case '~': AddToken(TokenType.TILDE); break;
                case '^': AddToken(TokenType.CARET); break;
                
                // Operators that can be compound
                case '+':
                    AddToken(Match('=') ? TokenType.PLUS_EQUAL : TokenType.PLUS);
                    break;
                case '-':
                    AddToken(Match('=') ? TokenType.MINUS_EQUAL : TokenType.MINUS);
                    break;
                case '*':
                    if (Match('*'))
                    {
                        AddToken(TokenType.DOUBLE_STAR);
                    }
                    else if (Match('='))
                    {
                        AddToken(TokenType.STAR_EQUAL);
                    }
                    else
                    {
                        AddToken(TokenType.STAR);
                    }
                    break;
                case '/':
                    if (Match('/'))
                    {
                        // Comment - skip to end of line
                        SkipComment();
                    }
                    else if (Match('='))
                    {
                        AddToken(TokenType.SLASH_EQUAL);
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case '%':
                    AddToken(TokenType.PERCENT);
                    break;
                case '&':
                    AddToken(TokenType.AMPERSAND);
                    break;
                case '|':
                    AddToken(TokenType.PIPE);
                    break;
                    
                // Comparison operators
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '!':
                    if (Match('='))
                    {
                        AddToken(TokenType.BANG_EQUAL);
                    }
                    else
                    {
                        throw new LexerError($"Unexpected character '!'", line);
                    }
                    break;
                case '<':
                    if (Match('<'))
                    {
                        AddToken(TokenType.LEFT_SHIFT);
                    }
                    else if (Match('='))
                    {
                        AddToken(TokenType.LESS_EQUAL);
                    }
                    else
                    {
                        AddToken(TokenType.LESS);
                    }
                    break;
                case '>':
                    if (Match('>'))
                    {
                        AddToken(TokenType.RIGHT_SHIFT);
                    }
                    else if (Match('='))
                    {
                        AddToken(TokenType.GREATER_EQUAL);
                    }
                    else
                    {
                        AddToken(TokenType.GREATER);
                    }
                    break;
                    
                // Comments
                case '#':
                    SkipComment();
                    break;
                    
                // String literals
                case '"':
                case '\'':
                    ScanString(c);
                    break;
                    
                default:
                    if (IsDigit(c))
                    {
                        ScanNumber();
                    }
                    else if (IsAlpha(c))
                    {
                        ScanIdentifier();
                    }
                    else
                    {
                        throw new LexerError($"Unexpected character '{c}'", line);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Skips a comment (# or //) to the end of the line
        /// </summary>
        private void SkipComment()
        {
            while (!IsAtEnd() && Peek() != '\n')
            {
                Advance();
            }
        }
        
        /// <summary>
        /// Scans a string literal
        /// </summary>
        private void ScanString(char quote)
        {
            StringBuilder sb = new StringBuilder();
            
            while (!IsAtEnd() && Peek() != quote)
            {
                if (Peek() == '\n')
                {
                    throw new LexerError("Unterminated string", line);
                }
                
                if (Peek() == '\\')
                {
                    Advance(); // consume backslash
                    if (IsAtEnd())
                    {
                        throw new LexerError("Unterminated string", line);
                    }
                    
                    char escaped = Advance();
                    switch (escaped)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case '\\': sb.Append('\\'); break;
                        case '"': sb.Append('"'); break;
                        case '\'': sb.Append('\''); break;
                        default:
                            sb.Append(escaped);
                            break;
                    }
                }
                else
                {
                    sb.Append(Advance());
                }
            }
            
            if (IsAtEnd())
            {
                throw new LexerError("Unterminated string", line);
            }
            
            Advance(); // consume closing quote
            AddToken(TokenType.STRING, sb.ToString());
        }
        
        /// <summary>
        /// Scans a number literal
        /// </summary>
        private void ScanNumber()
        {
            bool isFloat = false;
            
            while (IsDigit(Peek()))
            {
                Advance();
            }
            
            // Check for decimal point
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                isFloat = true;
                Advance(); // consume '.'
                
                while (IsDigit(Peek()))
                {
                    Advance();
                }
            }
            
            string numStr = source.Substring(start, current - start);
            
            if (isFloat)
            {
                AddToken(TokenType.NUMBER, double.Parse(numStr));
            }
            else
            {
                // Store as int in double for integer values
                AddToken(TokenType.NUMBER, (double)int.Parse(numStr));
            }
        }
        
        /// <summary>
        /// Scans an identifier or keyword
        /// </summary>
        private void ScanIdentifier()
        {
            while (IsAlphaNumeric(Peek()))
            {
                Advance();
            }
            
            string text = source.Substring(start, current - start);
            TokenType type = keywords.ContainsKey(text) ? keywords[text] : TokenType.IDENTIFIER;
            AddToken(type);
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool IsAtEnd()
        {
            return current >= source.Length;
        }
        
        private char Advance()
        {
            return source[current++];
        }
        
        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (source[current] != expected) return false;
            
            current++;
            return true;
        }
        
        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return source[current];
        }
        
        private char PeekNext()
        {
            if (current + 1 >= source.Length) return '\0';
            return source[current + 1];
        }
        
        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }
        
        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   c == '_';
        }
        
        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }
        
        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }
        
        private void AddToken(TokenType type, object literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }
        
        #endregion
    }
}
