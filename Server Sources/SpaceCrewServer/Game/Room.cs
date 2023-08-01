using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

using SocketLib;
using SocketLib.Dispatcher.WithoutReturn;

using Packet;

using SpaceCrewServer.Server;
using SpaceCrewServer.Manager;

namespace SpaceCrewServer.Game
{
    public partial class Room : IRoom
    {
        private class OwnerData
        {
            public OwnerData()
            {
                Interlocked.Exchange(ref m_userIndex, 0);
                Interlocked.Exchange(ref m_slot, 0);
            }

            public void Set(uint _userindex, uint _slot)
            {
                Interlocked.Exchange(ref m_userIndex, _userindex);
                Interlocked.Exchange(ref m_slot, _slot);
            }

            public uint UserIndex => m_userIndex;
            public uint Slot => m_slot;

            private volatile uint m_userIndex;
            private volatile uint m_slot;
        }

        private class TrickData
        {
            public TrickData(uint _slot, PacketDefine.Card _card, bool _isfirst)
            {
                Slot = _slot;
                Card = _card;
                IsFirst = _isfirst;
            }

            public uint Slot;
            public PacketDefine.Card Card;
            public bool IsFirst;
        }

        public Room(uint _roomindex, ISerializer _serializer, Dispatcher<User, Room, PacketDefine.PacketIndex> _dispatcher)
            : base(_roomindex)
        {
            TrickCount = 0;
            TurnSlot = 0;
            PublicState = PacketDefine.RoomState.Wait;
            Password = "";

            m_serializer = _serializer;
            m_dispatcher = _dispatcher;
            m_arrPlayer = new Player?[Define.PlayerSlotCount];
            m_mission = new(this);
            m_ownerdata = new();
            m_playerCount = 0;
            Mode = Define.PlayerMode.Normal;
            Interlocked.Exchange(ref m_stage, 0);
            m_listTrickData = new();
            m_dicTrickGetPlayer = new();
            m_updatetimer = new();

            m_privateState = (uint)State.Lobby;
            m_dicPriavteState = new();

            for (int i = 0; i < Define.PlayerSlotCount; i++)
            {
                m_arrPlayer[i] = null;
            }

            m_updatetimer.RegistFunction(PacketProcess);
            m_updatetimer.RegistFunction(EventProcess);
            m_updatetimer.RegistFunction(KeepAliveProcess, 1.0);
            m_updatetimer.RegistFunction(DisconnectProcess);
            m_updatetimer.RegistFunction(StateUpdate);

            StateInitialize();
        }

        public override void Update()
        {
            m_updatetimer.UpdateProcess();
        }

        public void SetStartStage(uint _stage)
        {
            Interlocked.Exchange(ref m_stage, _stage);
            Interlocked.Exchange(ref m_stage, 1);
        }

        public void SetPassword(string _password)
        {
            if(string.IsNullOrEmpty(_password))
            {
                return;
            }

            Password = _password;
            PublicState = PacketDefine.RoomState.Lock;
        }

        public bool AddPlayer(User _user, bool _iscreateadd, out uint _slotindex)
        {
            _slotindex = 0;

            bool isChange = false;
            for (uint i = 0; i < Define.PlayerSlotCount; ++i)
            {
                Player player = new Player(_user);
                if (null == Interlocked.CompareExchange<Player?>(ref m_arrPlayer[i], player, null))
                {
                    if (true == _iscreateadd)
                    {
                        m_ownerdata.Set(_user.UserIndex, i);
                    }

                    isChange = true;
                    _slotindex = i;
                    break;
                }
            }

            if (false == isChange)
            {
                return false;
            }

            RoomManager.Instance.Lobby.RemoveUser(_user);
            _user.SetRoom(this);

            return true;
        }

