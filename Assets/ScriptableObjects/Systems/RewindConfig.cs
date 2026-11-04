//-----------------------------------------------------------------------
// REWIND COMPANY - Rewind config asset placeholder (generated)
// No content needed - it's a ScriptableObject created in the Unity Editor.
//-----------------------------------------------------------------------

using UnityEngine;

namespace RewindCompany
{
    [CreateAssetMenu(fileName = "RewindConfig", menuName = "RewindCompany/Rewind Config")]
    public class RewindConfig : ScriptableObject
    {
        [Tooltip("Length of rewind window in seconds.")]
        public float rewindDuration = 30f;
        [Tooltip("Each snapshot is stamped every this many physics frames.")]
        public int snapshotInterval = 4;
        [Tooltip("How far the player can travel while rewinding.")]
        public float rewindReturnSpeed = 10f;
        [Tooltip("Max recorded player actions retained for undo.")]
        public int maxActionRecords = 200;
        [Tooltip("Playback speed of rewind transition.")]
        public float playbackSpeed = 1.5f;
        [Tooltip("If true, objects snap instantly to recorded state (cheap). If false, tween.")]
        public bool instantSnap = true;
    }
}
