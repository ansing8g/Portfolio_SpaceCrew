using System.Collections.Generic;

using Packet;

using SpaceCrewServer.Server;

namespace SpaceCrewServer.Game
{
    public class Mission_BadCondition : IMission
    {
        public Mission_BadCondition(Room _room)
        {
            m_room = _room;
            m_select = false;
            m_slot = 0;
            m_check = Define.MissionCheckType.None;
        }

        public bool UseMissionPacket()
        {
            if(true == m_select)
            {
                return false;
            }

            Packet.StoC.Mission_BadConditionStart_Noti packet = new Packet.StoC.Mission_BadConditionStart_Noti(PacketDefine.PacketResult.Success)
            {
                SelectUserIndex = m_room.TurnUserIndex
            };
            foreach(Player? player in m_room.GetPlayerList)
            {
                player?.User?.Send(packet);
            }

            return true;
        }

        public void Trick(uint _slot, PacketDefine.Card _card) { }

        public void GetTrick(uint _slot, List<PacketDefine.Card> _listTrickCard)
        {
            if(Define.MissionCheckType.Fail == m_check)
            {
                return;
            }

            m_check = m_slot == _slot ? Define.MissionCheckType.Fail : Define.MissionCheckType.None;
        }

        public bool CommunicationToken(uint _trick, uint _slot, PacketDefine.Card _card, PacketDefine.CommunicationTokenType _type) { return true; }

        public bool CommunicationTokenUseOnlyNone(uint _slot, PacketDefine.Card _card) { return false; }

        public Define.MissionCheckType StageEndCheck()
        {
            if(Define.MissionCheckType.Fail == m_check)
            {
                return Define.MissionCheckType.Fail;
            }

            foreach(Player? player in m_room.GetPlayerList)
            {
                if(null == player)
                {
                    continue;
                }

                if(0 < player.ListCard.Count)
                {
                    return Define.MissionCheckType.None;
                }
            }

            return Define.MissionCheckType.Success;
        }

        public bool SelectBadCondition(uint _userindex)
        {
            for(uint i = 0; i < m_room.GetPlayerList.Length; ++i)
            {
                if(_userindex == (m_room.GetPlayerList[i]?.UserIndex ?? 0))
                {
                    m_select = true;
                    m_slot = i;
                }
            }

            return m_select;
        }

        public PacketDefine.MissionType MissionType => PacketDefine.MissionType.BadCondition;

        private Room m_room;
        private bool m_select;
        private uint m_slot;
        private Define.MissionCheckType m_check;
    }
}
