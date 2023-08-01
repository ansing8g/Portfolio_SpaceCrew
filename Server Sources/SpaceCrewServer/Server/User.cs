using System;
using System.Net.Sockets;
using System.Threading;

using SocketLib;
using SocketLib.Tcp;

using SpaceCrewServer.Game;

namespace SpaceCrewServer.Server
{
    public class User
    {
        public User(SessionSocket _sessionsocket, ISerializer _serializer)
        {
            Socket = _sessionsocket;

            Interlocked.Exchange(ref m_userIndex, 0);
            Interlocked.Exchange(ref m_id, "");
            Interlocked.Exchange(ref m_room, null);
            Interlocked.Exchange(ref m_keepaliveTime, DateTime.Now.Ticks);
            m_serializer = _serializer;
        }

        public bool Send(Packet.StoC.PacketCommonStoC _packet)
        {
            if (null == _packet)
            {
                return false;
            }

            byte[]? byte_data = null;
            if (false == m_serializer.ToByte(_packet, out byte_data))
            {
                return false;
            }

            if (null == byte_data)
            {
                return false;
            }

            if (ServerBase.Instance.Config.UseNetworkSendLog)
            {
                if (Packet.PacketDefine.PacketIndex.StoC_KeepAlive != _packet.PacketIndex)
                {
                    ServerBase.Instance.WriteLog($"Send PacketIndex:{_packet.PacketIndex.ToString()}, Packet:{Newtonsoft.Json.JsonConvert.SerializeObject(_packet)}");
                }
            }

            return Socket.Send(byte_data);
        }

        public void Disconnect()
        {
            Socket.Disconnect();
        }

        public void SetID(string _id)
        {
            Interlocked.Exchange(ref m_id, _id);
        }

        public void SetUserIndex(uint _userIndex)
        {
            Interlocked.Exchange(ref m_userIndex, _userIndex);
        }

        public void SetRoom(IRoom? _room)
        {
            Interlocked.Exchange(ref m_room, _room);
        }

        public void SetKeepAliveTime(DateTime _time)
        {
            Interlocked.Exchange(ref m_keepaliveTime, _time.Ticks);
        }

        public uint UserIndex => m_userIndex;
        public string ID => m_id;
        public IRoom? Room => m_room;
        public long KeepAliveTime => Volatile.Read(ref m_keepaliveTime);

        public SessionSocket Socket { get; private set; }

        private volatile uint m_userIndex;
        private volatile string m_id;
        private volatile IRoom? m_room;
        private long m_keepaliveTime;
        private ISerializer m_serializer;
    }
}