        public void RemovePlayer(User _user, out PacketDefine.PlayerSlot _slot)
        {
            _slot = PacketDefine.PlayerSlot.Slot1;

            for (uint i = 0; i < Define.PlayerSlotCount; ++i)
            {
                if (_user.UserIndex != (m_arrPlayer[i]?.UserIndex ?? 0))
                {
                    continue;
                }

                if (false == RoomManager.Instance.Lobby.AddUser(_user))
                {
                    continue;
                }

                Interlocked.Exchange(ref m_arrPlayer[i], null);

                if (m_ownerdata.UserIndex == _user.UserIndex)
                {
                    ElectOwner(i);
                }

                _slot = (PacketDefine.PlayerSlot)i;
                _user.SetRoom(RoomManager.Instance.Lobby);

                return;
            }
        }

        public void DisconnectUser(User _user)
        {
            foreach (Player? player in m_arrPlayer)
            {
                if (_user.UserIndex == (player?.UserIndex ?? 0))
                {
                    player?.SetUser(null);
                    return;
                }
            }
        }

        public bool UserEmpty()
        {
            foreach (Player? player in m_arrPlayer)
            {
                if (null != player)
                {
                    return false;
                }
            }

            return true;
        }

        public bool Destroy()
        {
            foreach (Player? player in m_arrPlayer)
            {
                if (null != (player?.User ?? null))
                {
                    return false;
                }
            }

            RoomManager.Instance.RemoveRoom(RoomIndex);
            return true;
        }

        public bool Ready(User _user, bool _isready, out PacketDefine.PlayerSlot _slot)
        {
            _slot = PacketDefine.PlayerSlot.Slot1;

            for (uint i = 0; i < m_arrPlayer.Length; ++i)
            {
                Player? player = m_arrPlayer[i];
                if (null == player ||
                    _user.UserIndex != player.UserIndex)
                {
                    continue;
                }

                player.SetReady(_isready);
                _slot = (PacketDefine.PlayerSlot)i;

                return true;
            }

            return false;
        }

        public bool Surrender(User _user, bool _passed)
        {
            for (uint i = 0; i < m_arrPlayer.Length; ++i)
            {
                Player? player = m_arrPlayer[i];
                if (null == player ||
                    _user.UserIndex != player.UserIndex)
                {
                    continue;
                }

                player.SetSurrender(_passed);

                return true;
            }

            return false;
        }

        public void Escape(User _user, out List<User> _listuser)
        {
            _listuser = new();

            for (uint i = 0; i < Define.PlayerSlotCount; ++i)
            {
                if (null == m_arrPlayer[i] ||
                    null == m_arrPlayer[i]!.User)
                {
                    continue;
                }

                User nowUser = m_arrPlayer[i]!.User!;

                Interlocked.Exchange(ref m_arrPlayer[i], null);

                if (false == RoomManager.Instance.Lobby.AddUser(nowUser))
                {
                    nowUser.Disconnect();
                }

                nowUser.SetRoom(RoomManager.Instance.Lobby);

                _listuser.Add(nowUser);
            }
        }

        public bool NextStageVote(User _user, bool _passed)
        {
            if(State.StageSuccessEnd != m_privateState)
            {
                return false;
            }

            foreach(Player? player in m_arrPlayer)
            {
                if(null != player &&
                    _user.UserIndex == player.UserIndex)
                {
                    player.SetReady(_passed);
                    return true;
                }
            }

            return false;
        }

        public bool RePlayVote(User _user, bool _passed)
        {
            if (State.StageFailEnd != m_privateState)
            {
                return false;
            }

            foreach (Player? player in m_arrPlayer)
            {
                if (null != player &&
                    _user.UserIndex == player.UserIndex)
                {
                    player.SetReady(_passed);
                    return true;
                }
            }

            return false;
        }

        public void Start()
        {
            StateChange(State.Start);
        }

        public void ReStart()
        {
            StateChange(State.Start);
        }

        public void UseMissionPacket()
        {
            if (true == m_mission.UseMissionPacket())
            {
                return;
            }

            StateChange(State.Stage);
        }

