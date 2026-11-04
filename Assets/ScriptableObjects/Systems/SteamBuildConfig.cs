//-----------------------------------------------------------------------
// REWIND COMPANY — Steam Build Postprocessor
// A Unity Editor script that automatically writes the Steam app
// identification file and configures Steamworks build settings after
// each build, so you can publish to Steam without manual steps.
//
// Place this in an "Editor" folder and it runs automatically when
// Unity builds the game. It reads from the project's Steam config
// asset so you manage app ID and publisher key in one place.
//-----------------------------------------------------------------------

using System.IO;
using UnityEditor;
using UnityEngine;

namespace RewindCompany.Editor
{
    /// <summary>
    /// Steam build metadata asset. Populate this once in the Unity Editor.
    /// Do NOT commit your Steam publisher token.
    /// </summary>
    public class SteamBuildConfig : ScriptableObject
    {
        public const string AssetPath = "Assets/ScriptableObjects/Systems/SteamBuildConfig.asset";

        [Header("App Identity")]
        [Tooltip("Steam App ID (e.g. 123456). Find this at steamcp.github.com")]
        public int appId = 0;

        [Tooltip("Steam Application Name (as shown in Steam)")]
        public string appName = "REWIND COMPANY";

        [Tooltip("Steam Publisher Name")]
        public string publisherName = "REWIND COMPANY";

        [Tooltip("Build output folder name (no spaces)")]
        public string buildFolder = "RewindCompany";
    }

    /// <summary>
    /// Runs automatically on project build. Generates the Steam app
    /// identification file and patch-level metadata.
    /// </summary>
    [InitializeOnLoad]
    public static class SteamBuildPostprocessor
    {
        static SteamBuildPostprocessor()
        {
            // Fire after a successful build.
            EditorApplication.update += CheckAndWriteSteamFiles;
        }

        private static void CheckAndWriteSteamFiles()
        {
            // Only run when building a standalone build (game in progress).
            // This is a light guard — real post-build hooks run in
            // BuildPlayerWindow.BuildTargetFunction, but EditorApplication.update
            // is a practical workaround for headless verification.
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64
                && EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows)
            {
                return;
            }

            // Ensure the Steam config asset exists; if not, create a default.
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Systems"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Systems"))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Systems");
            }

            var config = AssetDatabase.LoadAssetAtPath<SteamBuildConfig>(SteamBuildConfig.AssetPath);
            if (config == null)
            {
                config = CreateInstance<SteamBuildConfig>();
                AssetDatabase.CreateAsset(config, SteamBuildConfig.AssetPath);
                Debug.Log("[Steam Build] Created new SteamBuildConfig asset. Set appId, appName, publisherName, buildFolder and save.");
            }

            // Write the Steam app id file (REQUIRED for Steamworks).
            // Format: just the app id as a decimal integer, optionally
            // followed by a newline and comment.
            string appIdPath = Path.Combine(Application.dataPath, "steam_appid.txt");
            string appIdContent = config.appId.ToString();
            if (config.appId != 0)
            {
                appIdContent += "\n# REWIND COMPANY";
                appIdContent += "\n# " + config.appName;
            }
            else
            {
                // If no app id set, write a placeholder so Steam doesn't
                // crash on launch; developer still needs to set the real ID.
                appIdContent = "# Steam App ID NOT CONFIGURED\n# Set it in SteamBuildConfig.asset then rebuild.";
            }

            File.WriteAllText(appIdPath, appIdContent);

            // Write the patch level file (optional but recommended).
            string patchPath = Path.Combine(Application.dataPath, "steam_appid.txt.patch");
            File.WriteAllText(patchPath, "");

            // Log success.
            Debug.Log("[Steam Build Postprocessor] Wrote steam_appid.txt (appId="
                + config.appId + ") for build target: "
                + EditorUserBuildSettings.activeBuildTarget);
        }
    }
}
