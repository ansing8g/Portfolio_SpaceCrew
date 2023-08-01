using System;
using System.Net.Sockets;
using System.Linq;

namespace SocketLib.Tcp
{
    public class SessionSocket
    {
        internal SessionSocket(Socket? _socket, TcpServerSocketEvent _event, uint _bufsize, uint _total_bufsize)
        {
            m_sessionsocket = _socket;
            m_event = _event;
            m_buf = new byte[_bufsize];
            m_bufsize = _bufsize;
            m_total_buf = new byte[_total_bufsize];
            m_total_bufsize = _total_bufsize;
            m_offset = 0;

            StateObject = null;

            if (null == m_sessionsocket)
            {
                m_sessionsocket!.NoDelay = true;
                m_sessionsocket!.LingerState = new LingerOption(true, 0);
            }
        }

        public bool Connected()
        {
            return null == m_sessionsocket ? false : m_sessionsocket.Connected;
        }

        public void Disconnect(bool _check_connect = true)
        {
            try
            {
                if (true == _check_connect &&
                    null == m_sessionsocket)
                {
                    return;
                }

                if (null != m_sessionsocket)
                {
                    m_sessionsocket.Shutdown(SocketShutdown.Both);
                    m_sessionsocket.Close();
                    m_sessionsocket.Dispose();
                    m_sessionsocket = null;
                }

                if (null != m_event)
                {
                    m_event.OnDisconnect(this);
                }

                m_offset = 0;
                StateObject = null;
            }
            catch (Exception e)
            {
                if (null != m_event)
                {
                    m_event.OnError(SocketDefine.SocketErrorType.Disconnect, e, this);
                }
            }
        }

        public bool Send(byte[] data)
        {
            if (false == Connected() ||
                null == data ||
                0 >= data.Length ||
                m_bufsize < data.Length)
            {
                return false;
            }

            try
            {
                byte[] total_data = BitConverter.GetBytes(data.Length).Concat(data).ToArray();

                //SocketError error;
                //int sendsize = m_sessionsocket!.Send(total_data, 0, total_data.Length, SocketFlags.None, out error);

                SocketError error;
                m_sessionsocket!.BeginSend(total_data, 0, total_data.Length, SocketFlags.None, out error, SendCallback, this);

                if (SocketError.Success != error &&
                    SocketError.IOPending != error)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                if (null != m_event)
                {
                    m_event.OnError(SocketDefine.SocketErrorType.Send, e, this);
                }

                return false;
            }

            return false;
        }

        private void SendCallback(IAsyncResult ar)
        {
            if (null == m_sessionsocket)
            {
                return;
            }

            try
            {
                int sendsize = m_sessionsocket.EndSend(ar);

                if (null != m_event)
                {
                    m_event.OnSend(this);
                }
            }
            catch (Exception e)
            {
                if (null != m_event)
                {
                    m_event.OnError(SocketDefine.SocketErrorType.Send, e, this);
                }
            }
        }

        internal bool Receive()
        {
            try
            {
                if (false == Connected())
                {
                    return false;
                }

                SocketError error;
                m_sessionsocket!.BeginReceive(m_buf, 0, m_buf.Length, SocketFlags.None, out error, ReceiveCallback, this);

                if (SocketError.Success != error &&
                    SocketError.IOPending != error)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                if (null != m_event)
                {
                    m_event.OnError(SocketDefine.SocketErrorType.Receive, e, this);
                }

                return false;
            }

            return true;
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            int recvsize = 0;

            try
            {
                if (false == Connected())
                {
                    return;
                }

                recvsize = m_sessionsocket!.EndReceive(ar);

                if (0 >= recvsize)
                {
                    Disconnect(false);
                    return;
                }

                if (m_total_bufsize < m_offset + recvsize)
                {
                    Disconnect();
                    return;
                }

                Buffer.BlockCopy(m_buf, 0, m_total_buf, (int)m_offset, recvsize);
                m_offset += (uint)recvsize;

                int startoffset = 0;
                int packetsize = 0;
                while (startoffset + sizeof(int) <= m_offset)
                {
                    packetsize = BitConverter.ToInt32(m_total_buf, startoffset);

                    if (0 >= packetsize ||
                        startoffset + packetsize > m_total_bufsize)
                    {
                        Disconnect();
                        return;
                    }

                    if (startoffset + sizeof(int) + packetsize > m_offset)
                    {
                        break;
                    }

                    startoffset += sizeof(int);

                    byte[] packetdata = new byte[packetsize];
                    Buffer.BlockCopy(m_total_buf, startoffset, packetdata, 0, packetsize);
                    startoffset += packetsize;

                    if (null != m_event)
                    {
                        m_event.OnReceive(this, packetdata);
                    }
                }

                if (false == Connected())
                {
                    Disconnect(false);
                    return;
                }

                if (0 < startoffset)
                {
                    Buffer.BlockCopy(m_total_buf, startoffset, m_total_buf, 0, (int)(m_offset - (uint)startoffset));
                    m_offset -= (uint)startoffset;
                }

                Receive();
            }
            catch (SocketException e)
            {
                if (null != m_event)
                {
                    m_event.OnError(SocketDefine.SocketErrorType.Receive, e, this);
                }

                Disconnect(false);
            }
        }

        private Socket? m_sessionsocket;
        private TcpServerSocketEvent m_event;
        private byte[] m_buf;
        private uint m_bufsize;
        private byte[] m_total_buf;
        private uint m_total_bufsize;
        private uint m_offset;

        public object? StateObject;
    }
}
