using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

using SocketLib;
using SocketLib.Tcp;
using SocketLib.Dispatcher.WithoutReturn;

using Packet;

using SpaceCrewServer.Server;
using SpaceCrewServer.Logic;

namespace SpaceCrewServer.Game
{
    public class Lobby : IRoom
    {
        public Lobby(ISerializer _serializer, Dispatcher<User, Lobby, PacketDefine.PacketIndex> _dispatcher)
            : base(0)
        {
            m_serializer = _serializer;
            m_dispatcher = _dispatcher;
            m_dicUser = new();
            m_updatetimer = new();

            m_updatetimer.RegistFunction(PacketProcess);
            m_updatetimer.RegistFunction(EventProcess);
            m_updatetimer.RegistFunction(KeepAliveProcess, 1.0);
            m_updatetimer.RegistFunction(DisconnectProcess);
        }

        public override void Update()
        {
            m_updatetimer.UpdateProcess();
        }

        public bool AddUser(User _user)
        {
            if (false == m_dicUser.TryAdd(_user.Socket, _user))
            {
                return false;
            }

            _user.SetRoom( this);

            return true;
        }

        public void RemoveUser(User _user)
        {
            m_dicUser.TryRemove(_user.Socket, out User? user);
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

                FunctionBase<User, Lobby, PacketDefine.PacketIndex>? func_handler = null;
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

                if(ServerBase.Instance.Config.UseNetworkRecvLog)
                {
                    if(PacketDefine.PacketIndex.CtoS_KeepAlive != packet.PacketIndex &&
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
            List<RoomEvent> listRoomEvent = new List<RoomEvent>();
            while(EventQueue.TryDequeue(out RoomEvent? roomevent))
            {
                listRoomEvent.Add(roomevent);
            }

            listRoomEvent.ForEach((RoomEvent _roomevent) =>
            {
                switch(_roomevent.Type)
                {
                    case Define.RoomEventType.LeaveRoom:
                        {
                            if(_roomevent.GetData(out RoomEventData_LeaveRoom? data) && null != data)
                            {
                                EventLobby.LeaveRoom(this, data);
                            }
                        }
                        break;
                    case Define.RoomEventType.RemoveRoom:
                        {
                            if(_roomevent.GetData(out RoomEventData_RemoveRoom? data) && null != data)
                            {
                                EventLobby.RemoveRoom(this, data);
                            }
                        }
                        break;
                    case Define.RoomEventType.StateUpdateRoom:
                        {
                            if(_roomevent.GetData(out RoomEventData_StateUpdateRoom? data) && null != data)
                            {
                                EventLobby.StateUpdateRoom(this, data);
                            }
                        }
                        break;
                }
            });
        }

        private void KeepAliveProcess()
        {
            if(false == ServerBase.Instance.Config.UseKeepAlive)
            {
                return;
            }

            IEnumerator<User> iter = m_dicUser.Values.GetEnumerator();
            while(iter.MoveNext())
            {
                DateTime recvTime = new DateTime(iter.Current.KeepAliveTime);
                if(recvTime.AddSeconds(ServerBase.Instance.Config.KeepAliveWaitSecond) <= DateTime.Now)
                {
                    iter.Current.Disconnect();
                    continue;
                }

                Packet.StoC.KeepAlive packet = new Packet.StoC.KeepAlive(PacketDefine.PacketResult.Success);
                if (recvTime.AddSeconds(ServerBase.Instance.Config.KeepAliveSendSecond) <= DateTime.Now)
                {
                    iter.Current.Send(packet);
                }
            }
        }

        private void DisconnectProcess()
        {
            while(DisconnectQueue.TryDequeue(out User? user))
            {
                RemoveUser(user);
            }
        }

        public void ForEach(Action<User> _func)
        {
            IEnumerator<User> iter = m_dicUser.Values.GetEnumerator();
            while(iter.MoveNext())
            {
                _func(iter.Current);
            }
        }

        private ISerializer m_serializer;
        private Dispatcher<User, Lobby, PacketDefine.PacketIndex> m_dispatcher;
        private ConcurrentDictionary<SessionSocket, User> m_dicUser;
        private Utility.UpdateTimer m_updatetimer;
    }
}
