namespace LOOPLanguage
{
    #region Token Types Enum
    
    /// <summary>
    /// All possible token types in the LOOP language
    /// </summary>
    public enum TokenType
    {
        // Structural
        INDENT,
        DEDENT,
        NEWLINE,
        EOF,
        
        // Literals
        IDENTIFIER,
        STRING,
        NUMBER,
        
        // Keywords - Control Flow
        IF,
        ELIF,
        ELSE,
        WHILE,
        FOR,
        BREAK,
        CONTINUE,
        PASS,
        
        // Keywords - Functions and Classes
        DEF,
        RETURN,
        LAMBDA,
        CLASS,
        
        // Keywords - Scope
        GLOBAL,
        IMPORT,
        
        // Keywords - Logical
        AND,
        OR,
        NOT,
        IN,
        IS,
        
        // Keywords - Literals
        TRUE,
        FALSE,
        NONE,
        
        // Operators - Arithmetic
        PLUS,           // +
        MINUS,          // -
        STAR,           // *
        SLASH,          // /
        DOUBLE_SLASH,   // //
        PERCENT,        // %
        DOUBLE_STAR,    // **
        
        // Operators - Comparison
        EQUAL_EQUAL,    // ==
        BANG_EQUAL,     // !=
        LESS,           // <
        GREATER,        // >
        LESS_EQUAL,     // <=
        GREATER_EQUAL,  // >=
        
        // Operators - Assignment
        EQUAL,          // =
        PLUS_EQUAL,     // +=
        MINUS_EQUAL,    // -=
        STAR_EQUAL,     // *=
        SLASH_EQUAL,    // /=
        
        // Operators - Bitwise
        AMPERSAND,      // &
        PIPE,           // |
        CARET,          // ^
        TILDE,          // ~
        LEFT_SHIFT,     // <<
        RIGHT_SHIFT,    // >>
        
        // Delimiters
        LEFT_PAREN,     // (
        RIGHT_PAREN,    // )
        LEFT_BRACKET,   // [
        RIGHT_BRACKET,  // ]
        LEFT_BRACE,     // {
        RIGHT_BRACE,    // }
        DOT,            // .
        COMMA,          // ,
        COLON           // :
    }
    
    #endregion
    
    #region Token Class
    
    /// <summary>
    /// Represents a single token in the source code
    /// </summary>
    public class Token
    {
        public TokenType Type { get; private set; }
        public string Lexeme { get; private set; }
        public object Literal { get; private set; }
        public int LineNumber { get; private set; }
        
        /// <summary>
        /// Creates a new token
        /// </summary>
        /// <param name="type">Type of the token</param>
        /// <param name="lexeme">Raw text from source</param>
        /// <param name="literal">Parsed literal value (for NUMBER, STRING)</param>
        /// <param name="line">Line number in source code</param>
        public Token(TokenType type, string lexeme, object literal, int line)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            LineNumber = line;
        }
        
        public override string ToString()
        {
            return $"{Type} {Lexeme} {Literal}";
        }
    }
    
    #endregion
}
