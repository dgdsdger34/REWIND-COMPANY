//-----------------------------------------------------------------------
// REWIND COMPANY — Options Menu scene template
// Connects all three existing UI systems (GraphicsManager, ResolutionManager,
// GraphicsOptionsUI) and adds functional UI: low/medium/high graphics presets,
// resolution selection (1280x720, 1366x768, 1600x900, 1920x1080), fullscreen
// toggle, VSync toggle, and FPS limit (30 / 60 / 120 / Unlimited).
//
// Saves and loads everything with PlayerPrefs. Loads on start so the game
// remembers the player's preferences.
//
// REUSE: this script does NOT duplicate GraphicsManager, ResolutionManager,
// or GraphicsOptionsUI. It reuses them and adds only the OptionsMenu logic.
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using RewindCompany.Graphics;
using RewindCompany.Systems;

namespace RewindCompany.UI
{
    /// <summary>
    /// Options menu controller. Wires the three systems together and
    /// handles PlayerPrefs persistence.
    /// </summary>
    public class OptionsMenu : MonoBehaviour
    {
        public static OptionsMenu Instance { get; private set; }

        [Header("System references (auto-wired by this script)")]

        // 1. GraphicsManager — applies graphics presets, AA, VSync, etc.
        public GraphicsManager graphicsManager;

        // 2. ResolutionManager — sets the backbuffer resolution.
        public ResolutionManager resolutionManager;

        // 3. GraphicsOptionsUI — tier selector, settings callback wiring.
        public GraphicsOptionsUI graphicsOptionsUI;

        [Header("UI references (assign in this scene)")]

        // Resolution slider (min 1280, max 1920, step 64)
        public Dropdown resolutionDropdown;

        // Fullscreen toggle
        public Toggle fullscreenToggle;

        // VSync toggle
        public Toggle vsyncToggle;

        // FPS limit select (30 / 60 / 120 / Unlimited)
        public Dropdown fpsLimitDropdown;

        // Back button
        public Button backButton;

        // Apply button
        public Button applyButton;

        // Reset button
        public Button resetButton;

        // Graphics tier label
        public Text graphicsTierLabel;

        // FPS limit label (optional)
        public Text fpsLabel;

        // Show/hide animation
        public CanvasGroup optionsCanvasGroup;

        // Saved state (for validation)
        public bool savedGraphicsPreset;
        public bool savedResolution;
        public bool savedFullscreen;
        public bool savedVSync;
        public bool savedFPSLimit;

        // Stored so we can restore on exit
        public bool currentFullscreen;
        public int currentWidth;
        public int currentHeight;
        public int currentFPSLimit;

        // PlayerPrefs keys (guids)
        public const string PREF_GRAPHICS_PRESET = "rewind_graphics_preset";
        public const string PREF_RESOLUTION = "rewind_resolution";
        public const string PREF_FULLSCREEN = "rewind_fullscreen";
        public const string PREF_VSYNC = "rewind_vsync";
        public const string PREF_FPS_LIMIT = "rewind_fps_limit";

        // FPS limit options
        private readonly int[] fpsLimits = { 30, 60, 120, -1 }; // -1 => Unlimited

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            // 1. Reuse GraphicsManager (auto-create if missing).
            if (graphicsManager == null)
            {
                graphicsManager = GraphicsManager.Instance;
            }
            if (graphicsManager == null)
            {
                var go = new GameObject("GraphicsManager");
                graphicsManager = go.AddComponent<GraphicsManager>();
                DontDestroyOnLoad(go);
            }

            // 2. Reuse ResolutionManager (auto-create if missing).
            if (resolutionManager == null)
            {
                resolutionManager = ResolutionManager.Instance;
            }
            if (resolutionManager == null)
            {
                var go = new GameObject("ResolutionManager");
                resolutionManager = go.AddComponent<ResolutionManager>();
                DontDestroyOnLoad(go);
            }

            // 3. Reuse GraphicsOptionsUI (auto-create if missing).
            if (graphicsOptionsUI == null)
            {
                graphicsOptionsUI = GraphicsOptionsUI.Instance;
            }
            if (graphicsOptionsUI == null)
            {
                var go = new GameObject("GraphicsOptionsUI");
                graphicsOptionsUI = go.AddComponent<GraphicsOptionsUI>();
                DontDestroyOnLoad(go);
            }

            // 4. Wire all UI callbacks.
            if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveAllListeners();
            if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveAllListeners();
            if (vsyncToggle != null) vsyncToggle.onValueChanged.RemoveAllListeners();
            if (fpsLimitDropdown != null) fpsLimitDropdown.onValueChanged.RemoveAllListeners();
            if (applyButton != null) applyButton.onClick.RemoveAllListeners();
            if (resetButton != null) resetButton.onClick.RemoveAllListeners();
            if (backButton != null) backButton.onClick.RemoveAllListeners();

