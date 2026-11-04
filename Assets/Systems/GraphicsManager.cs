//-----------------------------------------------------------------------
// REWIND COMPANY — Graphics Manager
// Applies the active GraphicsPreset at runtime. Executes resolution,
// anti-aliasing, anisotropic filtering, shadow distance, bloom, and
// the rewind VFX Baking tile size. Exposes a clean API for the Options
// menu to call.
//-----------------------------------------------------------------------

using UnityEngine;
using RewindCompany.Graphics;

namespace RewindCompany.Systems
{
    /// <summary>
    /// Central graphics controller. Holds the active preset and applies it
    /// to the render pipeline. Safe to call from the Options menu UI.
    /// </summary>
    public class GraphicsManager : MonoBehaviour
    {
        public static GraphicsManager Instance { get; private set; }

        [Header("Config")]
        public GraphicsConfig config;

        [Header("References (auto-assigned)")]
        public ResolutionManager resolutionManager;

        public bool isApplied;
        public int currentWidth = 1280;
        public int currentHeight = 720;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // Ensure GraphicsConfig.active is populated for UI and any
            // runtime lookups that happen before this frame.
            GraphicsConfig.Singleton;
            if (config == null)
            {
                config = GraphicsConfig.Singleton;
            }
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GraphicsConfig>();
                config.low = ScriptableObject.CreateInstance<GraphicsPresetLow>();
                config.medium = ScriptableObject.CreateInstance<GraphicsPresetMedium>();
                config.high = ScriptableObject.CreateInstance<GraphicsPresetHigh>();
                config.currentTier = GraphicsPresetTier.Low;
            }
            else
            {
                config.low = config.low ?? ScriptableObject.CreateInstance<GraphicsPresetLow>();
                config.medium = config.medium ?? ScriptableObject.CreateInstance<GraphicsPresetMedium>();
                config.high = config.high ?? ScriptableObject.CreateInstance<GraphicsPresetHigh>();
            }
            config.active = config;
        }

        /// <summary>
        /// Apply the full preset. Call this whenever the player switches
        /// quality tier or when the options menu saves.
        /// </summary>
        public void ApplyPreset(GraphicsPreset preset)
        {
            if (preset == null) return;

            currentWidth = preset.targetWidth;
            currentHeight = preset.targetHeight;

            // Resolution
            if (resolutionManager != null)
            {
                resolutionManager.SetResolution(preset.targetWidth, preset.targetHeight,
                    preset.fullscreen, preset.allowFullscreen);
            }

            // Anti-aliasing
            QualitySettings.antiAliasing = preset.antiAliasing;

            // Anisotropic filtering
            QualitySettings.anisotropicFiltering = preset.anisotropicFiltering ?
                AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;

            // VSync
            QualitySettings.vSyncCount = config.vsync ? 1 : 0;

            // Bloom
            if (GetComponent<Bloom>() != null)
            {
                var bloom = GetComponent<Bloom>();
                bloom.bloomIntensity = preset.bloomIntensity;
                bloom.bloomThreshold = preset.bloomThreshold;
            }
            else if (preset.bloomIntensity > 0f)
            {
                Debug.LogWarning("[GraphicsManager] Bloom component not found. "
                    + "Add a post-processing bloom component to see bloom.");
            }

            // Vignette
            if (GetComponent<Vignette>() != null)
            {
                var vig = GetComponent<Vignette>();
                vig.intensity = preset.vignetteIntensity;
            }
            else if (preset.vignetteIntensity > 0f)
            {
                Debug.LogWarning("[GraphicsManager] Vignette component not found. "
                    + "Add a vignette component to see vignette.");
            }

            // Shadows
            QualitySettings.shadowDistance = preset.shadowDistance;
            QualitySettings.shadowCascades = preset.shadowCascades;
            QualitySettings.shadowMapSize = preset.shadowMapSize;

            // Lighting
            RenderSettings.ambientIntensity = preset.ambientLightIntensity;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientProbe = new LightProbes();
            RenderSettings.ambientLight = Color.white * preset.ambientLightIntensity;

            // Rewind VFX baking tile
            if (GetComponent<RewindVFXBaking>() != null)
            {
                var vb = GetComponent<RewindVFXBaking>();
                vb.tileSize = preset.rewindVFXTileSize;
                vb.sampleCount = preset.rewindVFXSampleCount;
            }
            else if (preset.rewindVFXTileSize > 0)
            {
                Debug.LogWarning("[GraphicsManager] RewindVFXBaking component not found. "
                    + "Add the VFX baking component to bake time-rewind effects.");
            }

            // Motion blur
            if (GetComponent<MotionBlur>() != null)
            {
                var mb = GetComponent<MotionBlur>();
                mb.enabled = preset.enableMotionBlur;
                mb.strength = preset.motionBlurStrength;
            }
            else if (preset.enableMotionBlur)
            {
                Debug.LogWarning("[GraphicsManager] MotionBlur component not found. "
                    + "Add a motion blur component to enable motion blur.");
            }

            isApplied = true;
            Debug.Log("[RewindGraphics] Applied preset: " + preset.presetName);
        }

        /// <summary>
        /// Called from the Options menu to step to the next tier.
        /// </summary>
        public void CycleTier()
        {
            var next = (GraphicsPresetTier)(((int)config.currentTier + 1) % 3);
            config.SetTier(next);
            ApplyPreset(config.activePreset);
        }
    }
}
