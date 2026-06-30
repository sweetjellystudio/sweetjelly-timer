namespace SWeetJelly.Timer.Framework.Unity
{
    using System;
    using SWeetJelly.Timer.App;
    using UnityEngine;
    using UnityEngine.LowLevel;
    using UnityEngine.PlayerLoop;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    public sealed class PlayerLoopTimerDriver : IDisposable
    {
        private readonly ITimerTicker ticker;

        private PlayerLoopSystem timerSystem;
        private bool installed;

        public PlayerLoopTimerDriver(ITimerTicker ticker)
        {
            this.ticker = ticker ?? throw new ArgumentNullException(nameof(ticker));
        }

        public void Install()
        {
            if (installed)
                return;

            var loop = PlayerLoop.GetCurrentPlayerLoop();

            timerSystem = new PlayerLoopSystem
            {
                type = typeof(PlayerLoopTimerDriver),
                updateDelegate = Update,
                subSystemList = null
            };

            if (!PlayerLoopUtils.InsertSystem<Update>(ref loop, in timerSystem, 0))
            {
                Debug.LogWarning("[PlayerLoopTimerDriver] Failed to install timer system.");
                return;
            }

            PlayerLoop.SetPlayerLoop(loop);
            installed = true;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        }

        public void Uninstall()
        {
            if (!installed)
                return;

            var loop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopUtils.RemoveSystem<Update>(ref loop, in timerSystem);
            PlayerLoop.SetPlayerLoop(loop);

            installed = false;

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif
        }

        public void Dispose()
        {
            Uninstall();
        }

        private void Update()
        {
            ticker.Tick(Time.deltaTime, Time.unscaledDeltaTime);
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingPlayMode)
                return;

            Uninstall();
        }
#endif
    }
}