            if (resolutionDropdown != null) resolutionDropdown.ClearOptions();
            if (fpsLimitDropdown != null) fpsLimitDropdown.ClearOptions();

            if (resolutionDropdown != null && fpsLimitDropdown != null)
            {
                var resOptions = new System.Collections.Generic.List<Dropdown.OptionData>
                {
                    new("1280x720"),
                    new("1366x768"),
                    new("1600x900"),
                    new("1920x1080")
                };
                resolutionDropdown.AddOptions(resOptions);
            }

            var fpsOptions = new System.Collections.Generic.List<Dropdown.OptionData>
            {
                new("30"),
                new("60"),
                new("120"),
                new("Unlimited")
            };
            fpsLimitDropdown.AddOptions(fpsOptions);

            // 5. Connect button actions.
            if (applyButton != null)
            {
                applyButton.onClick.AddListener(SaveAndApply);
            }
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(LoadDefaults);
            }
            if (backButton != null)
            {
                backButton.onClick.AddListener(BackToMenu);
            }

            // 6. Load saved settings (requirement #10) and apply them.
            LoadSettings();
            ApplyCurrent();
            UpdateUI();
        }

        void Start()
        {
            // Keep the menu's internal display state in sync with the
            // saved preferences (so the toggle represents the truth).
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1;
            }
            if (vsyncToggle != null)
            {
                vsyncToggle.isOn = PlayerPrefs.GetInt(PREF_VSYNC, 1) == 1;
            }
            if (graphicsTierLabel != null)
            {
                graphicsTierLabel.text = "Graphics: " + GraphicsConfig.Singleton.activePreset.presetName;
            }
        }

        // ------------------------------------------------------------------
        // PlayerPrefs persistence (requirement #9)
        // ------------------------------------------------------------------

        /// <summary>
        /// Save every setting to PlayerPrefs.
        /// </summary>
        /// <summary>
        /// Called by the Apply button: save settings, then apply them to the
        /// running game.
        /// </summary>
        public void SaveAndApply()
        {
            SaveSettings();
            ApplyCurrent();
            UpdateUI();
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetInt(PREF_GRAPHICS_PRESET, (int)GraphicsConfig.Singleton.currentTier);
            PlayerPrefs.SetInt(PREF_RESOLUTION, GetResolutionIndex());
            PlayerPrefs.SetInt(PREF_FULLSCREEN, fullscreenToggle != null && fullscreenToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt(PREF_VSYNC, vsyncToggle != null && vsyncToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt(PREF_FPS_LIMIT, GetFPSLimitIndex());

            PlayerPrefs.Save();
            Debug.Log("[OptionsMenu] Settings saved.");
        }

        /// <summary>
        /// Load settings from PlayerPrefs (requirement #10).
        /// </summary>
        public void LoadSettings()
        {
            // Graphics preset
            int presetIndex = PlayerPrefs.GetInt(PREF_GRAPHICS_PRESET, 0);
            var tier = (GraphicsPresetTier)System.Math.Clamp(presetIndex, 0, 2);
            GraphicsConfig.Singleton.SetTier(tier);

            // Resolution
            int resIndex = PlayerPrefs.GetInt(PREF_RESOLUTION, 3);
            resIndex = System.Math.Clamp(resIndex, 0, 3);
            if (resolutionDropdown != null) resolutionDropdown.value = resIndex;

            // Fullscreen
            int fs = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1);
            if (fullscreenToggle != null) fullscreenToggle.isOn = fs == 1;

            // VSync
            int vs = PlayerPrefs.GetInt(PREF_VSYNC, 1);
            if (vsyncToggle != null) vsyncToggle.isOn = vs == 1;

            // FPS limit
            int fpsIndex = PlayerPrefs.GetInt(PREF_FPS_LIMIT, 1);
            fpsIndex = System.Math.Clamp(fpsIndex, 0, 3);
            if (fpsLimitDropdown != null) fpsLimitDropdown.value = fpsIndex;
        }

        /// <summary>
        /// Apply the currently loaded settings to the running game.
        /// </summary>
        public void ApplyCurrent()
        {
            // Graphics tier already applied by LoadSettings (SetTier).
            // Now apply presets and VSync.
            if (fullscreenToggle != null && fullscreenToggle.isOn)
            {
                Screen.fullScreen = true;
            }
            else
            {
                Screen.fullScreen = false;
            }
            QualitySettings.vSyncCount = vsyncToggle != null && vsyncToggle.isOn ? 1 : 0;

            if (resolutionDropdown != null)
            {
                int idx = resolutionDropdown.value;
                if (resolutionManager != null)
                {
                    resolutionManager.SetResolution(
                        ResolutionTable[idx].width,
                        ResolutionTable[idx].height,
                        Screen.fullScreen,
                        true);
                }
                else
                {
                    Debug.LogWarning("[OptionsMenu] ResolutionManager not assigned. "
                        + "Verify the scene wiring in the OptionsMenu inspector.");
                }
            }

            // Update the graphics registry so presets re-apply.
            if (graphicsManager != null)
            {
                graphicsManager.ApplyPreset(GraphicsConfig.Singleton.activePreset);
            }

            // FPS limit via QualitySettings
            if (fpsLimitDropdown != null)
            {
                int idx = fpsLimitDropdown.value;
                QualitySettings.maximumFPS = fpsLimits[System.Math.Clamp(idx, 0, 3)];
            }

            savedGraphicsPreset = true;
            savedResolution = true;
            savedFullscreen = true;
            savedVSync = true;
            savedFPSLimit = true;

            Debug.Log("[OptionsMenu] Settings applied.");
        }

        /// <summary>
        /// Load the default settings.
        /// </summary>
        public void LoadDefaults()
        {
            PlayerPrefs.DeleteKey(PREF_GRAPHICS_PRESET);
            PlayerPrefs.DeleteKey(PREF_RESOLUTION);
            PlayerPrefs.DeleteKey(PREF_FULLSCREEN);
            PlayerPrefs.DeleteKey(PREF_VSYNC);
            PlayerPrefs.DeleteKey(PREF_FPS_LIMIT);
            PlayerPrefs.Save();

            GraphicsConfig.Singleton.SetTier(GraphicsPresetTier.Low);
            if (resolutionDropdown != null) resolutionDropdown.value = 3;
            if (fullscreenToggle != null) fullscreenToggle.isOn = true;
            if (vsyncToggle != null) vsyncToggle.isOn = true;
            if (fpsLimitDropdown != null) fpsLimitDropdown.value = 1;

            ApplyCurrent();
            UpdateUI();
            Debug.Log("[OptionsMenu] Defaults loaded.");
        }

        /// <summary>
        /// Close the options menu and return to the main menu.
        /// </summary>
        public void BackToMenu()
        {
            if (optionsCanvasGroup != null)
            {
                optionsCanvasGroup.alpha = 0f;
                optionsCanvasGroup.blocksRaycasts = false;
            }
            isOptionsOpen = false;
            Debug.Log("[OptionsMenu] Closed.");
        }

        /// <summary>
        /// Open the options menu.
        /// </summary>
        public void Open()
        {
            if (optionsCanvasGroup != null)
            {
                optionsCanvasGroup.alpha = 1f;
                optionsCanvasGroup.blocksRaycasts = true;
            }
            isOptionsOpen = true;
            UpdateUI();
        }

        // ------------------------------------------------------------------
        // Internal helpers
        // ------------------------------------------------------------------

        private void UpdateUI()
        {
            if (graphicsTierLabel != null)
            {
                graphicsTierLabel.text = "Graphics: " + GraphicsConfig.Singleton.activePreset.presetName;
            }
            if (fpsLabel != null)
            {
                int idx = fpsLimitDropdown != null ? fpsLimitDropdown.value : 1;
                fpsLabel.text = fpsLimits[System.Math.Clamp(idx, 0, 3)] == -1 ? "Unlimited" : fpsLimits[System.Math.Clamp(idx, 0, 3)].ToString() + " FPS";
            }
        }

        private int GetResolutionIndex()
        {
            if (resolutionDropdown != null) return resolutionDropdown.value;
            // Fallback: infer from screen size.
            if (Screen.width >= 1920) return 3;
            if (Screen.width >= 1600) return 2;
            if (Screen.width >= 1366) return 1;
            return 0;
        }

        private int GetFPSLimitIndex()
        {
            if (fpsLimitDropdown != null) return fpsLimitDropdown.value;
            return 1; // default 60
        }

        private struct ResolutionOption
        {
            public int width;
            public int height;
        }

        private static readonly ResolutionOption[] ResolutionTable =
        {
            new ResolutionOption { width = 1280, height = 720 },
            new ResolutionOption { width = 1366, height = 768 },
            new ResolutionOption { width = 1600, height = 900 },
            new ResolutionOption { width = 1920, height = 1080 }
        };
    }
}
