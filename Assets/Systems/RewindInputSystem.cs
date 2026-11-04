//-----------------------------------------------------------------------
// REWIND COMPANY — Input Recording System
// Captures player actions into the rewind buffer so the past can be
// replayed. Actions are recorded at the moment they happen, so rewind
// restores not just positions/state but also *what happened*.
//-----------------------------------------------------------------------

using System;
using UnityEngine;
using RewindCompany;

namespace RewindCompany.Systems
{
    public class RewindInputSystem : MonoBehaviour
    {
        public static RewindInputSystem Instance { get; private set; }

        public RewindSystem rewindSystem;
        public EntityManager entityManager;

        // For recording position/rotation of the held object.
        public void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void OnEnable() { }

        void Update()
        {
            if (rewindSystem == null || rewindSystem.isRecording) return;
            if (entityManager == null) return;

            // Record pick up.
            if (Input.GetButtonDown("Pickup"))
            {
                RecordPickup();
            }

            // Record drop.
            if (Input.GetButtonDown("Drop"))
            {
                RecordDrop();
            }

            // Record activation (interact key).
            if (Input.GetButtonDown("Interact"))
            {
                RecordInteract();
            }

            // Record destroy (R + something otherwise, but handled by rewind).
        }

        void RecordPickup()
        {
            string id = "PICKUP_" + Time.frameCount;
            // In practice, reference the held object via an interaction proxy.
            // For now we record a placeholder; real code wires through PlayerController.
            PlayerActionRecord rec = new PlayerActionRecord(
                (long)(Time.unscaledTime * 1000), EntityActionType.Moved, id,
                Vector3.zero, Quaternion.identity, false, false);
            // Placeholder: only records if we have a valid held object reference.
            if (heldObjectId != null && entityManager != null && entityManager.HasRecord(heldObjectId))
            {
                rec.entityId = heldObjectId;
                rec.fromPosition = heldStartPosition;
                rec.fromRotation = heldStartRotation;
                rec.action = EntityActionType.Moved;
                rewindSystem.RecordAction(rec);
            }
            if (heldObjectId != null) heldObjectId = null;
        }

        void RecordDrop()
        {
            if (heldObjectId == null) return;
            PlayerActionRecord rec = new PlayerActionRecord(
                (long)(Time.unscaledTime * 1000), EntityActionType.Dropped, heldObjectId,
                heldStartPosition, heldStartRotation, false, false);
            rewindSystem.RecordAction(rec);
            heldObjectId = null;
        }

        void RecordInteract()
        {
            // Interaction with a target; real system reads hit target.
            // Placeholder record.
            PlayerActionRecord rec = new PlayerActionRecord(
                (long)(Time.unscaledTime * 1000), EntityActionType.Activated,
                "INTERACT_" + Time.frameCount, Vector3.zero, Quaternion.identity, true, false);
            // In real code we read the interact target and record its id.
            rewindSystem.RecordAction(rec);
        }

        string heldObjectId = null;
        Vector3 heldStartPosition;
        Quaternion heldStartRotation;

        public void OnPickup(string id, Vector3 pos, Quaternion rot)
        {
            heldObjectId = id;
            heldStartPosition = pos;
            heldStartRotation = rot;
        }

        public void OnDrop()
        {
            RecordDrop();
        }
    }
}
