namespace SWeetJelly.Timer.Framework.Bootstrapper
{
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using UnityEngine;
    using SWeetJelly.Timer.Contract;
    using SWeetJelly.Timer.App;
    using SWeetJelly.Timer.Framework.Unity;
    [CreateAssetMenu(
        fileName = "Timer Package Bootstrapper", 
        menuName = "SWeetJelly/Bootstrapper/Packages/Timer")]
    public class TimerPackageBootstrapperSo : PackageBootstrapperSo
    {
        private PlayerLoopTimerDriver driver;
        private TimerService service;

        public override string PackageName => "Timer";

        public override void Bootstrap(PackageBootstrapContext context)
        {
            service = new TimerService();

            driver = new PlayerLoopTimerDriver(service);
            driver.Install();

            context.Services.Register<ITimerService>(service);
        }

        public override void Shutdown(PackageBootstrapContext context)
        {
            driver?.Uninstall();
            service?.Dispose();

            context.Services.Unregister<ITimerService>();

            driver = null;
            service = null;
        }

    }
}