//-----------------------------------------------------------------------
// REWIND COMPANY — Scene Loading Manager
// Handles asynchronous scene loading, scene-ready events, and a simple
// menu/credits transition. The time-rewind manager is a singleton and
// survives scene loads, so rewind state persists across doors.
//-----------------------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RewindCompany.World
{
    /// <summary>
    /// Raised when a scene finishes loading so other modules can initialise.
    /// </summary>
    public class SceneLoadedEventArgs : EventArgs
    {
        public string sceneName;
        public bool isMain;
    }

    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }

        // Cached scenes to avoid repeated lookups.
        public static string MainOffice => "MainOffice";
        public static string Reception => "Reception";
        public static string MeetingRoom => "MeetingRoom";
        public static string Storage => "Storage";
        public static string ServerRoom => "ServerRoom";
        public static string Menu => "Menu";
        public static string Credits => "Credits";

        public event EventHandler<SceneLoadedEventArgs> OnSceneLoaded;

        [Header("Fallback")]
        public string defaultScene = "Menu";

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable() { }

        /// <summary>
        /// Loads a scene asynchronously. Returns the AsyncOperation; callers
        /// can yield on it to wait for the scene to be ready.
        /// </summary>
        public IEnumerator LoadScene(string sceneName, bool additive = false)
        {
            if (additive)
            {
                AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                while (!op.isDone)
                {
                    yield return null;
                }
            }
            else
            {
                AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
                while (!op.isDone)
                {
                    yield return null;
                }
            }

            OnSceneLoaded?.Invoke(this, new SceneLoadedEventArgs
            {
                sceneName = sceneName,
                isMain = sceneName == MainOffice || sceneName == Reception ||
                         sceneName == MeetingRoom || sceneName == Storage || sceneName == ServerRoom
            });
        }

        public void LoadMenu()
        {
            StartCoroutine(LoadScene(Menu));
        }

        public void LoadCredits()
        {
            StartCoroutine(LoadScene(Credits));
        }

        public void LoadMainOffice()
        {
            StartCoroutine(LoadScene(MainOffice));
        }

        public void LoadReception()
        {
            StartCoroutine(LoadScene(Reception));
        }

        public void LoadMeetingRoom()
        {
            StartCoroutine(LoadScene(MeetingRoom));
        }

        public void LoadStorage()
        {
            StartCoroutine(LoadScene(Storage));
        }

        public void LoadServerRoom()
        {
            StartCoroutine(LoadScene(ServerRoom));
        }

        /// <summary>
        /// Reloads the currently loaded scene (useful after a reset).
        /// </summary>
        public void ReloadCurrent()
        {
            int active = SceneManager.GetActiveScene().buildIndex;
            if (active >= 0)
            {
                SceneManager.LoadScene(active);
            }
        }
    }
}
