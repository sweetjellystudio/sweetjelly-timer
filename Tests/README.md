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
## 2.0.0 검증 추가

TimerLifecycleRegressionTest는 callback 중 Stop/Pause/Dispose, service 종료, Tick 재진입, stopwatch의 매 Tick 알림과 시작·재개 알림, callback 예외 이후 순회 복구, callback이 생성한 timer의 다음 Tick 진행, 중복 등록 실패와 소유 service 정리, null callback의 기존 예외를 검증한다. 추가 clock/speed 검사는 scaled/unscaled delta를 구분한다.

TimerDriverLifecycleTest는 같은 driver의 중복 설치 방지, 두 driver의 독립 해제, bootstrap Shutdown 후 handle dispose와 PlayerLoop 정리를 검증한다. 각 테스트는 원래 PlayerLoop와 자기 객체를 복원한다.

호스트와 선언된 의존성만 사용하는 별도 소비 환경에서 EditMode/PlayMode를 구분하고 실제 Editor 패치·플랫폼·실행/통과/실패/건너뜀 수·신선한 보고서를 기록한다. 실제 샘플·Inspector 관찰과 Windows Player 빌드는 별도 증거다. Git 소비 프로젝트는 bootstrapper Git URL도 직접 선언하며 timer 태그 검증은 사용자 게시 후 수행한다.