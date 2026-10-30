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
        private readonly Dictionary<TimerBase, Action<float>> tickCallbacks = new();
        private bool disposed;
        private bool ticking;

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
                tickCallbacks.Add(timer, onTick);
            }

            RegisterAndStart(timer);
            return timer;
        }

        public void Tick(float deltaTime, float unscaledDeltaTime)
        {
            if (disposed)
                return;
            if (ticking)
                throw new InvalidOperationException("TimerService.Tick cannot be reentered.");
            if (timers.Count == 0)
                return;

            ticking = true;
            try
            {
                sweep.Clear();
                sweep.AddRange(timers);
                foreach (var timer in sweep)
                {
                    if (IsTerminal(timer))
                    {
                        timers.Remove(timer);
                        tickCallbacks.Remove(timer);
                        continue;
                    }

                    var dt = timer.UseUnscaledTime ? unscaledDeltaTime : deltaTime;
                    dt *= timer.TickSpeed;
                    timer.Tick(dt);
                    if (disposed)
                        break;

                    if (dt > 0f && timer.IsRunning && tickCallbacks.TryGetValue(timer, out var callback))
                        callback(timer.CurrentTime);
                    if (disposed)
                        break;

                    if (IsTerminal(timer))
                    {
                        timers.Remove(timer);
                        tickCallbacks.Remove(timer);
                    }
                }
            }
            finally
            {
                sweep.Clear();
                ticking = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            foreach (var timer in timers)
                timer.Dispose();
            timers.Clear();
            tickCallbacks.Clear();
            if (!ticking)
                sweep.Clear();
        }

        private static bool IsTerminal(TimerBase timer) => timer.State == TimerState.Disposed
            || timer.State == TimerState.Stopped || timer.State == TimerState.Completed;

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
