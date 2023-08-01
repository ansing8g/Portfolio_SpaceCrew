using System.Collections.Generic;
using System.Threading;

using Packet;

using SpaceCrewServer.Server;

namespace SpaceCrewServer.Game
{
    public class Player
    {
        public class CommunicationTokenData
        {
            public CommunicationTokenData()
            {
                Clear();
            }

            public void Clear()
            {
                IsUse = false;
                Card = PacketDefine.Card.Rocket4;
                Type = PacketDefine.CommunicationTokenType.None;
            }

            public bool IsUse;
            public PacketDefine.Card Card;
            public PacketDefine.CommunicationTokenType Type;
        }

        public class PlayTrickData
        {
            public PlayTrickData()
            {
                IsPlay = false;
                Card = PacketDefine.Card.Blue1;
            }

            public bool IsPlay;
            public PacketDefine.Card Card;
        }

        public Player(User _user)
        {
            Interlocked.Exchange(ref m_user, _user);
            Interlocked.Exchange(ref m_userindex, _user.UserIndex);
            Interlocked.Exchange(ref m_isReady, 0);
            Interlocked.Exchange(ref m_isSurrender, 0);
            m_listCard = new();
            m_dicTrickCard = new();
            m_communicationToken = new();
            m_playTrick = new();
        }

        public void UserDisconnect()
        {
            Interlocked.Exchange(ref m_user, null);
        }

        public void SetUser(User? _user)
        {
            Interlocked.Exchange(ref m_user, _user);
        }

        public void InitializeReady()
        {
            Interlocked.Exchange(ref m_isReady, 0);
        }

        public void SetReady(bool _isReady)
        {
            Interlocked.Exchange(ref m_isReady, (uint)(_isReady ? 1 : 2));
        }

        public void InitializeSurrender()
        {
            Interlocked.Exchange(ref m_isSurrender, 0);
        }

        public void SetSurrender(bool _isSurrender)
        {
            Interlocked.Exchange(ref m_isSurrender, (uint)(_isSurrender ? 1 : 2));
        }

        public User? User => m_user;
        public uint UserIndex => m_userindex;
        public bool IsVoteReady => 0 != m_isReady;
        public bool IsReady => 1 == m_isReady;
        public bool IsVoteSurrender => 0 != m_isSurrender;
        public bool IsSurrender => 1 == m_isSurrender;
        public List<PacketDefine.Card> ListCard => m_listCard;
        public Dictionary<uint, List<PacketDefine.Card>> DicTrickCard => m_dicTrickCard;
        public CommunicationTokenData CommunicationToken => m_communicationToken;
        public PlayTrickData PlayTrick => m_playTrick;

        private volatile User? m_user;
        private volatile uint m_userindex;
        private volatile uint m_isReady;        // 0 : Not Vote, 1 : Ready, 2 : Not Ready
        private volatile uint m_isSurrender;    // 0 : Not Vote, 1 : Surrender, 2 : Not Surrender
        private List<PacketDefine.Card> m_listCard;
        private Dictionary<uint, List<PacketDefine.Card>> m_dicTrickCard;  //--> key : trick
        private CommunicationTokenData m_communicationToken;
        private PlayTrickData m_playTrick;

    }
}
