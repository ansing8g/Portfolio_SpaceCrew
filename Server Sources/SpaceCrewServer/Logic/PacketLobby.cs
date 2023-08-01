using System;
using System.Threading;

using Packet;

using SpaceCrewServer.Server;
using SpaceCrewServer.Manager;
using SpaceCrewServer.Game;
using SpaceCrewServer.DB;

namespace SpaceCrewServer.Logic
{
    public static class PacketLobby
    {
        public static void Enter(User _user, Lobby _lobby, Packet.CtoS.Enter _packet)
        {
            if(false == AuthencationDB.Instance.GetIndex(_packet.ID, out uint index))
            {
                _user.Send(new Packet.StoC.Enter(PacketDefine.PacketResult.Enter_DBFail));
                ServerBase.Instance.WriteLog($"Packet.CtoS.Enter AuthencationDB GetIndex Fail UserIndex:{_user.UserIndex}, ID:{_packet.ID}");
                return;
            }

            _user.SetID(_packet.ID);
            _user.SetUserIndex(index);

            _user.Send(new Packet.StoC.Enter(PacketDefine.PacketResult.Success)
            {
                UserIndex = _user.UserIndex
            });

            ServerBase.Instance.WriteLog($"Set UserIndex:{index}");
        }

        public static void LobbyInfo(User _user, Lobby _lobby, Packet.CtoS.LobbyRoomList _packet)
        {
            Packet.StoC.LobbyRoomList lobbyroomlist = new Packet.StoC.LobbyRoomList(PacketDefine.PacketResult.Success);

            RoomManager.Instance.ForeachRoom((_room) =>
            {
                Packet.StoC.LobbyRoomList.LobbyRoomData lobbyroomdata = new Packet.StoC.LobbyRoomList.LobbyRoomData();
                lobbyroomdata.State = _room.PublicState;
                lobbyroomdata.RoomIndex = _room.RoomIndex;
                for (int i = 0; i < Define.PlayerSlotCount; ++i)
                {
                    Player? player = _room.GetPlayerList[i];
                    lobbyroomdata.arrPlayerData[i] = null == player ? null : new PacketDefine.RoomPlayerData()
                    {
                        Slot = (PacketDefine.PlayerSlot)i,
                        UserIndex = player.UserIndex,
                        IsReady = player.IsReady,
                    };
                }

                lobbyroomlist.listRoomData.Add(lobbyroomdata);
            });

            _user.Send(lobbyroomlist);
        }

        public static void CreateRoom(User _user, Lobby _lobby, Packet.CtoS.CreateRoom _packet)
        {
            if (false == RoomManager.Instance.CreateRoom(out Room? room) ||
                null == room)
            {
                _user.Send(new Packet.StoC.CreateRoom(PacketDefine.PacketResult.CreateRoom_CreateFail));
                ServerBase.Instance.WriteLog($"Packet.CtoS.CreateRoom RoomManager CreateRoom UserIndex:{_user.UserIndex}, IsNullRoom:{null == room}");
                return;
            }

            room.SetStartStage(_packet.StartStage);
            room.SetPassword(_packet.Password);

            if (false == room.AddPlayer(_user, true, out uint slotindex))
            {
                _user.Send(new Packet.StoC.CreateRoom(PacketDefine.PacketResult.CreateRoom_AddFail));
                ServerBase.Instance.WriteLog($"Packet.CtoS.CreateRoom Room AddPlayer Fail UserIndex:{_user.UserIndex}, RoomIndex:{room.RoomIndex}");
                return;
            }

            _user.Send(new Packet.StoC.CreateRoom(PacketDefine.PacketResult.Success)
            {
                RoomIndex = room.RoomIndex,
                Password = room.Password,
                StartStage = room.Stage,
            });

            Packet.StoC.CreateRoomToLobby_Noti packetCreateRoomToLobbyNoti = new Packet.StoC.CreateRoomToLobby_Noti(PacketDefine.PacketResult.Success)
            {
                RoomIndex = room.RoomIndex,
                RoomState = room.PublicState
            };
            Packet.StoC.EnterRoomToLobby_Noti packetEnterRoomToLobbyNoti = new Packet.StoC.EnterRoomToLobby_Noti(PacketDefine.PacketResult.Success);
            packetEnterRoomToLobbyNoti.RoomIndex = room.RoomIndex;
            packetEnterRoomToLobbyNoti.EnterPlayerData.Slot = (PacketDefine.PlayerSlot)slotindex;
            packetEnterRoomToLobbyNoti.EnterPlayerData.UserIndex = _user.UserIndex;
            _lobby.ForEach((User _user) =>
            {
                _user.Send(packetCreateRoomToLobbyNoti);
                _user.Send(packetEnterRoomToLobbyNoti);
            });
        }

