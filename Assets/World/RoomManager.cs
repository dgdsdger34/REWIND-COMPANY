//-----------------------------------------------------------------------
// REWIND COMPANY — Room Manager
// Bridges the static room catalog (OfficeWorld) with the dynamic
// task loop and rewind system. Holds the "mission" the current room
// is trying to complete, and exposes per-room state so the manager
// dialogue can situate itself in the present window.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using RewindCompany.Systems;

namespace RewindCompany.World
{
    /// <summary>
    /// The type of room. Controls how the manager treats it.
    /// </summary>
    public enum RoomType
    {
        Lobby,
        Hub,
        Mission,
        Storage,
        Machine
    }

    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        [Header("Content")]
        public List<Room> rooms = new List<Room>();

        [Header("State")]
        public Room currentRoom;
        public Room previousRoom;
        public RoomType currentRoomType;

        [Header("References")]
        public TaskManager taskManager;
        public ConsequencesSystem consequences;

        public event EventHandler<RoomChangedEventArgs> OnRoomChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable() { }

        public void SetRoom(Room room)
        {
            if (currentRoom == room) return;
            previousRoom = currentRoom;
            currentRoom = room;
            currentRoomType = room != null ? ResolveRoomType(room) : RoomType.Lobby;
            OnRoomChanged?.Invoke(this, new RoomChangedEventArgs(room, currentRoomType));
        }

        RoomType ResolveRoomType(Room room)
        {
            if (room == null) return RoomType.Lobby;
            if (room.recommendedTasks.Count > 0) return RoomType.Mission;
            if (room.displayName.Contains("Storage", System.StringComparison.OrdinalIgnoreCase)) return RoomType.Storage;
            if (room.displayName.Contains("Server", System.StringComparison.OrdinalIgnoreCase)) return RoomType.Machine;
            return RoomType.Hub;
        }

        public string GetManagerLine()
        {
            // Fallback line when the manager script isn't wired.
            return "Thank you for coming in, I'm Mr. R. What's your name?";
        }

        public class RoomChangedEventArgs : EventArgs
        {
            public Room room;
            public RoomType type;
            public RoomChangedEventArgs(Room r, RoomType t) { room = r; type = t; }
        }
    }
}
