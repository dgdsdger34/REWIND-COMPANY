//-----------------------------------------------------------------------
// REWIND COMPANY — Time Manipulation System
// State snapshot buffer (the 30-second circular rewind store).
// Design: reliable state-recording system, not "teleport back in time".
// When the player rewinds, every tracked object's saved position/state
// is restored from the buffer. Objects remember their previous states.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using RewindCompany;

namespace RewindCompany.Systems
{
    /// <summary>
    /// Configurable limits for the time-rewind buffer.


    /// <summary>
    /// Central time-rewind system. Records object state, plays it back.
    /// Design notes:
    ///  - Records state deltas per entity, not by scene snapshot.
    ///  - Handles actor (= player) separately from world objects.
    ///  - Trigger countdowns run *across* the window so the future reacts to the past.
    /// </summary>
    public class RewindSystem : MonoBehaviour
    {
        public static RewindSystem Instance { get; private set; }

        [Header("References")]
        public RewindConfig config;
        public PlayerController player;
        public EntityManager entityManager;

        [Header("Buffers")]
        public List<StateSnapshot> stateBuffer = new List<StateSnapshot>();
        public List<PlayerActionRecord> actionRecords = new List<PlayerActionRecord>();

        [Header("State")]
        public bool isRewinding;
        public bool isRecording;
        public long rewindWindowStart;
        public int currentTick;
        public int rewindIndex;
        public bool hasUndo;
        public bool isPlayingBack;

        // Debug info
        public int currentBufferCount;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable() { Initialize(); }
        void OnDisable() { Cleanup(); }

        void Initialize()
        {
            isRecording = true;
            currentTick = 0;
            rewindIndex = 0;
            hasUndo = false;
            isRewinding = false;
            isPlayingBack = false;

            // Pre-allocate fixed-size snapshot slots to avoid per-frame GC.
            int pre = (int)(config.rewindDuration / 0.1f);
            stateBuffer.Capacity = pre;
            actionRecords.Capacity = config.maxActionRecords;
            for (int i = 0; i < pre; i++) stateBuffer.Add(new StateSnapshot());
        }

        void Cleanup()
        {
            Instance = null;
        }

        void Update()
        {
            // Snapshot when recording (every interval).
            // The buffer is a ring; we compact on rewind to keep a contiguous window.
            if (isRecording && !isRewinding)
            {
                currentTick++;
                if (currentTick % config.snapshotInterval == 0)
                {
                    CompactBuffer();
                    ApplySnapshot(currentTick);
                    hasUndo = true;
                }
            }

            // Input handling for rewind (player-triggered).
            if (CanRewind())
            {
                if (Input.GetButtonDown("Rewind"))
                {
                    StartRewind();
                }
            }

            if (isRewinding && !isPlayingBack)
            {
                // We do not "move the camera back" — we restore recorded state.
                // The transition happens via RestoreSnapshotSequence below.
                isPlayingBack = true;
                RestoreSnapshotSequence();
                isPlayingBack = false;
            }

            // Ruin: trigger countdowns for triggered events should only check against
            // the elapsed window; handled by entity/manager code.
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused) { isRecording = false; }
            else
            {
                // After replay, need fresh recording from the new present.
                isRecording = true;
                currentTick = 0;
                hasUndo = false;
            }
        }

        bool CanRewind()
        {
            return Input.GetKeyDown(KeyCode.R);
        }

        void StartRewind()
        {
            if (isRewinding) return;
            if (!hasUndo) return;

            // Save the current player position so we can return.
            if (player != null) player.RecordRewindPosition();

            isRewinding = true;
            rewindWindowStart = Time.realtimeSinceStartup;
            hasUndo = false;
        }

        void RestoreSnapshotSequence()
        {
            // Walk backwards from the rewind index to the start of the window.
            // Each step re-applies a snapshot. It is intentionally conservative:
            // we restore the LAST snapshot in the valid window first.
            if (rewindIndex <= 0) return;

            int targetIndex = 0;
            for (int i = 1; i <= rewindIndex; i++)
            {
                // Last valid index before rewindIndex points at the earliest snapshot
                // we need to restore. Actually we restore all states down to it.
                targetIndex = rewindIndex - i;
                if (targetIndex < 0) break;
                RestoreSnapshot(targetIndex);
                currentTick = targetIndex;
            }

            // Reset the action records to only those valid in the new present.
            TrimActionRecordsTo(currentTick);

            // Update UI.
            if (RewindUI.Instance != null)
                RewindUI.Instance.SetRewindAvailable(hasUndo);

            isRewinding = false;
        }

