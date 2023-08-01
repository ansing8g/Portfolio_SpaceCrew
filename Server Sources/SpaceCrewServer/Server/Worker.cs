using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

using SpaceCrewServer.Game;

namespace SpaceCrewServer.Server
{
    public class Worker
    {
        public Worker()
        {
            m_run = false;
            m_thread = new Thread(Work);
            m_dicRoom = new ConcurrentDictionary<uint, IRoom>();
        }

        public void RunStart()
        {
            m_run = true;
            m_thread.Start();
        }

        public void RunEnd()
        {
            m_run = false;
            m_thread.Join();
        }

        public bool AddRoom(IRoom _room)
        {
            return m_dicRoom.TryAdd(_room.RoomIndex, _room);
        }

        public void RemoveRoom(uint _roomindex)
        {
            m_dicRoom.TryRemove(_roomindex, out IRoom? room);
        }

        private void Work()
        {
            while (m_run)
            {
                IEnumerator<KeyValuePair<uint, IRoom>> iter = m_dicRoom.GetEnumerator();
                while (iter.MoveNext())
                {
                    iter.Current.Value.Update();
                }

                Thread.Sleep(1);
            }
        }

        private bool m_run;
        private Thread m_thread;
        private ConcurrentDictionary<uint, IRoom> m_dicRoom;
    }
}
