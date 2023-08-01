using System;
using System.Collections.Generic;

using UtilityLib.ConstData.MultiValue;

using Packet;

using SpaceCrewServer.Server;

namespace SpaceCrewServer.Manager
{
    public class MissionTableManager : Utility.Singleton<MissionTableManager>
    {
        public MissionTableManager()
        {
            m_constData = new ConstDataManager<uint, ConstData.IMission>();
            m_listCard = new List<PacketDefine.Card>();

            m_hsUseMissionPacket = new HashSet<PacketDefine.MissionType>();
        }

        public override bool Initialize()
        {
            if(false == m_constData.Load_File($"{ServerBase.Instance.Config.MissionCSVFilePath}", DataParser))
            {
                ServerBase.Instance.WriteLog($"MissionTableManager Initialize Load Fail. TablePath:{ServerBase.Instance.Config.MissionCSVFilePath}");
                return false;
            }

            foreach (PacketDefine.Card card in Enum.GetValues<PacketDefine.Card>())
            {
                m_listCard.Add(card);
            }

            m_hsUseMissionPacket.Add(PacketDefine.MissionType.GetCard);

            return true;
        }

        public override void Release()
        {
            m_listCard.Clear();
        }

        public bool GetMission(Define.PlayerMode _mode, uint _stage, out List<ConstData.IMission?>? _listMission)
        {
            return m_constData.GetValue(_stage, out _listMission);
        }

        public bool IsUseMissionPacket(PacketDefine.MissionType _type)
        {
            return m_hsUseMissionPacket.Contains(_type);
        }

        private bool DataParser(string _strData, out uint _key, out ConstData.IMission? _dataType)
        {
            _key = 0;
            _dataType = null;

            string[] arrStrData = _strData.Trim(new char[] { '\r', '\n', ' ' }).Split(',');
            if(4 != arrStrData.Length)
            {
                return false;
            }

            try
            {
                _key = uint.Parse(arrStrData[0]);
                switch(Enum.Parse<PacketDefine.MissionType>(arrStrData[1]))
                {
                    case PacketDefine.MissionType.GetCard:
                        {
                            ConstData.Mission_GetCard mission = new ConstData.Mission_GetCard();
                            mission.GetCardCount = byte.Parse(arrStrData[2]);
                            foreach(string strOrder in arrStrData[3].Split('|'))
                            {
                                if(string.IsNullOrEmpty(strOrder))
                                {
                                    continue;
                                }

                                mission.ListOrderType.Add(Enum.Parse<PacketDefine.MissionCardOrderType>(strOrder));
                            }

                            _dataType = mission;
                        }
                        break;
                    case PacketDefine.MissionType.BadCondition:
                        {
                            _dataType = new ConstData.Mission_BadCondition();
                        }
                        break;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private ConstDataManager<uint, ConstData.IMission> m_constData;
        private List<PacketDefine.Card> m_listCard;

        private HashSet<PacketDefine.MissionType> m_hsUseMissionPacket;
    }
}