        public static void EnterRoom(User _user, Lobby _lobby, Packet.CtoS.EnterRoom _packet)
        {
            if (false == RoomManager.Instance.GetRoom(_packet.RoomIndex, out Room? room) ||
                null == room)
            {
                _user.Send(new Packet.StoC.EnterRoom(PacketDefine.PacketResult.EnterRoom_GetFail));
                ServerBase.Instance.WriteLog($"Packet.CtoS.EnterRoom RoomManager GetRoom Fail UserIndex:{_user.UserIndex}, RoomIndex:{_packet.RoomIndex}");
                return;
            }

            if(false == string.IsNullOrEmpty(room.Password) &&
                false == room.Password.Equals(_packet.Password))
            {
                _user.Send(new Packet.StoC.EnterRoom(PacketDefine.PacketResult.EnterRoom_NotMatchPassword));
                ServerBase.Instance.WriteLog($"Packet.CtoS.EnterRoom Passward Not Match UserIndex:{_user.UserIndex}, RoomIndex:{_packet.RoomIndex}, RoomPassword:{room.Password}, InputPassword:{_packet.Password}");
                return;
            }

            if (false == room.AddPlayer(_user, false, out uint slotindex))
            {
                _user.Send(new Packet.StoC.EnterRoom(PacketDefine.PacketResult.EnterRoom_AddFail));
                ServerBase.Instance.WriteLog($"Packet.CtoS.EnterRoom AddPlayer Fail UserIndex:{_user.UserIndex}, RoomIndex:{_packet.RoomIndex}");
                return;
            }

            Packet.StoC.EnterRoom packetEnterRoom = new Packet.StoC.EnterRoom(PacketDefine.PacketResult.Success);
            packetEnterRoom.RoomIndex = room.RoomIndex;
            packetEnterRoom.Password = room.Password;
            packetEnterRoom.StartStage = room.Stage;
            packetEnterRoom.OwnerIndex = room.OwnerIndex;
            for (uint i = 0; i < Define.PlayerSlotCount; ++i)
            {
                Player? player = room.GetPlayerList[i];
                packetEnterRoom.arrPlayerData[i] = null == player ? null : new PacketDefine.RoomPlayerData()
                { 
                    Slot = (PacketDefine.PlayerSlot)i,
                    UserIndex = player!.UserIndex,
                    IsReady = player!.IsReady,
                };
            }

            _user.Send(packetEnterRoom);

            Packet.StoC.EnterRoom_Noti packetEnterRoomNoti = new Packet.StoC.EnterRoom_Noti(PacketDefine.PacketResult.Success)
            {
                EnterPlayerData = new PacketDefine.RoomPlayerData()
                {
                    Slot = (PacketDefine.PlayerSlot)slotindex,
                    UserIndex = _user.UserIndex,
                    IsReady = false,
                }
            };
            foreach (Player? player in room.GetPlayerList)
            {
                player?.User?.Send(packetEnterRoomNoti);
            }

            Packet.StoC.EnterRoomToLobby_Noti packetEnterRoomToLobbyNoti = new Packet.StoC.EnterRoomToLobby_Noti(PacketDefine.PacketResult.Success)
            {
                RoomIndex = room.RoomIndex,
                EnterPlayerData = new PacketDefine.RoomPlayerData()
                {
                    Slot = (PacketDefine.PlayerSlot)slotindex,
                    UserIndex = _user.UserIndex,
                    IsReady = false,
                }
            };
            _lobby.ForEach((User _user) =>
            {
                _user.Send(packetEnterRoomToLobbyNoti);
            });
        }

        public static void KeepAlive(User _user, Lobby _lobby, Packet.CtoS.KeepAlive _packet)
        {
            _user.SetKeepAliveTime(DateTime.Now);
        }

        public static void OnApplicationPause(User _user, Lobby _lobby, Packet.CtoS.OnApplicationPause _packet)
        {
            if(_packet.Pause)
            {
                _user.SetKeepAliveTime(DateTime.Now.AddSeconds(ServerBase.Instance.Config.KeepAlivePauseAddSecond));
            }
            else
            {
                _user.SetKeepAliveTime(DateTime.Now);
            }
        }
    }
}
