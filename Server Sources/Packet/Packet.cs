using System.Collections.Generic;

using SocketLib;

namespace Packet
{
    public static class PacketDefine
    {
        public enum PacketIndex
        {
            CtoS_Enter,
            CtoS_LobbyRoomList,
            CtoS_CreateRoom,
            CtoS_EnterRoom,
            CtoS_LeaveRoom,
            CtoS_Ready,
            CtoS_Mission_GetCard,
            CtoS_Mission_BadCondition,
            CtoS_Trick,
            CtoS_CommunicationToken,
            CtoS_Emoticon,
            CtoS_SurrenderVote,
            CtoS_Escape,
            CtoS_NextStageVote,
            CtoS_RePlayVote,
            CtoS_KeepAlive,
            CtoS_OnApplicationPause,

            CtoS_TestEnd,

            StoC_Enter,
            StoC_LobbyRoomList,
            StoC_CreateRoom,
            StoC_CreateRoomToLobby_Noti,
            StoC_RemoveRoomToLobby_Noti,
            StoC_EnterRoom,
            StoC_EnterRoom_Noti,
            StoC_EnterRoomToLobby_Noti,
            StoC_LeaveRoom,
            StoC_LeaveRoom_Noti,
            StoC_LeaveRoomToLobby_Noti,
            StoC_StateUpdateRoomToLobby_Noti,
            StoC_Ready_Noti,
            StoC_Start_Noti,
            StoC_Mission_GetCardStart_Noti,
            StoC_Mission_GetCard_Noti,
            StoC_Mission_GetCardEnd_Noti,
            StoC_Mission_BadConditionStart_Noti,
            StoC_Mission_BadCondition_Noti,
            StoC_TrickStart_Noti,
            StoC_Trick_Noti,
            StoC_TrickEnd_Noti,
            StoC_CommunicationToken_Noti,
            StoC_Emoticon_Noti,
            StoC_End_Noti,
            StoC_SurrenderVote_Noti,
            StoC_SurrenderVoteResult_Noti,
            StoC_Escape_Noti,
            StoC_NextStageVote_Noti,
            StoC_NextStageVoteResult_Noti,
            StoC_RePlayVote_Noti,
            StoC_RePlayVoteResult_Noti,
            StoC_KeepAlive,
        }

        public enum PacketResult
        {
            Fail,
            Success,
            Enter_DBFail,
            CreateRoom_CreateFail,
            CreateRoom_AddFail,
            EnterRoom_GetFail,
            EnterRoom_NotMatchPassword,
            EnterRoom_AddFail,
            Ready_Noti_RoomGaming,
            Ready_Noti_NotFoundPlayer,
            Mission_GetCard_Noti_NotFoundMission,
            Mission_GetCard_Noti_Fail,
            Mission_BadCondition_Noti_NotFoundMission,
            Mission_BadCondition_Noti_Fail,
            Trick_Noti_Fail,
            Trick_Noti_NotFoundCard,
            Trick_Noti_CantTrickCard,
            CommunicationToken_Noti_Fail,
            CommunicationToken_Noti_NotFoundCard,
            CommunicationToken_Noti_Used,
            CommunicationToken_Noti_RocketType,
            CommunicationToken_Noti_CantTiming,
            CommunicationToken_Noti_MissionFail,
            CommunicationToken_Noti_NotNone,
            CommunicationToken_Noti_NotOnly,
            CommunicationToken_Noti_NotTop,
            CommunicationToken_Noti_NotBottom,
            SurrenderVote_Noti_SurrenderFail,
            Escape_Noti_EscapeFail,
            NextStageVote_Noti_NextStageVoteFail,
            RePlayVote_Noti_RePlayVoteFail,
        }

        public enum RoomState : byte
        {
            Wait,
            Lock,
            Gaming,
        }

        public enum PlayerSlot : byte
        {
            Slot1 = 0,
            Slot2,
            Slot3,
            Slot4,
            Slot5,
            Max
        }

        public enum CardType : byte
        {
            Blue,
            Red,
            Yellow,
            Green,
            Rocket,
        }

