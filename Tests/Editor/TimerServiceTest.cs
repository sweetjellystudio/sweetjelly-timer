namespace SWeetJelly.Timer.Tests.EditMode
{
    using System;
    using NUnit.Framework;
    using SWeetJelly.Timer.App;
    using SWeetJelly.Timer.Contract;

    public sealed class TimerServiceTest
    {
        [Test]
        public void StartCountdown_WhenTickedToZero_CompletesAndInvokesCallback()
        {
            var service = new TimerService();
            var completedCount = 0;

            var handle = service.StartCountdown(
                1f,
                () => completedCount++);

            service.Tick(0.4f, 0.4f);

            Assert.That(handle.State, Is.EqualTo(TimerState.Running));
            Assert.That(handle.CurrentTime, Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(handle.Progress, Is.EqualTo(0.4f).Within(0.0001f));

            service.Tick(0.6f, 0.6f);

            Assert.That(handle.State, Is.EqualTo(TimerState.Completed));
            Assert.That(handle.CurrentTime, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(handle.Progress, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(completedCount, Is.EqualTo(1));

            service.Dispose();
        }

        [Test]
        public void StartCountdown_WhenPaused_DoesNotAdvanceUntilResumed()
        {
            var service = new TimerService();
            var handle = service.StartCountdown(1f);

            handle.Pause();
            service.Tick(0.5f, 0.5f);

            Assert.That(handle.State, Is.EqualTo(TimerState.Paused));
            Assert.That(handle.CurrentTime, Is.EqualTo(1f).Within(0.0001f));

            handle.Resume();
            service.Tick(0.25f, 0.25f);

            Assert.That(handle.State, Is.EqualTo(TimerState.Running));
            Assert.That(handle.CurrentTime, Is.EqualTo(0.75f).Within(0.0001f));

            service.Dispose();
        }

        [Test]
        public void StartFrequency_WhenTickPassesMultipleIntervals_InvokesCallbackPerInterval()
        {
            var service = new TimerService();
            var tickCount = 0;

            var handle = service.StartFrequency(
                0.5f,
                () => tickCount++);

            service.Tick(1.2f, 1.2f);

            Assert.That(tickCount, Is.EqualTo(2));
            Assert.That(handle.CurrentTime, Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(handle.Progress, Is.EqualTo(0.4f).Within(0.0001f));

            service.Dispose();
        }

        [Test]
        public void StartStopwatch_WhenTicked_AccumulatesElapsedTime()
        {
            var service = new TimerService();

            var handle = service.StartStopwatch();

            service.Tick(0.25f, 0.25f);
            service.Tick(0.5f, 0.5f);

            Assert.That(handle.State, Is.EqualTo(TimerState.Running));
            Assert.That(handle.CurrentTime, Is.EqualTo(0.75f).Within(0.0001f));
            Assert.That(handle.Progress, Is.EqualTo(0f));

            service.Dispose();
        }

        [Test]
        public void Tick_WithUnscaledTimer_UsesUnscaledDeltaTime()
        {
            var service = new TimerService();

            var handle = service.StartCountdown(
                1f,
                options: new TimerOptions(useUnscaledTime: true, tickSpeed: 1f));

            service.Tick(0.1f, 0.4f);

            Assert.That(handle.CurrentTime, Is.EqualTo(0.6f).Within(0.0001f));

            service.Dispose();
        }

        [Test]
        public void StartFrequency_WithZeroInterval_ThrowsArgumentOutOfRangeException()
        {
            var service = new TimerService();

            var exception = AssertThrows<ArgumentOutOfRangeException>(() =>
                service.StartFrequency(0f, () => { }));

            Assert.That(exception.ParamName, Is.EqualTo("intervalSeconds"));

            service.Dispose();
        }

        [Test]
        public void StartCountdown_AfterDispose_ThrowsObjectDisposedException()
        {
            var service = new TimerService();
            service.Dispose();

            var exception = AssertThrows<ObjectDisposedException>(() =>
                service.StartCountdown(1f));

            Assert.That(exception.ObjectName, Is.EqualTo(nameof(TimerService)));
        }

        private static TException AssertThrows<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException exception)
            {
                return exception;
            }

            Assert.Fail($"Expected exception of type {typeof(TException).Name}.");
            return null;
        }
    }
}