        public bool ElectMode()
        {
            if(false == IsAllReadyVote(out uint passedcount, out uint rejectedcount) ||
                0 < rejectedcount)
            {
                return false;
            }

            if ((uint)Define.PlayerMode.Normal <= passedcount)
            {
                Mode = Define.PlayerMode.Normal;
            }
            else if ((uint)Define.PlayerMode.Player3 == passedcount)
            {
                //m_mode = Define.PlayerMode.Player3;
                return false;
            }
            else if ((uint)Define.PlayerMode.Player2 == passedcount)
            {
                //m_mode = Define.PlayerMode.Player2;
                return false;
            }
            else
            {
                return false;
            }

            m_playerCount = passedcount;
            return true;
        }

        public List<PacketDefine.MissionType> GetMissionTypeList()
        {
            List<PacketDefine.MissionType> listMissionType = new List<PacketDefine.MissionType>();
            foreach (KeyValuePair<Type, IMission> mission in m_mission.GetMissionList)
            {
                if (null == mission.Value)
                {
                    continue;
                }

                listMissionType.Add(mission.Value.MissionType);
            }

            return listMissionType;
        }

        public T? GetMission<T>() where T : class, IMission
        {
            return m_mission.GetMission<T>();
        }

        public bool Trick(uint _userindex, PacketDefine.Card _card, out PacketDefine.PacketResult _result)
        {
            _result = PacketDefine.PacketResult.Fail;

            if (false == GetPlayer(_userindex, out uint slot, out Player? player) ||
                null == player)
            {
                _result = PacketDefine.PacketResult.Trick_Noti_Fail;
                return false;
            }

            int cardindex = player.ListCard.FindIndex(i => i == _card);
            if (cardindex < 0)
            {
                _result = PacketDefine.PacketResult.Trick_Noti_NotFoundCard;
                return false;
            }

            int index = m_listTrickData.FindIndex(i => i.IsFirst);
            if(0 <= index)
            {
                PacketDefine.CardType firstCardType = PacketDefine.GetCardType(m_listTrickData[index].Card);
                PacketDefine.CardType trickCardType = PacketDefine.GetCardType(_card);
                if(firstCardType != trickCardType)
                {
                    int count = player.ListCard.Where(i => firstCardType == PacketDefine.GetCardType(i)).Count();
                    if(0 < count)
                    {
                        _result = PacketDefine.PacketResult.Trick_Noti_CantTrickCard;
                        return false;
                    }
                }
            }    

            m_mission.Trick(slot, _card);

            player.ListCard.RemoveAt(cardindex);
            player.PlayTrick.IsPlay = true;
            player.PlayTrick.Card = _card;
            m_listTrickData.Add(new TrickData(slot, _card, 0 == m_listTrickData.Count));

            uint turnslot = (TurnSlot + 1) % (uint)m_arrPlayer.Length;
            for (int i = 0; i < m_arrPlayer.Length; ++i)
            {
                Player? turnPlayer = m_arrPlayer[turnslot];
                if (null != turnPlayer)
                {
                    if (0 == turnPlayer.ListCard.Count)
                    {
                        turnPlayer.PlayTrick.IsPlay = true;
                    }
                    else
                    {
                        TurnSlot = turnslot;
                        break;
                    }
                }

                turnslot = (turnslot + 1) % (uint)m_arrPlayer.Length;
            }

            return true;
        }

