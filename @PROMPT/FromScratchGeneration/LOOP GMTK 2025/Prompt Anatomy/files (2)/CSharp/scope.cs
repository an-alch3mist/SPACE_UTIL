using System.Collections.Generic;

namespace LOOPLanguage
{
    /// <summary>
    /// Manages variable scopes in the interpreter
    /// Supports nested scopes with parent chain lookup
    /// </summary>
    public class Scope
    {
        #region Fields
        
        private Dictionary<string, object> variables = new Dictionary<string, object>();
        private Scope parent;
        
        #endregion
        
        #region Constructors
        
        /// <summary>
        /// Creates a new scope with optional parent
        /// </summary>
        public Scope(Scope parentScope = null)
        {
            parent = parentScope;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Defines a variable in the current scope
        /// </summary>
        public void Define(string name, object value)
        {
            variables[name] = value;
        }
        
        /// <summary>
        /// Gets a variable value, searching up the scope chain
        /// </summary>
        public object Get(string name)
        {
            if (variables.ContainsKey(name))
            {
                return variables[name];
            }
            
            if (parent != null)
            {
                return parent.Get(name);
            }
            
            throw new RuntimeError($"Undefined variable '{name}'");
        }
        
        /// <summary>
        /// Sets a variable value, searching up the scope chain
        /// </summary>
        public void Set(string name, object value)
        {
            if (variables.ContainsKey(name))
            {
                variables[name] = value;
                return;
            }
            
            if (parent != null)
            {
                parent.Set(name, value);
                return;
            }
            
            // If not found anywhere, define it in current scope
            variables[name] = value;
        }
        
        /// <summary>
        /// Sets a variable in the global scope
        /// </summary>
        public void SetGlobal(string name, object value)
        {
            Scope globalScope = this;
            while (globalScope.parent != null)
            {
                globalScope = globalScope.parent;
            }
            globalScope.variables[name] = value;
        }
        
        /// <summary>
        /// Gets a variable from the global scope
        /// </summary>
        public object GetGlobal(string name)
        {
            Scope globalScope = this;
            while (globalScope.parent != null)
            {
                globalScope = globalScope.parent;
            }
            
            if (globalScope.variables.ContainsKey(name))
            {
                return globalScope.variables[name];
            }
            
            throw new RuntimeError($"Undefined global variable '{name}'");
        }
        
        /// <summary>
        /// Checks if a variable exists in the scope chain
        /// </summary>
        public bool Contains(string name)
        {
            if (variables.ContainsKey(name))
            {
                return true;
            }
            
            if (parent != null)
            {
                return parent.Contains(name);
            }
            
            return false;
        }
        
        /// <summary>
        /// Clears all variables in this scope
        /// </summary>
        public void Clear()
        {
            variables.Clear();
        }
        
        /// <summary>
        /// Gets the parent scope
        /// </summary>
        public Scope GetParent()
        {
            return parent;
        }
        
        #endregion
    }
}
