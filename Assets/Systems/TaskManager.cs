//-----------------------------------------------------------------------
// REWIND COMPANY — Task Manager
// Tracks the manager's assignments: given, in progress, completed,
// and failed. Tasks can be re-framed after a rewind (the "present"
// changes, so what was "done" may no longer be done).
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using RewindCompany.World;

namespace RewindCompany.Systems
{
    /// <summary>
    /// The lifecycle of a task.
    /// </summary>
    public enum TaskState
    {
        Given,
        InProgress,
        Completed,
        Failed,
        Pending // not yet given
    }

    /// <summary>
    public class TaskManager : MonoBehaviour
    {
        public static TaskManager Instance { get; private set; }

        [Header("Content")]
        public TaskList taskList;

        [Header("State")]
        public Task currentTask;
        public TaskState currentState = TaskState.Pending;
        public List<Task> completedTasks = new List<Task>();
        public List<Task> failedTasks = new List<Task>();

        [Header("References")]
        public OfficeObjectCatalog objectCatalog;

        public event EventHandler<TaskChangedEventArgs> OnTaskChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable() { Initialize(); }

        void Initialize()
        {
            if (taskList == null)
            {
                taskList = Resources.Load<TaskList>("TaskList");
            }
            if (currentTask == null && taskList != null && taskList.tasks.Count > 0)
            {
                currentTask = taskList.tasks[0];
            }
            if (currentTask != null) currentState = TaskState.Given;
        }

        public void GiveTask()
        {
            if (currentTask == null) return;
            if (currentState == TaskState.Completed || currentState == TaskState.Failed) return;
            currentState = TaskState.Given;
            OnTaskChanged?.Invoke(this, new TaskChangedEventArgs(currentTask, currentState));
        }

        public void SetInProgress()
        {
            if (currentTask == null) return;
            currentState = TaskState.InProgress;
            OnTaskChanged?.Invoke(this, new TaskChangedEventArgs(currentTask, currentState));
        }

        public void CompleteTask()
        {
            if (currentTask == null) return;
            currentState = TaskState.Completed;
            if (!completedTasks.Contains(currentTask))
            {
                completedTasks.Add(currentTask);
            }
            currentTask = null;
            OnTaskChanged?.Invoke(this, new TaskChangedEventArgs(currentTask, currentState));
        }

        public void FailTask()
        {
            if (currentTask == null) return;
            currentState = TaskState.Failed;
            if (!failedTasks.Contains(currentTask))
            {
                failedTasks.Add(currentTask);
            }

            // Move to the next task.
            SelectNextTask();
            OnTaskChanged?.Invoke(this, new TaskChangedEventArgs(currentTask, currentState));
        }

        public void SelectNextTask()
        {
            if (taskList == null || taskList.tasks.Count == 0) return;
            int index = taskList.tasks.FindIndex(t => t == currentTask);
            if (index < 0) index = 0;

            for (int i = 0; i < taskList.tasks.Count; i++)
            {
                int candidate = (index + i) % taskList.tasks.Count;
                if (taskList.tasks[candidate] != null &&
                    taskList.tasks[candidate] != currentTask &&
                    !completedTasks.Contains(taskList.tasks[candidate]) &&
                    !failedTasks.Contains(taskList.tasks[candidate]))
                {
                    currentTask = taskList.tasks[candidate];
                    break;
                }
            }
            if (currentTask != null) currentState = TaskState.Given;
            OnTaskChanged?.Invoke(this, new TaskChangedEventArgs(currentTask, currentState));
        }

        public bool HasRequiredObjects(int count)
        {
            return objectCatalog != null && objectCatalog.objects != null &&
                   objectCatalog.objects.Count >= count;
        }

        public class TaskChangedEventArgs : EventArgs
        {
            public Task task;
            public TaskState state;
            public TaskChangedEventArgs(Task t, TaskState s)
            {
                task = t;
                state = s;
            }
        }
    }
}
