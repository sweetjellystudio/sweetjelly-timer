namespace SWeetJelly.Timer.Domain
{
    using SWeetJelly.Timer.Contract;

    public sealed class StopwatchTimer : TimerBase
    {
        public StopwatchTimer(TimerOptions options)
            : base(0f, options)
        {
        }

        public override bool IsFinished => false;

        public override float Progress => 0f;

        protected override float GetStartTime()
        {
            return 0f;
        }

        protected override void OnTick(float deltaTime)
        {
            CurrentTime += deltaTime;
        }

        protected override float GetRawProgress()
        {
            return 0f;
        }
    }
}