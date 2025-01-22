using UnityEngine;

namespace SWeetJelly.TimerService
{
    /// <summary>
    /// Timer that counts up from zero to infinity.
    /// </summary>
    public class StopWatchTimer : BaseTimer
    {
        public StopWatchTimer() : base(0) {}

        public override void Tick()
        {
            if(IsRunning)
            {
                CurrentTime += Time.deltaTime;
            }
        }

        public override bool IsFinished => false;
    }
}