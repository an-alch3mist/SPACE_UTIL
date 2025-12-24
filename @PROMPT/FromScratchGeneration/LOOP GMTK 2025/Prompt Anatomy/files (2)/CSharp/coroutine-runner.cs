using System;
using System.Collections;
using UnityEngine;

namespace LOOPLanguage
{
    /// <summary>
    /// Safely runs coroutines with error handling
    /// Wraps execution to catch and report errors
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;
        
        public static CoroutineRunner Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("CoroutineRunner");
                    instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }
        
        /// <summary>
        /// Runs a coroutine with error handling
        /// </summary>
        public void RunSafely(IEnumerator coroutine, Action<Exception> onError)
        {
            StartCoroutine(SafeCoroutineWrapper(coroutine, onError));
        }
        
        /// <summary>
        /// Wraps a coroutine to catch exceptions
        /// </summary>
        private IEnumerator SafeCoroutineWrapper(IEnumerator coroutine, Action<Exception> onError)
        {
            bool hasMore = true;
            
            while (hasMore)
            {
                object current = null;
                
                try
                {
                    hasMore = coroutine.MoveNext();
                    current = coroutine.Current;
                }
                catch (Exception e)
                {
                    if (onError != null)
                    {
                        onError(e);
                    }
                    else
                    {
                        Debug.LogError($"Coroutine error: {e.Message}\n{e.StackTrace}");
                    }
                    yield break;
                }
                
                if (hasMore)
                {
                    yield return current;
                }
            }
        }
        
        /// <summary>
        /// Stops all running coroutines
        /// </summary>
        public void StopAll()
        {
            StopAllCoroutines();
        }
    }
}
