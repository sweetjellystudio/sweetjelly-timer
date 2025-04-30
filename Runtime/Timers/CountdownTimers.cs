using SWeetJelly.Core;

namespace SWeetJelly.TimerService
{
    /// <summary>
    /// Timer that counts down from a given value to zero.
    /// </summary>
    public class CountdownTimer : BaseTimer
    {
        public CountdownTimer(float value, bool useUnscaledTime = false, float tickSpeed = 1f) : base(value, useUnscaledTime, tickSpeed) {}

        public override void Tick(float deltaTime)
        {
            if(!IsRunning)
            {
                return;
            }

            CurrentTime -= deltaTime;

            if(CurrentTime <= 0)
            {
                Stop();
            }
        }

        public override bool IsFinished => CurrentTime <= 0;
    }
}