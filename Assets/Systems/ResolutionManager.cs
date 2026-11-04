//-----------------------------------------------------------------------
// REWIND COMPANY — Resolution Manager
// Sets the backbuffer resolution and fullscreen state at runtime.
// Calls directly to System.Windows.Forms or the graphics API through
// Unity's Screen class. Avoids the deprecated Display.SetResolution()
// warnings by using Screen.SetResolution (supported on all platforms).
//-----------------------------------------------------------------------

using UnityEngine;

namespace RewindCompany.Systems
{
    /// <summary>
    /// Resolves the display resolution at runtime. Safe to call from
    /// any platform; uses Unity's Screen API (public API, no native hooks).
    /// </summary>
    public class ResolutionManager : MonoBehaviour
    {
        public static ResolutionManager Instance { get; private set; }

        public bool isFullscreen = true;
        public int width = 1280;
        public int height = 720;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable() { }

        /// <summary>
        /// Apply a resolution. Pass -1 to keep current orientation/aspect.
        /// </summary>
        public void SetResolution(int w, int h, bool fullscreen = true, bool allowFullscreen = true)
        {
            width = w;
            height = h;
            isFullscreen = fullscreen;

            // Screen.SetResolution is the cross-platform way to set display
            // resolution. It does not require coding-behind.
            if (Screen.fullScreen != allowFullscreen || Screen.currentResolution.width != w || Screen.currentResolution.height != h)
            {
                int actualWidth = w;
                int actualHeight = h;
                Screen.SetResolution(actualWidth, actualHeight, fullscreen);
                Debug.Log("[RewindGraphics] Resolution set: " + actualWidth + "x" + actualHeight
                    + " fullscreen=" + fullscreen);
            }
        }

        /// <summary>
        /// Quick toggle fullscreen (intended for Options menu).
        /// </summary>
        public void ToggleFullscreen()
        {
            isFullscreen = !isFullscreen;
            Screen.fullScreen = isFullscreen;
            Debug.Log("[RewindGraphics] Fullscreen toggled: " + isFullscreen);
        }

        /// <summary>
        /// Resizes to match the current window size (keeps aspect).
        /// </summary>
        public void FitToWindow()
        {
            width = Screen.width;
            height = Screen.height;
            SetResolution(width, height, isFullscreen, true);
        }
    }
}
