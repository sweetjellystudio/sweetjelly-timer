# Timer Tests

이 폴더는 `com.sweetjelly.timer` 패키지를 현재 Unity 테스트 프로젝트에서 검증하기 위한 테스트를 담는다.

## EditMode

`EditMode` 테스트는 Unity 프레임 실행에 의존하지 않고 `TimerService.Tick(...)`으로 시간을 직접 주입해 순수 timer 동작을 검증한다.

- `TimerServiceTest`
  - countdown timer가 시간이 지나면 완료되고 완료 callback을 호출하는지 검증한다.
  - pause 상태에서는 시간이 진행되지 않고, resume 후 다시 진행되는지 검증한다.
  - frequency timer가 누적 시간이 여러 interval을 넘으면 interval 수만큼 callback을 호출하는지 검증한다.
  - stopwatch timer가 경과 시간을 누적하는지 검증한다.
  - `UseUnscaledTime` 옵션이 unscaled delta time을 사용하는지 검증한다.
  - 잘못된 frequency interval이 `ArgumentOutOfRangeException`을 발생시키는지 검증한다.
  - dispose된 `TimerService`를 다시 사용할 수 없는지 검증한다.

- `TimerPackageIntegrationTest`
  - `TimerPackageBootstrapperSo`이 `ITimerService`를 `ServiceRegistry`에 등록하는지 검증한다.
  - `TimerPackageBootstrapperSo.Shutdown(...)` 후 등록이 해제되는지 검증한다.

## PlayMode

`PlayMode` 테스트는 Unity PlayerLoop와 연결되는 framework 계층을 실제 프레임에서 검증한다.

- `TimerPlayerLoopFlowTest`
  - `PlayerLoopTimerDriver`를 설치하면 다음 프레임부터 `ITimerTicker.Tick(...)`이 호출되는지 검증한다.
  - 테스트 종료 시 driver를 uninstall해서 PlayerLoop 변경이 남지 않도록 정리한다.
