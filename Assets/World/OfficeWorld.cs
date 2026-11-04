//-----------------------------------------------------------------------
// REWIND COMPANY — Office World module
// Room definitions, object catalog, task list, and manager dialogue.
// This is the content layer. Rooms are assemblies of GameObjects whose
// states the rewind system records. Everything below is a ScriptableObject
// you populate in the Unity Editor (Create → RewindCompany → ...).
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using RewindCompany.Systems;

namespace RewindCompany.World
{
    /// <summary>
    /// A room in the office.
    /// </summary>
    [CreateAssetMenu(fileName = "Room", menuName = "RewindCompany/Room")]
    public class Room : ScriptableObject
    {
        public string roomId;
        public string displayName;
        public string description;
        public Vector3 centerOffset;
        public GameObject sceneTemplate; // prefab opened in the scene
        public List<string> recommendedTasks = new List<string>();
    }

    /// <summary>
    /// One manager dialogue entry.
    /// </summary>
    [System.Serializable]
    public struct DialogueLine
    {
        public string speaker;
        public string text;
        public string roomId;
        public float delay;
        public bool endOfSequence;
    }

    /// <summary>
    /// Task assignment definition.
    /// </summary>
    [CreateAssetMenu(fileName = "Task", menuName = "RewindCompany/Task")]
    public class Task : ScriptableObject
    {
        public string taskId;
        public string title;
        public string description;
        public string objective;
        public string reward;
        public string rewardBadge = "+$0";
        public int difficulty = 1;
        public int requiredObjects = 0;
        public string triggerDisplay = "Middle desk";
        public bool isStory = false;
    }

    /// <summary>
    /// Fixed list of tasks the company assigns.
    /// </summary>
    [CreateAssetMenu(fileName = "TaskList", menuName = "RewindCompany/Task List")]
    public class TaskList : ScriptableObject
    {
        public List<Task> tasks = new List<Task>();
    }

    /// <summary>
    /// The company manager's scripted dialogue. Lines can be chosen
    /// dynamically depending on which room the player is in.
    /// </summary>
    [CreateAssetMenu(fileName = "ManagerScript", menuName = "RewindCompany/Manager Script")]
    public class ManagerScript : ScriptableObject
    {
        public List<DialogueLine> lines = new List<DialogueLine>();

        public DialogueLine GetNext()
        {
            if (lines.Count == 0) return new DialogueLine();
            return lines[0];
        }

        public DialogueLine GetRoomLine(string roomId)
        {
            if (lines == null) return new DialogueLine();
            foreach (var l in lines)
            {
                if (l.roomId == roomId) return l;
            }
            return new DialogueLine();
        }
    }

    /// <summary>
    /// The office object catalog: every interactive object the rewind
    /// system can record state for, per room.
    /// </summary>
    [CreateAssetMenu(fileName = "OfficeObjects", menuName = "RewindCompany/Office Objects")]
    public class OfficeObjectCatalog : ScriptableObject
    {
        public List<OfficeObject> objects = new List<OfficeObject>();

        // Helper: pull all active objects in the scene that match this catalog.
        public List<GameObject> FindActiveObjects()
        {
            List<GameObject> found = new List<GameObject>();
            if (objects == null) return found;
            foreach (var o in objects)
            {
                if (o.prefab != null)
                {
                    GameObject go = o.prefab;
                    if (go != null && go.activeInScene)
                    {
                        found.Add(go);
                    }
                }
            }
            return found;
        }
    }

    [System.Serializable]
    public struct OfficeObject
    {
        public string id;
        public string caption;
        public GameObject prefab;
        public EntityFlag flag;
        public Vector3 spawnPosition;
        public Quaternion spawnRotation;
        public bool isSavedState = true;
    }

    /// <summary>
    /// Pre-built room definitions for the five-office-level mission.
    /// Edit in the Unity Editor via Create → RewindCompany → Office World.
    /// </summary>
    [CreateAssetMenu(fileName = "OfficeWorld", menuName = "RewindCompany/Office World")]
    public class OfficeWorldContent : ScriptableObject
    {
        public List<Room> rooms = new List<Room>();

        public Room FindRoom(string id)
        {
            if (rooms == null) return null;
            foreach (var r in rooms)
            {
                if (r.roomId == id) return r;
            }
            return null;
        }

        public void AssignTaskTo(Room room, Task task)
        {
            if (room == null || task == null) return;
            if (room.recommendedTasks == null) room.recommendedTasks = new List<string>();
            if (!room.recommendedTasks.Contains(task.taskId))
            {
                room.recommendedTasks.Add(task.taskId);
            }
        }
    }

}
