#   Timer package instructions

Read [architecture][architecture] before timer/driver changes, [usage][usage] before consumer changes and the [callback decision][decision] before changing callback or shutdown semantics.

*   Contract/Domain/App remain Unity-independent. Framework.Unity supplies PlayerLoop time; Framework.Bootstrapper owns service registration and driver lifetime. Preserve public signatures, serialized names and GUIDs unless a migration is agreed.
*   Frequency catch-up stops when a callback stops, pauses or disposes the handle. Stopwatch notifies at start/resume and after each advancing Tick. Callback exceptions propagate; do not silently log-and-continue or cancel timers as a new policy. Tick is synchronous, single-threaded and rejects reentry before mutating its iteration state.
*   Service disposal inside callbacks must stop further work and dispose owned handles safely. Bootstrap failure cleans only newly created resources; Shutdown unregisters only the module's own service. A module asset supports one active context, not independent concurrent runtime scopes.
*   Run SWeetJelly.Timer.Tests.EditMode and SWeetJelly.Timer.Tests.PlayMode separately. Preserve/restore the PlayerLoop, Time settings and temporary scenes in tests. Record actual Editor patch, target, counts and reports. Observe the real sample independently from tests.
*   Before release, validate manifest/root LICENSE, dependency graph, asmdefs, Git contents, metadata/GUID and migration docs. A minimal Git consumer explicitly lists bootstrapper's HTTPS version-tag URL because UPM package manifests do not support Git dependency URLs. Public timer-tag installation is a separate post-publication check.
*   PRs, commits, pushes, tags, Releases and game-consumer changes belong to the user. Shared workflow follows the installed unity-package harness and development-host policy.

[architecture]: ARCHITECTURE.md
[usage]: README.md
[decision]: docs/adr/0001-callback-and-bootstrap-lifetime.md