        public enum Card : ushort
        {
            Blue1       = CardType.Blue << 8 | 1,
            Blue2       = CardType.Blue << 8 | 2,
            Blue3       = CardType.Blue << 8 | 3,
            Blue4       = CardType.Blue << 8 | 4,
            Blue5       = CardType.Blue << 8 | 5,
            Blue6       = CardType.Blue << 8 | 6,
            Blue7       = CardType.Blue << 8 | 7,
            Blue8       = CardType.Blue << 8 | 8,
            Blue9       = CardType.Blue << 8 | 9,
            Red1        = CardType.Red << 8 | 1,
            Red2        = CardType.Red << 8 | 2,
            Red3        = CardType.Red << 8 | 3,
            Red4        = CardType.Red << 8 | 4,
            Red5        = CardType.Red << 8 | 5,
            Red6        = CardType.Red << 8 | 6,
            Red7        = CardType.Red << 8 | 7,
            Red8        = CardType.Red << 8 | 8,
            Red9        = CardType.Red << 8 | 9,
            Yellow1     = CardType.Yellow << 8 | 1,
            Yellow2     = CardType.Yellow << 8 | 2,
            Yellow3     = CardType.Yellow << 8 | 3,
            Yellow4     = CardType.Yellow << 8 | 4,
            Yellow5     = CardType.Yellow << 8 | 5,
            Yellow6     = CardType.Yellow << 8 | 6,
            Yellow7     = CardType.Yellow << 8 | 7,
            Yellow8     = CardType.Yellow << 8 | 8,
            Yellow9     = CardType.Yellow << 8 | 9,
            Green1      = CardType.Green << 8 | 1,
            Green2      = CardType.Green << 8 | 2,
            Green3      = CardType.Green << 8 | 3,
            Green4      = CardType.Green << 8 | 4,
            Green5      = CardType.Green << 8 | 5,
            Green6      = CardType.Green << 8 | 6,
            Green7      = CardType.Green << 8 | 7,
            Green8      = CardType.Green << 8 | 8,
            Green9      = CardType.Green << 8 | 9,
            Rocket1     = CardType.Rocket << 8 | 1,
            Rocket2     = CardType.Rocket << 8 | 2,
            Rocket3     = CardType.Rocket << 8 | 3,
            Rocket4     = CardType.Rocket << 8 | 4,
        }

        public enum MissionType : byte
        {
            GetCard,
            BadCondition,
        }

        public enum MissionCardOrderType : byte
        {
            None = 0,
            _1 = 1,
            _2 = 2,
            _3 = 3,
            _4 = 4,
            _5 = 5,
            Ⅰ = 10,
            Ⅱ = 20,
            Ⅲ = 30,
            Ⅳ = 40,
            Ω
        }

        public enum CommunicationTokenType : byte
        {
            None,
            Top,
            Only,
            Bottom
        }

        public const int PlayerSlotCount = (int)PlayerSlot.Max;

        public static PacketDefine.CardType GetCardType(PacketDefine.Card _card) { return (PacketDefine.CardType)((ushort)_card >> 8); }
        public static ushort GetCardNumber(PacketDefine.Card _card) { return (ushort)((ushort)_card & 0x00FF); }

        public class RoomPlayerData
        {
            public RoomPlayerData()
            {
                Slot = PacketDefine.PlayerSlot.Slot1;
                UserIndex = 0;
                IsReady = false;
            }

            public PacketDefine.PlayerSlot Slot;
            public uint UserIndex;
            public bool IsReady;
        }

        public class MissionData
        {
            public MissionType Type;
            public Card Card;
            public MissionCardOrderType Order;
        }

        public class MissionGetCardData
        {
            public MissionGetCardData()
            {
                Card = PacketDefine.Card.Blue1;
                Order = PacketDefine.MissionCardOrderType.None;
            }

            public PacketDefine.Card Card;
            public PacketDefine.MissionCardOrderType Order;
        }
    }

    public class PacketCommon : PacketBase<PacketDefine.PacketIndex>
    {
        protected PacketCommon(PacketDefine.PacketIndex _index)
            : base(_index)
        {
        }
    }

