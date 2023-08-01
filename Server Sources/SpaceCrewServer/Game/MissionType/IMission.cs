using System.Collections.Generic;

using Packet;

using SpaceCrewServer.Server;

namespace SpaceCrewServer.Game
{
    public interface IMission
    {
        public bool UseMissionPacket();
        public void Trick(uint _slot, PacketDefine.Card _card);
        public void GetTrick(uint _slot, List<PacketDefine.Card> _listTrickCard);
        public bool CommunicationToken(uint _trick, uint _slot, PacketDefine.Card _card, PacketDefine.CommunicationTokenType _type);
        public bool CommunicationTokenUseOnlyNone(uint _slot, PacketDefine.Card _card);
        public Define.MissionCheckType StageEndCheck();
        public PacketDefine.MissionType MissionType { get; }
    }
}
