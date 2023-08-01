using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using SpaceCrewServer.Game;
using SpaceCrewServer.Server;

namespace SpaceCrewServer.Manager
{
    public class RoomManager : Utility.Singleton<RoomManager>
    {
        public RoomManager()
        {
            Lobby = new Lobby(ServerBase.Instance.Serializer, ServerBase.Instance.LobbyDispatcher);

            m_dicRoom = new ConcurrentDictionary<uint, Room>();
            m_queueRoomIndex = new ConcurrentQueue<uint>();
        }

        public override bool Initialize()
        {
            for (uint i = Define.RoomIndexPoolCount; 0 < i; --i)
            {
                m_queueRoomIndex.Enqueue(i);
            }

            return true;
        }

        public override void Release()
        {
            m_dicRoom.Clear();
            m_queueRoomIndex.Clear();
        }

        public bool CreateRoom(out Room? _room)
        {
            _room = null;
            if (false == m_queueRoomIndex.TryDequeue(out uint roomindex))
            {
                return false;
            }

            Room room = new Room(roomindex, ServerBase.Instance.Serializer, ServerBase.Instance.RoomDispatcher);
            if (false == m_dicRoom.TryAdd(roomindex, room))
            {
                return false;
            }

            Worker worker = ServerBase.Instance.GetWorkerFromRoomIndex(roomindex);
            worker.AddRoom(room);

            _room = room;
            return true;
        }

        public bool GetRoom(uint _roomindex, out Room? _room)
        {
            _room = null;
            if (false == m_dicRoom.ContainsKey(_roomindex))
            {
                return false;
            }

            _room = m_dicRoom[_roomindex];
            return true;
        }

        public void RemoveRoom(uint _roomindex)
        {
            Worker worker = ServerBase.Instance.GetWorkerFromRoomIndex(_roomindex);
            worker.RemoveRoom(_roomindex);

            if (false == m_dicRoom.TryRemove(_roomindex, out Room? room) ||
                null == room)
            {
                return;
            }
        }

        public void ForeachRoom(Action<Room> _func)
        {
            IEnumerator<KeyValuePair<uint, Room>> iter = m_dicRoom.GetEnumerator();
            while (iter.MoveNext())
            {
                _func(iter.Current.Value);
            }
        }

        public Lobby Lobby { get; private set; }

        private ConcurrentDictionary<uint, Room> m_dicRoom;
        private ConcurrentQueue<uint> m_queueRoomIndex;
    }
}
