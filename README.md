# SWeetJelly Timer

`SWeetJelly Timer` is a timer package that provides countdown, stopwatch, and frequency-based timer logic.

The module is designed around a layered architecture. Timer domain logic is separated from Unity runtime integration, so timer behavior can remain pure and testable while Unity `PlayerLoop` integration is handled by a framework driver.

---

## Features

- Countdown timer
- Stopwatch timer
- Frequency timer
- Timer handle-based lifetime control
- Pause, resume, stop, reset, and dispose support
- Scaled and unscaled time support
- Tick speed control
- Unity `PlayerLoop` integration
- Bootstrapper integration module

---

## Package Structure

```plain
sweetjelly-timer/
  package.json

  Runtime/
    Contract/
      SWeetJelly.Timer.Contract.asmdef
      ITimerService.cs
      ITimerHandle.cs
      ITimerTicker.cs
      IProgressable.cs
      TimerOptions.cs
      TimerState.cs

    Domain/
      SWeetJelly.Timer.Domain.asmdef
      TimerBase.cs
      CountdownTimer.cs
      StopwatchTimer.cs
      FrequencyTimer.cs

    App/
      SWeetJelly.Timer.App.asmdef
      TimerService.cs

    Framework/
      SWeetJelly.Timer.Framework.asmdef
      PlayerLoopTimerDriver.cs
      PlayerLoopUtils.cs

    Framework/Bootstrapper/
      SWeetJelly.Timer.Framework.Bootstrapper.asmdef
      TimerPackageBootstrapperSo.cs
```

---

## Architecture

The timer module is split into several layers.

```plain
SWeetJelly.Timer.Contract
  - Public timer contracts
  - Used by external modules
  - Contains interfaces and simple value types

SWeetJelly.Timer.Domain
  - Pure timer logic
  - Contains countdown, stopwatch, and frequency timer implementations
  - Does not depend on UnityEngine, PlayerLoop, ServiceLocator, or Initialiser

SWeetJelly.Timer.App
  - Timer application service
  - Creates, registers, updates, and disposes timers
  - Implements ITimerService and ITimerTicker

SWeetJelly.Timer.Framework
  - Unity integration layer
  - Connects timer ticking to Unity PlayerLoop

SWeetJelly.Timer.Framework.Bootstrapper
  - Integration layer between Timer and Bootstrapper packages
  - Installs and uninstalls the PlayerLoop timer driver
```

---

## Assembly Dependencies

```plain
SWeetJelly.Timer.Contract
  - no dependency

SWeetJelly.Timer.Domain
  - SWeetJelly.Timer.Contract

SWeetJelly.Timer.App
  - SWeetJelly.Timer.Contract
  - SWeetJelly.Timer.Domain

SWeetJelly.Timer.Framework
  - SWeetJelly.Timer.Contract

SWeetJelly.Timer.Framework.Bootstrapper
  - SWeetJelly.Timer.Contract
  - SWeetJelly.Timer.App
  - SWeetJelly.Timer.Framework
  - SWeetJelly.Bootstrapper.Contract
```

External project code should usually depend only on:

```plain
SWeetJelly.Timer.Contract
```

The concrete timer service and Unity player loop driver should be created and wired from a bootstrapper, composition root, or initialiser integration module.

---

## Basic Usage

### Creating the timer service

```csharp
using SWeetJelly.Timer.App;
using SWeetJelly.Timer.Contract;
using SWeetJelly.Timer.Framework.Unity;

var timerService = new TimerService();

ITimerService timerServicePort = timerService;
ITimerTicker timerTickerPort = timerService;

var driver = new PlayerLoopTimerDriver(timerTickerPort);
driver.Install();
```

In most projects, this setup should be handled by a bootstrapper or initialiser module rather than gameplay code.

---

## Countdown Timer

```csharp
using SWeetJelly.Timer.Contract;

public sealed class CountdownExample
{
    private readonly ITimerService timerService;
    private ITimerHandle countdownHandle;

    public CountdownExample(ITimerService timerService)
    {
        this.timerService = timerService;
    }

    public void StartCountdown()
    {
        countdownHandle = timerService.StartCountdown(
            seconds: 5f,
            onCompleted: OnCountdownCompleted);
    }

    private void OnCountdownCompleted()
    {
        // Countdown finished.
    }

    public void Cancel()
    {
        countdownHandle?.Dispose();
        countdownHandle = null;
    }
}
```

---

## Frequency Timer

A frequency timer invokes a callback repeatedly at a fixed interval.

