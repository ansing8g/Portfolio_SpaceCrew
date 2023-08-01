using Packet;

using SpaceCrewServer.Game;
using SpaceCrewServer.Server;

namespace SpaceCrewServer.Logic
{
    public static class PacketMissionGetCard
    {
        public static void Mission_GetCard(User _user, Room _room, Packet.CtoS.Mission_GetCard _packet)
        {
            Mission_GetCard? mission = _room.GetMission<Mission_GetCard>();
            if (null == mission)
            {
                _user.Send(new Packet.StoC.Mission_GetCard_Noti(PacketDefine.PacketResult.Mission_GetCard_Noti_NotFoundMission));
                ServerBase.Instance.WriteLog($"Packet.CtoS.Mission_GetCard Mission Null UserIndex:{_user.UserIndex}, RoomIndex:{_room.RoomIndex}");
                return;
            }

            if (false == mission.SelectCard(_user.UserIndex, new Mission_GetCard.MissionCardInfo(_packet.SelectMissionGetCard.Card, _packet.SelectMissionGetCard.Order)))
            {
                _user.Send(new Packet.StoC.Mission_GetCard_Noti(PacketDefine.PacketResult.Mission_GetCard_Noti_Fail));
                ServerBase.Instance.WriteLog($"Packet.CtoS.Mission_GetCard SelectCard Fail UserIndex:{_user.UserIndex}, RoomIndex:{_room.RoomIndex}, Card:{_packet.SelectMissionGetCard.Card}, Order:{_packet.SelectMissionGetCard.Order}");
                return;
            }

            Packet.StoC.Mission_GetCard_Noti packetMissionGetCardNoti = new Packet.StoC.Mission_GetCard_Noti(PacketDefine.PacketResult.Success)
            {
                SelectMissionGetCard = _packet.SelectMissionGetCard,
                SelectUserIndex = _user.UserIndex
            };
            foreach (Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packetMissionGetCardNoti);
            }

            if(0 == mission.GetNotSelectedMissinoCardList.Count)
            {
                foreach (Player? player in _room.GetPlayerList)
                {
                    Packet.StoC.Mission_GetCardEnd_Noti packetMissionGetCardEndNoti = new Packet.StoC.Mission_GetCardEnd_Noti(PacketDefine.PacketResult.Success);
                    player?.User?.Send(packetMissionGetCardEndNoti);
                }
            }

            _room.UseMissionPacket();
        }
    }
}