        public bool CommunicationToken(uint _userindex, PacketDefine.Card _card, PacketDefine.CommunicationTokenType _type, out PacketDefine.PacketResult _result)
        {
            _result = PacketDefine.PacketResult.Success;

            if (false == GetPlayer(_userindex, out uint slot, out Player? player) ||
                null == player)
            {
                _result = PacketDefine.PacketResult.CommunicationToken_Noti_Fail;
                return false;
            }

            int index = player.ListCard.FindIndex(i => i == _card);
            if (-1 == index)
            {
                _result = PacketDefine.PacketResult.CommunicationToken_Noti_NotFoundCard;
                return false;
            }

            if(PacketDefine.CardType.Rocket == PacketDefine.GetCardType(_card))
            {
                _result = PacketDefine.PacketResult.CommunicationToken_Noti_RocketType;
                return false;
            }

            if (true == player.CommunicationToken.IsUse)
            {
                _result = PacketDefine.PacketResult.CommunicationToken_Noti_Used;
                return false;
            }

            if (0 < m_listTrickData.Count)
            {
                _result = PacketDefine.PacketResult.CommunicationToken_Noti_CantTiming;
                return false;
            }

            if (false == m_mission.CommunicationToken(TrickCount, slot, _card, _type))
            {
                _result = PacketDefine.PacketResult.CommunicationToken_Noti_MissionFail;
                return false;
            }

            if(true == m_mission.CommunicationTokenUseOnlyNone(slot, _card))
            {
                if(PacketDefine.CommunicationTokenType.None != _type)
                {
                    _result = PacketDefine.PacketResult.CommunicationToken_Noti_NotNone;
                    return false;
                }
            }
            else
            {
                List<PacketDefine.Card> listCard = player.ListCard.Where(i => PacketDefine.GetCardType(i) == PacketDefine.GetCardType(_card)).OrderBy(i => i).ToList();
                if(PacketDefine.CommunicationTokenType.Only == _type)
                {
                    if(1 != listCard.Count)
                    {
                        _result = PacketDefine.PacketResult.CommunicationToken_Noti_NotOnly;
                        return false;
                    }
                }
                else if(PacketDefine.CommunicationTokenType.Top == _type)
                {
                    if (listCard[listCard.Count - 1] != _card)
                    {
                        _result = PacketDefine.PacketResult.CommunicationToken_Noti_NotTop;
                        return false;
                    }
                }
                else if(PacketDefine.CommunicationTokenType.Bottom == _type)
                {
                    if (listCard[0] != _card)
                    {
                        _result = PacketDefine.PacketResult.CommunicationToken_Noti_NotBottom;
                        return false;
                    }
                }
                else
                {
                    _result = PacketDefine.PacketResult.CommunicationToken_Noti_Fail;
                    return false;
                }
            }

            player.CommunicationToken.Card = _card;
            player.CommunicationToken.Type = _type;
            player.CommunicationToken.IsUse = true;

            return true;
        }

        public bool CheckTrickEnd()
        {
            foreach(Player? player in m_arrPlayer)
            {
                if(null == player)
                {
                    continue;
                }

                if(false == player.PlayTrick.IsPlay)
                {
                    return false;
                }
            }

            StateChange(State.TrickEnd);

            if (true == CheckStageEnd())
            {
                return true;
            }

            StateChange(State.Trick);
            return true;
        }

        public bool CheckStageEnd()
        {
            switch (m_mission.StageEndCheck())
            {
                case Define.MissionCheckType.Fail:
                    {
                        StateChange(State.StageFailEnd);
                        return true;
                    }
                case Define.MissionCheckType.Success:
                    {
                        StateChange(State.StageSuccessEnd);
                        return true;
                    }
            }

            return false;
        }

        public bool IsAllSurrenderVote(out uint _passedcount, out uint _rejectedcount)
        {
            _passedcount = 0;
            _rejectedcount = 0;

            bool isAllVote = true;
            foreach (Player? player in m_arrPlayer)
            {
                if (null == player)
                {
                    continue;
                }

                if (player.IsVoteSurrender)
                {
                    _passedcount += (player.IsSurrender ? (uint)1 : 0);
                    _rejectedcount += (player.IsSurrender ? 0 : (uint)1);
                }
                else
                {
                    isAllVote = false;
                }                
            }

            return isAllVote;
        }

