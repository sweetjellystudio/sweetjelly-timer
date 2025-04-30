using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using SWeetJelly.Core;

namespace SWeetJelly.TimerService
{
    public class TimerManager : BaseTimerManager
    {
        private readonly List<BaseTimer> timers = new();
        private readonly List<BaseTimer> sweep = new();

        static PlayerLoopSystem timerSystem;

        public override void Initialise()
        {
            if(IsInitialised) return;

            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if(!InsertTimerManager<Update>(ref currentPlayerLoop, 0))
            {
                Debug.LogWarning("Timers not initialized, unable to register TimerManager into Update loop");
                return;
            }

            PlayerLoop.SetPlayerLoop(currentPlayerLoop);

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;

            void OnPlayModeState(PlayModeStateChange state)
            {
                if(state == PlayModeStateChange.ExitingPlayMode)
                {
                    PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
                    RemoveTimerManager<Update>(ref currentPlayerLoop);
                    PlayerLoop.SetPlayerLoop(currentPlayerLoop);

                    Unload();
                }
            }

#endif

            ServiceLocator.Global.Register<BaseTimerManager>(this);

            IsInitialised = true;
            OnInitialised();
        }

        void RemoveTimerManager<T>(ref PlayerLoopSystem loop)
        {
            PlayerLoopUtils.RemoveSystem<T>(ref loop, in timerSystem);
        }

        bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
        {
            timerSystem = new PlayerLoopSystem()
            {
                type = typeof(BaseTimerManager),
                updateDelegate = UpdateTimers,
                subSystemList = null
            };
            return PlayerLoopUtils.InsertSystem<T>(ref loop, in timerSystem, index);
        }

        public override void RegisterTimer(BaseTimer timer)
        {
            if(!timers.Contains(timer))
            {
                timers.Add(timer);
            }
        }

        public override void UnregisterTimer(BaseTimer timer)
        {
            if(timers.Contains(timer))
            {
                timers.Remove(timer);
            }
        }

        public override void UpdateTimers()
        {
            if(timers.Count == 0) return;

            sweep.RefreshWith(timers);
            foreach(var timer in sweep)
            {
                float deltaTime = timer.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                deltaTime *= timer.TickSpeed;
                timer.Tick(deltaTime);
            }
        }

        public override void Unload()
        {
            sweep.RefreshWith(timers);
            foreach(var timer in sweep)
            {
                if(timer is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                else
                {
                    timer.Stop();
                }
            }
            timers.Clear();
            sweep.Clear();
        }
    }
}
