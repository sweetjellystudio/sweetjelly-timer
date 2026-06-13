# SWeetJelly Timer

`SWeetJelly Timer`는 countdown, stopwatch, frequency-based timer logic을 제공하는 Unity timer module입니다.

이 모듈은 layered architecture를 기준으로 설계되어 있습니다. Timer domain logic은 Unity runtime integration과 분리되어 순수하고 testable하게 유지되며, Unity `PlayerLoop` 연동은 framework driver가 담당합니다.

## 기능

- Countdown timer
- Stopwatch timer
- Frequency timer
- Timer handle 기반 lifetime control
- Pause, resume, stop, reset, dispose 지원
- Scaled/unscaled time 지원
- Tick speed control
- Unity `PlayerLoop` integration
- Initialiser integration module

## 패키지 구조

```text
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

    InitialiserIntegration/
      SWeetJelly.Timer.InitialiserIntegration.asmdef
      TimerInitModule.cs
```

## 아키텍처

timer module은 여러 layer로 나뉩니다.

```text
SWeetJelly.Timer.Contract
  - public timer contract
  - 외부 module이 사용
  - interface와 단순 value type 포함

SWeetJelly.Timer.Domain
  - 순수 timer logic
  - countdown, stopwatch, frequency timer 구현 포함
  - UnityEngine, PlayerLoop, ServiceLocator, Initialiser에 의존하지 않음

SWeetJelly.Timer.App
  - timer application service
  - timer 생성, 등록, update, dispose 담당
  - ITimerService와 ITimerTicker 구현

SWeetJelly.Timer.Framework
  - Unity integration layer
  - timer ticking을 Unity PlayerLoop에 연결

SWeetJelly.Timer.InitialiserIntegration
  - Timer와 Initialiser module 사이의 integration layer
  - PlayerLoop timer driver 설치와 해제 담당
```

## Assembly Dependencies

```text
SWeetJelly.Timer.Contract
  - no dependency

SWeetJelly.Timer.Domain
  - SWeetJelly.Timer.Contract

SWeetJelly.Timer.App
  - SWeetJelly.Timer.Contract
  - SWeetJelly.Timer.Domain

SWeetJelly.Timer.Framework
  - SWeetJelly.Timer.Contract

SWeetJelly.Timer.InitialiserIntegration
  - SWeetJelly.Timer.Contract
  - SWeetJelly.Timer.App
  - SWeetJelly.Timer.Framework
  - SWeetJelly.Initialiser.Contract
```

외부 project code는 보통 다음 assembly에만 의존하는 것이 좋습니다.

```text
SWeetJelly.Timer.Contract
```

구체 timer service와 Unity player loop driver는 bootstrapper, composition root, initialiser integration module에서 생성하고 연결하세요.

## 기본 사용법

### Timer service 생성

```csharp
using SWeetJelly.Timer.App;
using SWeetJelly.Timer.Contract;
using SWeetJelly.Timer.Framework;

var timerService = new TimerService();

ITimerService timerServicePort = timerService;
ITimerTicker timerTickerPort = timerService;

var driver = new PlayerLoopTimerDriver(timerTickerPort);
driver.Install();
```

대부분의 프로젝트에서는 gameplay code가 아니라 bootstrapper나 initialiser module에서 이 설정을 처리하는 것이 좋습니다.

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
        // Countdown이 완료되었습니다.
    }

    public void Cancel()
    {
        countdownHandle?.Dispose();
        countdownHandle = null;
    }
}
```

## Frequency Timer

frequency timer는 고정 interval마다 callback을 반복 호출합니다.

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
        // 매초 호출됩니다.
    }

    public void Stop()
    {
        tickHandle?.Dispose();
        tickHandle = null;
    }
}
```

level timer 같은 gameplay system에 유용합니다.

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

## Timer Options

`TimerOptions`로 timer behavior를 설정할 수 있습니다.

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

`Time.deltaTime` 대신 `Time.unscaledDeltaTime`을 사용합니다.

### Tick Speed

```csharp
new TimerOptions(tickSpeed: 2f)
```

`tickSpeed: 2f`인 timer는 두 배 빠르게 진행됩니다.

## Timer Handle

timer를 시작하면 `ITimerHandle`이 반환됩니다.

handle은 timer lifetime을 제어하는 데 사용합니다.

```csharp
handle.Pause();
handle.Resume();
handle.Reset();
handle.Stop();
handle.Dispose();
```

일반적인 사용:

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

## Initialiser Integration

Initialiser module을 사용할 때는 `TimerInitModule`로 timer player loop driver를 설치할 수 있습니다.

```csharp
using SWeetJelly.Timer.App;
using SWeetJelly.Timer.Framework;
using SWeetJelly.Timer.InitialiserIntegration;

var timerService = new TimerService();
var timerDriver = new PlayerLoopTimerDriver(timerService);

var initModule = new TimerInitModule(timerDriver);
initModule.Initialise();
```

unload 시:

```csharp
initModule.Unload();
timerService.Dispose();
```

## 설계 메모

이전 구현은 timer instance가 `ServiceLocator`를 통해 global manager에 등록되는 구조였습니다.

새 구조는 그 패턴을 피합니다.

```text
Old flow:
Timer -> ServiceLocator -> BaseTimerManager -> PlayerLoop

New flow:
Project/App code -> ITimerService -> TimerService -> Timer domain objects
Unity PlayerLoop -> PlayerLoopTimerDriver -> ITimerTicker
```

이렇게 하면 timer object는 timer behavior에만 집중할 수 있습니다.

Timer domain object는 다음을 알지 않습니다.

- Unity `PlayerLoop`
- `Time.deltaTime`
- `ServiceLocator`
- Initialiser module
- global manager instance

## 권장 의존 방향

gameplay 또는 project application code는 timer contract에만 의존하는 것이 좋습니다.

```text
Project.App
  -> SWeetJelly.Timer.Contract
```

bootstrap 또는 framework code는 concrete implementation을 wiring합니다.

```text
Project.Framework / Bootstrap
  -> SWeetJelly.Timer.App
  -> SWeetJelly.Timer.Framework
```

이렇게 하면 `LevelSession` 같은 system이 timer service update 방식과 분리된 채 timer를 사용할 수 있습니다.

## Version Notes

### 2.0.0

이 버전은 breaking architecture refactor를 도입합니다.

주요 변경:

- 기존 `BaseTimerManager` 중심 흐름을 `ITimerService`로 교체
- timer instance의 직접적인 `ServiceLocator` 의존 제거
- Unity `PlayerLoop` integration을 `PlayerLoopTimerDriver`로 이동
- module을 `Contract`, `Domain`, `App`, `Framework`, `InitialiserIntegration`으로 분리
- Timer domain object가 순수 timer logic을 가지도록 변경

### 1.x

`1.x` 버전은 이전 inheritance/global-manager 기반 구조를 사용하는 초기 내부 릴리즈로 간주합니다.
