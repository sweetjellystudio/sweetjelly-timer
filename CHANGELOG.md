# CHANGELOG

## v2.0.0 (unreleased)

*   [Changed] Minimum Unity is 6.3 LTS; bootstrapper dependency is 2.0.0.
*   [Fixed] Frequency callbacks stop catch-up after Stop/Pause/Dispose.
*   [Fixed] Callback-driven service disposal and Tick reentry no longer corrupt iteration.
*   [Fixed] Stopwatch callbacks now observe every advancing Tick while preserving start/resume notifications.
*   [Fixed] Failed bootstrap cleans newly created resources; shutdown unregisters only the owned service.
*   [Fixed] Frequency callback nullability matches the existing non-null requirement; sample camera no longer carries undeclared URP/audio references.
*   [Added] Regression/lifecycle tests and maintained package architecture, agent and decision docs.

This is a local candidate; the user handles publication.

## v1.0.0

*   [Added] Added `ITimerService` as the public timer API for starting countdown, frequency, and stopwatch timers.
*   [Added] Added `ITimerHandle` as the public handle for controlling timer lifetime, pause/resume, reset, stop, and disposal.
*   [Added] Added `ITimerTicker` for framework-side ticking of the timer service.
*   [Added] Added `TimerOptions` to configure timer behavior such as unscaled time and tick speed.
*   [Added] Added `PlayerLoopTimerDriver` to connect the timer service to Unity's player loop.
*   [Added] Added `TimerPackageBootstrapperSo` as an integration module for initialiser-based setup.
*   [Changed] All timer subclasses have been updated to inherit from `BaseTimer` by changing interface `ITimer` to abstract class `BaseTimer` in the Core module.
*   [Changed] Refactored the timer module into layered assemblies: `Contract`, `Domain`, `App`, `Framework/Unity`, and `Framework/Bootstrapper`.
*   [Changed] Replaced the `BaseTimerManager` inheritance-based architecture with service/contract-based timer management.
*   [Changed] Timer instances are now created and managed through `ITimerService` instead of self-registering to a global manager.
*   [Changed] Timer update execution has been separated from timer management. Runtime ticking is now driven by `ITimerTicker`.
*   [Changed] Unity `PlayerLoop` integration has been moved out of the timer manager and into `PlayerLoopTimerDriver`.
*   [Changed] `CountdownTimer`, `StopwatchTimer`, and `FrequencyTimer` now contain only pure timer logic and no longer depend on Unity runtime services or global service lookup.
*   [Removed] Removed direct `ServiceLocator` dependency from timer instances.
*   [Removed] Removed Unity `PlayerLoop`, `Time.deltaTime`, editor play mode handling, and service registration responsibilities from the timer manager.
*   [Removed] Removed the old `BaseTimerManager`-centric registration flow.
