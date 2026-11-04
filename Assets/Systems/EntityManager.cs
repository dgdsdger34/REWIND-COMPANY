//-----------------------------------------------------------------------
// REWIND COMPANY — Entity Management System
// Central registry for interactive objects in the office.
// Owns spawn/unspawn, state restore, activation, and consequence wiring.
// Designed so the TimeRewindSystem can restore any supported entity
// by id (reliable state-recording, not per-object hard coding).
//-----------------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;
using RewindCompany;

namespace RewindCompany.Systems
{
    /// <summary>
    /// Flags identifying how an entity participates in time rewind.
    /// </summary>
    public enum EntityFlag
    {
        None,
        Moveable,      // can be picked up and moved
        Pickup,        // lives on a held item
        Switch,        // door/button/fan
        Trigger,       // room-specific event
        Destroyable,   // explosives, breakable objects
        Machine,       // lever, elevator, computer
        Ignored        // the camera, player, particles, etc.
    };

    /// <summary>
    /// Per-entity data the rewind system understands.
    /// This is the bridge: TimeRewindSystem stores deltas here; the entity
    /// reads its own data when restoring.
    /// </summary>
    [System.Serializable]
    public struct EntityRecord
    {
        public string id;
        public EntityFlag flag;
        public Vector3 startPosition;
        public Quaternion startRotation;
        public bool isActive;
        public bool isDestroyed;
        public int spawnOrder;
    };

    public class EntityManager : MonoBehaviour
    {
        public static EntityManager Instance { get; private set; }

        public List<EntityRecord> records = new List<EntityRecord>();

        // Lookup for fast restore.
        private Dictionary<string, int> indexByEntity = new Dictionary<string, int>();

        [Header("References")]
        public RewindSystem rewind;

        public void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable() { Initialize(); }

        void Initialize()
        {
            indexByEntity.Clear();
            for (int i = 0; i < records.Count; i++)
            {
                indexByEntity[records[i].id] = i;
            }
        }

        public void Register(string id, EntityRecord rec)
        {
            records.Add(rec);
            indexByEntity[id] = records.Count - 1;
        }

        public void Unregister(string id)
        {
            int idx;
            if (!indexByEntity.TryGetValue(id, out idx)) return;
            records[idx] = records[records.Count - 1];
            if (records.Count > 1) indexByEntity[records[idx].id] = idx;
            records.RemoveAt(records.Count - 1);
            indexByEntity.Remove(id);
        }

        public bool TryGetIndex(string id, out int index)
        {
            return indexByEntity.TryGetValue(id, out index);
        }

        // ---- Restore operations called by ReqwindSystem ----

        public void RestorePosition(string id, Vector3 pos, Quaternion rot)
        {
            int idx;
            if (!TryGetIndex(id, out idx)) return;
            EntityRecord r = records[idx];
            r.startPosition = pos;
            r.startRotation = rot;
            records[idx] = r;
        }

        public void SetActivated(string id, bool active)
        {
            int idx;
            if (!TryGetIndex(id, out idx)) return;
            EntityRecord r = records[idx];
            r.isActive = active;
            records[idx] = r;
        }

        public void Unspawn(string id)
        {
            int idx;
            if (!TryGetIndex(id, out idx)) return;
            EntityRecord r = records[idx];
            r.isDestroyed = true;
            records[idx] = r;
        }

        // ---- World query helpers ----

        public EntityRecord GetRecord(string id)
        {
            int idx;
            if (!TryGetIndex(id, out idx)) return default;
            return records[idx];
        }

        public bool HasRecord(string id)
        {
            return indexByEntity.ContainsKey(id);
        }

        public int RecordCount => records.Count;
    }
}