    namespace CtoS
    {
        public class PacketCommonCtoS : PacketCommon
        {
            protected PacketCommonCtoS(PacketDefine.PacketIndex _index)
                : base(_index)
            {
            }
        }


        public class Enter : PacketCommonCtoS
        {
            public Enter()
                : base(PacketDefine.PacketIndex.CtoS_Enter)
            {
                ID = "";
            }

            public string ID;
        }

        public class LobbyRoomList : PacketCommonCtoS
        {
            public LobbyRoomList()
                : base(PacketDefine.PacketIndex.CtoS_LobbyRoomList)
            {
            }
        }

        public class CreateRoom : PacketCommonCtoS
        {
            public CreateRoom()
                : base(PacketDefine.PacketIndex.CtoS_CreateRoom)
            {
                StartStage = 1;
                Password = "";
            }

            public uint StartStage;
            public string Password;
        }

        public class EnterRoom : PacketCommonCtoS
        {
            public EnterRoom()
                : base(PacketDefine.PacketIndex.CtoS_EnterRoom)
            {
                RoomIndex = 0;
                Password = "";
            }

            public uint RoomIndex;
            public string Password;
        }

        public class LeaveRoom : PacketCommonCtoS
        {
            public LeaveRoom()
                : base(PacketDefine.PacketIndex.CtoS_LeaveRoom)
            {
            }
        }

        public class Ready : PacketCommonCtoS
        {
            public Ready()
                : base(PacketDefine.PacketIndex.CtoS_Ready)
            {
                IsReady = false;
            }

            public bool IsReady;
        }

        public class Mission_GetCard : PacketCommonCtoS
        {
            public Mission_GetCard()
                : base(PacketDefine.PacketIndex.CtoS_Mission_GetCard)
            {
                SelectMissionGetCard = new PacketDefine.MissionGetCardData();
            }

            public PacketDefine.MissionGetCardData SelectMissionGetCard;
        }

        public class Mission_BadCondition : PacketCommonCtoS
        {
            public Mission_BadCondition()
                : base(PacketDefine.PacketIndex.CtoS_Mission_BadCondition)
            {
                SelectUserIndex = 0;
            }

            public uint SelectUserIndex;
        }

        public class Trick : PacketCommonCtoS
        {
            public Trick()
                : base(PacketDefine.PacketIndex.CtoS_Trick)
            {
                Card = PacketDefine.Card.Blue1;
            }

            public PacketDefine.Card Card;
        }

        public class CommunicationToken : PacketCommonCtoS
        {
            public CommunicationToken()
                : base(PacketDefine.PacketIndex.CtoS_CommunicationToken)
            {
                Card = PacketDefine.Card.Blue1;
                Type = PacketDefine.CommunicationTokenType.None;
            }

            public PacketDefine.Card Card;
            public PacketDefine.CommunicationTokenType Type;
        }

        public class Emoticon : PacketCommonCtoS
        {
            public Emoticon()
                : base(PacketDefine.PacketIndex.CtoS_Emoticon)
            {
                EmoticonIndex = 0;
            }

            public uint EmoticonIndex;
        }

        public class SurrenderVote : PacketCommonCtoS
        {
            public SurrenderVote()
                : base(PacketDefine.PacketIndex.CtoS_SurrenderVote)
            {
                IsSurrender = false;
            }

            public bool IsSurrender;
        }

        public class Escape : PacketCommonCtoS
        {
            public Escape()
                : base(PacketDefine.PacketIndex.CtoS_Escape)
            {
            }
        }

        public class NextStageVote : PacketCommonCtoS
        {
            public NextStageVote()
                : base(PacketDefine.PacketIndex.CtoS_NextStageVote)
            {
                IsPassed = false;
            }

            public bool IsPassed;
        }

        public class RePlayVote : PacketCommonCtoS
        {
            public RePlayVote()
                : base(PacketDefine.PacketIndex.CtoS_RePlayVote)
            {
                IsPassed = false;
            }

            public bool IsPassed;
        }

        public class KeepAlive : PacketCommonCtoS
        {
            public KeepAlive()
                : base(PacketDefine.PacketIndex.CtoS_KeepAlive)
            {
            }
        }