        void RestoreSnapshot(int index)
        {
            if (index < 0 || index >= stateBuffer.Count) return;
            var snap = stateBuffer[index];
            if (snap.deltas == null) return;

            foreach (var d in snap.deltas)
            {
                switch (d.type)
                {
                    case EntityActionType.Moved:
                        entityManager.RestorePosition(d.entityId, d.position, d.rotation);
                        break;
                    case EntityActionType.PositionsOnly:
                        entityManager.RestorePosition(d.entityId, d.position, d.rotation);
                        break;
                    case EntityActionType.Activated:
                        entityManager.SetActivated(d.entityId, true);
                        break;
                    case EntityActionType.Deactivated:
                        entityManager.SetActivated(d.entityId, false);
                        break;
                    case EntityActionType.Destroyed:
                        entityManager.Unspawn(d.entityId);
                        break;
                    case EntityActionType.Dropped:
                        entityManager.RestorePosition(d.entityId, d.position, d.rotation);
                        break;
                    default:
                        break;
                }
            }
        }

        // Ring buffer compaction: keep only the last N valid snapshots.
        // This gives us a fast, contiguous, O(1) window for reversal.
        void CompactBuffer()
        {
            if (rewindIndex >= stateBuffer.Count) return;

            // Shift older snapshots left to keep a contiguous window.
            int windowStart = 0;
            if (rewindIndex > 0) windowStart = 1; // we keep snapshot at index 0 as the "earliest"

            int write = 0;
            for (int i = windowStart; i <= rewindIndex; i++)
            {
                if (i < stateBuffer.Count)
                    stateBuffer[write] = stateBuffer[i];
                write++;
            }

            // Trim excess.
            for (int i = write; i < stateBuffer.Count; i++) stateBuffer[i] = new StateSnapshot();

            rewindIndex = write - 1;
        }

        void ApplySnapshot(int index)
        {
            if (index >= stateBuffer.Count) return;
            var snap = stateBuffer[index];
           SnapDeltaLoop:
            foreach (var d in snap.deltas)
            {
                switch (d.type)
                {
                    case EntityActionType.Moved:
                        entityManager.RestorePosition(d.entityId, d.position, d.rotation);
                        break;
                    case EntityActionType.PositionsOnly:
                        entityManager.RestorePosition(d.entityId, d.position, d.rotation);
                        break;
                    case EntityActionType.Activated:
                        entityManager.SetActivated(d.entityId, true);
                        break;
                    case EntityActionType.Deactivated:
                        entityManager.SetActivated(d.entityId, false);
                        break;
                    case EntityActionType.Destroyed:
                        entityManager.Unspawn(d.entityId);
                        break;
                    case EntityActionType.Dropped:
                        entityManager.RestorePosition(d.entityId, d.position, d.rotation);
                        break;
                    default:
                        break;
                }
            }
        }

        void TrimActionRecordsTo(int tick)
        {
            // Keep only action records whose timestamp >= the start of the current
            // rewind window (i.e. records that are still valid after rewind).
            int keep = 0;
            for (int i = 0; i < actionRecords.Count; i++)
            {
                if (actionRecords[i].timestamp >= rewindWindowStart)
                {
                    actionRecords[keep++] = actionRecords[i];
                }
            }
            for (int i = keep; i < actionRecords.Count; i++)
                actionRecords[i] = new PlayerActionRecord();
            actionRecords.RemoveRange(keep, actionRecords.Count - keep);
        }

        public void RecordPosition(string entityId, Vector3 pos, Quaternion rot)
        {
            if (!isRecording || isRewinding) return;
            // Only record if we're near the front of the window (oldest valid).
            // We append to the buffer lazily; snapshots are flushed on rewind.
        }

        public void RecordAction(PlayerActionRecord rec)
        {
            if (actionRecords.Count >= config.maxActionRecords) return;
            actionRecords.Add(rec);
            hasUndo = true;
        }

        public void ClearAll() { stateBuffer.Clear(); }
    }
}
