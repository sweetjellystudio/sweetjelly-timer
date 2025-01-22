using System;
using UnityEngine;

namespace SWeetJelly.TimerService
{
    /// <summary>
    /// Timer that ticks at a given frequency. ( N times per second )
    /// </summary>
    public class FrequencyTimer : BaseTimer
    {
        public int TicksPerSecond {get; private set;}
        public Action OnTick = delegate{};
        float timeThreshold;

        public FrequencyTimer(int ticksPerSecond) : base(0)
        {
            CalculateTimeThreshold(ticksPerSecond);
        }

        public override void Tick()
        {
            if(IsRunning && CurrentTime <= timeThreshold)
            {
                CurrentTime += initialTime;
            }

            if(IsRunning && CurrentTime > timeThreshold)
            {
                CurrentTime -= timeThreshold;
                OnTick.Invoke();
            }
        }

        public override bool IsFinished => !IsRunning;

        public override void Reset()
        {
            CurrentTime = 0;
        }

        public void Reset(int newTicksPerSecond)
        {
            CalculateTimeThreshold(newTicksPerSecond);
            Reset();
        }

        void CalculateTimeThreshold(int ticksPerSecond)
        {
            TicksPerSecond = ticksPerSecond;
            timeThreshold = 1f / TicksPerSecond;
        }
    }
}