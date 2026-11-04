//-----------------------------------------------------------------------
// REWIND COMPANY — Time Manipulation System
// Senior indie dev: clean, modular, expandable architecture.
// Author: Solo developer
// Purpose: Reliable state-recording time rewind (not teleporting).
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RewindCompany
{
    /// <summary>
    /// The kind of action an entity performed that can be undone by rewind.
    /// Kept as a flat enum so snapshots are compactly serialisable.
    /// </summary>
    public enum EntityActionType
    {
        None,
        Moved,        // Transform changed
        Dropped,      // Pickup dropped
        Activated,    // Button/fan/object activated
        Deactivated,  // Switched off
        Destroyed,    // Object destroyed
        PositionOnly, // Teleport / forced move
        Ignored       // Explicitly not recorded this frame
    }

    /// <summary>
    /// A single state change in time.
    /// One snapshot can hold several deltas (e.g. moved + activated).
    /// </summary>
    [Serializable]
    public struct StateDelta
    {
        public EntityActionType type;
        public string entityId;
        public Vector3 position;
        public Quaternion rotation;
        public bool activated;
        [SerializeField] internal bool destroyed; // avoid hide-inspector noise
        public bool isDestroyed => destroyed;

        public StateDelta(EntityActionType t, string id, Vector3 pos, Quaternion rot, bool act, bool des)
        {
            type = t;
            entityId = id;
            position = pos;
            rotation = rot;
            activated = act;
            destroyed = des;
        }
    }

    /// <summary>
    /// A full snapshot of recorded state at a point in time.
    /// Used as one "rewind tick" buffer entry.
    /// </summary>
    [Serializable]
    public class StateSnapshot
    {
        public long timestamp; // fixed-frame ticks, physics-independent
        public List<StateDelta> deltas = new List<StateDelta>();
        public int tick;

        public void AddDelta(StateDelta d)
        {
            if (deltas == null) deltas = new List<StateDelta>();
            deltas.Add(d);
        }

        public void Clear()
        {
            deltas.Clear();
        }
    }

    /// <summary>
    /// Record of a player action the system should be able to undo,
    /// including the side-effects it may have caused.
    /// </summary>
    [Serializable]
    public struct PlayerActionRecord
    {
        public long timestamp;
        public EntityActionType action;
        public string entityId;
        public Vector3 fromPosition;
        public Quaternion fromRotation;
        public bool wasActivated;
        public bool wasDestroyed;
        public List<string> createdEntities = new List<string>(); // spawned on-act activation
        public List<string> triggeredConsequences = new List<string>(); // consequence IDs

        public PlayerActionRecord(long ts, EntityActionType a, string id,
            Vector3 fromPos, Quaternion fromRot, bool wasAct, bool wasDes)
        {
            timestamp = ts;
            action = a;
            entityId = id;
            fromPosition = fromPos;
            fromRotation = fromRot;
            wasActivated = wasAct;
            wasDestroyed = wasDes;
            createdEntities.Clear();
            triggeredConsequences.Clear();
        }
    }
}
