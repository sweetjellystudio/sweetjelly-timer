using UnityEngine;

namespace SWeetJelly.TimerService
{
    /// <summary>
    /// Timer that counts down from a given value to zero.
    /// </summary>
    public class CountdownTimer : BaseTimer
    {
        public CountdownTimer(float value) : base(value) {}

        public override void Tick()
        {
            if(IsRunning && CurrentTime <= 0)
            {
                Stop();
            }

            if(IsRunning && CurrentTime > 0)
            {
                CurrentTime -= Time.deltaTime;
            }
        }

        public override bool IsFinished => CurrentTime <= 0;
    }
}