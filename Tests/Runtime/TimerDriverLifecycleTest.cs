namespace SWeetJelly.Timer.Tests.PlayMode
{
    using System;
    using System.Collections;
    using NUnit.Framework;
    using SWeetJelly.Bootstrapper.App;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Timer.App;
    using SWeetJelly.Timer.Contract;
    using SWeetJelly.Timer.Framework.Bootstrapper;
    using SWeetJelly.Timer.Framework.Unity;
    using UnityEngine;
    using UnityEngine.LowLevel;
    using UnityEngine.TestTools;

    public sealed class TimerDriverLifecycleTest
    {
        [UnityTest]
        public IEnumerator Drivers_InstallOnceAndUninstallIndependently()
        {
            var original = PlayerLoop.GetCurrentPlayerLoop();
            var firstTicker = new RecordingTicker();
            var secondTicker = new RecordingTicker();
            using var first = new PlayerLoopTimerDriver(firstTicker);
            using var second = new PlayerLoopTimerDriver(secondTicker);
            try
            {
                first.Install();
                first.Install();
                second.Install();
                Assert.That(Count(PlayerLoop.GetCurrentPlayerLoop()), Is.EqualTo(Count(original) + 2));
                yield return null;
                yield return null;
                Assert.That(firstTicker.Count, Is.GreaterThan(0));
                first.Uninstall();
                var firstCount = firstTicker.Count;
                var secondCount = secondTicker.Count;
                yield return null;
                yield return null;
                Assert.That(firstTicker.Count, Is.EqualTo(firstCount));
                Assert.That(secondTicker.Count, Is.GreaterThan(secondCount));
                Assert.That(Count(PlayerLoop.GetCurrentPlayerLoop()), Is.EqualTo(Count(original) + 1));
            }
            finally
            {
                first.Dispose();
                second.Dispose();
                PlayerLoop.SetPlayerLoop(original);
            }
        }

        [UnityTest]
        public IEnumerator BootstrapShutdown_DisposesHandleAndRemovesDriver()
        {
            var original = PlayerLoop.GetCurrentPlayerLoop();
            var registry = new ServiceRegistry();
            var holder = new GameObject("Timer lifecycle consumer");
            var module = ScriptableObject.CreateInstance<TimerPackageBootstrapperSo>();
            var context = new PackageBootstrapContext(holder, null, null, registry);
            try
            {
                module.Bootstrap(context);
                var service = registry.Resolve<ITimerService>();
                var handle = service.StartStopwatch();
                yield return null;
                yield return null;
                Assert.That(handle.CurrentTime, Is.GreaterThan(0));
                module.Shutdown(context);
                Assert.That(handle.State, Is.EqualTo(TimerState.Disposed));
                Assert.That(registry.TryResolve<ITimerService>(out _), Is.False);
                Assert.That(Count(PlayerLoop.GetCurrentPlayerLoop()), Is.EqualTo(Count(original)));
                Assert.Throws<ObjectDisposedException>(() => service.StartCountdown(1));
            }
            finally
            {
                module.Shutdown(context);
                PlayerLoop.SetPlayerLoop(original);
                UnityEngine.Object.Destroy(module);
                UnityEngine.Object.Destroy(holder);
            }
        }

        private static int Count(PlayerLoopSystem system)
        {
            var count = system.type == typeof(PlayerLoopTimerDriver) ? 1 : 0;
            if (system.subSystemList != null)
                foreach (var child in system.subSystemList) count += Count(child);
            return count;
        }

        private sealed class RecordingTicker : ITimerTicker
        {
            public int Count;
            public void Tick(float deltaTime, float unscaledDeltaTime) => Count++;
        }
    }
}
