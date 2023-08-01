using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace UtilityLib
{
    public class TimerManager
    {
        public TimerManager()
        {
            m_listTimer = new List<Timer>();
        }

        public void Regist(double _interval_milliseconds, Func<Task> _func, bool _loop = true)
        {
            Timer timer = new Timer();
            timer.Interval = _interval_milliseconds;
            timer.Elapsed += async (sender, e) => await _func();
            timer.AutoReset = _loop;
            timer.Start();
            
            m_listTimer.Add(timer);
        }

        public void Regist(double _interval_milliseconds, Action _func, bool _loop = true)
        {
            Timer timer = new Timer();
            timer.Interval = _interval_milliseconds;
            timer.Elapsed += (sender, e) => _func();
            timer.AutoReset = _loop;
            timer.Start();

            m_listTimer.Add(timer);
        }

        private List<Timer> m_listTimer;
    }
}
