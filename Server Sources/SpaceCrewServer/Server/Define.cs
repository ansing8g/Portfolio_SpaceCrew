using System.Collections.Generic;

using DBLib.File;

using Packet;

namespace SpaceCrewServer.Server
{
    public static class Define
    {
        public enum RoomEventType : byte
        {
            LeaveRoom,
            RemoveRoom,
            StateUpdateRoom,
        }

        public enum PlayerMode : uint
        {
            Player2 = 2,
            Player3 = 3,
            Normal = 4,
        }

        public enum MissionCheckType : byte
        {
            None,
            Fail,
            Success,
        }

        public const uint RoomIndexPoolCount = 1000;
        public const uint PlayMinCount = 2;
        public const uint PlayerSlotCount = (uint)PacketDefine.PlayerSlotCount;
        public const uint MissionGetCardMinCount = 4;
        public const uint MissionGetCardMaxCount = 10;
        public const uint MissionGetCardOrderCount = 4;
        public const uint SurrenderPassedPercentage = 50;

        public class ConfigData
        {
            public ConfigData()
            {
                Port = 0;
                MissionCSVFilePath = "";
                UseKeepAlive = false;
                FileDBConfig = new FileCacheDBConfig();
                UseNetworkRecvLog = false;
                UseNetworkSendLog = false;
                KeepAliveSendSecond = 0.0;
                KeepAliveWaitSecond = 0.0;
                KeepAlivePauseAddSecond = 0.0;
            }

            public uint Port;
            public string MissionCSVFilePath;
            public bool UseKeepAlive;
            public FileCacheDBConfig FileDBConfig;
            public bool UseNetworkRecvLog;
            public bool UseNetworkSendLog;
            public double KeepAliveSendSecond;
            public double KeepAliveWaitSecond;
            public double KeepAlivePauseAddSecond;
        }

        public class PacketData
        {
            public PacketData(User _user, byte[] _data)
            {
                User = _user;
                Data = _data;
            }

            public User User;
            public byte[] Data;
        }
    }

    public static class ConstData
    {
        public abstract class IMission
        {
            public IMission(PacketDefine.MissionType _type)
            {
                Type = _type;
            }

            public PacketDefine.MissionType Type { get; private set; }
        }

        public class Mission_GetCard : IMission
        {
            public Mission_GetCard()
                : base(PacketDefine.MissionType.GetCard)
            {
                GetCardCount = 0;
                ListOrderType = new List<PacketDefine.MissionCardOrderType>();
            }

            public byte GetCardCount;
            public List<PacketDefine.MissionCardOrderType> ListOrderType;
        }

        public class Mission_BadCondition : IMission
        {
            public Mission_BadCondition()
                : base(PacketDefine.MissionType.BadCondition){}
        }

    }
}
