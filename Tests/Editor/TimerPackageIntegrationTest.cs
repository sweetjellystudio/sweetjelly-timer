namespace SWeetJelly.Timer.Tests.EditMode
{
    using NUnit.Framework;
    using SWeetJelly.Bootstrapper.App;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Timer.Contract;
    using SWeetJelly.Timer.Framework.Bootstrapper;
    using UnityEngine;

    public sealed class TimerPackageIntegrationTest
    {
        [Test]
        public void TimerPackageBootstrapperSo_RegistersTimerService()
        {
            var holder = new GameObject("Timer Test Holder");
            var module = ScriptableObject.CreateInstance<TimerPackageBootstrapperSo>();

            try
            {
                var registry = new ServiceRegistry();
                var context = new PackageBootstrapContext(holder, null, null, registry);

                module.Bootstrap(context);

                Assert.That(registry.TryResolve<ITimerService>(out var service), Is.True);
                Assert.That(service, Is.Not.Null);

                module.Shutdown(context);

                Assert.That(registry.TryResolve<ITimerService>(out _), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(module);
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }
    }
}
