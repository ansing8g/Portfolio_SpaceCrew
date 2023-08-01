
using SpaceCrewServer.Server;
using SpaceCrewServer.Game;

namespace SpaceCrewServer.Logic
{
    public static class EventLobby
    {
        public static void LeaveRoom(Lobby _lobby, RoomEventData_LeaveRoom _data)
        {
            Packet.StoC.LeaveRoomToLobby_Noti packet = new Packet.StoC.LeaveRoomToLobby_Noti(Packet.PacketDefine.PacketResult.Success);
            packet.RoomIndex = _data.RoomIndex;
            packet.LeavePlayerData.Slot = _data.Slot;
            packet.LeavePlayerData.UserIndex = _data.UserIndex;
            _lobby.ForEach((User _user) =>
            {
                _user.Send(packet);
            });
        }

        public static void RemoveRoom(Lobby _lobby, RoomEventData_RemoveRoom _data)
        {
            Packet.StoC.RemoveRoomToLobby_Noti packet = new Packet.StoC.RemoveRoomToLobby_Noti(Packet.PacketDefine.PacketResult.Success)
            {
                RoomIndex = _data.RoomIndex
            };
            _lobby.ForEach((User _user) =>
            {
                _user.Send(packet);
            });
        }

        public static void StateUpdateRoom(Lobby _lobby, RoomEventData_StateUpdateRoom _data)
        {
            Packet.StoC.StateUpdateRoomToLobby_Noti packet = new Packet.StoC.StateUpdateRoomToLobby_Noti(Packet.PacketDefine.PacketResult.Success)
            {
                RoomIndex = _data.RoomIndex,
                State = _data.State
            };
            _lobby.ForEach((User _user) =>
            {
                _user.Send(packet);
            });
        }
    }
}
