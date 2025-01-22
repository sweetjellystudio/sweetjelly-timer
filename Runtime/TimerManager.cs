using System;
using System.Collections.Generic;

namespace SWeetJelly.TimerService
{
    public class TimerManager : ITimerManager
    {
        private readonly List<ITimer> timers = new();
        private readonly List<ITimer> sweep = new();

        public void RegisterTimer(ITimer timer)
        {
            if(!timers.Contains(timer))
            {
                timers.Add(timer);
            }
        }

        public void UnregisterTimer(ITimer timer)
        {
            if(timers.Contains(timer))
            {
                timers.Remove(timer);
            }
        }

        public void UpdateTimers()
        {
            if(timers.Count == 0) return;

            sweep.RefreshWith(timers);
            foreach(var timer in sweep)
            {
                timer.Tick();
            }
        }

        public void Clear()
        {
            sweep.RefreshWith(timers);
            foreach(var timer in sweep)
            {
                if(timer is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                else
                {
                    //timer.Stop();
                }
            }
            timers.Clear();
            sweep.Clear();
        }
    }
}