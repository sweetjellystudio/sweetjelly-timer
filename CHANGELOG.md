# CHANGELOG

## v2.0.0

- [Changed] Refactored the timer module into layered assemblies: `Contract`, `Domain`, `App`, `Framework`, and `InitialiserIntegration`.
- [Changed] Replaced the `BaseTimerManager` inheritance-based architecture with service/contract-based timer management.
- [Changed] Timer instances are now created and managed through `ITimerService` instead of self-registering to a global manager.
- [Changed] Timer update execution has been separated from timer management. Runtime ticking is now driven by `ITimerTicker`.
- [Changed] Unity `PlayerLoop` integration has been moved out of the timer manager and into `PlayerLoopTimerDriver`.
- [Changed] `CountdownTimer`, `StopwatchTimer`, and `FrequencyTimer` now contain only pure timer logic and no longer depend on Unity runtime services or global service lookup.
- [Added] Added `ITimerService` as the public timer API for starting countdown, frequency, and stopwatch timers.
- [Added] Added `ITimerHandle` as the public handle for controlling timer lifetime, pause/resume, reset, stop, and disposal.
- [Added] Added `ITimerTicker` for framework-side ticking of the timer service.
- [Added] Added `TimerOptions` to configure timer behavior such as unscaled time and tick speed.
- [Added] Added `PlayerLoopTimerDriver` to connect the timer service to Unity's player loop.
- [Added] Added `TimerInitModule` as an integration module for initialiser-based setup.
- [Removed] Removed direct `ServiceLocator` dependency from timer instances.
- [Removed] Removed Unity `PlayerLoop`, `Time.deltaTime`, editor play mode handling, and service registration responsibilities from the timer manager.
- [Removed] Removed the old `BaseTimerManager`-centric registration flow.

## v1.0.1

- [Changed] All timer subclasses have been updated to inherit from `BaseTimer` by changing interface `ITimer` to abstract class `BaseTimer` in the Core module.

## v1.0.0

- basic logic