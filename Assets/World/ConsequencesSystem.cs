//-----------------------------------------------------------------------
// REWIND COMPANY — Consequences System
// Captures unintended side-effects that arise when the rewind changes
// the present. E.g. an un-destroyed machine leaves a hazard; a rewound
// meeting person is now out of place.
// Design: consequence "drips" in from the future, triggered by the
// state differences that rewind reveals. Modular and expandable.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using RewindCompany;

namespace RewindCompany.Systems
{
    /// <summary>
    /// A consequence the player discovers.
    /// Each consequence has an id, description, and severity.
    /// </summary>
    [System.Serializable]
    public struct Consequence
    {
        public string id;
        public string title;
        public string description;
        public float severity; // 0..1
        public bool isPermanent;
        public List<string> triggeredBy; // rewind window conditions

        public Consequence(string i, string t, string d, float s, bool p)
        {
            id = i; title = t; description = d; severity = s; isPermanent = p;
            triggeredBy = new List<string>();
        }
    }

    public class ConsequencesSystem : MonoBehaviour
    {
        public static ConsequencesSystem Instance { get; private set; }

        public List<Consequence> consequences = new List<Consequence>();

        [Header("References")]
        public RewindSystem rewind;
        public EntityManager entityManager;

        // Discoveries
        public List<string> discoveredIds = new List<string>();

        public void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable() { }

        public bool IsDiscovered(string id)
        {
            return id != null && discoveredIds.Contains(id);
        }

        public void Discover(string id)
        {
            if (id == null) return;
            if (!discoveredIds.Contains(id))
            {
                discoveredIds.Add(id);
                // In a full game this would unlock narrative content.
            }
        }

        public void NotifyObjectDestroyed(string id)
        {
            // If the object is destroyed in a rewound state and is still
            // referenced, flag the consequence.
            for (int i = 0; i < consequences.Count; i++)
            {
                if (consequences[i].id == id && !IsDiscovered(id))
                {
                    Discover(id);
                }
            }
        }
    }
}
