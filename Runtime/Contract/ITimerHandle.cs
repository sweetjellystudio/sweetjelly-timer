namespace SWeetJelly.Timer.Contract
{
    using System;

    public interface ITimerHandle : IProgressable, IDisposable
    {
        float CurrentTime { get; }
        float InitialTime { get; }
        TimerState State { get; }

        bool IsRunning { get; }

        event Action<ITimerHandle> Started;
        event Action<ITimerHandle> Paused;
        event Action<ITimerHandle> Resumed;
        event Action<ITimerHandle> Stopped;
        event Action<ITimerHandle> Completed;
        event Action<ITimerHandle> ResetCompleted;

        void Pause();
        void Resume();
        void Stop();
        void Reset();
        void Reset(float time);
    }
}