```csharp
using SWeetJelly.Timer.Contract;

public sealed class FrequencyExample
{
    private readonly ITimerService timerService;
    private ITimerHandle tickHandle;

    public FrequencyExample(ITimerService timerService)
    {
        this.timerService = timerService;
    }

    public void Start()
    {
        tickHandle = timerService.StartFrequency(
            intervalSeconds: 1f,
            onTick: OnSecondTick);
    }

    private void OnSecondTick()
    {
        // Called every second.
    }

    public void Stop()
    {
        tickHandle?.Dispose();
        tickHandle = null;
    }
}
```

This is useful for gameplay systems such as level timers.

```csharp
public sealed class LevelSession : IDisposable
{
    private readonly ITimerService timerService;
    private readonly LevelModel levelModel;

    private ITimerHandle tickHandle;

    public LevelSession(
        LevelModel levelModel,
        ITimerService timerService)
    {
        this.levelModel = levelModel;
        this.timerService = timerService;
    }

    public void Start()
    {
        tickHandle = timerService.StartFrequency(
            intervalSeconds: 1f,
            onTick: HandleSecondTick);
    }

    private void HandleSecondTick()
    {
        levelModel.AdvanceTime(1);
    }

    public void Dispose()
    {
        tickHandle?.Dispose();
        tickHandle = null;
    }
}
```

---

## Stopwatch Timer

```csharp
using SWeetJelly.Timer.Contract;

public sealed class StopwatchExample
{
    private readonly ITimerService timerService;
    private ITimerHandle stopwatchHandle;

    public StopwatchExample(ITimerService timerService)
    {
        this.timerService = timerService;
    }

    public void Start()
    {
        stopwatchHandle = timerService.StartStopwatch();
    }

    public float GetElapsedTime()
    {
        return stopwatchHandle?.CurrentTime ?? 0f;
    }

    public void Stop()
    {
        stopwatchHandle?.Dispose();
        stopwatchHandle = null;
    }
}
```

---

## Timer Options

Timer behavior can be configured with `TimerOptions`.

```csharp
var options = new TimerOptions(
    useUnscaledTime: true,
    tickSpeed: 1f);

var handle = timerService.StartCountdown(
    seconds: 3f,
    onCompleted: OnCompleted,
    options: options);
```

### Use Unscaled Time

```csharp
new TimerOptions(useUnscaledTime: true)
```

Uses `Time.unscaledDeltaTime` instead of `Time.deltaTime`.

### Tick Speed

```csharp
new TimerOptions(tickSpeed: 2f)
```

A timer with `tickSpeed: 2f` progresses twice as fast.

---

## Timer Handle

`ITimerHandle` is returned when a timer starts.

It can be used to control the timer lifetime.

```csharp
handle.Pause();
handle.Resume();
handle.Reset();
handle.Stop();
handle.Dispose();
```

Typical usage:

```csharp
private ITimerHandle handle;

public void StartTimer()
{
    handle = timerService.StartCountdown(10f);
}

public void PauseTimer()
{
    handle?.Pause();
}

public void ResumeTimer()
{
    handle?.Resume();
}

public void DisposeTimer()
{
    handle?.Dispose();
    handle = null;
}
```

---

## Bootstrapper Integration

When using the Bootstrapper package, `TimerPackageBootstrapperSo` can be used to install the timer player loop driver.

```csharp
using SWeetJelly.Timer.App;
using SWeetJelly.Timer.Framework.Unity;
using SWeetJelly.Timer.Framework.Bootstrapper;

var timerService = new TimerService();
var timerDriver = new PlayerLoopTimerDriver(timerService);

var initModule = new TimerPackageBootstrapperSo(timerDriver);
initModule.Initialise();
```

On unload:

```csharp
initModule.Unload();
timerService.Dispose();
```

---

## Design Notes

The previous implementation used a global manager-based structure where timer instances registered themselves through `ServiceLocator`.

The new structure avoids this pattern.

```plain
Old flow:
Timer -> ServiceLocator -> BaseTimerManager -> PlayerLoop

New flow:
Project/App code -> ITimerService -> TimerService -> Timer domain objects
Unity PlayerLoop -> PlayerLoopTimerDriver -> ITimerTicker
```

This keeps timer objects focused on timer behavior only.

Timer domain objects do not know:

* Unity `PlayerLoop`
* `Time.deltaTime`
* `ServiceLocator`
* Bootstrapper packages
* Global manager instances

---

## Recommended Dependency Direction

Gameplay or project application code should depend on timer contracts only.

```plain
Project.App
  -> SWeetJelly.Timer.Contract
```

Bootstrap or framework code is responsible for wiring concrete implementations.

```plain
Project.Framework / Bootstrap
  -> SWeetJelly.Timer.App
  -> SWeetJelly.Timer.Framework
```

This allows systems such as `LevelSession` to use timers without knowing how the timer service is updated.
