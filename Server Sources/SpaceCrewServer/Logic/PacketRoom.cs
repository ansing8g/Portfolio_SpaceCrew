using System;
using System.Collections.Generic;

using Packet;

using SpaceCrewServer.Game;
using SpaceCrewServer.Manager;
using SpaceCrewServer.Server;

namespace SpaceCrewServer.Logic
{
    public static class PacketRoom
    {
        public static void LeaveRoom(User _user, Room _room, Packet.CtoS.LeaveRoom _packet)
        {
            _room.RemovePlayer(_user, out PacketDefine.PlayerSlot slot);

            _user.Send(new Packet.StoC.LeaveRoom(PacketDefine.PacketResult.Success));

            bool isDestory = _room.Destroy();
            if (false == isDestory)
            {
                foreach (Player? player in _room.GetPlayerList)
                {
                    player?.User?.Send(new Packet.StoC.LeaveRoom_Noti(PacketDefine.PacketResult.Success)
                    {
                        OwnerIndex = _room.OwnerIndex,
                        LeavePlayerData = new()
                        {
                            Slot = slot,
                            UserIndex = _user.UserIndex
                        }
                    });
                }
            }

            RoomManager.Instance.Lobby.EventQueue.Enqueue(new(Define.RoomEventType.LeaveRoom, new RoomEventData_LeaveRoom()
            {
                RoomIndex = _room.RoomIndex,
                Slot = slot,
                UserIndex = _user.UserIndex
            }));

            if (true == isDestory)
            {
                RoomManager.Instance.Lobby.EventQueue.Enqueue(new(Define.RoomEventType.RemoveRoom, new RoomEventData_RemoveRoom()
                {
                    RoomIndex = _room.RoomIndex
                }));
            }
        }

        public static void Ready(User _user, Room _room, Packet.CtoS.Ready _packet)
        {
            if (PacketDefine.RoomState.Gaming == _room.PublicState)
            {
                _user.Send(new Packet.StoC.Ready_Noti(PacketDefine.PacketResult.Ready_Noti_RoomGaming));
                ServerBase.Instance.WriteLog($"Packet.CtoS.Ready Room Gaming UserIndex:{_user.UserIndex}, RoomIndex:{_room.RoomIndex}");
                return;
            }

            if (false == _room.Ready(_user, _packet.IsReady, out PacketDefine.PlayerSlot slot))
            {
                _user.Send(new Packet.StoC.Ready_Noti(PacketDefine.PacketResult.Ready_Noti_NotFoundPlayer));
                ServerBase.Instance.WriteLog($"Packet.CtoS.Ready Room Ready Fail UserIndex:{_user.UserIndex}, RoomIndex:{_room.RoomIndex}, Ready:{_packet.IsReady}");
                return;
            }

            Packet.StoC.Ready_Noti packet = new Packet.StoC.Ready_Noti(PacketDefine.PacketResult.Success);
            packet.PlayerData.Slot = slot;
            packet.PlayerData.UserIndex = _user.UserIndex;
            packet.PlayerData.IsReady = _packet.IsReady;
            foreach(Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packet);
            }

            if (false == _room.ElectMode())
            {
                return;
            }

            _room.Start();
        }

        public static void Trick(User _user, Room _room, Packet.CtoS.Trick _packet)
        {
            if (false == _room.Trick(_user.UserIndex, _packet.Card, out PacketDefine.PacketResult result))
            {
                _user.Send(new Packet.StoC.Trick_Noti(result));
                ServerBase.Instance.WriteLog($"Packet.CtoS.Trick Trick Fail UserIndex:{_user.UserIndex}, RoomIndex:{_room.RoomIndex}, Card:{_packet.Card}");
                return;
            }

            Packet.StoC.Trick_Noti packetTrick = new(PacketDefine.PacketResult.Success);
            packetTrick.TurnUserIndex = _user.UserIndex;
            packetTrick.Card = _packet.Card;
            foreach (Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packetTrick);
            }

            if (true == _room.CheckTrickEnd())
            {
                return;
            }

