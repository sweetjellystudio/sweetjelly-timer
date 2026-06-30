namespace SWeetJelly.Timer.Tests.PlayMode
{
    using System.Collections;
    using NUnit.Framework;
    using SWeetJelly.Timer.App;
    using SWeetJelly.Timer.Framework.Unity;
    using UnityEngine.TestTools;

    public sealed class TimerPlayerLoopFlowTest
    {
        [UnityTest]
        public IEnumerator PlayerLoopTimerDriver_WhenInstalled_CallsTickerOnFrame()
        {
            var ticker = new FakeTimerTicker();
            var driver = new PlayerLoopTimerDriver(ticker);

            try
            {
                driver.Install();

                yield return null;
                yield return null;

                Assert.That(ticker.TickCount, Is.GreaterThan(0));
                Assert.That(ticker.LastDeltaTime, Is.GreaterThanOrEqualTo(0f));
                Assert.That(ticker.LastUnscaledDeltaTime, Is.GreaterThanOrEqualTo(0f));
            }
            finally
            {
                driver.Uninstall();
            }
        }

        private sealed class FakeTimerTicker : ITimerTicker
        {
            public int TickCount { get; private set; }
            public float LastDeltaTime { get; private set; }
            public float LastUnscaledDeltaTime { get; private set; }

            public void Tick(float deltaTime, float unscaledDeltaTime)
            {
                TickCount++;
                LastDeltaTime = deltaTime;
                LastUnscaledDeltaTime = unscaledDeltaTime;
            }
        }
    }
}
