using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LOOPLanguage
{
    /// <summary>
    /// Main interpreter for executing LOOP language code
    /// Implements instruction budget system for smooth frame rates
    /// </summary>
    public class PythonInterpreter
    {
        #region Fields
        
        // Instruction budget system
        private const int INSTRUCTIONS_PER_FRAME = 100;
        private int instructionCount = 0;
        
        // Scope management
        public Scope currentScope;
        private Scope globalScope;
        
        // Line tracking for errors
        private int currentLineNumber = 1;
        
        // Global variables tracking
        private HashSet<string> globalVariables = new HashSet<string>();
        
        // Game reference
        private GameBuiltinMethods gameMethods;
        
        #endregion
        
        #region Initialization
        
        public PythonInterpreter(GameBuiltinMethods game)
        {
            gameMethods = game;
            Reset();
        }
        
        /// <summary>
        /// Resets the interpreter state
        /// </summary>
        public void Reset()
        {
            globalScope = new Scope();
            currentScope = globalScope;
            instructionCount = 0;
            globalVariables.Clear();
            
            RegisterBuiltins();
            RegisterEnums();
            RegisterConstants();
        }
        
        /// <summary>
        /// Registers all built-in functions
        /// </summary>
        private void RegisterBuiltins()
        {
            // Standard built-ins
            globalScope.Define("print", new BuiltinFunction("print", Print));
            globalScope.Define("sleep", new BuiltinFunction("sleep", Sleep, true));
            globalScope.Define("range", new BuiltinFunction("range", Range));
            globalScope.Define("len", new BuiltinFunction("len", Len));
            globalScope.Define("str", new BuiltinFunction("str", Str));
            globalScope.Define("int", new BuiltinFunction("int", Int));
            globalScope.Define("float", new BuiltinFunction("float", Float));
            globalScope.Define("abs", new BuiltinFunction("abs", Abs));
            globalScope.Define("min", new BuiltinFunction("min", Min));
            globalScope.Define("max", new BuiltinFunction("max", Max));
            globalScope.Define("sum", new BuiltinFunction("sum", Sum));
            globalScope.Define("sorted", new BuiltinFunction("sorted", Sorted));
            
            // Game built-ins (all return IEnumerators or instant)
            if (gameMethods != null)
            {
                // Movement
                globalScope.Define("move", new BuiltinFunction("move", args => gameMethods.Move(args), true));
                
                // Farming
                globalScope.Define("harvest", new BuiltinFunction("harvest", args => gameMethods.Harvest(), true));
                globalScope.Define("plant", new BuiltinFunction("plant", args => gameMethods.Plant(args), true));
                globalScope.Define("till", new BuiltinFunction("till", args => gameMethods.Till(), true));
                
                // Queries (instant - no yield)
                globalScope.Define("can_harvest", new BuiltinFunction("can_harvest", args => gameMethods.CanHarvest()));
                globalScope.Define("get_ground_type", new BuiltinFunction("get_ground_type", args => gameMethods.GetGroundType()));
                globalScope.Define("get_entity_type", new BuiltinFunction("get_entity_type", args => gameMethods.GetEntityType()));
                globalScope.Define("get_pos_x", new BuiltinFunction("get_pos_x", args => gameMethods.GetPosX()));
                globalScope.Define("get_pos_y", new BuiltinFunction("get_pos_y", args => gameMethods.GetPosY()));
                globalScope.Define("get_world_size", new BuiltinFunction("get_world_size", args => gameMethods.GetWorldSize()));
                globalScope.Define("get_water", new BuiltinFunction("get_water", args => gameMethods.GetWater()));
                
                // Inventory
                globalScope.Define("num_items", new BuiltinFunction("num_items", args => gameMethods.NumItems(args)));
                globalScope.Define("use_item", new BuiltinFunction("use_item", args => gameMethods.UseItem(args), true));
                
                // Utility
                globalScope.Define("do_a_flip", new BuiltinFunction("do_a_flip", args => gameMethods.DoAFlip(), true));
                globalScope.Define("is_even", new BuiltinFunction("is_even", args => gameMethods.IsEven(args)));
                globalScope.Define("is_odd", new BuiltinFunction("is_odd", args => gameMethods.IsOdd(args)));
            }
        }
        
        /// <summary>
        /// Registers enum types in global scope
        /// </summary>
        private void RegisterEnums()
        {
            // Create enum objects as dictionaries
            Dictionary<string, object> grounds = new Dictionary<string, object>
            {
                { "Soil", Grounds.Soil },
                { "Turf", Grounds.Turf },
                { "Grassland", Grounds.Grassland }
            };
            globalScope.Define("Grounds", grounds);
            
            Dictionary<string, object> items = new Dictionary<string, object>
            {
                { "Hay", Items.Hay },
                { "Wood", Items.Wood },
                { "Carrot", Items.Carrot },
                { "Pumpkin", Items.Pumpkin },
                { "Power", Items.Power },
                { "Sunflower", Items.Sunflower },
                { "Water", Items.Water }
            };
            globalScope.Define("Items", items);
            
            Dictionary<string, object> entities = new Dictionary<string, object>
            {
                { "Grass", Entities.Grass },
                { "Bush", Entities.Bush },
                { "Tree", Entities.Tree },
                { "Carrot", Entities.Carrot },
                { "Pumpkin", Entities.Pumpkin },
                { "Sunflower", Entities.Sunflower }
            };
            globalScope.Define("Entities", entities);
        }
        
        /// <summary>
        /// Registers built-in constants
        /// </summary>
        private void RegisterConstants()
        {
            // Direction constants
            globalScope.Define("North", "up");
            globalScope.Define("South", "down");
            globalScope.Define("East", "right");
            globalScope.Define("West", "left");
        }
        
        #endregion
        
        #region Main Execution
        
        /// <summary>
        /// Executes a program as a coroutine
        /// </summary>
        public IEnumerator Execute(List<Stmt> statements)
        {
            instructionCount = 0;
            
            IEnumerator executor = ExecuteStatements(statements);
            while (executor.MoveNext())
            {
                yield return executor.Current;
            }
        }
        
        /// <summary>
        /// Executes a list of statements
        /// </summary>
        private IEnumerator ExecuteStatements(List<Stmt> statements)
        {
            foreach (Stmt stmt in statements)
            {
                // Check instruction budget
                if (instructionCount >= INSTRUCTIONS_PER_FRAME)
                {
                    yield return null;
                    instructionCount = 0;
                }
                
                // Execute statement
                object result = null;
                Exception caughtException = null;
                
                try
                {
                    currentLineNumber = stmt.LineNumber;
                    instructionCount++;
                    result = ExecuteStatement(stmt);
                }
                catch (BreakException)
                {
                    throw; // Propagate control flow exceptions
                }
                catch (ContinueException)
                {
                    throw;
                }
                catch (ReturnException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    caughtException = e;
                }
                
                // Handle IEnumerator result (yield)
                if (result is IEnumerator enumerator && caughtException == null)
                {
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                }
                
                // Re-throw exception after yielding (if any)
                if (caughtException != null)
                {
                    if (caughtException is RuntimeError)
                    {
                        throw caughtException;
                    }
                    else
                    {
                        throw new RuntimeError(caughtException.Message, currentLineNumber);
                    }
                }
            }
        }
        
        /// <summary>
        /// Executes a single statement
        /// </summary>
        private object ExecuteStatement(Stmt stmt)
        {
            if (stmt is ExpressionStmt exprStmt)
            {
                return Evaluate(exprStmt.Expression);
            }
            else if (stmt is AssignmentStmt assignStmt)
            {
                return ExecuteAssignment(assignStmt);
            }
            else if (stmt is IfStmt ifStmt)
            {
                return ExecuteIf(ifStmt);
            }
            else if (stmt is WhileStmt whileStmt)
            {
                return ExecuteWhile(whileStmt);
            }
            else if (stmt is ForStmt forStmt)
            {
                return ExecuteFor(forStmt);
            }
            else if (stmt is FunctionDefStmt funcDef)
            {
                return ExecuteFunctionDef(funcDef);
            }
            else if (stmt is ClassDefStmt classDef)
            {
                return ExecuteClassDef(classDef);
            }
            else if (stmt is ReturnStmt returnStmt)
            {
                object value = returnStmt.Value != null ? Evaluate(returnStmt.Value) : null;
                throw new ReturnException(value);
            }
            else if (stmt is BreakStmt)
            {
                throw new BreakException();
            }
            else if (stmt is ContinueStmt)
            {
                throw new ContinueException();
            }
            else if (stmt is PassStmt)
            {
                return null;
            }
            else if (stmt is GlobalStmt globalStmt)
            {
                foreach (string varName in globalStmt.Variables)
                {
                    globalVariables.Add(varName);
                }
                return null;
            }
            else if (stmt is ImportStmt importStmt)
            {
                return ExecuteImport(importStmt);
            }
            
            throw new RuntimeError($"Unknown statement type: {stmt.GetType().Name}", currentLineNumber);
        }
        
        #endregion
        
        #region Statement Execution
        
        private object ExecuteAssignment(AssignmentStmt stmt)
        {
            object value = Evaluate(stmt.Value);
            
            // Handle compound assignments
            if (stmt.Operator != "=")
            {
                object currentValue = currentScope.Get(stmt.Target);
                
                switch (stmt.Operator)
                {
                    case "+=":
                        value = AddValues(currentValue, value);
                        break;
                    case "-=":
                        value = ToNumber(currentValue) - ToNumber(value);
                        break;
                    case "*=":
                        value = ToNumber(currentValue) * ToNumber(value);
                        break;
                    case "/=":
                        double divisor = ToNumber(value);
                        if (divisor == 0) throw new RuntimeError("Division by zero", currentLineNumber);
                        value = (int)(ToNumber(currentValue) / divisor);
                        break;
                }
            }
            
            // Check if global
            if (globalVariables.Contains(stmt.Target))
            {
                currentScope.SetGlobal(stmt.Target, value);
            }
            else
            {
                currentScope.Set(stmt.Target, value);
            }
            
            return null;
        }
        
        private IEnumerator ExecuteIf(IfStmt stmt)
        {
            instructionCount++;
            bool condition = IsTruthy(Evaluate(stmt.Condition));
            
            if (condition)
            {
                IEnumerator executor = ExecuteStatements(stmt.ThenBranch);
                while (executor.MoveNext())
                {
                    yield return executor.Current;
                }
            }
            else if (stmt.ElseBranch != null)
            {
                IEnumerator executor = ExecuteStatements(stmt.ElseBranch);
                while (executor.MoveNext())
                {
                    yield return executor.Current;
                }
            }
        }
        
        private IEnumerator ExecuteWhile(WhileStmt stmt)
        {
            while (true)
            {
                instructionCount++;
                
                // Check budget
                if (instructionCount >= INSTRUCTIONS_PER_FRAME)
                {
                    yield return null;
                    instructionCount = 0;
                }
                
                bool condition = IsTruthy(Evaluate(stmt.Condition));
                if (!condition) break;
                
                IEnumerator bodyExecutor = null;
                Exception caughtException = null;
                
                try
                {
                    bodyExecutor = ExecuteStatements(stmt.Body);
                }
                catch (BreakException)
                {
                    break;
                }
                catch (ContinueException)
                {
                    continue;
                }
                catch (Exception e)
                {
                    caughtException = e;
                }
                
                if (bodyExecutor != null && caughtException == null)
                {
                    bool shouldBreak = false;
                    bool shouldContinue = false;
                    
                    while (true)
                    {
                        bool hasMore = false;
                        try
                        {
                            hasMore = bodyExecutor.MoveNext();
                        }
                        catch (BreakException)
                        {
                            shouldBreak = true;
                            break;
                        }
                        catch (ContinueException)
                        {
                            shouldContinue = true;
                            break;
                        }
                        
                        if (!hasMore) break;
                        yield return bodyExecutor.Current;
                    }
                    
                    if (shouldBreak) break;
                    if (shouldContinue) continue;
                }
                
                if (caughtException != null)
                {
                    throw caughtException;
                }
            }
        }
        
        private IEnumerator ExecuteFor(ForStmt stmt)
        {
            object iterableValue = Evaluate(stmt.Iterable);
            List<object> items = ToList(iterableValue);
            
            foreach (object item in items)
            {
                instructionCount++;
                
                // Check budget
                if (instructionCount >= INSTRUCTIONS_PER_FRAME)
                {
                    yield return null;
                    instructionCount = 0;
                }
                
                currentScope.Set(stmt.Variable, item);
                
                IEnumerator bodyExecutor = null;
                Exception caughtException = null;
                
                try
                {
                    bodyExecutor = ExecuteStatements(stmt.Body);
                }
                catch (BreakException)
                {
                    break;
                }
                catch (ContinueException)
                {
                    continue;
                }
                catch (Exception e)
                {
                    caughtException = e;
                }
                
                if (bodyExecutor != null && caughtException == null)
                {
                    bool shouldBreak = false;
                    bool shouldContinue = false;
                    
                    while (true)
                    {
                        bool hasMore = false;
                        try
                        {
                            hasMore = bodyExecutor.MoveNext();
                        }
                        catch (BreakException)
                        {
                            shouldBreak = true;
                            break;
                        }
                        catch (ContinueException)
                        {
                            shouldContinue = true;
                            break;
                        }
                        
                        if (!hasMore) break;
                        yield return bodyExecutor.Current;
                    }
                    
                    if (shouldBreak) break;
                    if (shouldContinue) continue;
                }
                
                if (caughtException != null)
                {
                    throw caughtException;
                }
            }
        }
        
        private object ExecuteFunctionDef(FunctionDefStmt stmt)
        {
            UserFunction func = new UserFunction(stmt.Name, stmt.Parameters, stmt.Body, currentScope);
            currentScope.Set(stmt.Name, func);
            return null;
        }
        
        private object ExecuteClassDef(ClassDefStmt stmt)
        {
            Dictionary<string, UserFunction> methods = new Dictionary<string, UserFunction>();
            
            foreach (FunctionDefStmt method in stmt.Methods)
            {
                UserFunction func = new UserFunction(method.Name, method.Parameters, method.Body, currentScope);
                methods[method.Name] = func;
            }
            
            UserClass userClass = new UserClass(stmt.Name, methods);
            currentScope.Set(stmt.Name, userClass);
            return null;
        }
        
        private object ExecuteImport(ImportStmt stmt)
        {
            // Import is only for enums - it's a no-op as enums are already registered
            return null;
        }
        
        #endregion
        
        #region Expression Evaluation
        
        /// <summary>
        /// Evaluates an expression and returns its value
        /// </summary>
        public object Evaluate(Expr expr)
        {
            instructionCount++;
            currentLineNumber = expr.LineNumber;
            
            if (expr is LiteralExpr literalExpr)
            {
                return literalExpr.Value;
            }
            else if (expr is VariableExpr varExpr)
            {
                return currentScope.Get(varExpr.Name);
            }
            else if (expr is BinaryExpr binaryExpr)
            {
                return EvaluateBinary(binaryExpr);
            }
            else if (expr is UnaryExpr unaryExpr)
            {
                return EvaluateUnary(unaryExpr);
            }
            else if (expr is CallExpr callExpr)
            {
                return EvaluateCall(callExpr);
            }
            else if (expr is IndexExpr indexExpr)
            {
                return EvaluateIndex(indexExpr);
            }
            else if (expr is SliceExpr sliceExpr)
            {
                return EvaluateSlice(sliceExpr);
            }
            else if (expr is ListExpr listExpr)
            {
                return EvaluateList(listExpr);
            }
            else if (expr is TupleExpr tupleExpr)
            {
                return EvaluateTuple(tupleExpr);
            }
            else if (expr is DictExpr dictExpr)
            {
                return EvaluateDict(dictExpr);
            }
            else if (expr is LambdaExpr lambdaExpr)
            {
                return EvaluateLambda(lambdaExpr);
            }
            else if (expr is ListCompExpr listCompExpr)
            {
                return EvaluateListComp(listCompExpr);
            }
            else if (expr is MemberAccessExpr memberExpr)
            {
                return EvaluateMemberAccess(memberExpr);
            }
            
            throw new RuntimeError($"Unknown expression type: {expr.GetType().Name}", currentLineNumber);
        }
        
        private object EvaluateBinary(BinaryExpr expr)
        {
            // Short-circuit evaluation for AND and OR
            if (expr.Operator == TokenType.AND)
            {
                object left = Evaluate(expr.Left);
                if (!IsTruthy(left)) return left;
                return Evaluate(expr.Right);
            }
            else if (expr.Operator == TokenType.OR)
            {
                object left = Evaluate(expr.Left);
                if (IsTruthy(left)) return left;
                return Evaluate(expr.Right);
            }
            
            // Evaluate both operands
            object leftVal = Evaluate(expr.Left);
            object rightVal = Evaluate(expr.Right);
            
            switch (expr.Operator)
            {
                // Arithmetic
                case TokenType.PLUS:
                    return AddValues(leftVal, rightVal);
                case TokenType.MINUS:
                    return ToNumber(leftVal) - ToNumber(rightVal);
                case TokenType.STAR:
                    return ToNumber(leftVal) * ToNumber(rightVal);
                case TokenType.SLASH:
                    double divisor = ToNumber(rightVal);
                    if (divisor == 0) throw new RuntimeError("Division by zero", currentLineNumber);
                    return (int)(ToNumber(leftVal) / divisor);
                case TokenType.PERCENT:
                    return ToNumber(leftVal) % ToNumber(rightVal);
                case TokenType.DOUBLE_STAR:
                    return Math.Pow(ToNumber(leftVal), ToNumber(rightVal));
                
                // Comparison
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(leftVal, rightVal);
                case TokenType.BANG_EQUAL:
                    return !IsEqual(leftVal, rightVal);
                case TokenType.LESS:
                    return ToNumber(leftVal) < ToNumber(rightVal);
                case TokenType.GREATER:
                    return ToNumber(leftVal) > ToNumber(rightVal);
                case TokenType.LESS_EQUAL:
                    return ToNumber(leftVal) <= ToNumber(rightVal);
                case TokenType.GREATER_EQUAL:
                    return ToNumber(leftVal) >= ToNumber(rightVal);
                
                // Membership
                case TokenType.IN:
                    return IsInCollection(leftVal, rightVal);
                
                // Identity
                case TokenType.IS:
                    return ReferenceEquals(leftVal, rightVal);
                
                // Bitwise
                case TokenType.AMPERSAND:
                    return (int)ToNumber(leftVal) & (int)ToNumber(rightVal);
                case TokenType.PIPE:
                    return (int)ToNumber(leftVal) | (int)ToNumber(rightVal);
                case TokenType.CARET:
                    return (int)ToNumber(leftVal) ^ (int)ToNumber(rightVal);
                case TokenType.LEFT_SHIFT:
                    return (int)ToNumber(leftVal) << (int)ToNumber(rightVal);
                case TokenType.RIGHT_SHIFT:
                    return (int)ToNumber(leftVal) >> (int)ToNumber(rightVal);
            }
            
            throw new RuntimeError($"Unknown binary operator: {expr.Operator}", currentLineNumber);
        }
        
        private object EvaluateUnary(UnaryExpr expr)
        {
            object operand = Evaluate(expr.Operand);
            
            switch (expr.Operator)
            {
                case TokenType.MINUS:
                    return -ToNumber(operand);
                case TokenType.PLUS:
                    return ToNumber(operand);
                case TokenType.NOT:
                    return !IsTruthy(operand);
                case TokenType.TILDE:
                    return ~(int)ToNumber(operand);
            }
            
            throw new RuntimeError($"Unknown unary operator: {expr.Operator}", currentLineNumber);
        }
        
        private object EvaluateCall(CallExpr expr)
        {
            object callee = Evaluate(expr.Callee);
            List<object> arguments = new List<object>();
            
            foreach (Expr arg in expr.Arguments)
            {
                arguments.Add(Evaluate(arg));
            }
            
            // Built-in function
            if (callee is BuiltinFunction builtinFunc)
            {
                return builtinFunc.Call(arguments);
            }
            
            // Lambda function
            if (callee is LambdaFunction lambda)
            {
                return lambda.Call(this, arguments);
            }
            
            // User-defined function
            if (callee is UserFunction userFunc)
            {
                return CallUserFunction(userFunc, arguments);
            }
            
            // Class constructor
            if (callee is UserClass userClass)
            {
                return CallClassConstructor(userClass, arguments);
            }
            
            throw new RuntimeError("Can only call functions and classes", currentLineNumber);
        }
        
        private object CallUserFunction(UserFunction func, List<object> arguments)
        {
            if (arguments.Count != func.Parameters.Count)
            {
                throw new RuntimeError(
                    $"Function '{func.Name}' expects {func.Parameters.Count} argument(s), got {arguments.Count}",
                    currentLineNumber
                );
            }
            
            // Create new scope
            Scope functionScope = new Scope(func.ClosureScope);
            
            // Bind parameters
            for (int i = 0; i < func.Parameters.Count; i++)
            {
                functionScope.Define(func.Parameters[i], arguments[i]);
            }
            
            // Execute function body
            Scope previousScope = currentScope;
            currentScope = functionScope;
            
            object returnValue = null;
            
            try
            {
                IEnumerator executor = ExecuteStatements(func.Body);
                while (executor.MoveNext())
                {
                    // Function calls return IEnumerator
                    if (executor.Current != null)
                    {
                        return executor;
                    }
                }
            }
            catch (ReturnException returnEx)
            {
                returnValue = returnEx.Value;
            }
            finally
            {
                currentScope = previousScope;
            }
            
            return returnValue;
        }
        
        private object CallClassConstructor(UserClass userClass, List<object> arguments)
        {
            ClassInstance instance = new ClassInstance(userClass);
            
            // Call __init__ if it exists
            if (userClass.Methods.ContainsKey("__init__"))
            {
                UserFunction initMethod = userClass.Methods["__init__"];
                
                // Create scope with 'self'
                Scope methodScope = new Scope(initMethod.ClosureScope);
                methodScope.Define("self", instance);
                
                // Bind remaining parameters
                if (arguments.Count + 1 != initMethod.Parameters.Count)
                {
                    throw new RuntimeError(
                        $"__init__ expects {initMethod.Parameters.Count - 1} argument(s), got {arguments.Count}",
                        currentLineNumber
                    );
                }
                
                for (int i = 1; i < initMethod.Parameters.Count; i++)
                {
                    methodScope.Define(initMethod.Parameters[i], arguments[i - 1]);
                }
                
                // Execute __init__
                Scope previousScope = currentScope;
                currentScope = methodScope;
                
                try
                {
                    IEnumerator executor = ExecuteStatements(initMethod.Body);
                    while (executor.MoveNext()) { }
                }
                catch (ReturnException)
                {
                    // __init__ should not return a value
                }
                finally
                {
                    currentScope = previousScope;
                }
            }
            
            return instance;
        }
        
        private object EvaluateIndex(IndexExpr expr)
        {
            object obj = Evaluate(expr.Object);
            object index = Evaluate(expr.Index);
            
            if (obj is List<object> list)
            {
                int idx = (int)ToNumber(index);
                
                // Handle negative indices
                if (idx < 0) idx = list.Count + idx;
                
                if (idx < 0 || idx >= list.Count)
                {
                    throw new RuntimeError("List index out of range", currentLineNumber);
                }
                
                return list[idx];
            }
            else if (obj is Dictionary<object, object> dict)
            {
                if (!dict.ContainsKey(index))
                {
                    throw new RuntimeError($"Key '{index}' not found in dictionary", currentLineNumber);
                }
                return dict[index];
            }
            else if (obj is string str)
            {
                int idx = (int)ToNumber(index);
                
                // Handle negative indices
                if (idx < 0) idx = str.Length + idx;
                
                if (idx < 0 || idx >= str.Length)
                {
                    throw new RuntimeError("String index out of range", currentLineNumber);
                }
                
                return str[idx].ToString();
            }
            
            throw new RuntimeError("Object does not support indexing", currentLineNumber);
        }
        
        private object EvaluateSlice(SliceExpr expr)
        {
            object obj = Evaluate(expr.Object);
            
            if (obj is List<object> list)
            {
                int start = 0;
                int stop = list.Count;
                int step = 1;
                
                if (expr.Start != null) start = (int)ToNumber(Evaluate(expr.Start));
                if (expr.Stop != null) stop = (int)ToNumber(Evaluate(expr.Stop));
                if (expr.Step != null) step = (int)ToNumber(Evaluate(expr.Step));
                
                // Handle negative indices
                if (start < 0) start = list.Count + start;
                if (stop < 0) stop = list.Count + stop;
                
                // Clamp to valid range
                start = Math.Max(0, Math.Min(start, list.Count));
                stop = Math.Max(0, Math.Min(stop, list.Count));
                
                List<object> result = new List<object>();
                if (step > 0)
                {
                    for (int i = start; i < stop; i += step)
                    {
                        result.Add(list[i]);
                    }
                }
                
                return result;
            }
            else if (obj is string str)
            {
                int start = 0;
                int stop = str.Length;
                int step = 1;
                
                if (expr.Start != null) start = (int)ToNumber(Evaluate(expr.Start));
                if (expr.Stop != null) stop = (int)ToNumber(Evaluate(expr.Stop));
                if (expr.Step != null) step = (int)ToNumber(Evaluate(expr.Step));
                
                // Handle negative indices
                if (start < 0) start = str.Length + start;
                if (stop < 0) stop = str.Length + stop;
                
                // Clamp to valid range
                start = Math.Max(0, Math.Min(start, str.Length));
                stop = Math.Max(0, Math.Min(stop, str.Length));
                
                string result = "";
                if (step > 0)
                {
                    for (int i = start; i < stop; i += step)
                    {
                        result += str[i];
                    }
                }
                
                return result;
            }
            
            throw new RuntimeError("Object does not support slicing", currentLineNumber);
        }
        
        private object EvaluateList(ListExpr expr)
        {
            List<object> result = new List<object>();
            foreach (Expr element in expr.Elements)
            {
                result.Add(Evaluate(element));
            }
            return result;
        }
        
        private object EvaluateTuple(TupleExpr expr)
        {
            List<object> result = new List<object>();
            foreach (Expr element in expr.Elements)
            {
                result.Add(Evaluate(element));
            }
            // Tuples are represented as lists but marked as immutable
            return result;
        }
        
        private object EvaluateDict(DictExpr expr)
        {
            Dictionary<object, object> result = new Dictionary<object, object>();
            
            for (int i = 0; i < expr.Keys.Count; i++)
            {
                object key = Evaluate(expr.Keys[i]);
                object value = Evaluate(expr.Values[i]);
                result[key] = value;
            }
            
            return result;
        }
        
        private object EvaluateLambda(LambdaExpr expr)
        {
            return new LambdaFunction(expr.Parameters, expr.Body, currentScope);
        }
        
        private object EvaluateListComp(ListCompExpr expr)
        {
            List<object> result = new List<object>();
            object iterableValue = Evaluate(expr.Iterable);
            List<object> items = ToList(iterableValue);
            
            foreach (object item in items)
            {
                instructionCount++;
                
                // Create new scope for iteration variable
                Scope compScope = new Scope(currentScope);
                compScope.Define(expr.Variable, item);
                
                Scope previousScope = currentScope;
                currentScope = compScope;
                
                try
                {
                    // Check condition
                    bool shouldInclude = true;
                    if (expr.Condition != null)
                    {
                        shouldInclude = IsTruthy(Evaluate(expr.Condition));
                    }
                    
                    if (shouldInclude)
                    {
                        result.Add(Evaluate(expr.Element));
                    }
                }
                finally
                {
                    currentScope = previousScope;
                }
            }
            
            return result;
        }
        
        private object EvaluateMemberAccess(MemberAccessExpr expr)
        {
            object obj = Evaluate(expr.Object);
            
            // Enum access
            if (obj is Dictionary<string, object> dict)
            {
                if (dict.ContainsKey(expr.Member))
                {
                    return dict[expr.Member];
                }
                throw new RuntimeError($"Enum does not have member '{expr.Member}'", currentLineNumber);
            }
            
            // Class instance
            if (obj is ClassInstance instance)
            {
                return instance.Get(expr.Member);
            }
            
            throw new RuntimeError($"Object does not support member access", currentLineNumber);
        }
        
        #endregion
        
        #region Built-in Functions
        
        private object Print(List<object> args)
        {
            string output = string.Join(" ", args.Select(arg => ToString(arg)));
            Debug.Log(output);
            if (ConsoleManager.Instance != null)
            {
                ConsoleManager.Instance.AddOutput(output);
            }
            return null;
        }
        
        private object Sleep(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeError("sleep() takes exactly 1 argument", currentLineNumber);
            }
            
            float seconds = (float)ToNumber(args[0]);
            return new WaitForSeconds(seconds);
        }
        
        private object Range(List<object> args)
        {
            int start = 0;
            int stop = 0;
            int step = 1;
            
            if (args.Count == 1)
            {
                stop = (int)ToNumber(args[0]);
            }
            else if (args.Count == 2)
            {
                start = (int)ToNumber(args[0]);
                stop = (int)ToNumber(args[1]);
            }
            else if (args.Count == 3)
            {
                start = (int)ToNumber(args[0]);
                stop = (int)ToNumber(args[1]);
                step = (int)ToNumber(args[2]);
            }
            else
            {
                throw new RuntimeError("range() takes 1 to 3 arguments", currentLineNumber);
            }
            
            List<object> result = new List<object>();
            if (step > 0)
            {
                for (int i = start; i < stop; i += step)
                {
                    result.Add((double)i);
                }
            }
            else if (step < 0)
            {
                for (int i = start; i > stop; i += step)
                {
                    result.Add((double)i);
                }
            }
            
            return result;
        }
        
        private object Len(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeError("len() takes exactly 1 argument", currentLineNumber);
            }
            
            object obj = args[0];
            
            if (obj is List<object> list)
                return (double)list.Count;
            else if (obj is string str)
                return (double)str.Length;
            else if (obj is Dictionary<object, object> dict)
                return (double)dict.Count;
            
            throw new RuntimeError("Object has no len()", currentLineNumber);
        }
        
        private object Str(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeError("str() takes exactly 1 argument", currentLineNumber);
            }
            
            return ToString(args[0]);
        }
        
        private object Int(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeError("int() takes exactly 1 argument", currentLineNumber);
            }
            
            return (double)(int)ToNumber(args[0]);
        }
        
        private object Float(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeError("float() takes exactly 1 argument", currentLineNumber);
            }
            
            return ToNumber(args[0]);
        }
        
        private object Abs(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeError("abs() takes exactly 1 argument", currentLineNumber);
            }
            
            return Math.Abs(ToNumber(args[0]));
        }
        
        private object Min(List<object> args)
        {
            if (args.Count == 0)
            {
                throw new RuntimeError("min() requires at least 1 argument", currentLineNumber);
            }
            
            List<object> items;
            if (args.Count == 1 && args[0] is List<object>)
            {
                items = (List<object>)args[0];
            }
            else
            {
                items = args;
            }
            
            if (items.Count == 0)
            {
                throw new RuntimeError("min() arg is an empty sequence", currentLineNumber);
            }
            
            double minVal = ToNumber(items[0]);
            foreach (object item in items)
            {
                double val = ToNumber(item);
                if (val < minVal) minVal = val;
            }
            
            return minVal;
        }
        
        private object Max(List<object> args)
        {
            if (args.Count == 0)
            {
                throw new RuntimeError("max() requires at least 1 argument", currentLineNumber);
            }
            
            List<object> items;
            if (args.Count == 1 && args[0] is List<object>)
            {
                items = (List<object>)args[0];
            }
            else
            {
                items = args;
            }
            
            if (items.Count == 0)
            {
                throw new RuntimeError("max() arg is an empty sequence", currentLineNumber);
            }
            
            double maxVal = ToNumber(items[0]);
            foreach (object item in items)
            {
                double val = ToNumber(item);
                if (val > maxVal) maxVal = val;
            }
            
            return maxVal;
        }
        
        private object Sum(List<object> args)
        {
            if (args.Count != 1)
            {
                throw new RuntimeError("sum() takes exactly 1 argument", currentLineNumber);
            }
            
            List<object> items = ToList(args[0]);
            double total = 0;
            
            foreach (object item in items)
            {
                total += ToNumber(item);
            }
            
            return total;
        }
        
        private object Sorted(List<object> args)
        {
            if (args.Count < 1 || args.Count > 3)
            {
                throw new RuntimeError("sorted() takes 1 to 3 arguments", currentLineNumber);
            }
            
            List<object> items = new List<object>(ToList(args[0]));
            LambdaFunction keyFunc = null;
            bool reverse = false;
            
            // Parse optional arguments (simple implementation - not full kwargs)
            if (args.Count >= 2 && args[1] is LambdaFunction)
            {
                keyFunc = (LambdaFunction)args[1];
            }
            if (args.Count >= 3)
            {
                reverse = IsTruthy(args[2]);
            }
            
            // Sort with key function
            if (keyFunc != null)
            {
                items.Sort((a, b) =>
                {
                    object keyA = keyFunc.Call(this, new List<object> { a });
                    object keyB = keyFunc.Call(this, new List<object> { b });
                    
                    int comparison = CompareValues(keyA, keyB);
                    return reverse ? -comparison : comparison;
                });
            }
            else
            {
                items.Sort((a, b) =>
                {
                    int comparison = CompareValues(a, b);
                    return reverse ? -comparison : comparison;
                });
            }
            
            return items;
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool IsTruthy(object value)
        {
            if (value == null) return false;
            if (value is bool b) return b;
            if (value is double d) return d != 0;
            if (value is string s) return s.Length > 0;
            if (value is List<object> list) return list.Count > 0;
            return true;
        }
        
        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            
            // Numeric comparison
            if (IsNumeric(a) && IsNumeric(b))
            {
                return ToNumber(a) == ToNumber(b);
            }
            
            return a.Equals(b);
        }
        
        private bool IsNumeric(object value)
        {
            return value is double || value is int || value is float;
        }
        
        private double ToNumber(object value)
        {
            if (value == null)
                throw new RuntimeError("Cannot convert None to number", currentLineNumber);
            
            if (value is double d)
                return d;
            if (value is int i)
                return (double)i;
            if (value is float f)
                return (double)f;
            if (value is bool b)
                return b ? 1.0 : 0.0;
            if (value is string s)
            {
                if (double.TryParse(s, out double result))
                    return result;
            }
            
            throw new RuntimeError($"Cannot convert {value.GetType().Name} to number", currentLineNumber);
        }
        
        private string ToString(object value)
        {
            if (value == null) return "None";
            if (value is bool b) return b ? "True" : "False";
            if (value is double d)
            {
                // Check if it's an integer value
                if (d == Math.Floor(d) && !double.IsInfinity(d) && !double.IsNaN(d))
                {
                    return ((int)d).ToString();
                }
                return d.ToString();
            }
            if (value is List<object> list)
            {
                return "[" + string.Join(", ", list.Select(item => ToString(item))) + "]";
            }
            if (value is Dictionary<object, object> dict)
            {
                string[] pairs = dict.Select(kvp => $"{ToString(kvp.Key)}: {ToString(kvp.Value)}").ToArray();
                return "{" + string.Join(", ", pairs) + "}";
            }
            
            return value.ToString();
        }
        
        private List<object> ToList(object value)
        {
            if (value is List<object> list)
                return list;
            
            if (value is string s)
            {
                List<object> chars = new List<object>();
                foreach (char c in s)
                {
                    chars.Add(c.ToString());
                }
                return chars;
            }
            
            if (value is Dictionary<object, object> dict)
            {
                return new List<object>(dict.Keys);
            }
            
            throw new RuntimeError("Object is not iterable", currentLineNumber);
        }
        
        private object AddValues(object a, object b)
        {
            // String concatenation
            if (a is string || b is string)
            {
                return ToString(a) + ToString(b);
            }
            
            // Numeric addition
            return ToNumber(a) + ToNumber(b);
        }
        
        private bool IsInCollection(object item, object collection)
        {
            if (collection is List<object> list)
            {
                foreach (object element in list)
                {
                    if (IsEqual(item, element))
                        return true;
                }
                return false;
            }
            
            if (collection is Dictionary<object, object> dict)
            {
                return dict.ContainsKey(item);
            }
            
            if (collection is string str)
            {
                return str.Contains(ToString(item));
            }
            
            throw new RuntimeError("in operator requires iterable", currentLineNumber);
        }
        
        private int CompareValues(object a, object b)
        {
            // Numeric comparison
            if (IsNumeric(a) && IsNumeric(b))
            {
                double numA = ToNumber(a);
                double numB = ToNumber(b);
                return numA.CompareTo(numB);
            }
            
            // String comparison
            if (a is string strA && b is string strB)
            {
                return strA.CompareTo(strB);
            }
            
            // List comparison (element-wise)
            if (a is List<object> listA && b is List<object> listB)
            {
                int minLen = Math.Min(listA.Count, listB.Count);
                for (int i = 0; i < minLen; i++)
                {
                    int cmp = CompareValues(listA[i], listB[i]);
                    if (cmp != 0) return cmp;
                }
                return listA.Count.CompareTo(listB.Count);
            }
            
            throw new RuntimeError("Cannot compare these types", currentLineNumber);
        }
        
        #endregion
    }
}
