
using SpaceCrewServer.Server;
using SpaceCrewServer.Game;

namespace SpaceCrewServer.Logic
{
    public static class PacketMissionBadCondition
    {
        public static void Mission_BadCondition(User _user, Room _room, Packet.CtoS.Mission_BadCondition _packet)
        {
            Mission_BadCondition? mission = _room.GetMission<Mission_BadCondition>();
            if(null == mission)
            {
                _user.Send(new Packet.StoC.Mission_BadCondition_Noti(Packet.PacketDefine.PacketResult.Mission_BadCondition_Noti_NotFoundMission));
                ServerBase.Instance.WriteLog($"Packet.CtoS.Mission_BadCondition Mission Null UserIndex:{_user.UserIndex}, RoomIndex:{_room.RoomIndex}");
                return;
            }

            if(false == mission.SelectBadCondition(_packet.SelectUserIndex))
            {
                _user.Send(new Packet.StoC.Mission_BadCondition_Noti(Packet.PacketDefine.PacketResult.Mission_BadCondition_Noti_NotFoundMission));
                ServerBase.Instance.WriteLog($"Packet.CtoS.Mission_BadCondition Mission Null UserIndex:{_user.UserIndex}, RoomIndex:{_room.RoomIndex}, SelectUserIndex:{_packet.SelectUserIndex}");
                return;
            }

            Packet.StoC.Mission_BadCondition_Noti packet = new Packet.StoC.Mission_BadCondition_Noti(Packet.PacketDefine.PacketResult.Success)
            {
                SelectUserIndex = _packet.SelectUserIndex
            };
            foreach(Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packet);
            }

            _room.UseMissionPacket();
        }
    }
}
