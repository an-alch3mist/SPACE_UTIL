using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LOOPLanguage
{
    /// <summary>
    /// Main controller for the LOOP language interpreter
    /// Handles script execution, UI, and error reporting
    /// </summary>
    public class LOOPController : MonoBehaviour
    {
        #region Inspector Fields
        
        [Header("UI References")]
        public InputField codeInput;
        public Button runButton;
        public Button stopButton;
        public Button clearButton;
        public Text errorText;
        
        [Header("Demo Scripts")]
        public Dropdown demoScriptsDropdown;
        
        #endregion
        
        #region Fields
        
        private PythonInterpreter interpreter;
        private GameBuiltinMethods gameMethods;
        private Lexer lexer;
        private Parser parser;
        
        private bool isRunning = false;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Start()
        {
            // Initialize components
            gameMethods = new GameBuiltinMethods();
            interpreter = new PythonInterpreter(gameMethods);
            lexer = new Lexer();
            parser = new Parser();
            
            // Setup UI
            if (runButton != null)
                runButton.onClick.AddListener(OnRunClicked);
            
            if (stopButton != null)
                stopButton.onClick.AddListener(OnStopClicked);
            
            if (clearButton != null)
                clearButton.onClick.AddListener(OnClearClicked);
            
            if (demoScriptsDropdown != null)
            {
                SetupDemoScripts();
                demoScriptsDropdown.onValueChanged.AddListener(OnDemoScriptSelected);
            }
            
            UpdateButtonStates();
        }
        
        #endregion
        
        #region UI Callbacks
        
        private void OnRunClicked()
        {
            if (isRunning) return;
            
            string code = codeInput != null ? codeInput.text : "";
            RunCode(code);
        }
        
        private void OnStopClicked()
        {
            StopExecution();
        }
        
        private void OnClearClicked()
        {
            if (ConsoleManager.Instance != null)
            {
                ConsoleManager.Instance.Clear();
            }
            
            ClearError();
        }
        
        private void OnDemoScriptSelected(int index)
        {
            if (codeInput == null || index <= 0) return;
            
            string script = GetDemoScript(index - 1);
            if (script != null)
            {
                codeInput.text = script;
            }
        }
        
        #endregion
        
        #region Execution
        
        /// <summary>
        /// Runs the provided code
        /// </summary>
        public void RunCode(string code)
        {
            if (isRunning)
            {
                DisplayError("Already running a script");
                return;
            }
            
            if (string.IsNullOrEmpty(code))
            {
                DisplayError("No code to run");
                return;
            }
            
            ClearError();
            isRunning = true;
            UpdateButtonStates();
            
            // Reset interpreter
            interpreter.Reset();
            
            try
            {
                // Lexical analysis
                List<Token> tokens = lexer.Tokenize(code);
                
                // Parsing
                List<Stmt> statements = parser.Parse(tokens);
                
                // Execution
                IEnumerator executor = interpreter.Execute(statements);
                CoroutineRunner.Instance.RunSafely(executor, OnExecutionError);
                
                // Schedule completion check
                StartCoroutine(WaitForCompletion(executor));
            }
            catch (LexerError e)
            {
                DisplayError(e.Message);
                isRunning = false;
                UpdateButtonStates();
            }
            catch (ParseError e)
            {
                DisplayError(e.Message);
                isRunning = false;
                UpdateButtonStates();
            }
            catch (Exception e)
            {
                DisplayError($"Unexpected error: {e.Message}");
                isRunning = false;
                UpdateButtonStates();
            }
        }
        
        /// <summary>
        /// Stops the current execution
        /// </summary>
        public void StopExecution()
        {
            if (!isRunning) return;
            
            StopAllCoroutines();
            CoroutineRunner.Instance.StopAll();
            
            interpreter.Reset();
            isRunning = false;
            UpdateButtonStates();
            
            if (ConsoleManager.Instance != null)
            {
                ConsoleManager.Instance.AddOutput("--- Execution stopped ---");
            }
        }
        
        /// <summary>
        /// Waits for script execution to complete
        /// </summary>
        private IEnumerator WaitForCompletion(IEnumerator executor)
        {
            // Wait until executor is finished
            while (isRunning)
            {
                yield return null;
                
                // Check if still running
                if (!isRunning) break;
            }
            
            isRunning = false;
            UpdateButtonStates();
            
            if (ConsoleManager.Instance != null)
            {
                ConsoleManager.Instance.AddOutput("--- Execution complete ---");
            }
        }
        
        /// <summary>
        /// Handles execution errors
        /// </summary>
        private void OnExecutionError(Exception e)
        {
            DisplayError(e.Message);
            isRunning = false;
            UpdateButtonStates();
        }
        
        #endregion
        
        #region Error Handling
        
        private void DisplayError(string message)
        {
            Debug.LogError($"LOOP Error: {message}");
            
            if (errorText != null)
            {
                errorText.text = message;
                errorText.gameObject.SetActive(true);
            }
            
            if (ConsoleManager.Instance != null)
            {
                ConsoleManager.Instance.AddOutput($"ERROR: {message}");
            }
        }
        
        private void ClearError()
        {
            if (errorText != null)
            {
                errorText.text = "";
                errorText.gameObject.SetActive(false);
            }
        }
        
        #endregion
        
        #region Demo Scripts
        
        private void SetupDemoScripts()
        {
            if (demoScriptsDropdown == null) return;
            
            demoScriptsDropdown.ClearOptions();
            
            List<string> options = new List<string>
            {
                "-- Select Demo Script --",
                "Lambda: List Comprehension",
                "Lambda: Sorted with Tuple",
                "Lambda: Multiple Conditions",
                "Lambda: Complex IIFE",
                "Lambda: Sort Dictionary",
                "Lambda: Nested Indexing",
                "Lambda: Multi-Parameter",
                "Lambda: Descending Sort",
                "Tuple: Basic",
                "Tuple: In List",
                "Tuple: Single Element",
                "Enum: Basic",
                "Enum: Function Calls",
                "Constants: Movement",
                "Operator: Exponentiation",
                "List: Negative Index",
                "List: Slicing",
                "List: Comprehension",
                "Budget: 100 Iterations",
                "Budget: 1000 Iterations",
                "Game: Basic Functions",
                "Game: Grid Processing",
                "Integration: Complex 1",
                "Integration: Complex 2",
                "Integration: Complex 3",
                "Full Game Scenario"
            };
            
            demoScriptsDropdown.AddOptions(options);
        }
        
        private string GetDemoScript(int index)
        {
            switch (index)
            {
                case 0: return DemoScripts.LAMBDA_WITH_LIST_COMP;
                case 1: return DemoScripts.SORTED_WITH_LAMBDA_TUPLE;
                case 2: return DemoScripts.LAMBDA_MULTIPLE_CONDITIONS;
                case 3: return DemoScripts.COMPLEX_IIFE;
                case 4: return DemoScripts.LAMBDA_SORT_DICT;
                case 5: return DemoScripts.LAMBDA_NESTED_INDEXING;
                case 6: return DemoScripts.LAMBDA_MULTI_PARAM;
                case 7: return DemoScripts.LAMBDA_DESCENDING_SORT;
                case 8: return DemoScripts.TUPLE_BASIC;
                case 9: return DemoScripts.TUPLE_IN_LIST;
                case 10: return DemoScripts.TUPLE_SINGLE_ELEMENT;
                case 11: return DemoScripts.ENUM_BASIC;
                case 12: return DemoScripts.ENUM_FUNCTION_CALLS;
                case 13: return DemoScripts.CONSTANTS_MOVEMENT;
                case 14: return DemoScripts.EXPONENTIATION_PRECEDENCE;
                case 15: return DemoScripts.LIST_NEGATIVE_INDEX;
                case 16: return DemoScripts.LIST_SLICING;
                case 17: return DemoScripts.LIST_COMPREHENSION;
                case 18: return DemoScripts.INSTRUCTION_BUDGET_TEST;
                case 19: return DemoScripts.LARGE_LOOP_TEST;
                case 20: return DemoScripts.GAME_FUNCTIONS_BASIC;
                case 21: return DemoScripts.GAME_FUNCTIONS_GRID;
                case 22: return DemoScripts.COMPLEX_INTEGRATION_1;
                case 23: return DemoScripts.COMPLEX_INTEGRATION_2;
                case 24: return DemoScripts.COMPLEX_INTEGRATION_3;
                case 25: return DemoScripts.FULL_GAME_SCENARIO;
                default: return null;
            }
        }
        
        #endregion
        
        #region UI State
        
        private void UpdateButtonStates()
        {
            if (runButton != null)
                runButton.interactable = !isRunning;
            
            if (stopButton != null)
                stopButton.interactable = isRunning;
        }
        
        #endregion
    }
}
