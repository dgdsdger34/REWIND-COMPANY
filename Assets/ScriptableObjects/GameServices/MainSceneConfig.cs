//-----------------------------------------------------------------------
// REWIND COMPANY — Main Scene Config
// Defines the scene graph layout and the five office rooms.
// Used by GameBoot to construct the main scene automatically.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RewindCompany.World
{
    /// <summary>
    /// A single room in the main scene.
    /// </summary>
    [CreateAssetMenu(fileName = "Room", menuName = "RewindCompany/MainScene Room")]
    public class MainSceneRoom : ScriptableObject
    {
        public string roomId;
        public string displayName;
        public GameObject roomPrefab;
        public List<GameObject> spawnObjects = new List<GameObject>();
        public List<string> recommendedTasks = new List<string>();
    }

    /// <summary>
    /// The main scene configuration. Contains all rooms and the
    /// GameBoot setup list.
    /// </summary>
    [CreateAssetMenu(fileName = "MainSceneConfig", menuName = "RewindCompany/MainScene Config")]
    public class MainSceneConfig : ScriptableObject
    {
        public List<MainSceneRoom> rooms = new List<MainSceneRoom>();

        public MainSceneRoom FindRoom(string id)
        {
            if (rooms == null) return null;
            foreach (var r in rooms)
            {
                if (r.roomId == id) return r;
            }
            return null;
        }

        public void AssignTaskTo(MainSceneRoom room, Task task)
        {
            if (room == null || task == null) return;
            if (room.recommendedTasks == null) room.recommendedTasks = new List<string>();
            if (!room.recommendedTasks.Contains(task.taskId))
            {
                room.recommendedTasks.Add(task.taskId);
            }
        }
    }

    /// <summary>
    /// Concrete main scene template (illustrative example). Designers
    /// usually build these in the scene and save as prefabs rather than
    /// as ScriptableObjects, but this shows the intended structure
    /// so you can replicate it with your own prefabs.
    /// </summary>
    public class MainSceneTemplates : ScriptableObject
    {
        public static MainSceneRoom Reception()
        {
            MainSceneRoom r = CreateInstance<MainSceneRoom>();
            r.roomId = "Reception";
            r.displayName = "Reception";
            r.roomPrefab = null; // assign real prefab in scene
            r.recommendedTasks = new List<string> { "REC-WELCOME", "CMP-PASS" };
            return r;
        }

        public static MainSceneRoom MainOffice()
        {
            MainSceneRoom r = CreateInstance<MainSceneRoom>();
            r.roomId = "MainOffice";
            r.displayName = "Main Office";
            r.roomPrefab = null;
            r.recommendedTasks = new List<string> { "FIL-FILING", "MRT-PRNT" };
            return r;
        }

        public static MainSceneRoom MeetingRoom()
        {
            MainSceneRoom r = CreateInstance<MainSceneRoom>();
            r.roomId = "MeetingRoom";
            r.displayName = "Meeting Room";
            r.roomPrefab = null;
            r.recommendedTasks = new List<string> { "MRT-PRNT" };
            return r;
        }

        public static MainSceneRoom Storage()
        {
            MainSceneRoom r = CreateInstance<MainSceneRoom>();
            r.roomId = "Storage";
            r.displayName = "Storage Room";
            r.roomPrefab = null;
            r.recommendedTasks = new List<string> { "FIL-FILING", "DSTW" };
            return r;
        }

        public static MainSceneRoom ServerRoom()
        {
            MainSceneRoom r = CreateInstance<MainSceneRoom>();
            r.roomId = "ServerRoom";
            r.displayName = "Server Room";
            r.roomPrefab = null;
            r.recommendedTasks = new List<string> { "CMP-PASS", "MON-PWN" };
            return r;
        }
    }
}
