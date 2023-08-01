using System;
using System.Collections.Generic;
using System.Threading;

using Packet;
using SpaceCrewServer.Manager;

namespace SpaceCrewServer.Game
{
    public partial class Room
    {
        private enum State : uint
        {
            Lobby,
            Start,
            Mission,
            Stage,
            Trick,
            TrickEnd,
            StageSuccessEnd,
            StageFailEnd,
        }

        private class StateCallback
        {
            public StateCallback(Action _previous, Action _update, Action _next)
            {
                Previous = _previous;
                Update = _update;
                Next = _next;
            }

            public Action Previous;
            public Action Update;
            public Action Next;
        }

        private void StateInitialize()
        {
            m_dicPriavteState.Add(State.Lobby, new StateCallback(Lobby_Previous, Lobby_Update, Lobby_Next));
            m_dicPriavteState.Add(State.Start, new StateCallback(Start_Previous, Start_Update, Start_Next));
            m_dicPriavteState.Add(State.Mission, new StateCallback(Mission_Previous, Mission_Update, Mission_Next));
            m_dicPriavteState.Add(State.Stage, new StateCallback(Stage_Previous, Stage_Update, Stage_Next));
            m_dicPriavteState.Add(State.Trick, new StateCallback(Trick_Previous, Trick_Update, Trick_Next));
            m_dicPriavteState.Add(State.TrickEnd, new StateCallback(TrickEnd_Previous, TrickEnd_Update, TrickEnd_Next));
            m_dicPriavteState.Add(State.StageSuccessEnd, new StateCallback(StageSuccessEnd_Previous, StageSuccessEnd_Update, StageSuccessEnd_Next));
            m_dicPriavteState.Add(State.StageFailEnd, new StateCallback(StageFailEnd_Previous, StageFailEnd_Update, StageFailEnd_Next));

            Lobby_Previous();
        }

        private void StateChange(State _state)
        {
            State previous = m_privateState;
            m_privateState = _state;

            if (m_dicPriavteState.ContainsKey(previous))
            {
                m_dicPriavteState[previous].Next();
            }

            if (m_dicPriavteState.ContainsKey((State)m_privateState))
            {
                m_dicPriavteState[(State)m_privateState].Previous();
            }
        }

        private void StateUpdate()
        {
            if (m_dicPriavteState.ContainsKey((State)m_privateState))
            {
                m_dicPriavteState[(State)m_privateState].Update();
            }
        }

        private void Lobby_Previous()
        {
            Interlocked.Exchange(ref m_stage, 1);
        }

        private void Lobby_Update() { }

        private void Lobby_Next() { }

        private void Start_Previous()
        {
            foreach(Player? player in m_arrPlayer)
            {
                player?.ListCard.Clear();
                player?.DicTrickCard.Clear();
                player?.CommunicationToken.Clear();
            }

            List<PacketDefine.Card> listDeck = new List<PacketDefine.Card>();
            foreach(PacketDefine.Card card in Enum.GetValues<PacketDefine.Card>())
            {
                listDeck.Add(card);
            }

            DistributionCard(listDeck);

            PublicState = PacketDefine.RoomState.Gaming;
            m_mission.GenerateMission(listDeck);

            Packet.StoC.Start_Noti packetStart = new Packet.StoC.Start_Noti(PacketDefine.PacketResult.Success);
            packetStart.Stage = Stage;
            packetStart.listMission = GetMissionTypeList();
            foreach (Player? player in m_arrPlayer)
            {
                if (null == player)
                {
                    continue;
                }

                packetStart.listCard.Clear();
                foreach (PacketDefine.Card card in player.ListCard)
                {
                    packetStart.listCard.Add(card);
                }

                player.User?.Send(packetStart);
            }

            RoomManager.Instance.Lobby.EventQueue.Enqueue(new RoomEvent(Server.Define.RoomEventType.StateUpdateRoom, new RoomEventData_StateUpdateRoom()
            {
                RoomIndex = RoomIndex,
                State = PacketDefine.RoomState.Gaming
            }));

            StateChange(State.Mission);
            UseMissionPacket();
        }

