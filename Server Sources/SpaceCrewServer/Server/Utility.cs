using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpaceCrewServer.Server
{
    public static class Utility
    {
        public abstract class Singleton<T> where T : class, new()
        {
            private static T? m_instance = null;
            public static T Instance
            {
                get
                {
                    m_instance = m_instance ?? new T();
                    return m_instance;
                }
                private set { }
            }

            public abstract bool Initialize();
            public abstract void Release();
        }

        public class UpdateTimer
        {
            private class UpdateTimeData
            {
                public UpdateTimeData(Action _func, double _delaysecond)
                {
                    Func = _func;
                    DelaySecond = _delaysecond;
                    RunTime = DateTime.MinValue;
                }

                public Action Func;
                public double DelaySecond;
                public DateTime RunTime;
            }

            public UpdateTimer()
            {
                m_bag = new ConcurrentBag<UpdateTimeData>();
            }

            public void RegistFunction(Action _func, double _delaysecond = 0.0)
            {
                m_bag.Add(new UpdateTimeData(_func, _delaysecond));
            }

            public void UpdateProcess()
            {
                IEnumerator<UpdateTimeData> iter = m_bag.GetEnumerator();
                while(iter.MoveNext())
                {
                    if(0.0 < iter.Current.DelaySecond)
                    {
                        if(iter.Current.RunTime.AddSeconds(iter.Current.DelaySecond) <= DateTime.Now)
                        {
                            iter.Current.RunTime = DateTime.Now;
                            iter.Current.Func();
                        }
                    }
                    else
                    {
                        iter.Current.Func();
                    }
                }
            }

            private ConcurrentBag<UpdateTimeData> m_bag;
        }
    }
}
