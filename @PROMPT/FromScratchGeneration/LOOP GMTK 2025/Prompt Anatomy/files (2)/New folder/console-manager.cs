using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace LOOPLanguage
{
    /// <summary>
    /// Manages the in-game console for print() output
    /// Displays script output to the player
    /// </summary>
    public class ConsoleManager : MonoBehaviour
    {
        #region Singleton
        
        private static ConsoleManager instance;
        
        public static ConsoleManager Instance
        {
            get { return instance; }
        }
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        #endregion
        
        #region Fields
        
        [Header("UI References")]
        public Text consoleText;
        public ScrollRect scrollRect;
        
        private List<string> lines = new List<string>();
        private const int MAX_LINES = 100;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Adds a line of output to the console
        /// </summary>
        public void AddOutput(string text)
        {
            lines.Add(text);
            
            // Limit number of lines
            if (lines.Count > MAX_LINES)
            {
                lines.RemoveAt(0);
            }
            
            UpdateDisplay();
        }
        
        /// <summary>
        /// Clears all console output
        /// </summary>
        public void Clear()
        {
            lines.Clear();
            UpdateDisplay();
        }
        
        /// <summary>
        /// Gets all console text as a single string
        /// </summary>
        public string GetAllText()
        {
            return string.Join("\n", lines.ToArray());
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Updates the console display
        /// </summary>
        private void UpdateDisplay()
        {
            if (consoleText != null)
            {
                consoleText.text = GetAllText();
                
                // Scroll to bottom
                if (scrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    scrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }
        
        #endregion
    }
}
