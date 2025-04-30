using SWeetJelly.Core;

namespace SWeetJelly.TimerService
{
    /// <summary>
    /// Timer that counts up from zero to infinity.
    /// </summary>
    public class StopWatchTimer : BaseTimer
    {
        public StopWatchTimer(bool useUnscaledTime = false, float tickSpeed = 1f) : base(0, useUnscaledTime, tickSpeed) {}

        public override void Tick(float deltaTime)
        {
            if(IsRunning)
            {
                CurrentTime += deltaTime;
            }
        }

        public override bool IsFinished => false;
    }
}