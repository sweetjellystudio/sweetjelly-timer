using System;
using UnityEngine;
using SWeetJelly.Core;

namespace SWeetJelly.TimerService
{
    public abstract class BaseTimer : IDisposable, ITimer
    {
        public float CurrentTime {get; protected set;}
        public bool IsRunning {get; protected set;}
        public abstract bool IsFinished{get;}

        public float Progress => Mathf.Clamp(CurrentTime / initialTime, 0, 1);

        public Action OnTimerStart = delegate{};
        public Action OnTimerStop = delegate{};
        public Action OnTimerPause = delegate{};
        public Action OnTimerReset = delegate{};

        private TimerManager timerManager;
        protected float initialTime;

        protected BaseTimer(float value)
        {
            initialTime = value;
            timerManager = ServiceLocator.Global.Get<TimerManager>();
        }

        public void Start()
        {
            CurrentTime = initialTime;
            if(!IsRunning)
            {
                IsRunning = true;
                timerManager.RegisterTimer(this);
                OnTimerStart.Invoke();
            }
        }

        public void Stop()
        {
            if(IsRunning)
            {
                IsRunning = false;
                OnTimerStop.Invoke();
                timerManager.UnregisterTimer(this);
            }
        }

        public abstract void Tick();

        public void Resume() => IsRunning = true;

        public void Pause()
        {
            IsRunning = false;
            OnTimerPause.Invoke();
        }

        public virtual void Reset()
        {
            CurrentTime = initialTime;
            OnTimerReset.Invoke();
        }

        public virtual void Reset(float newValue)
        {
            initialTime = newValue;
            Reset();
        }

        bool disposed;

        ~BaseTimer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposed) return;
            if(disposing)
            {
                timerManager.UnregisterTimer(this);
            }
            disposed = true;
        }
    }
}