using System;
using System.Collections.Generic;
using Packet;

using SpaceCrewServer.Manager;
using SpaceCrewServer.Server;

namespace SpaceCrewServer.Game
{
    public class Mission
    {
        public Mission(Room _room)
        {
            m_room = _room;
            m_dicMission = new Dictionary<Type, IMission>();
        }

        public void GenerateMission(List<PacketDefine.Card> listDeck)
        {
            m_dicMission.Clear();
            if (false == MissionTableManager.Instance.GetMission(m_room.Mode, m_room.Stage, out List<ConstData.IMission?>? listMission) ||
                null == listMission)
            {
                Random rand = new Random(DateTime.Now.Millisecond);
                m_dicMission.Add(typeof(Mission_GetCard), new Mission_GetCard(m_room, (uint)rand.Next((int)Define.MissionGetCardMinCount, (int)Define.MissionGetCardMaxCount), listDeck, Define.MissionGetCardOrderCount));
                return;
            }

            foreach (ConstData.IMission? mission in listMission)
            {
                switch (mission?.Type)
                {
                    case PacketDefine.MissionType.GetCard:
                        {
                            ConstData.Mission_GetCard? getcard = mission as ConstData.Mission_GetCard;
                            uint count = getcard?.GetCardCount ?? 0;
                            if (0 >= count)
                            {
                                break;
                            }

                            m_dicMission.Add(typeof(Mission_GetCard), new Mission_GetCard(m_room, count, listDeck, getcard?.ListOrderType ?? new List<PacketDefine.MissionCardOrderType>()));
                        }
                        break;
                    case PacketDefine.MissionType.BadCondition:
                        {
                            m_dicMission.Add(typeof(Mission_BadCondition), new Mission_BadCondition(m_room));
                        }
                        break;
                    default:
                        {

                        }
                        break;
                }
            }
        }

        public bool UseMissionPacket()
        {
            foreach (KeyValuePair<Type, IMission> mission in m_dicMission)
            {
                if (true == mission.Value.UseMissionPacket())
                {
                    return true;
                }
            }

            return false;
        }

        public T? GetMission<T>() where T : class, IMission
        {
            if (false == m_dicMission.ContainsKey(typeof(T)))
            {
                return null;
            }

            return m_dicMission[typeof(T)] as T;
        }

        public void Trick(uint _slot, PacketDefine.Card _card)
        {
            foreach (IMission mission in m_dicMission.Values)
            {
                mission.Trick(_slot, _card);
            }
        }

        public void GetTrick(uint _slot, List<PacketDefine.Card> _listTrickCard)
        {
            foreach (IMission mission in m_dicMission.Values)
            {
                mission.GetTrick(_slot, _listTrickCard);
            }
        }

        public bool CommunicationToken(uint _trick, uint _slot, PacketDefine.Card _card, PacketDefine.CommunicationTokenType _type)
        {
            foreach (IMission mission in m_dicMission.Values)
            {
                if (false == mission.CommunicationToken(_trick, _slot, _card, _type))
                {
                    return false;
                }
            }

            return true;
        }

        public bool CommunicationTokenUseOnlyNone(uint _slot, PacketDefine.Card _card)
        {
            foreach (IMission mission in m_dicMission.Values)
            {
                if (true == mission.CommunicationTokenUseOnlyNone(_slot, _card))
                {
                    return true;
                }
            }

            return false;
        }

        public Define.MissionCheckType StageEndCheck()
        {
            Define.MissionCheckType check = Define.MissionCheckType.Success;
            foreach (IMission mission in m_dicMission.Values)
            {
                switch (mission.StageEndCheck())
                {
                    case Define.MissionCheckType.None:
                        {
                            check = Define.MissionCheckType.None;
                        }
                        break;
                    case Define.MissionCheckType.Fail:
                        {
                            return Define.MissionCheckType.Fail;
                        }
                };
            }

            return check;
        }

        public Dictionary<Type, IMission> GetMissionList => m_dicMission;

        private Room m_room;
        private Dictionary<Type, IMission> m_dicMission;
    }
}
