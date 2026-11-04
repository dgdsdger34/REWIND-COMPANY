//-----------------------------------------------------------------------
// REWIND COMPANY — Manager Dialogue System
// Reads the manager script (ScriptableObject), injects the current room
// and task context, and delivers lines over a dialogue manager stub.
// This is the "mysterious storytelling" layer.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using RewindCompany.World;
using RewindCompany.Systems;

namespace RewindCompany
{
    public class ManagerDialogue : MonoBehaviour
    {
        public static ManagerDialogue Instance { get; private set; }

        [Header("Content")]
        public ManagerScript managerScript;

        [Header("State")]
        public string currentSpeaker = "Mr. R";
        public string currentLine;
        public bool isDialogueActive;

        [Header("References")]
        public RoomManager roomManager;
        public TaskManager taskManager;

        public event EventHandler OnDialogueEnd;
        public event EventHandler OnDialogueStart;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable() { }

        /// <summary>
        /// Fetch the next line. In a full game this would be a
        /// coroutine that waits for the player to read the text.
        /// </summary>
        public DialogueLine GetNextLine()
        {
            if (managerScript == null) return new DialogueLine();

            // Inject context: room + task.
            string roomId = roomManager != null && roomManager.currentRoom != null ?
                roomManager.currentRoom.roomId : "Unknown";
            string taskId = taskManager != null && taskManager.currentTask != null ?
                taskManager.currentTask.taskId : "none";

            // Look for a line specific to this room.
            DialogueLine roomLine = managerScript.GetRoomLine(roomId);
            DialogueLine generic = new DialogueLine();

            // If we have a room-specific line use it; otherwise fall back.
            if (!string.IsNullOrEmpty(roomLine.speaker) && !string.IsNullOrEmpty(roomLine.text))
            {
                roomLine.roomId = roomId;
                roomLine.delay = Mathf.Max(roomLine.delay, 1f);
                return roomLine;
            }

            // Otherwise grab the next generic line but append task context.
            var fallback = managerScript.GetNext();
            if (!string.IsNullOrEmpty(fallback.text))
            {
                fallback.text += "\n\n[Current task: " + taskId + "]";
            }
            return fallback;
        }

        public void SetDialogueActive(bool active)
        {
            isDialogueActive = active;
            if (active)
            {
                OnDialogueStart?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                OnDialogueEnd?.Invoke(this, EventArgs.Empty);
            }
        }

        [System.Serializable]
        public struct DialogueLine
        {
            public string speaker;
            public string text;
            public string roomId;
            public float delay;
            public bool endOfSequence;
        }
    }
}
