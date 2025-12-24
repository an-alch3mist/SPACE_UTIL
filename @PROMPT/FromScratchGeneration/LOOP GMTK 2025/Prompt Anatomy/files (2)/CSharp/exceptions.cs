using System;

namespace LOOPLanguage
{
    #region Custom Exception Classes
    
    /// <summary>
    /// Base exception for all LOOP language errors
    /// </summary>
    public class LOOPException : Exception
    {
        public LOOPException(string message) : base(message) { }
    }
    
    /// <summary>
    /// Thrown during lexical analysis (tokenization)
    /// </summary>
    public class LexerError : LOOPException
    {
        public int LineNumber { get; private set; }
        
        public LexerError(string message, int lineNumber) 
            : base($"LexerError (Line {lineNumber}): {message}")
        {
            LineNumber = lineNumber;
        }
    }
    
    /// <summary>
    /// Thrown during parsing (syntax analysis)
    /// </summary>
    public class ParseError : LOOPException
    {
        public int LineNumber { get; private set; }
        
        public ParseError(string message, int lineNumber) 
            : base($"ParseError (Line {lineNumber}): {message}")
        {
            LineNumber = lineNumber;
        }
    }
    
    /// <summary>
    /// Thrown during runtime execution
    /// </summary>
    public class RuntimeError : LOOPException
    {
        public int LineNumber { get; private set; }
        
        public RuntimeError(string message) : base(message) { }
        
        public RuntimeError(string message, int lineNumber) 
            : base($"RuntimeError (Line {lineNumber}): {message}")
        {
            LineNumber = lineNumber;
        }
    }
    
    /// <summary>
    /// Special control flow exception for break statements
    /// </summary>
    public class BreakException : Exception { }
    
    /// <summary>
    /// Special control flow exception for continue statements
    /// </summary>
    public class ContinueException : Exception { }
    
    /// <summary>
    /// Special control flow exception for return statements
    /// </summary>
    public class ReturnException : Exception
    {
        public object Value { get; private set; }
        
        public ReturnException(object value)
        {
            Value = value;
        }
    }
    
    #endregion
}
