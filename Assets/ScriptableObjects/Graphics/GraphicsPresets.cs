//-----------------------------------------------------------------------
// REWIND COMPANY — Graphics Presets
// Three quality tiers (Low / Medium / High) that the player selects via
// the Options menu. Each preset sets resolution, anti-aliasing, shadows,
// shadows distance, bloom, ambient occlusion, post-processing, and the
// tiling/quality thresholds used by the time-rewind VFX.
//
// Presets are ScriptableObjects so you can edit them in the Unity
// Inspector without touching code. They map to the resolution slider in
// the options menu.
//-----------------------------------------------------------------------

using System;
using UnityEngine;

namespace RewindCompany.Graphics
{
    /// <summary>
    /// One named quality preset. These are the tiers the options menu exposes.
    /// </summary>
    [Serializable]
    public enum GraphicsPresetTier
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// One complete graphics preset. Fields map directly to Unity render
    /// settings exposed through the Quality window / Graphics Settings.
    /// </summary>
    [CreateAssetMenu(fileName = "GraphicsPreset", menuName = "RewindCompany/Graphics Preset")]
    public class GraphicsPreset : ScriptableObject
    {
        public string presetName = "Low";
        public string description = "Fastest load, lowest fidelity.";

        [Header("Resolution")]
        public int targetWidth = 1280;
        public int targetHeight = 720;
        public bool fullscreen = true;
        public bool allowFullscreen = true;

        [Header("Anti-Aliasing / Visuals")]
        public int antiAliasing = 2;          // 2x AA
        public bool anisotropicFiltering = true; // 2x max
        public float bloomIntensity = 0.35f;
        public float bloomThreshold = 0.8f;
        public float vignetteIntensity = 0.25f;

        [Header("Shadows")]
        public float shadowDistance = 30f;
        public int shadowCascades = 2;        // 2 cascades
        public int shadowMapSize = 1024;      // 1024
        public float dynamicShadowBias = 0.05f;

        [Header("Lighting")]
        public float ambientLightIntensity = 0.6f;
        public float globalIlluminationScale = 1f;
        public bool realtimeGlobalIllumination = false;

        [Header("Post-Processing")]
        public bool enableDepthOfField = true;
        public bool enableSubsurfaceScattering = false;
        public bool enableMotionBlur = false; // saves CPU/Computely for rewind
        public float motionBlurStrength = 0.5f;

        [Header("Lighting Quality")]
        public int shadowDistanceLegacy = 30; // for fallbacks
        public bool softShadows = true;
        public bool realtimeReflectionProbes = false;

        [Header("Time-Rewind VFX Baking")]
        public int rewindVFXTileSize = 512;
        public int rewindVFXSampleCount = 64;

        public override string ToString()
        {
            return presetName;
        }
    }

    /// <summary>
    /// Master graphics configuration that holds all three presets and
    /// exposes the currently active one, plus the options-menu binding.
    /// </summary>
    [CreateAssetMenu(fileName = "GraphicsConfig", menuName = "RewindCompany/Graphics Config")]
    public class GraphicsConfig : ScriptableObject
    {
        public GraphicsPresetLow low = null;
        public GraphicsPresetMedium medium = null;
        public GraphicsPresetHigh high = null;

        [Header("Settings")]
        public bool isStereo = false;
        public bool vsync = true;
        public float targetFPS = 60f;

        public GraphicsPresetTier currentTier = GraphicsPresetTier.Low;

        public GraphicsPreset activePreset
        {
            get
            {
                switch (currentTier)
                {
                    case GraphicsPresetTier.Low: return low;
                    case GraphicsPresetTier.Medium: return medium;
                    case GraphicsPresetTier.High: return high;
                    default: return low;
                }
            }
        }

        public void SetTier(GraphicsPresetTier tier)
        {
            currentTier = tier;
            Debug.Log("[RewindGraphics] Graphics tier set to: " + tier);
        }

        public static GraphicsConfig active;

        public static GraphicsConfig Singleton
        {
            get
            {
                if (active == null)
                {
                    // Fallback: attempt to load from Resources.
                    active = Resources.Load<GraphicsConfig>("GraphicsConfig");
                    if (active == null)
                    {
                        active = ScriptableObject.CreateInstance<GraphicsConfig>();
                        active.low = ScriptableObject.CreateInstance<GraphicsPresetLow>();
                        active.medium = ScriptableObject.CreateInstance<GraphicsPresetMedium>();
                        active.high = ScriptableObject.CreateInstance<GraphicsPresetHigh>();
                        active.currentTier = GraphicsPresetTier.Low;
                    }
                }
                return active;
            }
        }
    }

    /// <summary>
    /// Each preset is a separate asset. Provide three concrete files with
    /// defaults that are tuned for the low-poly office style.
    /// </summary>
    public class GraphicsPresetLow : GraphicsPreset
    {
        public GraphicsPresetLow()
        {
            presetName = "Low";
            description = "Fastest load, lowest fidelity.";
            targetWidth = 1280;
            targetHeight = 720;
            antiAliasing = 2;
            anisotropicFiltering = false;
            bloomIntensity = 0.15f;
            bloomThreshold = 0.9f;
            vignetteIntensity = 0.3f;
            shadowDistance = 15f;
            shadowCascades = 1;
            shadowMapSize = 512;
            ambientLightIntensity = 0.5f;
            globalIlluminationScale = 0.6f;
            enableDepthOfField = false;
            enableSubsurfaceScattering = false;
            enableMotionBlur = false;
            rewindVFXTileSize = 256;
            rewindVFXSampleCount = 16;
        }
    }

    public class GraphicsPresetMedium : GraphicsPreset
    {
        public GraphicsPresetMedium()
        {
            presetName = "Medium";
            description = "Balanced quality and performance.";
            targetWidth = 1920;
            targetHeight = 1080;
            antiAliasing = 4;
            anisotropicFiltering = true;
            bloomIntensity = 0.3f;
            bloomThreshold = 0.8f;
            vignetteIntensity = 0.25f;
            shadowDistance = 30f;
            shadowCascades = 2;
            shadowMapSize = 1024;
            ambientLightIntensity = 0.65f;
            globalIlluminationScale = 1f;
            enableDepthOfField = true;
            enableSubsurfaceScattering = false;
            enableMotionBlur = false;
            rewindVFXTileSize = 512;
            rewindVFXSampleCount = 32;
        }
    }

    public class GraphicsPresetHigh : GraphicsPreset
    {
        public GraphicsPresetHigh()
        {
            presetName = "High";
            description = "Maximum visual fidelity for the studio version.";
            targetWidth = 2560;
            targetHeight = 1440;
            antiAliasing = 8;
            anisotropicFiltering = true;
            bloomIntensity = 0.5f;
            bloomThreshold = 0.7f;
            vignetteIntensity = 0.2f;
            shadowDistance = 50f;
            shadowCascades = 3;
            shadowMapSize = 2048;
            ambientLightIntensity = 0.8f;
            globalIlluminationScale = 1.2f;
            enableDepthOfField = true;
            enableSubsurfaceScattering = false;
            enableMotionBlur = true;
            motionBlurStrength = 0.6f;
            rewindVFXTileSize = 1024;
            rewindVFXSampleCount = 64;
        }
    }
}
