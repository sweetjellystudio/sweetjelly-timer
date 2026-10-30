#   Callback and bootstrap lifetime

Accepted 2026-10-30 for 2.0.0. Frequency catch-up must respect callback state changes immediately, and disposal from a callback must not mutate the active sweep. Reject recursive Tick before modifying the snapshot and restore iteration state on exceptions. Keep callback exceptions observable to the caller instead of introducing silent continuation or automatic cancellation.

The user selected per-Tick stopwatch notifications while retaining start/resume notifications. The existing sample's OnStopwatchTick already uses the callback to display elapsed time; start/resume-only behavior left that display at zero. App owns the additional callback mapping so the public interfaces and pure Domain types do not need a new event contract.

Bootstrap registration failure releases newly created driver/service resources; Shutdown touches only the module's matching owned service. Committing fields after registration avoids replacing a working module's fields during a failed repeated registration. Cleanup must not unregister an unrelated existing service. Unity 6.3 LTS and bootstrapper 2.0.0 become the declared baseline; publication remains a user action.
