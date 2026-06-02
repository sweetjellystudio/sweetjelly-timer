namespace SWeetJelly.Timer.InitialiserIntegration
{
    using SWeetJelly.Initialiser.Contract;
    using SWeetJelly.Initialiser.Framework;
    using UnityEngine;
    using SWeetJelly.Timer.Contract;
    using SWeetJelly.Timer.App;
    using SWeetJelly.Timer.Framework;

    [RegisterModule("Timer")]
    [CreateAssetMenu(
        fileName = "Timer Init Module", 
        menuName = "SWeetJelly/Initialiser/Modules/Timer")]
    public class TimerInitModule: InitModuleSo
    {
        private PlayerLoopTimerDriver driver;
        private TimerService service;

        public override string ModuleName => "Timer";

        public override void Install(InitModuleInstallContext context)
        {
            service = new TimerService();

            driver = new PlayerLoopTimerDriver(service);
            driver.Install();

            context.Services.Register<ITimerService>(service);
        }

        public void Uninstall(InitModuleInstallContext context)
        {
            driver?.Uninstall();
            service?.Dispose();

            context.Services.Unregister<ITimerService>();

            driver = null;
            service = null;
        }

    }
}