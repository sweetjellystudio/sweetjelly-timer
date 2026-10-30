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
- Bootstrapper integration module

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

    Framework/Bootstrapper/
      SWeetJelly.Timer.Framework.Bootstrapper.asmdef
      TimerPackageBootstrapperSo.cs
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

SWeetJelly.Timer.Framework.Bootstrapper
  - Timer와 Bootstrapper package 사이의 integration layer
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

SWeetJelly.Timer.Framework.Bootstrapper
  - SWeetJelly.Timer.Contract
  - SWeetJelly.Timer.App
  - SWeetJelly.Timer.Framework
  - SWeetJelly.Bootstrapper.Contract
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
using SWeetJelly.Timer.Framework.Unity;

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
new TimerOptions(useUnscaledTime: true, tickSpeed: 1f)
```

`Time.deltaTime` 대신 `Time.unscaledDeltaTime`을 사용합니다.

### Tick Speed

```csharp
new TimerOptions(useUnscaledTime: false, tickSpeed: 2f)
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

## Bootstrapper Integration

Create > SWeetJelly > Bootstrapper > Packages > Timer에서 TimerPackageBootstrapperSo 에셋을 만듭니다. PackageBootstrapperProfileSo에 추가하고 BootstrapperBehaviour에 profile을 할당합니다. 실제 실행은 Bootstrap(context)/Shutdown(context)이며 ScriptableObject를 new로 만들거나 이전 Initialise/Unload 예제를 사용하지 않습니다.

```csharp
using SWeetJelly.Bootstrapper.Framework.Unity;
using SWeetJelly.Timer.Contract;

// In a consumer after bootstrap, or in composition code with an explicit context:
ITimerService timerService = BootstrapperBehaviour.Services.Resolve<ITimerService>();
var handle = timerService.StartCountdown(3f);
```

module이 PlayerLoop driver와 service를 소유합니다. 소비자는 handle을 Dispose하고, module Shutdown은 자신의 driver·등록을 해제하며 남은 timer를 정리합니다.

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
- Bootstrapper package
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

2.0.0은 Unity 6.3 LTS와 bootstrapper 2.0.0을 기준으로 의존성·callback·정리 동작을 갱신합니다. 기존 서비스 구조는 유지합니다. 상세 이력은 CHANGELOG.md를 참고하세요.

## 2.0.0 이전과 Git 설치

Unity 6.3 LTS와 bootstrapper 2.0.0이 필요합니다. timer 태그 게시 후 소비 프로젝트 manifest에 두 Git 패키지를 명시합니다.

```json
{
  "dependencies": {
    "com.sweetjelly.bootstrapper": "https://github.com/sweetjellystudio/sweetjelly-bootstrapper.git#v2.0.0",
    "com.sweetjelly.timer": "https://github.com/sweetjellystudio/sweetjelly-timer.git#v2.0.0"
  }
}
```

패키지 manifest는 bootstrapper 버전을 선언하고 Git URL은 소비 프로젝트가 지정합니다. uGUI는 bootstrapper의 선언 의존성으로 해석되며 UI Toolkit 의존성은 추가하지 않습니다.

공개 timer 시그니처와 직렬화 필드는 유지합니다. Stopwatch callback은 시작·재개 알림과 함께 실행 중 매 Tick 이후 경과 시간을 전달합니다. Frequency의 Stop/Pause/Dispose는 같은 Tick의 catch-up 중에도 적용합니다. Callback 예외는 계속 호출자에게 전파하고, Tick 재진입은 순회를 변경하기 전에 거부합니다. Callback 안의 service Dispose는 지원합니다. StartFrequency는 기존처럼 null callback을 거부하며 nullable 표기를 실제 요구와 일치시켰습니다.

Reset은 Idle 상태로 되돌릴 뿐 재시작하거나 서비스에서 제거된 handle을 다시 등록하지 않습니다. 중지·완료 후 새 작업은 service에서 새 timer로 시작하세요. 수동 조립에서는 driver/service를 직접 Dispose하고, profile 방식에서는 package bootstrapper가 이 자원을 소유합니다.

[아키텍처][architecture]와 [callback 결정][decision]을 참고하세요. 실제 검증 Editor 패치·플랫폼은 지원 정책과 별도로 기록합니다.

[architecture]: ARCHITECTURE.md
[decision]: docs/adr/0001-callback-and-bootstrap-lifetime.md