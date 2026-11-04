//-----------------------------------------------------------------------
// REWIND COMPANY — Graphics Options UI
// Menu-driven controller for the three quality presets. Hooks into the
// GraphicsConfig asset, applies presets through GraphicsManager, and
// exposes a clean API for UI buttons to call.
//
// The actual UI panel (text fields, buttons, slider) is wired to this
// class in the Unity Editor via the Unity Inspector.
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using RewindCompany.Graphics;
using RewindCompany.Systems;

namespace RewindCompany.UI
{
    /// <summary>
    /// Presents the three quality presets to the player through the
    /// Options menu. No UI code here — just the logic the menu buttons
    /// call.
    /// </summary>
    public class GraphicsOptionsUI : MonoBehaviour
    {
        public static GraphicsOptionsUI Instance { get; private set; }

        [Header("References")]
        public GraphicsManager graphicsManager;

        [Header("UI: slider output")]
        public Text tierLabel;

        public bool isOptionsOpen;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            // Ensure the GraphicsManager exists.
            if (graphicsManager == null) graphicsManager = GraphicsManager.Instance;
            if (graphicsManager == null)
            {
                var go = new GameObject("GraphicsManager");
                graphicsManager = go.AddComponent<GraphicsManager>();
                DontDestroyOnLoad(go);
            }

            // Make sure a GraphicsConfig asset with all three presets exists.
            var cfg = GraphicsConfig.Singleton;
            if (cfg.low == null) cfg.low = ScriptableObject.CreateInstance<GraphicsPresetLow>();
            if (cfg.medium == null) cfg.medium = ScriptableObject.CreateInstance<GraphicsPresetMedium>();
            if (cfg.high == null) cfg.high = ScriptableObject.CreateInstance<GraphicsPresetHigh>();

            if (tierLabel != null) tierLabel.onValueChanged.RemoveAllListeners();
            UpdateUI();
        }

        /// <summary>
        /// Called by the Options menu to switch to the next tier.
        /// </summary>
        public void NextTier()
        {
            GraphicsConfig.Singleton.SetTier((GraphicsPresetTier)(((int)GraphicsConfig.Singleton.currentTier + 1) % 3));
            graphicsManager.ApplyPreset(GraphicsConfig.Singleton.activePreset);
            UpdateUI();
        }

        /// <summary>
        /// Called by the Options menu to go back one tier.
        /// </summary>
        public void PrevTier()
        {
            var prev = (GraphicsPresetTier)(((int)GraphicsConfig.Singleton.currentTier - 1 + 3) % 3);
            GraphicsConfig.Singleton.SetTier(prev);
            graphicsManager.ApplyPreset(GraphicsConfig.Singleton.activePreset);
            UpdateUI();
        }

        /// <summary>
        /// Apply the preset immediately for a slider thumb drag.
        /// </summary>
        public void ApplyCurrent()
        {
            graphicsManager.ApplyPreset(GraphicsConfig.Singleton.activePreset);
        }

        /// <summary>
        /// Refresh the UI (e.g. on open).
        /// </summary>
        public void UpdateUI()
        {
            if (tierLabel != null)
            {
                tierLabel.text = "Graphics: " + GraphicsConfig.Singleton.activePreset.presetName;
            }
        }

        /// <summary>
        /// Reset to defaults.
        /// </summary>
        public void ResetToDefaults()
        {
            GraphicsConfig.Singleton.SetTier(GraphicsPresetTier.Low);
            graphicsManager.ApplyPreset(GraphicsConfig.Singleton.activePreset);
            UpdateUI();
        }
    }
}
