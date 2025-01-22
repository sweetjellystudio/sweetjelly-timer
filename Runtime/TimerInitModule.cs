using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using SWeetJelly.Core;

namespace SWeetJelly.TimerService
{
    [RegisterModule("SWeetJelly Timer", false)]
    public class TimerInitModule: InitModule
    {
        public override string ModuleName => "SWeetJelly Timer";
        static PlayerLoopSystem timerSystem;

        private ITimerManager timerManager;

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public override void CreateComponent(GameObject holderObject)
        {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            timerManager = new TimerManager();

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

                    timerManager.Clear();
                }
            }
#endif
        }

        void RemoveTimerManager<T>(ref PlayerLoopSystem loop)
        {
            PlayerLoopUtils.RemoveSystem<T>(ref loop, in timerSystem);
        }

        bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
        {
            timerSystem = new PlayerLoopSystem()
            {
                type = typeof(TimerManager),
                updateDelegate = timerManager.UpdateTimers,
                subSystemList = null
            };
            return PlayerLoopUtils.InsertSystem<T>(ref loop, in timerSystem, index);
        }
    }
}