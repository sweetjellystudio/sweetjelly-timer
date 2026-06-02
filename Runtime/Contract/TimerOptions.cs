namespace SWeetJelly.Timer.Contract
{
    public readonly struct TimerOptions
    {
        public bool UseUnscaledTime { get; }
        public float TickSpeed { get; }

        public TimerOptions(bool useUnscaledTime, float tickSpeed)
        {
            UseUnscaledTime = useUnscaledTime;
            TickSpeed = tickSpeed <= 0f ? 1f : tickSpeed;
        }

        public static TimerOptions Default => new TimerOptions(false, 1f);
    }
}