        public bool IsAllReadyVote(out uint _passedcount, out uint _rejectedcount)
        {
            _passedcount = 0;
            _rejectedcount = 0;

            bool isAllVote = true;
            foreach (Player? player in m_arrPlayer)
            {
                if (null == player)
                {
                    continue;
                }

                if (player.IsVoteReady)
                {
                    _passedcount += (player.IsReady ? (uint)1 : 0);
                    _rejectedcount += (player.IsReady ? 0 : (uint)1);
                }
                else
                {
                    isAllVote = false;
                }
            }

            return isAllVote;
        }

        private void PacketProcess()
        {
            LinkedList<Define.PacketData> llpacket = new LinkedList<Define.PacketData>();
            while (PacketQueue.TryDequeue(out Define.PacketData? _data))
            {
                llpacket.AddLast(_data);
            }

            foreach (Define.PacketData packetdata in llpacket)
            {
                PacketBase<PacketDefine.PacketIndex>? packet_base = null;
                if (false == m_serializer.ToPacketBase(packetdata.Data, out packet_base) ||
                    null == packet_base)
                {
                    continue;
                }

                FunctionBase<User, Room, PacketDefine.PacketIndex>? func_handler = null;
                Type? packet_type = null;
                if (false == m_dispatcher.GetFunction(packet_base.PacketIndex, out func_handler, out packet_type) ||
                    null == func_handler ||
                    null == packet_type)
                {
                    continue;
                }

                PacketBase<PacketDefine.PacketIndex>? packet = null;
                if (false == m_serializer.ToPacket(packetdata.Data, packet_type, out packet) ||
                    null == packet)
                {
                    continue;
                }

                if (ServerBase.Instance.Config.UseNetworkRecvLog)
                {
                    if (PacketDefine.PacketIndex.CtoS_KeepAlive != packet.PacketIndex &&
                        PacketDefine.PacketIndex.CtoS_OnApplicationPause != packet.PacketIndex)
                    {
                        ServerBase.Instance.WriteLog($"Recv PacketIndex:{packet.PacketIndex.ToString()}, Packet:{Newtonsoft.Json.JsonConvert.SerializeObject(packet)}");
                    }
                }

                func_handler.ExecuteFunction(packetdata.User, this, packet);
            }
        }

        private void EventProcess()
        {

        }

        private void KeepAliveProcess()
        {
            if (false == ServerBase.Instance.Config.UseKeepAlive)
            {
                return;
            }

            foreach (Player? player in m_arrPlayer)
            {
                if(null == player ||
                    null == player.User)
                {
                    continue;
                }

                DateTime recvTime = new DateTime(player.User.KeepAliveTime);
                if (recvTime.AddSeconds(ServerBase.Instance.Config.KeepAliveWaitSecond) <= DateTime.Now)
                {
                    player.User.Disconnect();
                    continue;
                }

                Packet.StoC.KeepAlive packet = new Packet.StoC.KeepAlive(PacketDefine.PacketResult.Success);
                if (recvTime.AddSeconds(ServerBase.Instance.Config.KeepAliveSendSecond) <= DateTime.Now)
                {
                    player.User.Send(packet);
                }
            }
        }

        private void DisconnectProcess()
        {
            while (DisconnectQueue.TryDequeue(out User? user))
            {
                DisconnectUser(user);
                Destroy();
            }
        }

        private void ElectOwner(uint _offset)
        {
            if (true == Destroy())
            {
                return;
            }

            uint slot = m_ownerdata.Slot;
            for (uint i = 0; i < m_arrPlayer.Length; ++i)
            {
                uint userindex = m_arrPlayer[slot]?.User?.UserIndex ?? 0;
                if (0 != userindex)
                {
                    m_ownerdata.Set(userindex, slot);
                }

                slot = (slot + 1) % (uint)PacketDefine.PlayerSlot.Max;
            }
        }

