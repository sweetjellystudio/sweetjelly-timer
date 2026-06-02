namespace SWeetJelly.Timer.Domain
{
    using SWeetJelly.Timer.Contract;

    /// <summary>
    /// Timer that counts down from a given value to zero.
    /// </summary>
    public class CountdownTimer : TimerBase
    {
        public CountdownTimer(float seconds, TimerOptions options) : base(seconds, options)
        {
        }

        public override bool IsFinished => CurrentTime <= 0;

        protected override void OnTick(float deltaTime)
        {
            CurrentTime -= deltaTime;

            if(CurrentTime <= 0)
            {
                CurrentTime = 0;
            }
        }

        protected override float GetRawProgress()
        {
            if (InitialTime <= 0f)
                return 1f;

            return 1f - (CurrentTime / InitialTime);
        }
    }
}