        public class OnApplicationPause : PacketCommonCtoS
        {
            public OnApplicationPause()
                : base(PacketDefine.PacketIndex.CtoS_OnApplicationPause)
            {
                Pause = false;
            }

            public bool Pause;
        }

        public class TestEnd : PacketCommonCtoS
        {
            public TestEnd()
                : base(PacketDefine.PacketIndex.CtoS_TestEnd)
            {
            }
        }
    }

    namespace StoC
    {
        public class PacketCommonStoC : PacketCommon
        {
            protected PacketCommonStoC(PacketDefine.PacketIndex _index, PacketDefine.PacketResult _result)
                : base(_index)
            {
                Result = _result;
            }

            public PacketDefine.PacketResult Result { get; set; }
        }

        public class Enter : PacketCommonStoC
        {
            public Enter() : this(PacketDefine.PacketResult.Fail) {}
            public Enter(PacketDefine.PacketResult _result)
                : base(PacketDefine.PacketIndex.StoC_Enter, _result)
            {
                UserIndex = 0;
            }

            public uint UserIndex;
        }

        public class LobbyRoomList : PacketCommonStoC
        {
            public class LobbyRoomData
            {
                public LobbyRoomData()
                {
                    RoomIndex = 0;
                    State = PacketDefine.RoomState.Wait;
                    arrPlayerData = new PacketDefine.RoomPlayerData?[PacketDefine.PlayerSlotCount];
                }

                public uint RoomIndex;
                public PacketDefine.RoomState State;
                public PacketDefine.RoomPlayerData?[] arrPlayerData;
            }

            public LobbyRoomList() : this(PacketDefine.PacketResult.Fail) { }
            public LobbyRoomList(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_LobbyRoomList, _result)
            {
                listRoomData = new List<LobbyRoomData>();
            }

            public List<LobbyRoomData> listRoomData;
        }

        public class CreateRoom : PacketCommonStoC
        {
            public CreateRoom() : this(PacketDefine.PacketResult.Fail) { }
            public CreateRoom(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_CreateRoom, _result)
            {
                RoomIndex = 0;
                Password = "";
                StartStage = 0;
            }

            public uint RoomIndex;
            public string Password;
            public uint StartStage;
        }

        public class CreateRoomToLobby_Noti : PacketCommonStoC
        {
            public CreateRoomToLobby_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public CreateRoomToLobby_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_CreateRoomToLobby_Noti, _result)
            {
                RoomIndex = 0;
                RoomState = PacketDefine.RoomState.Wait;
            }

            public uint RoomIndex;
            public PacketDefine.RoomState RoomState;
        }

        public class RemoveRoomToLobby_Noti : PacketCommonStoC
        {
            public RemoveRoomToLobby_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public RemoveRoomToLobby_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_RemoveRoomToLobby_Noti, _result)
            {
                RoomIndex = 0;
            }

