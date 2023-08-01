using Packet;
using SpaceCrewServer.Server;

namespace SpaceCrewServer.Game
{
    public class RoomEvent
    {
        public RoomEvent(Define.RoomEventType _type, IRoomEventData _data)
        {
            m_type = _type;
            m_data = _data;
        }

        public bool GetData<T>(out T? _t) where T : class, IRoomEventData
        {
            _t = m_data as T;
            return null == _t ? false : true;
        }

        public Define.RoomEventType Type => m_type;

        private Define.RoomEventType m_type;
        private IRoomEventData m_data;
    }

    public interface IRoomEventData { }

    public class RoomEventData_LeaveRoom : IRoomEventData
    {
        public RoomEventData_LeaveRoom()
        {
            RoomIndex = 0;
            Slot = PacketDefine.PlayerSlot.Slot1;
            UserIndex = 0;
        }

        public uint RoomIndex;
        public PacketDefine.PlayerSlot Slot;
        public uint UserIndex;
    }

    public class RoomEventData_RemoveRoom : IRoomEventData
    {
        public RoomEventData_RemoveRoom()
        {
            RoomIndex = 0;
        }

        public uint RoomIndex;
    }

    public class RoomEventData_StateUpdateRoom : IRoomEventData
    {
        public RoomEventData_StateUpdateRoom()
        {
            RoomIndex = 0;
            State = PacketDefine.RoomState.Wait;
        }

        public uint RoomIndex;
        public PacketDefine.RoomState State;
    }
}
