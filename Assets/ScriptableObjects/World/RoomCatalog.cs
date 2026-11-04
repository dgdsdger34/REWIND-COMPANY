//-----------------------------------------------------------------------
// REWIND COMPANY — Room Catalog
// Concrete, ready-to-wire room templates for the five-office-level
// mission: Reception, Main Office, Meeting Room, Storage Room, Server
// Room. Each room is a ScriptableObject containing an object catalog
// and its recommended tasks, so you can drop it into the scene as a
// prefab and wire it to EntityManager.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using RewindCompany.Systems;
using RewindCompany.World;

namespace RewindCompany
{
    /// <summary>
    /// A complete room template: object catalog + recommended tasks.
    /// Use these to build your actual room prefabs in the scene.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomTemplate", menuName = "RewindCompany/Room Template")]
    public class RoomTemplate : ScriptableObject
    {
        public string roomId;
        public string displayName;
        public string description;
        public GameObject roomPrefab;           // the actual prefab in the scene
        public OfficeObjectCatalog objectCatalog; // interactive objects
        public List<string> recommendedTaskIds = new List<string>();
        public RoomType roomType = RoomType.Hub;
    }

    public static class RoomTemplates
    {
        // ---------- Reception ----------
        public static RoomTemplate Reception()
        {
            RoomTemplate t = CreateInstance<RoomTemplate>();
            t.roomId = "RECEPTION";
            t.displayName = "Reception";
            t.description = "The glass lobby where the company meets. Receptionist is at the desk, a computer behind her, and a long hallway leading deeper into the building.";
            t.roomType = RoomType.Hub;
            t.recommendedTaskIds = new List<string> { "REC-WELCOME", "CMP-PASS" };

            t.objectCatalog = CreateInstance<OfficeObjectCatalog>();
            t.objectCatalog.objects = new List<OfficeObject>
            {
                new OfficeObject
                {
                    id = "RECEPTION_DESK",
                    caption = "Reception desk",
                    prefab = null, // assign in scene
                    flag = EntityFlag.Switch,
                    spawnPosition = new Vector3(0f, 0f, 0f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                },
                new OfficeObject
                {
                    id = "RECEPTION_COMPUTER",
                    caption = "Secret access computer",
                    prefab = null,
                    flag = EntityFlag.Machine,
                    spawnPosition = new Vector3(-1.5f, 0f, -2.0f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                },
                new OfficeObject
                {
                    id = "RECEPTION_PHONE",
                    caption = "Office phone",
                    prefab = null,
                    flag = EntityFlag.Switch,
                    spawnPosition = new Vector3(2.0f, 0f, 0.5f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                }
            };
            return t;
        }

        // ---------- Main Office ----------
        public static RoomTemplate MainOffice()
        {
            RoomTemplate t = CreateInstance<RoomTemplate>();
            t.roomId = "MAIN_OFFICE";
            t.displayName = "Main Office";
            t.description = "Open-plan desks and a tall window. Filing cabinet against the wall, a fan in the corner, a never-ending paper tray that jams.";
            t.roomType = RoomType.Hub;
            t.recommendedTaskIds = new List<string> { "FIL-FILING", "MRT-PRNT" };

            t.objectCatalog = CreateInstance<OfficeObjectCatalog>();
            t.objectCatalog.objects = new List<OfficeObject>
            {
                new OfficeObject
                {
                    id = "FILING_CABINET",
                    caption = "Filing cabinet",
                    prefab = null,
                    flag = EntityFlag.Moveable,
                    spawnPosition = new Vector3(1.0f, 0f, 2.5f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                },
                new OfficeObject
                {
                    id = "FAN",
                    caption = "Desk fan",
                    prefab = null,
                    flag = EntityFlag.Switch,
                    spawnPosition = new Vector3(-3.0f, 0f, 0.0f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                },
                new OfficeObject
                {
                    id = "PAPER_TRAY",
                    caption = "Paper tray (jams)",
                    prefab = null,
                    flag = EntityFlag.Breakable,
                    spawnPosition = new Vector3(0.5f, 0f, -1.5f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                }
            };
            return t;
        }

        // ---------- Meeting Room ----------
        public static RoomTemplate MeetingRoom()
        {
            RoomTemplate t = CreateInstance<RoomTemplate>();
            t.roomId = "MEETING_ROOM";
            t.displayName = "Meeting Room";
            t.description = "Boardroom with a whiteboard, a large projector screen, and a light bar above the table. The printer is on the side wall.";
            t.roomType = RoomType.Hub;
            t.recommendedTaskIds = new List<string> { "MRT-PRNT" };

            t.objectCatalog = CreateInstance<OfficeObjectCatalog>();
            t.objectCatalog.objects = new List<OfficeObject>
            {
                new OfficeObject
                {
                    id = "PROJECTOR",
                    caption = "Projector",
                    prefab = null,
                    flag = EntityFlag.Switch,
                    spawnPosition = new Vector3(0f, 0f, -3.0f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                },
                new OfficeObject
                {
                    id = "WHITEBOARD",
                    caption = "Whiteboard",
                    prefab = null,
                    flag = EntityFlag.Switch,
                    spawnPosition = new Vector3(-2.0f, 0f, 1.0f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                },
                new OfficeObject
                {
                    id = "PRINTER",
                    caption = "Side-room printer",
                    prefab = null,
                    flag = EntityFlag.Machine,
                    spawnPosition = new Vector3(3.0f, 0f, 0.0f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                }
            };
            return t;
        }

        // ---------- Storage Room ----------
        public static RoomTemplate Storage()
        {
            RoomTemplate t = CreateInstance<RoomTemplate>();
            t.roomId = "STORAGE";
            t.displayName = "Storage Room";
            t.description = "A ventilated cold room cluttered with thicker-than-usual boxes. The floor is sticky with a strange liquid after someone changes time.";
            t.roomType = RoomType.Storage;
            t.recommendedTaskIds = new List<string> { "FIL-FILING", "DSTW" };

            t.objectCatalog = CreateInstance<OfficeObjectCatalog>();
            t.objectCatalog.objects = new List<OfficeObject>
            {
                new OfficeObject
                {
                    id = "STORAGE_SHELF",
                    caption = "Storage shelf",
                    prefab = null,
                    flag = EntityFlag.Moveable,
                    spawnPosition = new Vector3(0f, 0f, -2.0f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                },
                new OfficeObject
                {
                    id = "CRATE_1",
                    caption = "Cloudy crate",
                    prefab = null,
                    flag = EntityFlag.Destroyable,
                    spawnPosition = new Vector3(2.0f, 0f, 0.0f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                },
                new OfficeObject
                {
                    id = "CRATE_2",
                    caption = "Pale crate",
                    prefab = null,
                    flag = EntityFlag.Destroyable,
                    spawnPosition = new Vector3(-2.0f, 0f, 0.0f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                }
            };
            return t;
        }

        // ---------- Server Room ----------
        public static RoomTemplate ServerRoom()
        {
            RoomTemplate t = CreateInstance<RoomTemplate>();
            t.roomId = "SERVER_ROOM";
            t.displayName = "Server Room";
            t.description = "A climate-controlled hall of blinking machines. The main server rack is dark, and the breaker panel is off. The hum of a failed machine is the only sound.";
            t.roomType = RoomType.Machine;
            t.recommendedTaskIds = new List<string> { "CMP-PASS", "MON-PWN" };

            t.objectCatalog = CreateInstance<OfficeObjectCatalog>();
            t.objectCatalog.objects = new List<OfficeObject>
            {
                new OfficeObject
                {
                    id = "SERVER_RACK",
                    caption = "Main server rack",
                    prefab = null,
                    flag = EntityFlag.Machine,
                    spawnPosition = new Vector3(0f, 0f, 0f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                },
                new OfficeObject
                {
                    id = "BREAKER_PANEL",
                    caption = "Breaker panel (off)",
                    prefab = null,
                    flag = EntityFlag.Switch,
                    spawnPosition = new Vector3(-2.0f, 0f, 1.5f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                },
                new OfficeObject
                {
                    id = "FAUCET",
                    caption = "Condensing faucet",
                    prefab = null,
                    flag = EntityFlag.Switch,
                    spawnPosition = new Vector3(2.0f, 0f, 0.5f),
                    spawnRotation = Quaternion.identity,
                    isSavedState = true
                }
            };
            return t;
        }
    }

    /// <summary>
    /// Consolidated task list that wires all tasks to their rooms.
    /// Build this in the Editor, then link it to TaskManager.
    /// </summary>
    [CreateAssetMenu(fileName = "TaskList", menuName = "RewindCompany/Task List")]
    public class RoomTaskList : ScriptableObject
    {
        public List<Task> tasks = new List<Task>();
    }

    public static class RoomTaskListBuilder
    {
        public static void Build(RoomTaskList list)
        {
            if (list.tasks == null) list.tasks = new List<Task>();

            // Welcome
            var welcome = ScriptableObject.CreateInstance<Task>();
            welcome.taskId = "REC-WELCOME";
            welcome.title = "Welcome";
            welcome.description = "Meet the receptionist.";
            welcome.objective = "Find out who you are and where you work.";
            welcome.reward = "+1 Reckoning Point";
            welcome.difficulty = 1;
            welcome.triggerDisplay = "Reception desk";
            list.tasks.Add(welcome);

            // Server access
            var pass = ScriptableObject.CreateInstance<Task>();
            pass.taskId = "CMP-PASS";
            pass.title = "Server Access";
            pass.description = "The manager panicked; the password was changed. Wait — it's 3:00 PM, why is the server cold?";
            pass.objective = "Log in to the reception computer to check the access log.";
            pass.reward = "+3 Reckoning Points";
            pass.difficulty = 3;
            pass.triggerDisplay = "Reception computer";
            list.tasks.Add(pass);

            // File filing
            var filing = ScriptableObject.CreateInstance<Task>();
            filing.taskId = "FIL-FILING";
            filing.title = "File Filing";
            filing.description = "Filing is broken. Rewind to find the missing pages.";
            filing.objective = "Sort the intake files on the main filing cabinet.";
            filing.reward = "+2 Reckoning Points";
            filing.difficulty = 2;
            filing.requiredObjects = 3;
            filing.triggerDisplay = "Main office filing cabinet";
            list.tasks.Add(filing);

            // Print in meeting room
            var print = ScriptableObject.CreateInstance<Task>();
            print.taskId = "MRT-PRNT";
            print.title = "Print";
            print.description = "A book was printed here. It's missing a page. Rewind and catch the leak.";
            print.objective = "Print a 30-page manual in the meeting room.";
            print.reward = "+1 Reckoning Point";
            print.difficulty = 1;
            print.triggerDisplay = "Meeting room printer";
            list.tasks.Add(print);

            // Server room restore
            var server = ScriptableObject.CreateInstance<Task>();
            server.taskId = "MON-PWN";
            server.title = "Server Room";
            server.description = "The server room is off. Someone needs to fix it. But the power is out. Rewind the breaker.";
            server.objective = "Restore power to the server room and log in.";
            server.reward = "+5 Reckoning Points";
            server.difficulty = 4;
            server.triggerDisplay = "Server room breaker";
            list.tasks.Add(server);

            // The climax
            var climax = ScriptableObject.CreateInstance<Task>();
            climax.taskId = "DSTW";
            climax.title = "Sustain the Present";
            climax.description = "You've rewound too many times. Things are starting to fall apart. Stabilize the day.";
            climax.objective = "Choose which future to commit to and return to the present.";
            climax.reward = "Unlock the final ending";
            climax.difficulty = 5;
            climax.isStory = true;
            climax.triggerDisplay = "All rooms";
            list.tasks.Add(climax);
        }
    }
}