        private void DistributionCard(List<PacketDefine.Card> _listDeck)
        {
            LinkedList<uint> llSlot = new LinkedList<uint>();
            for(uint i = 0; i < m_arrPlayer.Length; ++i)
            {
                if (null == m_arrPlayer[i])
                {
                    continue;
                }

                llSlot.AddLast(i);
            }

            LinkedListNode<uint>? node = llSlot.First;
            foreach (PacketDefine.Card card in _listDeck.OrderBy(i => Guid.NewGuid()))
            {
                if (PacketDefine.Card.Rocket4 == card)
                {
                    TurnSlot = node!.Value;
                }

                m_arrPlayer[node!.Value]!.ListCard.Add(card);
                node = node.Next ?? llSlot.First;
            }
        }

        private bool GetPlayer(uint _userindex, out uint _slot, out Player? _player)
        {
            for(uint i = 0; i < m_arrPlayer.Length; ++i)
            {
                Player? player = m_arrPlayer[i];
                if (_userindex == (player?.UserIndex ?? 0))
                {
                    _slot = i;
                    _player = player;
                    return true;
                }
            }

            _slot = 0;
            _player = null;
            return false;
        }

        private void ProcessTrickWinner(out uint _slot, out List<PacketDefine.Card> _listCard)
        {
            Func<PacketDefine.Card, PacketDefine.CardType> FuncGetCardType = (PacketDefine.Card _card) => { return (PacketDefine.CardType)((ushort)_card >> 8); };
            Func<PacketDefine.Card, int> FuncGetCardValue = (PacketDefine.Card _card) => { return ((ushort)_card & 0x00FF); };

            TrickData winner = m_listTrickData[0];
            for (int i = 1; i < m_listTrickData.Count; ++i)
            {
                PacketDefine.CardType winnerCardType = FuncGetCardType(winner.Card);
                PacketDefine.CardType cardtype = FuncGetCardType(m_listTrickData[i].Card);
                if(winnerCardType == cardtype)
                {
                    int winnerCardValue = FuncGetCardValue(winner.Card);
                    int cardvalue = FuncGetCardValue(m_listTrickData[i].Card);

                    if(winnerCardValue < cardvalue)
                    {
                        winner = m_listTrickData[i];
                    }
                }
                else if(PacketDefine.CardType.Rocket == cardtype)
                {
                    winner = m_listTrickData[i];
                }
            }

            List<PacketDefine.Card> listTrickCard = new List<PacketDefine.Card>();
            foreach (TrickData trickdata in m_listTrickData)
            {
                listTrickCard.Add(trickdata.Card);
            }

            m_arrPlayer[winner.Slot]!.DicTrickCard.Add(TrickCount, listTrickCard);
            m_dicTrickGetPlayer.Add(TrickCount, m_arrPlayer[winner.Slot]!);

            _slot = winner.Slot;
            _listCard = listTrickCard;
        }

        public uint Stage => m_stage;
        public Player?[] GetPlayerList => m_arrPlayer;
        public uint OwnerIndex => m_ownerdata.UserIndex;
        public uint TurnUserIndex => m_arrPlayer[TurnSlot]?.UserIndex ?? 0;

        public Define.PlayerMode Mode { get; private set; }
        public uint TurnSlot { get; private set; }
        public uint TrickCount { get; private set; }
        public PacketDefine.RoomState PublicState { get; private set; }
        public string Password { get; private set; }

        private ISerializer m_serializer;
        private Dispatcher<User, Room, PacketDefine.PacketIndex> m_dispatcher;
        private volatile Player?[] m_arrPlayer;
        private Mission m_mission;
        private OwnerData m_ownerdata;
        private uint m_playerCount;
        private volatile uint m_stage;
        private List<TrickData> m_listTrickData;
        private Dictionary<uint, Player?> m_dicTrickGetPlayer;   //--> key : trick
        private Utility.UpdateTimer m_updatetimer;
    }
}
