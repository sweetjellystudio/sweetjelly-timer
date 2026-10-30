# Timer Dev Sample

This sample shows the consumer-side flow for `com.sweetjelly.timer`.

- `Timer Package Bootstrapper.asset` installs `TimerPackageBootstrapperSo` through Bootstrapper.
- `Sample Package Bootstrapper Profile.asset` is assigned to `BootstrapperBehaviour`.
- `TimerSampleConsumerBehaviour` resolves `ITimerService` and demonstrates countdown, frequency, stopwatch, and timer handle controls.

## Setup

Open `Sample Scene` and enter Play Mode.

Use the on-screen buttons to start different timer types and control the active timer handle with pause, resume, reset, stop, and dispose.


2.0.0: Stopwatch elapsed time updates on each advancing Tick. Reset returns the active handle to Idle; use a Start button to schedule a new timer after stop/completion. The sample camera requires no URP or audio dependency. Timer and bootstrapper Git URLs must both be declared in a Git consumer's project manifest.
