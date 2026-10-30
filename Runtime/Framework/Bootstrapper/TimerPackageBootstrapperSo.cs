namespace SWeetJelly.Timer.Framework.Bootstrapper
{
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Timer.App;
    using SWeetJelly.Timer.Contract;
    using SWeetJelly.Timer.Framework.Unity;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Timer Package Bootstrapper", menuName = "SWeetJelly/Bootstrapper/Packages/Timer")]
    public class TimerPackageBootstrapperSo : PackageBootstrapperSo
    {
        private PlayerLoopTimerDriver driver;
        private TimerService service;

        public override string PackageName => "Timer";

        public override void Bootstrap(PackageBootstrapContext context)
        {
            var newService = new TimerService();
            var newDriver = new PlayerLoopTimerDriver(newService);
            try
            {
                newDriver.Install();
                context.Services.Register<ITimerService>(newService);
            }
            catch
            {
                newDriver.Dispose();
                newService.Dispose();
                throw;
            }
            service = newService;
            driver = newDriver;
        }

        public override void Shutdown(PackageBootstrapContext context)
        {
            if (service == null)
                return;
            driver?.Dispose();
            service.Dispose();
            if (context.Services.TryResolve<ITimerService>(out var registered)
                && ReferenceEquals(registered, service))
                context.Services.Unregister<ITimerService>();
            driver = null;
            service = null;
        }
    }
}