        private void Start_Update() { }

        private void Start_Next() { }

        private void Mission_Previous() { }

        private void Mission_Update() { }

        private void Mission_Next() { }

        private void Stage_Previous()
        {
            TrickCount = 1;
            m_listTrickData.Clear();
            m_dicTrickGetPlayer.Clear();

            foreach (Player? player in m_arrPlayer)
            {
                if(null == player)
                {
                    continue;
                }

                player.InitializeReady();
                player.InitializeSurrender();
                player.CommunicationToken.Card = PacketDefine.Card.Rocket4;
                player.CommunicationToken.Type = PacketDefine.CommunicationTokenType.None;
                player.CommunicationToken.IsUse = false;
                player.PlayTrick.IsPlay = false;
                player.PlayTrick.Card = PacketDefine.Card.Blue1;
            }

            StateChange(State.Trick);
        }

        private void Stage_Update() { }

        private void Stage_Next() { }

        private void Trick_Previous()
        {
            Player? turnPlayer = m_arrPlayer[TurnSlot];
            if (null == turnPlayer)
            {
                return;
            }

            Packet.StoC.TrickStart_Noti packetTickStart = new Packet.StoC.TrickStart_Noti(PacketDefine.PacketResult.Success);
            packetTickStart.TurnUserIndex = turnPlayer.UserIndex;
            foreach (Player? player in m_arrPlayer)
            {
                player?.User?.Send(packetTickStart);
            }
        }

        private void Trick_Update() { }

        private void Trick_Next() { }

        private void TrickEnd_Previous()
        {
            ProcessTrickWinner(out uint slot, out List<PacketDefine.Card> listTrickCard);
            m_mission.GetTrick(slot, listTrickCard);

            TurnSlot = slot;

            foreach (Player? player in m_arrPlayer)
            {
                if (null == player)
                {
                    continue;
                }

                player.PlayTrick.IsPlay = false;
                player.PlayTrick.Card = PacketDefine.Card.Blue1;
            }

            Packet.StoC.TrickEnd_Noti packet = new Packet.StoC.TrickEnd_Noti(PacketDefine.PacketResult.Success);
            packet.TrickWinnerUserIndex = m_arrPlayer[slot]?.UserIndex ?? 0;
            packet.ListGetCard = listTrickCard;
            foreach(Player? player in m_arrPlayer)
            {
                player?.User?.Send(packet);
            }
        }

        private void TrickEnd_Update() { }

        private void TrickEnd_Next()
        {
            ++TrickCount;
            m_listTrickData.Clear();
        }

        private void StageSuccessEnd_Previous()
        {
            Packet.StoC.End_Noti packet = new Packet.StoC.End_Noti(PacketDefine.PacketResult.Success)
            {
                 IsClear = true
            };
            foreach(Player? player in m_arrPlayer)
            {
                player?.User?.Send(packet);
            }
        }

        private void StageSuccessEnd_Update()
        {
            //if(false == IsAllReady(out uint count))
            //{
            //    return;
            //}
            //
            //StateChange(State.Start);
        }

        private void StageSuccessEnd_Next()
        {
            Interlocked.Increment(ref m_stage);
        }

        private void StageFailEnd_Previous()
        {
            Packet.StoC.End_Noti packet = new Packet.StoC.End_Noti(PacketDefine.PacketResult.Success)
            {
                IsClear = false
            };
            foreach (Player? player in m_arrPlayer)
            {
                player?.User?.Send(packet);
            }
        }

        private void StageFailEnd_Update()
        {
            //if (false == IsAllReady(out uint count))
            //{
            //    return;
            //}
            //
            //StateChange(State.Start);
        }

        private void StageFailEnd_Next() { }

        private State m_privateState;
        private Dictionary<State, StateCallback> m_dicPriavteState;
    }
}
