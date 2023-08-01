using System.Collections.Concurrent;

using SpaceCrewServer.Server;

namespace SpaceCrewServer.Game
{
    public abstract class IRoom
    {
        protected IRoom(uint _roomindex)
        {
            RoomIndex = _roomindex;
            PacketQueue = new ConcurrentQueue<Define.PacketData>();
            DisconnectQueue = new ConcurrentQueue<User>();
            EventQueue = new ConcurrentQueue<RoomEvent>();
        }

        public abstract void Update();
        public uint RoomIndex { get; set; }
        public ConcurrentQueue<Define.PacketData> PacketQueue;
        public ConcurrentQueue<User> DisconnectQueue;
        public ConcurrentQueue<RoomEvent> EventQueue;
    }
}
