using System.Collections.Generic;

namespace LOOPLanguage
{
    /// <summary>
    /// Runtime representation of a user-defined class
    /// </summary>
    public class UserClass
    {
        public string Name { get; private set; }
        public Dictionary<string, UserFunction> Methods { get; private set; }
        
        public UserClass(string name, Dictionary<string, UserFunction> methods)
        {
            Name = name;
            Methods = methods;
        }
    }
    
    /// <summary>
    /// Runtime representation of a user-defined function
    /// </summary>
    public class UserFunction
    {
        public string Name { get; private set; }
        public List<string> Parameters { get; private set; }
        public List<Stmt> Body { get; private set; }
        public Scope ClosureScope { get; private set; }
        
        public UserFunction(string name, List<string> parameters, List<Stmt> body, Scope closureScope)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
            ClosureScope = closureScope;
        }
    }
    
    /// <summary>
    /// Runtime representation of a class instance
    /// </summary>
    public class ClassInstance
    {
        public UserClass Class { get; private set; }
        public Dictionary<string, object> Fields { get; private set; }
        
        public ClassInstance(UserClass userClass)
        {
            Class = userClass;
            Fields = new Dictionary<string, object>();
        }
        
        public object Get(string name)
        {
            if (Fields.ContainsKey(name))
            {
                return Fields[name];
            }
            
            if (Class.Methods.ContainsKey(name))
            {
                return Class.Methods[name];
            }
            
            throw new RuntimeError($"Undefined property '{name}'");
        }
        
        public void Set(string name, object value)
        {
            Fields[name] = value;
        }
    }
}