            Packet.StoC.TrickStart_Noti packetTickStart = new(PacketDefine.PacketResult.Success);
            packetTickStart.TurnUserIndex = _room.TurnUserIndex;
            foreach (Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packetTickStart);
            }
        }

        public static void CommunicationToken(User _user, Room _room, Packet.CtoS.CommunicationToken _packet)
        {
            if (false == _room.CommunicationToken(_user.UserIndex, _packet.Card, _packet.Type, out PacketDefine.PacketResult _result))
            {
                _user.Send(new Packet.StoC.CommunicationToken_Noti(_result));
                ServerBase.Instance.WriteLog($"Packet.CtoS.CommunicationToken CommunicationToken Fail UserIndex:{_user.UserIndex}, RoomIndex:{_room.RoomIndex}, Card:{_packet.Card}, Type:{_packet.Type}");
                return;
            }

            Packet.StoC.CommunicationToken_Noti packet = new(PacketDefine.PacketResult.Success)
            {
                UserIndex = _user.UserIndex,
                Card = _packet.Card,
                Type = _packet.Type
            };
            foreach (Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packet);
            }
        }

        public static void Emoticon(User _user, Room _room, Packet.CtoS.Emoticon _packet)
        {
            Packet.StoC.Emoticon_Noti packet = new(PacketDefine.PacketResult.Success)
            {
                EmoticonIndex = _packet.EmoticonIndex,
                UserIndex = _user.UserIndex,
            };
            foreach (Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packet);
            }
        }

        public static void SurrenderVote(User _user, Room _room, Packet.CtoS.SurrenderVote _packet)
        {
            if(false == _room.Surrender(_user, _packet.IsSurrender))
            {
                _user.Send(new Packet.StoC.SurrenderVote_Noti(PacketDefine.PacketResult.SurrenderVote_Noti_SurrenderFail));
                ServerBase.Instance.WriteLog($"Packet.CtoS.SurrenderVote Surrender Fail UserIndex:{_user.UserIndex}, RoomIndex:{_room.RoomIndex}, Surrender:{_packet.IsSurrender}");
                return;
            }

            Packet.StoC.SurrenderVote_Noti packetSurrenderVoteNoti = new(PacketDefine.PacketResult.Success);
            for(int i = 0; i < _room.GetPlayerList.Length; ++i)
            {
                Player? player = _room.GetPlayerList[i];
                if (null == player)
                {
                    continue;
                }

                packetSurrenderVoteNoti.ListSurrenderVote.Add(((PacketDefine.PlayerSlot)i, player.UserIndex, player.IsVoteSurrender, player.IsSurrender));
            }
            foreach (Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packetSurrenderVoteNoti);
            }

            if(false == _room.IsAllSurrenderVote(out uint passedcount, out uint rejectedcount))
            {
                return;
            }

            bool isPassed = passedcount * 100 / (passedcount + rejectedcount) >= Define.SurrenderPassedPercentage;
            Packet.StoC.SurrenderVoteResult_Noti packetSurrenderVoteResultNoti = new(PacketDefine.PacketResult.Success)
            {
                IsPassed = isPassed,
            };
            foreach (Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packetSurrenderVoteResultNoti);
            }

            if(false == isPassed)
            {
                return;
            }

            _room.ReStart();
        }

        public static void Escape(User _user, Room _room, Packet.CtoS.Escape _packet)
        {
            _room.Escape(_user, out List<User> listUser);

            Packet.StoC.Escape_Noti packet = new(PacketDefine.PacketResult.Success);
            listUser.ForEach(i => i.Send(packet));

            _room.Destroy();
        }

        public static void NextStageVote(User _user, Room _room, Packet.CtoS.NextStageVote _packet)
        {
            if(false == _room.NextStageVote(_user, _packet.IsPassed))
            {
                _user.Send(new Packet.StoC.NextStageVote_Noti(PacketDefine.PacketResult.NextStageVote_Noti_NextStageVoteFail));
                ServerBase.Instance.WriteLog($"Packet.CtoS.NextStageVote NextStageVote Fail UserIndex:{_user.UserIndex}, RoomIndex:{_room.RoomIndex}, MoveNextStage:{_packet.IsPassed}");
                return;
            }

            Packet.StoC.NextStageVote_Noti packetNextStageVoteNoti = new(PacketDefine.PacketResult.Success);
            for (int i = 0; i < _room.GetPlayerList.Length; ++i)
            {
                Player? player = _room.GetPlayerList[i];
                if (null == player)
                {
                    continue;
                }

                packetNextStageVoteNoti.ListNextStageVote.Add(((PacketDefine.PlayerSlot)i, player.UserIndex, player.IsVoteReady, player.IsReady));
            }
            foreach (Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packetNextStageVoteNoti);
            }

            if(false == _room.IsAllReadyVote(out uint passedcount, out uint rejectedcount))
            {
                return;
            }

            bool isPassed = 0 == rejectedcount;
            Packet.StoC.NextStageVoteResult_Noti packetNextStageVoteResultNoti = new(PacketDefine.PacketResult.Success)
            {
                IsPassed = isPassed,
            };
            foreach (Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packetNextStageVoteResultNoti);
            }

            if(isPassed)
            {
                _room.ReStart();
            }
            else
            {
                _room.Escape(_user, out List<User> listUser);
                _room.Destroy();
            }
        }

        public static void RePlayVote(User _user, Room _room, Packet.CtoS.RePlayVote _packet)
        {
            if(false == _room.RePlayVote(_user, _packet.IsPassed))
            {
                _user.Send(new Packet.StoC.RePlayVote_Noti(PacketDefine.PacketResult.RePlayVote_Noti_RePlayVoteFail));
                ServerBase.Instance.WriteLog($"Packet.CtoS.RePlayVote RePlayVote Fail UserIndex:{_user.UserIndex}, RoomIndex:{_room.RoomIndex}, RePlay:{_packet.IsPassed}");
                return;
            }

            Packet.StoC.RePlayVote_Noti packetRePlayVoteNoti = new(PacketDefine.PacketResult.Success);
            for (int i = 0; i < _room.GetPlayerList.Length; ++i)
            {
                Player? player = _room.GetPlayerList[i];
                if (null == player)
                {
                    continue;
                }

                packetRePlayVoteNoti.ListRePlayVote.Add(((PacketDefine.PlayerSlot)i, player.UserIndex, player.IsVoteReady, player.IsReady));
            }
            foreach (Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packetRePlayVoteNoti);
            }

            if (false == _room.IsAllReadyVote(out uint passedcount, out uint rejectedcount))
            {
                return;
            }

            bool isPassed = 0 == rejectedcount;
            Packet.StoC.RePlayVoteResult_Noti packetRePlayVoteResultNoti = new(PacketDefine.PacketResult.Success)
            {
                IsPassed = isPassed,
            };
            foreach (Player? player in _room.GetPlayerList)
            {
                player?.User?.Send(packetRePlayVoteResultNoti);
            }

            if (isPassed)
            {
                _room.ReStart();
            }
            else
            {
                _room.Escape(_user, out List<User> listUser);
                _room.Destroy();
            }
        }

        public static void KeepAlive(User _user, Room _room, Packet.CtoS.KeepAlive _packet)
        {
            _user.SetKeepAliveTime(DateTime.Now);
        }

        public static void OnApplicationPause(User _user, Room _room, Packet.CtoS.OnApplicationPause _packet)
        {
            if (_packet.Pause)
            {
                _user.SetKeepAliveTime(DateTime.Now.AddSeconds(ServerBase.Instance.Config.KeepAlivePauseAddSecond));
            }
            else
            {
                _user.SetKeepAliveTime(DateTime.Now);
            }
        }

        public static void TestEnd(User _user, Room _room, Packet.CtoS.TestEnd _packet)
        {
            foreach (Player? player in _room.GetPlayerList)
            {
                if (null == player?.User)
                {
                    continue;
                }

                player.User.Disconnect();
                _room.RemovePlayer(player.User, out PacketDefine.PlayerSlot slot);
            }

            _room.Destroy();

            RoomManager.Instance.Lobby.EventQueue.Enqueue(new(Define.RoomEventType.RemoveRoom, new RoomEventData_RemoveRoom()
            {
                RoomIndex = _room.RoomIndex
            }));
        }
    }
}
