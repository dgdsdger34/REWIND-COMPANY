//-----------------------------------------------------------------------
// REWIND COMPANY — Main scene wiring template
// Attach to an empty GameObject in the main scene. It wires together
// every system the time-rewind loop needs, auto-creates missing
// singletons, and seeds fallback content.
//
// A solo developer attaches this once and forgets about it. Everything
// else (rooms, tasks, manager, rewind configs) is set in the Inspector.
//-----------------------------------------------------------------------

using UnityEngine;
using RewindCompany.Systems;
using RewindCompany.World;

namespace RewindCompany
{
    public class GameBoot : MonoBehaviour
    {
        [Header("Scene references (assign in Inspector)")]
        public SceneManager sceneManager;
        public OfficeWorldContent officeWorld;
        public TaskList taskList;

        [Header("Systems (auto-created if missing)")]
        public GameObject rewindSystemObject;
        public GameObject entityManagerObject;
        public GameObject taskManagerObject;
        public GameObject roomManagerObject;
        public GameObject managerDialogueObject;
        public GameObject rewindCooldownObject;
        public GameObject rewindUIObject;

        void Awake()
        {
            // 1. Scene manager (singleton, DontDestroyOnLoad).
            if (SceneManager.Instance == null)
            {
                GameObject go = new GameObject("SceneManager");
                sceneManager = go.AddComponent<SceneManager>();
                DontDestroyOnLoad(go);
            }
            else
            {
                sceneManager = SceneManager.Instance;
            }

            // 2. Auto-create systems if missing, using singletons otherwise.
            rewindSystemObject = Ensure<RewindSystem>(ref rewindSystemObject, "RewindSystem");
            entityManagerObject = Ensure<EntityManager>(ref entityManagerObject, "EntityManager");
            taskManagerObject = Ensure<TaskManager>(ref taskManagerObject, "TaskManager");
            roomManagerObject = Ensure<RoomManager>(ref roomManagerObject, "RoomManager");
            managerDialogueObject = Ensure<ManagerDialogue>(ref managerDialogueObject, "ManagerDialogue");
            rewindCooldownObject = Ensure<RewindCooldownSystem>(ref rewindCooldownObject, "RewindCooldownSystem");
            rewindUIObject = Ensure<RewindUI>(ref rewindUIObject, "RewindUI");

            // 3. Cross-wire references.
            RewindSystem rewind = RewindSystem.Instance;
            EntityManager entity = EntityManager.Instance;
            TaskManager task = TaskManager.Instance;
            RoomManager room = RoomManager.Instance;
            ManagerDialogue dialogue = ManagerDialogue.Instance;

            rewind.entityManager = entity;
            entity.rewind = rewind;

            if (task != null) task.taskList = taskList;
            if (task != null && task.rewind != null) task.rewind = rewind;
            if (room != null) room.taskManager = task;
            if (dialogue != null) dialogue.roomManager = room;

            RewindUI ui = RewindUI.Instance;
            RewindCooldownSystem cooldown = RewindCooldownSystem.Instance;

            if (cooldown != null)
            {
                cooldown.rewind = rewind;
                cooldown.ui = ui;
            }

            // 4. Seed a fallback TaskList if the user has not provided one.
            if (taskList == null)
            {
                taskList = Resources.Load<TaskList>("TaskList");
            }
            if (taskList == null)
            {
                TaskList list = ScriptableObject.CreateInstance<TaskList>();
                Task t = ScriptableObject.CreateInstance<Task>();
                t.taskId = "WELCOME-1";
                t.title = "Welcome";
                t.description = "The company manager needs you.";
                t.objective = "Find your desk and report to reception.";
                t.reward = "+1 Reckoning Point";
                t.difficulty = 1;
                t.triggerDisplay = "Reception desk";
                list.tasks.Add(t);
                taskList = list;
            }

            if (task != null)
            {
                if (task.taskList == null) task.taskList = taskList;
                task.GiveTask();
            }

            // 5. Seed the office world content if missing.
            if (officeWorld == null)
            {
                officeWorld = ScriptableObject.CreateInstance<OfficeWorldContent>();
            }
            if (officeWorld.rooms == null)
            {
                officeWorld.rooms = new System.Collections.Generic.List<Room>();
            }

            // 6. (Optional) auto-load MainOffice on first run:
            //    SceneManager.Instance.LoadMainOffice();

            Debug.Log("[REWIND] GameBoot: all systems wired up successfully.");
        }

        /// <summary>
        /// Lazy-create a subsystem GameObject, but reuse an existing singleton if
        /// one is already in the scene (avoids duplicate singletons).
        /// </summary>
        T Ensure<T>(ref GameObject go, string name) where T : MonoBehaviour
        {
            if (go == null)
            {
                // Reuse an existing singleton if present.
                T existing = FindObjectOfType<T>();
                if (existing != null)
                {
                    Debug.Log("[REWIND] GameBoot: found existing " + typeof(T).Name + " (singleton).");
                    return existing;
                }

                go = new GameObject(name);
                T comp = go.AddComponent<T>();
                go.SetActive(true);
            }
            return go.GetComponent<T>();
        }

        // Convenience: load a named room scene after the current one finishes.
        public System.Collections.IEnumerator LoadRoom(string sceneName)
        {
            yield return SceneManager.Instance.LoadScene(sceneName);
            EntityManager em = FindObjectOfType<EntityManager>();
            if (em != null) em.Initialize();
        }
    }
}
