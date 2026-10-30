#   Timer architecture

Timer owns synchronous timer state and a Unity ticking adapter. It does not own game state, input, screens, or a global service locator. Existing layers remain because they represent different responsibilities. See [usage][usage] and the [decision][decision].

##  Responsibilities

Contract owns ITimerService, ITimerHandle, TimerState and TimerOptions. Domain owns TimerBase, CountdownTimer, FrequencyTimer and StopwatchTimer. App's TimerService creates/owns timers and exposes ITimerTicker to adapters. These assemblies have no engine references. Framework.Unity installs a per-driver PlayerLoop delegate and supplies scaled/unscaled delta. Framework.Bootstrapper creates the service/driver and registers ITimerService in a bootstrap context.

The package directly depends on bootstrapper 2.0.0, which supplies its own declared uGUI dependency. Timer's pure contracts/domain have no direct UI dependency. Consumers can use manual ITimerTicker ticking or the Unity/bootstrap adapter; the UPM package still resolves its declared bootstrapper dependency. Git consumers list the bootstrapper Git URL in their project manifest rather than embedding a Git URL in this package manifest.

##  Time and callback contract

Each service Tick snapshots registered timers. Timers created during callbacks begin advancing on a subsequent Tick. Each timer selects scaled/unscaled delta and applies TickSpeed. Paused/idle timers do not advance. Completed/stopped/disposed timers are swept from the service. Reset restores an idle handle; it does not start it or re-register a handle already removed from the service. Start a new service timer when a new scheduled operation is needed.

Frequency timers catch up all elapsed intervals while still running. A callback's Stop/Pause/Dispose prevents subsequent catch-up callbacks in that Tick. Pausing retains the accumulated remainder; after resume, later ticking may consume it. Stopwatch callbacks retain start/resume notifications and additionally receive elapsed time after each advancing Tick. No callback is added for paused or nonadvancing ticks.

Callback exceptions propagate to the caller of Tick and stop that call's remaining processing. A finally block restores service iteration state so future Tick calls remain valid. Reentrant Tick is rejected with InvalidOperationException before modifying the active snapshot. This is not a thread-safe scheduler or an async/cancellation-token API. Handle Dispose/Stop are the existing cancellation controls.

##  Ownership and integration

Service disposal safely disposes all owned handles even from a callback. No more timers from that snapshot are advanced afterwards. The service owns its private callback map and releases entries with removed/disposed timers. Public handle events retain their existing lifetime semantics.

Each PlayerLoopTimerDriver installs once and removes only its own type/delegate entry; multiple driver instances can coexist. Dispose/Uninstall is the cleanup path, with Editor Play Mode exit support guarded by UNITY_EDITOR. Install retains the existing warning/return behavior if no matching Unity Update loop exists.

TimerPackageBootstrapperSo commits its service/driver fields only after successful registration. Failed registration removes the new driver and disposes the new service, preserving any already registered service. Shutdown is safe before Bootstrap and after prior Shutdown, and unregisters only a matching owned service. A ScriptableObject module asset is configuration plus one active runtime context; separate prototype/demo executions do not imply simultaneous scopes in one shared asset.

##  Support and evidence

2.0.0 requires Unity 6.3 LTS and bootstrapper 2.0.0. Public timer signatures and serialized sample assets remain; callback/lifetime bug paths change as documented. Tests cover clocks, speed, state transitions, callback-driven mutation, errors, bootstrap and actual PlayerLoop behavior. The sample demonstrates real service consumption. Host compilation, isolated tests, sample observation, Windows Player builds and post-publication Git installation are separate evidence; no result implies other platforms or IL2CPP support was tested.

[usage]: README.md
[decision]: docs/adr/0001-callback-and-bootstrap-lifetime.md
