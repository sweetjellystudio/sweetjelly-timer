using SWeetJelly.Core;

namespace SWeetJelly.TimerService
{
    /// <summary>
    /// Timer that ticks at a given frequency. ( N times per second )
    /// </summary>
    public class FrequencyTimer : BaseTimer
    {
        public int TicksPerSecond {get; private set;}
        float timeThreshold;

        public FrequencyTimer(int ticksPerSecond, bool useUnscaledTime = false, float tickSpeed = 1f) : base(0, useUnscaledTime, tickSpeed)
        {
            CalculateTimeThreshold(ticksPerSecond);
        }

        public override void Tick(float deltaTime)
        {
            if(!IsRunning)
            {
                return;
            }

            CurrentTime += deltaTime;

            while(CurrentTime > timeThreshold)
            {
                CurrentTime -= timeThreshold;
                OnStopped();
                OnStarted();
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