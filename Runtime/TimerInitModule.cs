using UnityEngine;
using SWeetJelly.Core;

namespace SWeetJelly.TimerService
{
    [RegisterModule("SWeetJelly Timer", false)]
    public class TimerInitModule: InitModule
    {
        public override string ModuleName => "SWeetJelly Timer";
        private BaseTimerManager timerManager;

        public override void CreateComponent(GameObject holderObject)
        {
            timerManager = new TimerManager();

            timerManager.Initialise();
        }
    }
}