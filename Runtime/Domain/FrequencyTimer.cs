namespace SWeetJelly.Timer.Domain
{
    using System;
    using SWeetJelly.Timer.Contract;

    public sealed class FrequencyTimer : TimerBase
    {
        private readonly float intervalSeconds;
        private readonly Action onTick;

        public FrequencyTimer(
            float intervalSeconds,
            Action onTick,
            TimerOptions options)
            : base(intervalSeconds, options)
        {
            if (intervalSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds));

            this.intervalSeconds = intervalSeconds;
            this.onTick = onTick ?? throw new ArgumentNullException(nameof(onTick));
        }

        public override bool IsFinished => false;

        public override float Progress
        {
            get
            {
                if (intervalSeconds <= 0f)
                    return 0f;

                return Clamp01(CurrentTime / intervalSeconds);
            }
        }

        protected override float GetStartTime()
        {
            return 0f;
        }

        protected override void OnTick(float deltaTime)
        {
            CurrentTime += deltaTime;

            while (State == TimerState.Running && CurrentTime >= intervalSeconds)
            {
                CurrentTime -= intervalSeconds;
                onTick.Invoke();
            }
        }

        protected override float GetRawProgress()
        {
            return CurrentTime / intervalSeconds;
        }
    }
}