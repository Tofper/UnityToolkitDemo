using System.Collections;
using UnityEngine;

namespace Scripts.Utilities
{
    /// <summary>
    /// Helper class to run Unity Coroutines from non-MonoBehaviour classes or static contexts,
    /// specifically for use with UI Toolkit elements that might not have a MonoBehaviour context.
    /// Implements a simple DontDestroyOnLoad singleton pattern.
    /// </summary>
    public class UIToolkitCoroutineHelper : MonoBehaviour
    {
        private static UIToolkitCoroutineHelper _instance;

        /// <summary>
        /// Gets the singleton instance of the UIToolkitCoroutineHelper.
        /// Creates the instance if it doesn't exist.
        /// </summary>
        public static UIToolkitCoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("UIToolkitCoroutineHelper");
                    // Hide in hierarchy and prevent saving to scene
                    go.hideFlags = HideFlags.HideAndDontSave;
                    _instance = go.AddComponent<UIToolkitCoroutineHelper>();
                    // Ensure the helper persists across scene loads
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Starts a Unity Coroutine on the helper instance.
        /// </summary>
        /// <param name="routine">The IEnumerator routine to start.</param>
        /// <returns>A reference to the started Coroutine.</returns>
        public static Coroutine StartUIToolkitCoroutine(IEnumerator routine)
        {
            if (Instance == null)
            {
                Debug.LogError($"{nameof(UIToolkitCoroutineHelper)}: Attempted to start coroutine, but Instance is null.");
                return null; // Cannot start coroutine without an instance
            }
            if (routine == null)
            {
                Debug.LogWarning($"{nameof(UIToolkitCoroutineHelper)}: Attempted to start a null coroutine.");
                return null;
            }
            return Instance.StartCoroutine(routine);
        }

        /// <summary>
        /// Stops a specific Unity Coroutine running on the helper instance.
        /// </summary>
        /// <param name="coroutine">The Coroutine reference to stop. Can be null.</param>
        public static void StopUIToolkitCoroutine(Coroutine coroutine)
        {
            // Check if the instance exists and the coroutine reference is valid before stopping
            if (_instance != null && coroutine != null)
            {
                _instance.StopCoroutine(coroutine);
            }
            else if (_instance == null)
            {
                Debug.LogWarning($"{nameof(UIToolkitCoroutineHelper)}: Attempted to stop coroutine, but Instance is null.");
            }
        }

        /// <summary>
        /// Called when the MonoBehaviour is destroyed.
        /// Cleans up the static instance reference.
        /// </summary>
        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                Debug.Log($"{nameof(UIToolkitCoroutineHelper)}: Singleton instance destroyed.");
            }
        }
    }
}