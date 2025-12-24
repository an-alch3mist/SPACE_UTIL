using System;
using System.Collections.Generic;

namespace LOOPLanguage
{
    /// <summary>
    /// Represents a built-in function that can be called from LOOP code
    /// </summary>
    public class BuiltinFunction
    {
        public string Name { get; private set; }
        public Func<List<object>, object> Implementation { get; private set; }
        public bool ReturnsIEnumerator { get; private set; }
        
        /// <summary>
        /// Creates a new built-in function
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="implementation">Function implementation</param>
        /// <param name="returnsIEnumerator">True if function returns IEnumerator (yields)</param>
        public BuiltinFunction(string name, Func<List<object>, object> implementation, bool returnsIEnumerator = false)
        {
            Name = name;
            Implementation = implementation;
            ReturnsIEnumerator = returnsIEnumerator;
        }
        
        public object Call(List<object> arguments)
        {
            return Implementation(arguments);
        }
    }
}
