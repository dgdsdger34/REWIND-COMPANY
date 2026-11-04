//-----------------------------------------------------------------------
// REWIND COMPANY - Editor-only Steam postprocessor wrapper.
// Kept separate from the runtime config above so the Editor files are
// excluded from builds and games, but the static class stays in
// ScriptableObjects/Systems so it can be referenced by the Editor.
// 
// This wrapper keeps Unity's Editor scripts tidy and excludes them
// from builds automatically (Unity excludes Editor folders).
//-----------------------------------------------------------------------

using UnityEditor;

namespace RewindCompany.Editor
{
    /// <summary>
    /// Placeholder editor script. The actual work lives in
    /// Assets/ScriptableObjects/Systems/SteamBuildPostprocessor.cs.
    /// This file exists so the Editor folders are recognised by the IDE.
    /// </summary>
    public class SteamEditorPostprocessor : MonoBehaviour
    {
        // Empty - the static postprocessor in the Systems folder does the work.
    }
}
