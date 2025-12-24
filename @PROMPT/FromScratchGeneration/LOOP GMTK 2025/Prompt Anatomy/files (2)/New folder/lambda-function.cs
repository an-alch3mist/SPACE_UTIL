using System.Collections.Generic;

namespace LOOPLanguage
{
    /// <summary>
    /// Runtime representation of a lambda expression
    /// Supports closures by capturing the scope at definition time
    /// </summary>
    public class LambdaFunction
    {
        public List<string> Parameters { get; private set; }
        public Expr Body { get; private set; }
        public Scope ClosureScope { get; private set; }
        
        /// <summary>
        /// Creates a new lambda function
        /// </summary>
        /// <param name="parameters">Parameter names</param>
        /// <param name="body">Expression body</param>
        /// <param name="closureScope">Captured scope (for closures)</param>
        public LambdaFunction(List<string> parameters, Expr body, Scope closureScope)
        {
            Parameters = parameters;
            Body = body;
            ClosureScope = closureScope;
        }
        
        /// <summary>
        /// Calls the lambda with given arguments
        /// </summary>
        public object Call(PythonInterpreter interpreter, List<object> arguments)
        {
            // Validate argument count
            if (arguments.Count != Parameters.Count)
            {
                throw new RuntimeError(
                    $"Lambda expects {Parameters.Count} argument(s), got {arguments.Count}"
                );
            }
            
            // Create new local scope with closure as parent
            Scope lambdaScope = new Scope(ClosureScope);
            
            // Bind parameters to arguments
            for (int i = 0; i < Parameters.Count; i++)
            {
                lambdaScope.Define(Parameters[i], arguments[i]);
            }
            
            // Evaluate body expression in this scope
            Scope previousScope = interpreter.currentScope;
            interpreter.currentScope = lambdaScope;
            
            try
            {
                object result = interpreter.Evaluate(Body);
                return result;
            }
            finally
            {
                interpreter.currentScope = previousScope;
            }
        }
    }
}