            public uint RoomIndex;
        }

        public class EnterRoom : PacketCommonStoC
        {
            public EnterRoom() : this(PacketDefine.PacketResult.Fail) { }
            public EnterRoom(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_EnterRoom, _result)
            {
                RoomIndex = 0;
                Password = "";
                StartStage = 0;
                OwnerIndex = 0;
                arrPlayerData = new PacketDefine.RoomPlayerData?[PacketDefine.PlayerSlotCount];
            }

            public uint RoomIndex;
            public string Password;
            public uint StartStage;
            public uint OwnerIndex;
            public PacketDefine.RoomPlayerData?[] arrPlayerData;
        }

        public class EnterRoom_Noti : PacketCommonStoC
        {
            public EnterRoom_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public EnterRoom_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_EnterRoom_Noti, _result)
            {
                EnterPlayerData = new PacketDefine.RoomPlayerData();
            }

            public PacketDefine.RoomPlayerData EnterPlayerData;
        }

        public class EnterRoomToLobby_Noti : PacketCommonStoC
        {
            public EnterRoomToLobby_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public EnterRoomToLobby_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_EnterRoomToLobby_Noti, _result)
            {
                RoomIndex = 0;
                EnterPlayerData = new PacketDefine.RoomPlayerData();
            }

            public uint RoomIndex;
            public PacketDefine.RoomPlayerData EnterPlayerData;
        }

        public class LeaveRoom : PacketCommonStoC
        {
            public LeaveRoom() : this(PacketDefine.PacketResult.Fail) { }
            public LeaveRoom(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_LeaveRoom, _result)
            {
            }
        }

        public class LeaveRoom_Noti : PacketCommonStoC
        {
            public LeaveRoom_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public LeaveRoom_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_LeaveRoom_Noti, _result)
            {
                OwnerIndex = 0;
                LeavePlayerData = new PacketDefine.RoomPlayerData();
            }

            public uint OwnerIndex;
            public PacketDefine.RoomPlayerData LeavePlayerData;
        }

        public class LeaveRoomToLobby_Noti : PacketCommonStoC
        {
            public LeaveRoomToLobby_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public LeaveRoomToLobby_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_LeaveRoomToLobby_Noti, _result)
            {
                RoomIndex = 0;
                LeavePlayerData = new PacketDefine.RoomPlayerData();
            }

            public uint RoomIndex;
            public PacketDefine.RoomPlayerData LeavePlayerData;
        }

        public class StateUpdateRoomToLobby_Noti : PacketCommonStoC
        {
            public StateUpdateRoomToLobby_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public StateUpdateRoomToLobby_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_StateUpdateRoomToLobby_Noti, _result)
            {
                State = PacketDefine.RoomState.Wait;
                RoomIndex = 0;
            }

            public PacketDefine.RoomState State;
            public uint RoomIndex;
        }

        public class Ready_Noti : PacketCommonStoC
        {
            public Ready_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public Ready_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_Ready_Noti, _result)
            {
                PlayerData = new PacketDefine.RoomPlayerData();
            }

            public PacketDefine.RoomPlayerData PlayerData;
        }

        public class Start_Noti : PacketCommonStoC
        {
            public Start_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public Start_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_Start_Noti, _result)
            {
                Stage = 0;
                listCard = new List<PacketDefine.Card>();
                listMission = new List<PacketDefine.MissionType>();
            }

            public uint Stage;
            public List<PacketDefine.Card> listCard;
            public List<PacketDefine.MissionType> listMission;
        }

        public class Mission_GetCardStart_Noti : PacketCommonStoC
        {
            public Mission_GetCardStart_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public Mission_GetCardStart_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_Mission_GetCardStart_Noti, _result)
            {
                listMissionGetCard = new List<PacketDefine.MissionGetCardData>();
                SelectUserIndex = 0;
            }

            public List<PacketDefine.MissionGetCardData> listMissionGetCard;
            public uint SelectUserIndex;
        }

        public class Mission_GetCard_Noti : PacketCommonStoC
        {
            public Mission_GetCard_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public Mission_GetCard_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_Mission_GetCard_Noti, _result)
            {
                SelectMissionGetCard = new PacketDefine.MissionGetCardData();
                SelectUserIndex = 0;
            }

            public PacketDefine.MissionGetCardData SelectMissionGetCard;
            public uint SelectUserIndex;
        }

        public class Mission_GetCardEnd_Noti : PacketCommonStoC
        {
            public Mission_GetCardEnd_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public Mission_GetCardEnd_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_Mission_GetCardEnd_Noti, _result)
            {
            }
        }

        public class Mission_BadConditionStart_Noti : PacketCommonStoC
        {
            public Mission_BadConditionStart_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public Mission_BadConditionStart_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_Mission_BadConditionStart_Noti, _result)
            {
                SelectUserIndex = 0;
            }

            public uint SelectUserIndex;
        }

        public class Mission_BadCondition_Noti : PacketCommonStoC
        {
            public Mission_BadCondition_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public Mission_BadCondition_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_Mission_BadCondition_Noti, _result)
            {
                SelectUserIndex = 0;
            }

            public uint SelectUserIndex;
        }

        public class TrickStart_Noti : PacketCommonStoC
        {
            public TrickStart_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public TrickStart_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_TrickStart_Noti, _result)
            {
                TurnUserIndex = 0;
            }

            public uint TurnUserIndex;
        }

        public class Trick_Noti : PacketCommonStoC
        {
            public Trick_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public Trick_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_Trick_Noti, _result)
            {
                TurnUserIndex = 0;
                Card = PacketDefine.Card.Blue1;
            }

            public uint TurnUserIndex;
            public PacketDefine.Card Card;
        }

        public class TrickEnd_Noti : PacketCommonStoC
        {
            public TrickEnd_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public TrickEnd_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_TrickEnd_Noti, _result)
            {
                TrickWinnerUserIndex = 0;
                ListGetCard = new List<PacketDefine.Card>();
            }

            public uint TrickWinnerUserIndex;
            public List<PacketDefine.Card> ListGetCard;
        }

        public class CommunicationToken_Noti : PacketCommonStoC
        {
            public CommunicationToken_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public CommunicationToken_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_CommunicationToken_Noti, _result)
            {
                UserIndex = 0;
                Card = PacketDefine.Card.Blue1;
                Type = PacketDefine.CommunicationTokenType.None;
            }

            public uint UserIndex;
            public PacketDefine.Card Card;
            public PacketDefine.CommunicationTokenType Type;
        }

        public class Emoticon_Noti : PacketCommonStoC
        {
            public Emoticon_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public Emoticon_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_Emoticon_Noti, _result)
            {
                UserIndex = 0;
                EmoticonIndex = 0;
            }

            public uint UserIndex;
            public uint EmoticonIndex;
        }

        public class End_Noti : PacketCommonStoC
        {
            public End_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public End_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_End_Noti, _result)
            {
                IsClear = false;
            }

            public bool IsClear;
        }

        public class SurrenderVote_Noti : PacketCommonStoC
        {
            public SurrenderVote_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public SurrenderVote_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_SurrenderVote_Noti, _result)
            {
                ListSurrenderVote = new();
            }

            public List<(PacketDefine.PlayerSlot Slot, uint UserIndex, bool IsVote, bool IsPassed)> ListSurrenderVote;
        }

        public class SurrenderVoteResult_Noti : PacketCommonStoC
        {
            public SurrenderVoteResult_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public SurrenderVoteResult_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_SurrenderVoteResult_Noti, _result)
            {
                IsPassed = false;
            }

            public bool IsPassed;
        }

        public class Escape_Noti : PacketCommonStoC
        {
            public Escape_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public Escape_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_Escape_Noti, _result)
            {
            }
        }

        public class NextStageVote_Noti : PacketCommonStoC
        {
            public NextStageVote_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public NextStageVote_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_NextStageVote_Noti, _result)
            {
                ListNextStageVote = new();
            }

            public List<(PacketDefine.PlayerSlot Slot, uint UserIndex, bool IsVote, bool IsPassed)> ListNextStageVote;
        }

        public class NextStageVoteResult_Noti : PacketCommonStoC
        {
            public NextStageVoteResult_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public NextStageVoteResult_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_NextStageVoteResult_Noti, _result)
            {
                IsPassed = false;
            }

            public bool IsPassed;
        }

        public class RePlayVote_Noti : PacketCommonStoC
        {
            public RePlayVote_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public RePlayVote_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_RePlayVote_Noti, _result)
            {
                ListRePlayVote = new();
            }

            public List<(PacketDefine.PlayerSlot Slot, uint UserIndex, bool IsVote, bool IsPassed)> ListRePlayVote;
        }

        public class RePlayVoteResult_Noti : PacketCommonStoC
        {
            public RePlayVoteResult_Noti() : this(PacketDefine.PacketResult.Fail) { }
            public RePlayVoteResult_Noti(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_RePlayVoteResult_Noti, _result)
            {
                IsPassed = false;
            }

            public bool IsPassed;
        }

        public class KeepAlive : PacketCommonStoC
        {
            public KeepAlive() : this(PacketDefine.PacketResult.Fail) { }
            public KeepAlive(PacketDefine.PacketResult _result = PacketDefine.PacketResult.Fail)
                : base(PacketDefine.PacketIndex.StoC_KeepAlive, _result)
            {
            }
        }
    }
}
