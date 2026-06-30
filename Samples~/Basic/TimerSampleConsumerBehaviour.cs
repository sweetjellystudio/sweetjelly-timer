namespace SWeetJelly.Samples.Timer
{
    using System;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Timer.Contract;
    using UnityEngine;

    public sealed class TimerSampleConsumerBehaviour : MonoBehaviour
    {
        private ITimerService timerService;
        private ITimerHandle activeHandle;
        private string status = "Waiting for Timer service...";
        private int frequencyTicks;
        private float stopwatchElapsed;

        private void OnEnable()
        {
            BootstrapperBehaviour.Bootstrapped += OnBootstrapped;
            ResolveIfReady();
        }

        private void OnDisable()
        {
            BootstrapperBehaviour.Bootstrapped -= OnBootstrapped;
            DisposeActiveHandle();
        }

        private void OnBootstrapped(object sender, EventArgs args)
        {
            ResolveIfReady();
        }

        private void ResolveIfReady()
        {
            if (timerService != null || !BootstrapperBehaviour.IsBootstrapped || BootstrapperBehaviour.Services == null)
                return;

            if (!BootstrapperBehaviour.Services.TryResolve<ITimerService>(out timerService))
            {
                status = "ITimerService is not registered.";
                return;
            }

            status = "Timer service is ready.";
        }

        private void OnGUI()
        {
            const float x = 16f;
            var y = 16f;

            GUI.Label(new Rect(x, y, 620f, 24f), status);
            y += 28f;

            GUI.Label(new Rect(x, y, 620f, 24f), FormatHandleState());
            y += 28f;

            GUI.Box(new Rect(x, y, 300f, 18f), string.Empty);
            GUI.Box(new Rect(x, y, 300f * Mathf.Clamp01(activeHandle?.Progress ?? 0f), 18f), string.Empty);
            y += 34f;

            GUI.enabled = timerService != null;

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Start Countdown"))
                StartCountdown();

            y += 34f;

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Start Fast Countdown"))
                StartFastCountdown();

            y += 34f;

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Start Frequency"))
                StartFrequency();

            y += 34f;

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Start Stopwatch"))
                StartStopwatch();

            y += 42f;

            GUI.enabled = activeHandle != null;

            if (GUI.Button(new Rect(x, y, 90f, 28f), "Pause"))
                activeHandle.Pause();

            if (GUI.Button(new Rect(x + 100f, y, 90f, 28f), "Resume"))
                activeHandle.Resume();

            y += 34f;

            if (GUI.Button(new Rect(x, y, 90f, 28f), "Reset"))
                activeHandle.Reset();

            if (GUI.Button(new Rect(x + 100f, y, 90f, 28f), "Stop"))
                activeHandle.Stop();

            y += 34f;

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Dispose Handle"))
                DisposeActiveHandle();

            GUI.enabled = true;
        }

        private void StartCountdown()
        {
            DisposeActiveHandle();
            activeHandle = timerService.StartCountdown(5f, OnCountdownCompleted);
            status = "Countdown started for 5 seconds.";
        }

        private void StartFastCountdown()
        {
            DisposeActiveHandle();
            activeHandle = timerService.StartCountdown(
                5f,
                OnCountdownCompleted,
                new TimerOptions(useUnscaledTime: false, tickSpeed: 2f));
            status = "Fast countdown started with tick speed x2.";
        }

        private void StartFrequency()
        {
            DisposeActiveHandle();
            frequencyTicks = 0;
            activeHandle = timerService.StartFrequency(1f, OnFrequencyTick);
            status = "Frequency timer started. It ticks every second.";
        }

        private void StartStopwatch()
        {
            DisposeActiveHandle();
            stopwatchElapsed = 0f;
            activeHandle = timerService.StartStopwatch(OnStopwatchTick);
            status = "Stopwatch started.";
        }

        private void OnCountdownCompleted()
        {
            status = "Countdown completed.";
        }

        private void OnFrequencyTick()
        {
            frequencyTicks++;
            status = $"Frequency tick: {frequencyTicks}";
        }

        private void OnStopwatchTick(float elapsed)
        {
            stopwatchElapsed = elapsed;
            status = $"Stopwatch elapsed: {stopwatchElapsed:0.00}s";
        }

        private string FormatHandleState()
        {
            if (activeHandle == null)
                return "Active Handle: none";

            return $"State: {activeHandle.State}   Time: {activeHandle.CurrentTime:0.00}/{activeHandle.InitialTime:0.00}   Progress: {activeHandle.Progress:P0}   Finished: {activeHandle.IsFinished}";
        }

        private void DisposeActiveHandle()
        {
            activeHandle?.Dispose();
            activeHandle = null;
        }
    }
}
