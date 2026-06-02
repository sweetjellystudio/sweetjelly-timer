namespace SWeetJelly.Timer.Domain
{
    using System;
    using SWeetJelly.Timer.Contract;

    #nullable enable

    public abstract class TimerBase : ITimerHandle
    {
        public float CurrentTime { get; protected set; }
        public float InitialTime { get; private set; }

        public bool UseUnscaledTime { get; }
        public float TickSpeed { get; }

        public TimerState State { get; private set; } = TimerState.Idle;
        public bool IsRunning => State == TimerState.Running;

        public abstract bool IsFinished { get; }

        public virtual float Progress
        {
            get
            {
                if (InitialTime <= 0f)
                    return IsFinished ? 1f : 0f;

                return Clamp01(GetRawProgress());
            }
        }

        public event Action<ITimerHandle>? Started;
        public event Action<ITimerHandle>? Paused;
        public event Action<ITimerHandle>? Resumed;
        public event Action<ITimerHandle>? Stopped;
        public event Action<ITimerHandle>? Completed;
        public event Action<ITimerHandle>? ResetCompleted;

        private bool disposed;

        protected TimerBase(
            float initialTime,
            TimerOptions options)
        {
            if (initialTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(initialTime));

            InitialTime = initialTime;
            CurrentTime = initialTime;

            UseUnscaledTime = options.UseUnscaledTime;
            TickSpeed = options.TickSpeed;
        }

        public void Start()
        {
            ThrowIfDisposed();

            CurrentTime = GetStartTime();
            State = TimerState.Running;
            Started?.Invoke(this);
        }

        public void Pause()
        {
            ThrowIfDisposed();

            if (State != TimerState.Running)
                return;

            State = TimerState.Paused;
            Paused?.Invoke(this);
        }

        public void Resume()
        {
            ThrowIfDisposed();

            if (State != TimerState.Paused)
                return;

            State = TimerState.Running;
            Resumed?.Invoke(this);
        }

        public void Stop()
        {
            if (disposed)
                return;

            if (State == TimerState.Stopped || State == TimerState.Completed)
                return;

            State = TimerState.Stopped;
            Stopped?.Invoke(this);
        }

        public virtual void Reset()
        {
            ThrowIfDisposed();

            CurrentTime = GetStartTime();
            State = TimerState.Idle;
            ResetCompleted?.Invoke(this);
        }

        public virtual void Reset(float time)
        {
            if (time < 0f)
                throw new ArgumentOutOfRangeException(nameof(time));

            InitialTime = time;
            Reset();
        }

        public void Tick(float deltaTime)
        {
            ThrowIfDisposed();

            if (State != TimerState.Running)
                return;

            if (deltaTime <= 0f)
                return;

            OnTick(deltaTime);

            if (IsFinished)
            {
                State = TimerState.Completed;
                Completed?.Invoke(this);
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            State = TimerState.Disposed;
        }

        protected abstract void OnTick(float deltaTime);

        protected virtual float GetStartTime()
        {
            return InitialTime;
        }

        protected abstract float GetRawProgress();

        protected static float Clamp01(float value)
        {
            if (value < 0f)
                return 0f;

            if (value > 1f)
                return 1f;

            return value;
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}