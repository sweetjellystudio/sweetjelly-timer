namespace SWeetJelly.Timer.Contract
{
    using System;

    #nullable enable

    public interface ITimerService
    {
        ITimerHandle StartCountdown(float seconds, Action? onCompleted = null, TimerOptions? options = null);

        ITimerHandle StartFrequency(float intervalSeconds, Action onTick, TimerOptions? options = null);

        ITimerHandle StartStopwatch(Action<float>? onTick = null, TimerOptions? options = null);
    }
}
