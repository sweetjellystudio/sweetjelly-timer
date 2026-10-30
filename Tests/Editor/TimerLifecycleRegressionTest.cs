namespace SWeetJelly.Timer.Tests.EditMode
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using SWeetJelly.Bootstrapper.App;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Timer.App;
    using SWeetJelly.Timer.Contract;
    using SWeetJelly.Timer.Framework.Bootstrapper;
    using SWeetJelly.Timer.Framework.Unity;
    using UnityEngine;
    using UnityEngine.LowLevel;

    public sealed class TimerLifecycleRegressionTest
    {
        [TestCase(TimerState.Stopped)]
        [TestCase(TimerState.Paused)]
        [TestCase(TimerState.Disposed)]
        public void Frequency_StateChangeInCallbackStopsCatchUp(TimerState state)
        {
            using var service = new TimerService();
            var count = 0;
            ITimerHandle handle = null;
            handle = service.StartFrequency(1, () =>
            {
                count++;
                if (state == TimerState.Stopped) handle.Stop();
                else if (state == TimerState.Paused) handle.Pause();
                else handle.Dispose();
            });
            service.Tick(3.2f, 3.2f);
            Assert.That(count, Is.EqualTo(1));
            Assert.That(handle.State, Is.EqualTo(state));
        }

        [Test]
        public void DisposeDuringTick_DisposesAllHandlesWithoutMutatingActiveIteration()
        {
            using var service = new TimerService();
            var first = service.StartFrequency(1, service.Dispose);
            var second = service.StartStopwatch();
            Assert.DoesNotThrow(() => service.Tick(1, 1));
            Assert.That(first.State, Is.EqualTo(TimerState.Disposed));
            Assert.That(second.State, Is.EqualTo(TimerState.Disposed));
        }

        [Test]
        public void Stopwatch_CallbackObservesElapsedTimeOnEveryTick()
        {
            using var service = new TimerService();
            var values = new List<float>();
            service.StartStopwatch(values.Add);
            service.Tick(0.25f, 0.25f);
            service.Tick(0.5f, 0.5f);
            Assert.That(values, Is.EqualTo(new[] { 0f, 0.25f, 0.75f }));
        }

        [Test]
        public void BootstrapFailure_DoesNotLeavePlayerLoopDriverInstalled()
        {
            var originalLoop = PlayerLoop.GetCurrentPlayerLoop();
            var before = CountDrivers(originalLoop);
            var registry = new ServiceRegistry();
            using var existing = new TimerService();
            registry.Register<ITimerService>(existing);
            var holder = new GameObject("Timer failed registration verification");
            var module = ScriptableObject.CreateInstance<TimerPackageBootstrapperSo>();
            var context = new PackageBootstrapContext(holder, null, null, registry);
            try
            {
                Assert.Throws<ArgumentException>(() => module.Bootstrap(context));
                Assert.That(CountDrivers(PlayerLoop.GetCurrentPlayerLoop()), Is.EqualTo(before));
                Assert.That(registry.Resolve<ITimerService>(), Is.SameAs(existing));
            }
            finally
            {
                module.Shutdown(context);
                PlayerLoop.SetPlayerLoop(originalLoop);
                UnityEngine.Object.DestroyImmediate(module);
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void TickReentry_IsRejectedWithoutCorruptingOuterIteration()
        {
            using var service = new TimerService();
            service.StartFrequency(1, () =>
                Assert.Throws<InvalidOperationException>(() => service.Tick(0.1f, 0.1f)));
            Assert.DoesNotThrow(() => service.Tick(1, 1));
        }

        [Test]
        public void ShutdownWithoutBootstrap_DoesNotUnregisterAnotherService()
        {
            using var existing = new TimerService();
            var registry = new ServiceRegistry();
            registry.Register<ITimerService>(existing);
            var holder = new GameObject("Timer shutdown ownership verification");
            var module = ScriptableObject.CreateInstance<TimerPackageBootstrapperSo>();
            try
            {
                module.Shutdown(new PackageBootstrapContext(holder, null, null, registry));
                Assert.That(registry.TryResolve<ITimerService>(out var actual), Is.True);
                Assert.That(actual, Is.SameAs(existing));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(module);
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }
        [TestCase(false, 3f)]
        [TestCase(true, 2f)]
        public void TickSpeed_UsesSelectedClock(bool unscaled, float expected)
        {
            using var service = new TimerService();
            var handle = service.StartCountdown(4, options: new TimerOptions(unscaled, 2));
            service.Tick(0.5f, 1);
            Assert.That(handle.CurrentTime, Is.EqualTo(expected));
        }

        [Test]
        public void CallbackFailure_PreservesExceptionAndAllowsNextTick()
        {
            using var service = new TimerService();
            var calls = 0;
            var error = new InvalidOperationException("timer callback failure");
            service.StartFrequency(1, () => { if (++calls == 1) throw error; });
            Assert.That(Assert.Throws<InvalidOperationException>(() => service.Tick(1, 1)), Is.SameAs(error));
            Assert.DoesNotThrow(() => service.Tick(1, 1));
            Assert.That(calls, Is.EqualTo(2));
        }

        [Test]
        public void TimerCreatedInCallback_StartsAdvancingNextTick()
        {
            using var service = new TimerService();
            ITimerHandle created = null;
            service.StartCountdown(1, () => created = service.StartStopwatch());
            service.Tick(1, 1);
            Assert.That(created.CurrentTime, Is.Zero);
            service.Tick(1, 1);
            Assert.That(created.CurrentTime, Is.EqualTo(1));
        }

        [Test]
        public void Stopwatch_PauseResumePreservesNotifications()
        {
            using var service = new TimerService();
            var values = new List<float>();
            var handle = service.StartStopwatch(values.Add);
            service.Tick(0.25f, 0.25f);
            handle.Pause();
            service.Tick(0.5f, 0.5f);
            handle.Resume();
            service.Tick(0.25f, 0.25f);
            Assert.That(values, Is.EqualTo(new[] { 0f, 0.25f, 0.25f, 0.5f }));
        }

        [Test]
        public void RepeatedBootstrapFailure_PreservesOriginalModuleOwnership()
        {
            var originalLoop = PlayerLoop.GetCurrentPlayerLoop();
            var registry = new ServiceRegistry();
            var holder = new GameObject("Timer repeated bootstrap verification");
            var module = ScriptableObject.CreateInstance<TimerPackageBootstrapperSo>();
            var context = new PackageBootstrapContext(holder, null, null, registry);
            try
            {
                module.Bootstrap(context);
                var first = registry.Resolve<ITimerService>();
                Assert.Throws<ArgumentException>(() => module.Bootstrap(context));
                Assert.That(registry.Resolve<ITimerService>(), Is.SameAs(first));
                module.Shutdown(context);
                Assert.That(CountDrivers(PlayerLoop.GetCurrentPlayerLoop()), Is.EqualTo(CountDrivers(originalLoop)));
            }
            finally
            {
                module.Shutdown(context);
                PlayerLoop.SetPlayerLoop(originalLoop);
                UnityEngine.Object.DestroyImmediate(module);
                UnityEngine.Object.DestroyImmediate(holder);
            }
        }
        [Test]
        public void Frequency_NullCallbackRetainsArgumentNullException()
        {
            using var service = new TimerService();
            var error = Assert.Throws<ArgumentNullException>(() => service.StartFrequency(1, null));
            Assert.That(error.ParamName, Is.EqualTo("onTick"));
        }
        private static int CountDrivers(PlayerLoopSystem system)
        {
            var count = system.type == typeof(PlayerLoopTimerDriver) ? 1 : 0;
            if (system.subSystemList != null)
                foreach (var child in system.subSystemList) count += CountDrivers(child);
            return count;
        }
    }
}
