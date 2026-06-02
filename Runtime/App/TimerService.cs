namespace SWeetJelly.Timer.App
{
    using System;
    using System.Collections.Generic;
    using SWeetJelly.Timer.Contract;
    using SWeetJelly.Timer.Domain;

    #nullable enable

    public sealed class TimerService : ITimerService, ITimerTicker, IDisposable
    {
        private readonly List<TimerBase> timers = new();
        private readonly List<TimerBase> sweep = new();

        private bool disposed;

        public ITimerHandle StartCountdown(
            float seconds,
            Action? onCompleted = null,
            TimerOptions? options = null)
        {
            ThrowIfDisposed();

            var timer = new CountdownTimer(
                seconds,
                options ?? TimerOptions.Default);

            if (onCompleted != null)
                timer.Completed += _ => onCompleted.Invoke();

            RegisterAndStart(timer);
            return timer;
        }

        public ITimerHandle StartFrequency(
            float intervalSeconds,
            Action onTick,
            TimerOptions? options = null)
        {
            ThrowIfDisposed();

            if (intervalSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds));

            var timer = new FrequencyTimer(
                intervalSeconds,
                onTick,
                options ?? TimerOptions.Default);

            RegisterAndStart(timer);
            return timer;
        }

        public ITimerHandle StartStopwatch(
            Action<float>? onTick = null,
            TimerOptions? options = null)
        {
            ThrowIfDisposed();

            var timer = new StopwatchTimer(options ?? TimerOptions.Default);

            if (onTick != null)
            {
                timer.Started += _ => onTick.Invoke(timer.CurrentTime);
                timer.Resumed += _ => onTick.Invoke(timer.CurrentTime);
            }

            RegisterAndStart(timer);
            return timer;
        }

        public void Tick(float deltaTime, float unscaledDeltaTime)
        {
            if (disposed)
                return;

            if (timers.Count == 0)
                return;

            sweep.Clear();
            sweep.AddRange(timers);

            foreach (var timer in sweep)
            {
                if (timer.State == TimerState.Disposed ||
                    timer.State == TimerState.Stopped ||
                    timer.State == TimerState.Completed)
                {
                    timers.Remove(timer);
                    continue;
                }

                var dt = timer.UseUnscaledTime ? unscaledDeltaTime : deltaTime;
                dt *= timer.TickSpeed;

                timer.Tick(dt);

                if (timer.State == TimerState.Disposed ||
                    timer.State == TimerState.Stopped ||
                    timer.State == TimerState.Completed)
                {
                    timers.Remove(timer);
                }
            }

            sweep.Clear();
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            sweep.Clear();
            sweep.AddRange(timers);

            foreach (var timer in sweep)
                timer.Dispose();

            timers.Clear();
            sweep.Clear();
        }

        private void RegisterAndStart(TimerBase timer)
        {
            timers.Add(timer);
            timer.Start();
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(TimerService));
        }
    }
}