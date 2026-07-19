# 우가우가 프로젝트 구조 분석 및 리팩토링 제안

## 1. 문서 목적

이 문서는 `Markdown/README.md`와 `Markdown/AI_RULES.md`를 기준으로 현재 Unity 프로젝트를 정적 분석한 결과와, 게임 플레이 및 기능 변경 없이 진행할 수 있는 리팩토링 후보를 정리한다.

현재 단계에서는 다음 원칙을 적용했다.

- C# 코드, Scene, Prefab, ScriptableObject, ProjectSettings는 수정하지 않았다.
- 새 기능, 밸런스, 연출, UI 디자인 변경을 제안 범위에서 제외했다.
- 리팩토링은 승인 후 작은 단위로 나누어 진행한다.
- 파일 이동 시 `.meta`를 함께 보존하여 Unity 직렬화 참조를 유지한다.
- 이름 변경 시 직렬화 필드는 `FormerlySerializedAs` 적용 여부를 검토한다.
- 정적 분석 결과이므로 실제 플레이 모드 동작은 리팩토링 시작 전에 기준 시나리오로 기록해야 한다.

## 2. 기준 문서에서 확인한 핵심 원칙

### 게임 설계 원칙

- 플레이어는 직접 공격하지 않고 로프로 사냥감을 제압한다.
- 로프의 텐션, 내구도, 길이와 위치 판단이 게임의 중심이다.
- 팀원 AI가 공격을 담당하고 플레이어와 협력한다.
- 조작은 단순하게 유지하고 상황 판단에서 깊이를 만든다.
- 텍스트보다 아이콘, 색상, 애니메이션 등 시각적 피드백을 우선한다.

### 개발 원칙

- 기존 기능을 함부로 변경하지 않는다.
- 버그 수정과 리팩토링은 기능 변경 없이 진행한다.
- ScriptableObject는 데이터만 보유한다.
- Singleton을 최소화한다.
- Manager끼리 직접 참조하지 않는다.
- `SerializeField`를 우선 사용한다.
- `FindObjectOfType` 계열의 런타임 탐색에 의존하지 않는다.
- 작업을 작은 단위로 끝내고 영향받는 파일을 기록한다.

## 3. 프로젝트 현황

| 항목 | 현황 |
|---|---:|
| Unity 버전 | 2022.3.51f1 |
| 플랫폼/렌더링 | Windows, URP 2D |
| 빌드 등록 Scene | 3개 (`HuntScene`, `ResultScene`, `HomeScene`) |
| 퍼스트파티 C# 파일 | 58개 |
| 퍼스트파티 C# 코드 | 약 3,759줄 |
| MonoBehaviour 직접 상속 클래스 | 34개 |
| ScriptableObject 클래스 | 2개 |
| Singleton 형태의 `Instance` | 5개 |
| 테스트 코드 | 없음 |
| Assembly Definition | 없음 |
| 주요 외부 패키지 | Input System, Cinemachine, DOTween, URP, TextMesh Pro |

현재 게임 흐름은 다음과 같이 구성되어 있다.

```text
HomeScene
  TeamManager가 후보 모집/배치/비용 계산
  HomeExit가 팀 확정 후 HuntScene 전환
      ↓
HuntScene
  Player + Rope가 사냥감 제어
  TeammateSpawner가 저장된 팀 생성
  TeammateController가 사냥감 접근/공격
  HuntingStageManager가 성공/실패 결과 생성
  StageEndingManager가 타임라인 재생 후 Scene 전환
      ↓
ResultScene
  ResultSceneManager가 결과 연출 시작
  ResultPanel이 결과 표시 및 고기 보상 반영
  HomeScene으로 복귀
```

런타임 전체 상태는 `GameManager`가 고기와 선택된 팀원 데이터를 보관하며, 사냥 결과는 정적 `StageResultStorage`를 통해 Scene 사이에 전달된다. Scene 전환은 정적 `SceneLoader`와 영속 `SceneTransition` 조합으로 처리한다.

`Markdown/TODO.md`에는 향후 포만도 시스템과 `SaveData` 도입 계획이 있다. 포만도가 이동 속도, 당기는 힘, 로프 무게에 영향을 주도록 계획되어 있으므로 현재의 `Player`, `Rope`, 영속 상태 사이 경계를 정리할 필요가 있다는 근거가 된다. 다만 포만도, 저장 시스템, 능력치 변경은 이번 기능 보존 리팩토링에서 구현하지 않는다.

## 4. 현재 구조의 장점

- `TeammateData`와 `RewardTable`은 로직 없이 데이터만 보유해 `AI_RULES.md`의 ScriptableObject 원칙을 지킨다.
- `IRopeHoldable`, `IRopeCatchable`, `IRopePhysicsBody`로 로프 양 끝의 역할이 명시되어 있다.
- 투사체는 `Projectile` 기반 클래스와 직선/포물선 구현으로 구분되어 있다.
- 공통 UI 애니메이션은 `AnimatedUI`와 하위 클래스로 재사용되고 있다.
- Inspector 참조는 전반적으로 `SerializeField`를 사용한다.
- `RopeVisual`이 로프의 시각 표현을 별도 클래스로 담당하는 등 일부 책임 분리가 이미 적용되어 있다.
- Scene별 오케스트레이션 클래스가 존재하여 Home/Hunt/Result 흐름을 찾기 쉽다.

## 5. 핵심 구조 분석

### 5.1 `Rope`의 책임 집중

`Rope.cs`는 447줄로 프로젝트에서 가장 크며 다음 책임을 동시에 수행한다.

- 로프 상태(`Idle`, `Casting`, `Attached`) 전환
- 캐스팅 시작/연장/완료
- Verlet 포인트 생성과 물리 시뮬레이션
- 길이 제약 계산
- 텐션 계산 및 양 끝 힘 분배
- 당기기/풀기/분리
- 내구도 감소와 회복
- 카메라 우선순위 변경
- 시각 계층에서 읽는 상태 제공

핵심 시스템이 한 파일에 집중되어 변경 영향 범위가 크다. 다만 작은 프로젝트 규모를 고려하면 처음부터 다수의 인터페이스와 서비스로 분해하는 것은 과하다. 우선 순수 계산 성격이 강한 시뮬레이션만 분리하고, 카메라 반응은 이벤트 구독 컴포넌트로 옮기는 정도가 적절하다.

권장 경계:

- `Rope`: 상태 전환과 외부 공개 API의 오케스트레이션
- `RopeSimulation`: 포인트, Verlet 적분, 제약 계산
- `RopeCameraFeedback`: 부착/리셋에 따른 카메라 우선순위 처리
- 내구도와 텐션 계산은 첫 단계에서는 `Rope`에 유지하고, 크기와 변경 빈도를 본 뒤 추가 분리를 판단

### 5.2 Manager와 전역 상태 결합

현재 `Instance`를 가진 클래스는 `GameManager`, `TeamManager`, `HuntingStageManager`, `StageEndingManager`, `SceneTransition`이다. 다음 직접 참조가 존재한다.

- `HuntingStageManager → StageEndingManager`
- `TeammateSpawner → StageEndingManager`
- `TeamManager → GameManager`
- `ResultSceneManager → GameManager`
- `SceneLoader → SceneTransition`
- UI인 `ResultPanel`, `PlayerMeatCountUI`도 `GameManager`를 직접 참조
- 캐릭터인 `Animal`이 `HuntingStageManager`를 직접 참조

특히 Manager 간 직접 참조는 `AI_RULES.md`와 충돌한다. 모든 Singleton을 한 번에 제거하면 위험이 크므로 다음과 같이 점진적으로 줄이는 편이 안전하다.

- 같은 Scene 안의 참조는 Inspector로 명시적으로 연결한다.
- 하위 객체가 상위 흐름을 직접 호출하기보다 이벤트를 발생시키고 Scene Coordinator가 구독한다.
- 영속 게임 상태는 당장은 하나의 얇은 세션 객체로 유지하되 UI가 직접 상태를 변경하지 않게 한다.
- `SceneTransition`은 전환 표현만 담당하고 Scene 이름 결정은 Scene 흐름 계층에 둔다.

### 5.3 클래스 책임 혼합

#### `ResultPanel`

결과 표시, 고기 아이콘 생성/흡수 애니메이션, 입력 처리뿐 아니라 `GameManager.PlayerMeat`를 직접 증가시킨다. UI가 게임 경제 상태를 변경하므로 데이터와 표현의 경계가 흐리다.

권장 분리:

- `ResultPanel`은 표시와 사용자 확인 이벤트만 담당한다.
- 보상 적용은 `ResultSceneController` 또는 별도 결과 흐름 클래스가 담당한다.
- 기존에는 고기 하나의 흡수 완료마다 재화가 증가하므로, 시각적 타이밍을 보존하는 이벤트 계약이 필요하다.

#### `HuntingStageManager` / `StageEndingManager`

전자는 결과 계산과 저장, 후자는 플레이 비활성화·캐릭터 애니메이션·Timeline·Scene 이동을 담당한다. 두 클래스의 경계는 비교적 명확하지만 직접 Singleton 참조가 결합을 만든다. Inspector 참조 또는 이벤트 연결로 변경할 수 있다.

#### `Character`

이동, 애니메이터 상태, 방향 전환을 함께 처리한다. 별도 `RigidbodyMover`와 `CharacterVisual`도 존재하지만 실제 Scene/Prefab에서 사용되지 않아 두 이동 구조가 공존한다. 현재 플레이 감각에 직접 영향을 주므로 어느 방식을 표준으로 삼을지는 플레이 기준값을 먼저 기록한 후 결정해야 한다.

### 5.4 데이터와 로직

- `TeammateData`, `RewardTable`은 적절한 데이터 객체다.
- `StageResultData`는 Scene 간 전달 DTO로 단순하며 현재 규모에 적합하다.
- `ObservableValue`는 이름만 보면 일반 데이터 클래스처럼 보이지만 실제로는 `MonoBehaviour` 컴포넌트다. `ObservableFloat` 또는 `FloatValueComponent`처럼 런타임 형태가 드러나는 이름을 검토할 수 있다.
- 고기 수량과 팀원 수가 `float` 기반 `ObservableValue`로 관리된다. 정수형 전환은 표시와 계산 결과에 영향을 줄 수 있으므로 기능 보존 리팩토링 범위에서는 제외한다.
- `RopeData`는 현재 코드와 에셋 어디에서도 사용되지 않는다. 향후 설계 초안인지 폐기 코드인지 확인 후 처리해야 한다.
- 향후 TODO의 포만도/저장 기능을 고려하면 `GameManager`에 새 값을 계속 추가하기보다 런타임 세션 상태와 저장 데이터의 경계를 먼저 정의하는 편이 확장에 유리하다. 이번 단계에서는 경계만 고려하고 해당 기능이나 `SaveData`를 추가하지 않는다.

### 5.5 Inspector 사용성

현재 Header는 일부 적용되어 있으나 Tooltip, Min, Range는 거의 없다. 기획자가 자주 조절할 가능성이 큰 값은 다음과 같다.

- Rope: 포인트 간격, 당김 거리/힘, 중력, 텐션 임계값, 위험 텐션, 내구도 감소/회복
- Player: 스태미나 소비량, 회복량, 회복 지연, 로프 무게
- Animal: 체력, 로프 무게
- Teammate: 공격력, 사거리, 선딜/후딜, 편차
- Team: 최대 인원, 후보 간격, 대기 범위
- Projectile: 속도, 비행 시간, 포물선 높이, 수명
- UI: 애니메이션 시간, 간격, 스케일

기획 데이터 정리 원칙:

- 이미 캐릭터 유형별로 달라지는 팀원 수치는 `TeammateData`에 유지한다.
- 한 Scene 또는 한 Prefab에서만 쓰는 값은 `SerializeField`로 유지한다.
- 여러 Prefab/Scene이 공유하고 기획자가 반복 조정하는 값만 ScriptableObject 후보로 삼는다.
- 음수가 허용되지 않는 시간, 속도, 거리에는 `[Min(0f)]`를 우선 적용한다.
- 제한 범위가 디자인적으로 확정된 값에만 `[Range]`를 적용한다.
- 단위와 영향이 불명확한 값에는 Tooltip을 추가한다.
- 필드명 변경 시 기존 Scene/Prefab 값 유실 방지를 최우선으로 한다.

현재 코드에 남은 대표적인 하드코딩 값은 상호작용 UI의 화면 오프셋 `50`, 로프 투사체 낙하 한계 `-1`, 카메라 Priority `20/0`, 결과 데이터의 팀원 수 `3`, 애니메이터 파라미터 문자열, 텐션 색상 보간 속도 `10`이다. 값을 Inspector로 옮길 때는 현재 값을 기본값으로 그대로 유지한다.

### 5.6 네이밍과 코드 스타일

프로젝트 전체에 명시적 접근 제한자, 중괄호/공백, 멤버 순서, 주석 언어와 형식이 혼재한다. 예시는 다음과 같다.

- 축약 변수: `rb`, `cam`, `lt`, `tc`, `tm`, `tmc`, `bg`, `seq`, `dist`, `dir`
- 역할이 모호한 메서드: `CoPlay`, `Tick`, `LoadTeam`, `Move`, `UpdateData`
- 표기 불일치: `UnLockMovement`, `Anim_PerformAttack`, `HP`, `RequiredMeat`
- `public` 필드와 `[SerializeField] private` 필드 혼용
- 사용하지 않는 `using`, 빈 Unity 메시지, 주석 처리된 구현 존재
- 한 줄 조건문과 중괄호 사용 방식, 연산자 주변 공백 불일치

권장 기준:

- 타입/메서드/프로퍼티: PascalCase
- private 필드/지역 변수/매개변수: camelCase
- Inspector 필드: `[SerializeField] private`
- 이벤트: 과거형 또는 상태 변화가 드러나는 이름(예: `RopeReset`, `ResultConfirmed`)
- Coroutine: `...Routine`, 입력 콜백: `On...Input`
- Unity 메시지 → public API → protected API → private 메서드 순으로 멤버 정렬
- 코드가 설명하는 내용의 주석은 제거하고 의도/제약을 설명하는 주석만 유지
- Animation Event 메서드명 변경은 `.anim` 이벤트 참조를 함께 검증

이름 변경 예시:

| 현재 | 제안 |
|---|---|
| `rb` | `rigidbody2D` |
| `cam` | `mainCamera` |
| `lt` | `elapsedLifetime` |
| `tm` | `teammate` |
| `tmc` | `teammateController` |
| `CoPlay` | `FlashRoutine` |
| `CoAttackSequence` | `AttackSequenceRoutine` |
| `UnLockMovement` | `UnlockMovement` |
| `UpdateMarkerPos` | `UpdateMarkerPositions` |
| `SetVisibleTeammates` | `UpdateVisibleTeammates` |

### 5.7 폴더 구조

현재 `Character`, `Component`, `Manager`, `Object`, `Util`처럼 단수/복수와 역할 기준이 섞여 있다. `Component`와 `Object`는 범위가 넓어 파일 위치를 예측하기 어렵다.

권장 구조는 다음과 같다.

```text
Assets/Scripts
├─ Core
│  ├─ GameSession
│  └─ SceneFlow
├─ Characters
│  ├─ Common
│  ├─ Player
│  ├─ Animals
│  └─ Teammates
├─ Rope
│  ├─ Core
│  ├─ Input
│  └─ Presentation
├─ Combat
│  └─ Projectiles
├─ Interaction
├─ Stages
│  ├─ Home
│  ├─ Hunt
│  └─ Result
├─ UI
│  ├─ Common
│  ├─ Animation
│  └─ Screens
├─ Data
├─ Presentation
└─ Utilities
```

프로젝트 규모를 고려해 빈 폴더나 파일 하나뿐인 깊은 계층은 만들지 않는다. 예를 들어 `Rope/Core`와 `Rope/Presentation`은 책임 분리가 실제로 일어나는 시점에만 생성한다.

주요 이동 예시:

| 현재 | 제안 위치 |
|---|---|
| `Manager/GameManager.cs` | `Core/GameSession/GameManager.cs` 또는 승인 시 `GameSession.cs` |
| `Manager/SceneLoader.cs`, `SceneTransition.cs` | `Core/SceneFlow/` |
| `Manager/TeamManager.cs`, `Object/HomeExit.cs` | `Stages/Home/` |
| `Manager/HuntingStageManager.cs`, `StageEndingManager.cs`, `TeammateSpawner.cs` | `Stages/Hunt/` |
| `Manager/ResultSceneManager.cs` | `Stages/Result/` |
| `Character/Controller/Player...` | `Characters/Player/` |
| `Character/Controller/Animal...` | `Characters/Animals/` |
| `Character/Controller/Teammate...` | `Characters/Teammates/` |
| `Weapons/` | `Combat/Projectiles/` |
| `Object/EndPoint.cs` | `Stages/Hunt/` |
| `UI/Animated/` | `UI/Animation/` |
| `UI/Component/` | `UI/Common/` |
| `Util/` | `Utilities/` |

파일 이동은 클래스 수정과 별도 단계로 수행하는 것이 좋다. Unity Editor에서 이동하거나 `.meta` 파일을 함께 이동해야 Scene/Prefab의 MonoScript 참조가 보존된다.

### 5.8 사용되지 않거나 불완전해 보이는 코드

정적 코드 참조와 Scene/Prefab YAML 참조를 기준으로 다음 항목은 현재 활성 사용처를 확인하지 못했다.

- `CharacterMovement`
- `CharacterVisual`
- `RigidbodyMover` (`CharacterVisual`에서만 참조되며 둘 다 에셋 연결 없음)
- `RopeData`
- `StraightProjectile`
- `HoverUI`

추가로 `EndPoint` 내부에는 주석 처리된 과거 구현이 남아 있고, `DamageFlash`는 Scene에 존재하지만 `Animal`의 연결/호출 코드가 주석 처리되어 있다. Reflection, Animation Event, 향후 작업 의도를 정적 분석만으로 확정할 수 없으므로 승인 없이 삭제하지 않는다.

### 5.9 테스트와 변경 안전성

현재 자동 테스트와 asmdef가 없다. 핵심 플레이가 물리, Coroutine, Timeline, DOTween, Animation Event, Scene 직렬화에 의존하여 단순 컴파일만으로 기능 보존을 확인하기 어렵다.

리팩토링 전에 최소 기준 시나리오를 기록해야 한다.

1. Home에서 후보 모집/퇴출, 줄 이동, 필요 고기 표시, 최대 인원/비용 검증
2. 팀 확정 시 고기 차감과 Hunt Scene 팀원 생성
3. 플레이어 이동, 로프 조준/발사/부착/당김/해제
4. 텐션/내구도/스태미나 UI와 카메라 전환
5. 근거리/원거리 팀원의 접근, 공격, 투사체 충돌
6. 사냥 성공 Timeline, 결과 등급/시간/고기 표시와 재화 반영
7. 사냥 실패 Timeline과 Home 복귀
8. Scene을 반복 순환했을 때 영속 객체 중복 여부

순수 로직인 `RewardCalculator`와 향후 분리할 로프 계산에는 EditMode 테스트를 추가하기 좋다. 다만 테스트 및 asmdef 추가도 프로젝트 파일 변경이므로 별도 승인 범위에 포함한다.

## 6. 우선순위별 리팩토링 제안

### P0. 변경 안전망과 직렬화 보존 기준 수립

목적: 기능 변경 없는 리팩토링임을 확인할 수 있는 기준을 먼저 만든다.

- 위 수동 플레이 기준 시나리오와 현재 Inspector 핵심값 기록
- Scene/Prefab Missing Script 및 Console 오류 확인
- 주요 ScriptableObject 값과 Scene 전환 순서 기준화
- 가능하면 `RewardCalculator` EditMode 테스트 추가
- 각 단계마다 컴파일, Missing Script, 플레이 시나리오를 재검증

영향: 런타임 기능 변경 없음. 테스트를 추가할 경우 테스트 폴더와 asmdef가 추가될 수 있다.

### P1. 안전한 정리와 폴더 재구성

목적: 동작 코드를 바꾸기 전에 탐색성과 일관성을 높인다.

- 승인된 폴더 구조로 스크립트와 `.meta` 함께 이동
- 사용하지 않는 `using`, 빈 메서드, 명백한 과거 주석 정리
- 접근 제한자, 공백, 중괄호, 멤버 순서 통일
- 죽은 코드 후보는 별도 확인 후 삭제 또는 `Legacy`가 아닌 명시적 보류 목록으로 관리
- 첫 단계에서는 namespace와 asmdef 도입을 보류하여 직렬화 위험 축소

영향: 코드 동작은 동일하며 Unity 참조 보존이 핵심 검증 항목이다.

### P1. `Rope` 책임 분리

목적: 프로젝트 핵심 시스템의 변경 위험을 낮춘다.

- Verlet 포인트/제약 계산을 `RopeSimulation`으로 추출
- 카메라 Priority 변경을 `RopeCameraFeedback`으로 이동
- `Points`는 변경 가능한 `List` 대신 읽기 전용 노출 검토
- 상태 전환, 힘 계산, 내구도 갱신 메서드의 이름과 순서 정리
- 모든 수치와 Update/FixedUpdate 타이밍은 그대로 유지

영향 클래스: `Rope`, `RopePoint`, `RopeVisual`, 신규 시뮬레이션/카메라 피드백 클래스.

### P1. Scene 흐름과 Manager 결합 완화

목적: `AI_RULES.md`의 “Manager끼리 직접 참조하지 않는다” 원칙에 맞춘다.

- `HuntingStageManager`와 `StageEndingManager`를 Inspector 참조 또는 이벤트로 연결
- `TeammateSpawner`가 종료 Manager에 직접 등록하지 않도록 Hunt Scene Coordinator가 연결
- `Animal`은 사망/도착 이벤트만 알리고 Hunt Scene Coordinator가 결과 처리
- `TeamManager`의 영속 상태 반영을 명시적 의존성 또는 상위 흐름으로 이동
- Singleton 제거는 한 번에 하지 않고 Scene-local 객체부터 단계적으로 수행

영향 클래스: `Animal`, Hunt 관련 Manager, `TeammateSpawner`, Home 관련 Manager, `GameManager`.

### P2. 팀 편성 책임 분리

목적: 팀 선택 규칙과 화면상 후보 배치를 분리한다.

- `TeamManager`를 팀 선택 흐름 중심으로 축소
- 줄 위치, Cut Line, 대기 위치를 `CandidateLineup`으로 추출
- 후보 비용 합계와 유효성 계산을 명확한 메서드로 정리
- `TeammateCandidate`의 데이터/모집 상태 프로퍼티 캡슐화
- `RequiredMeat` public 필드를 private 직렬화 필드 + 읽기 전용 프로퍼티로 변경

영향 클래스: `TeamManager`, `TeammateCandidate`, `HomeExit`, `TeammateDataUI`, 신규 배치 컴포넌트.

### P2. 결과 화면의 데이터 변경과 표현 분리

목적: UI가 게임 상태를 직접 변경하지 않도록 한다.

- `ResultPanel`은 결과 표시, 애니메이션, 확인 이벤트만 담당
- 재화 보상 적용은 `ResultSceneManager` 계층에서 담당
- 고기 흡수 애니메이션의 개별 완료 타이밍은 기존과 동일하게 보존
- 결과 데이터가 없을 때의 처리 정책은 버그 수정 항목으로 별도 승인

영향 클래스: `ResultPanel`, `ResultSceneManager`, `GameManager`.

### P2. 네이밍과 Inspector 개선

목적: 코드와 Inspector에서 역할과 단위를 바로 이해할 수 있게 한다.

- 축약어와 모호한 메서드명을 의미 중심으로 변경
- ScriptableObject public 필드를 private 직렬화 + 읽기 전용 프로퍼티로 캡슐화
- `[Header]`, `[Tooltip]`, `[Min]`, 확정된 경우에만 `[Range]` 적용
- 하드코딩된 조정값을 현재 기본값 그대로 직렬화
- 이름 변경 시 모든 코드 참조, Scene/Prefab 직렬화, Animation Event를 함께 검증

영향: 대부분의 런타임 스크립트와 데이터 에셋. 넓은 변경이므로 기능 단위로 나눠 진행해야 한다.

### P3. 공통 캐릭터 이동 구조 정리

목적: 현재 공존하는 transform 기반 이동과 `RigidbodyMover` 기반 이동의 방향을 통일한다.

- 실제 플레이 감각과 물리 결과를 기준으로 표준 이동 방식을 결정
- 사용되지 않는 `CharacterMovement`, `CharacterVisual`, `RigidbodyMover` 처리 결정
- `Character`의 이동/시각 책임 분리는 플레이 결과가 완전히 같음을 확인할 수 있을 때만 진행

영향: Player, Animal, Teammate 전체. 플레이 감각에 직접 영향을 줄 가능성이 있어 후순위가 적절하다.

### P3. namespace와 Assembly Definition 검토

목적: 프로젝트가 더 커질 때 타입 충돌과 컴파일 범위를 관리한다.

- 폴더 구조가 안정된 뒤 namespace 도입 여부 검토
- Editor 코드나 테스트가 늘어나기 전에는 다수의 asmdef 분리를 지양
- 현재 규모에서는 하나의 Runtime asmdef도 실익이 제한적이므로 필수 작업이 아니다.

## 7. 리팩토링과 분리해야 할 코드 리뷰 항목

다음은 정적 분석 중 발견했지만 기능 보존 리팩토링에 섞어 수정하면 안 되는 항목이다. 재현 확인 후 별도 버그 수정 승인 대상으로 다룬다.

- `RangedTeammate.GetTargetPosition()`이 계산한 `aimOffset` 적용 위치가 아니라 원래 타겟 위치를 반환한다.
- `Teammate.AttackRange`가 프로퍼티를 읽을 때마다 새로운 Random 값을 반환하여 같은 프레임의 거리 판단도 달라질 수 있다.
- `HuntingStageManager`의 성공/실패 결과에 `TeammateCount = 3`이 하드코딩되어 있다.
- `RewardCalculator`는 보상 규칙 배열이 비어 있으면 마지막 요소 접근에서 예외가 발생한다.
- `ResultSceneManager`는 `StageResultStorage.Current`가 없을 때 null 결과를 그대로 `ResultPanel`에 전달할 수 있다.
- `GameManager` 중복 생성 시 컴포넌트만 파괴하므로 중복 GameObject와 부속 컴포넌트가 남을 가능성을 확인해야 한다.
- `ResultSceneManager.SetVisibleTeammates()`는 저장된 팀원 수가 표시 오브젝트 수보다 많을 때 범위를 벗어날 수 있다.
- `PositionTrack`은 `FindObjectsByType<Teammate>`를 사용해 런타임 전역 탐색에 의존한다. 이는 `FindObjectOfType` 금지 원칙의 취지와 충돌한다.
- `PlayerInteractor`의 `Camera.main` 반복 접근과 매 프레임 텍스트 할당은 현재 규모에서는 작지만 캐시/변경 시점 갱신으로 개선할 수 있다.
- `Player.OnRopeReel()`에서 스태미나 고갈 후 `rope.Release()`가 콜백을 통해 필드를 정리한 뒤 다시 null을 대입하는 흐름은 기능 확인이 필요하다.
- `RopeProjectile`을 재사용할 때 기존 Rigidbody 속도를 명시적으로 초기화하지 않으므로 재발사 궤적을 확인해야 한다.

## 8. 권장 실행 순서와 승인 단위

한 번에 전체를 바꾸지 않고 아래 단위별로 승인과 검증을 받는 것을 권장한다.

1. P0 기준 시나리오와 직렬화 안전망
2. 폴더 이동만 수행
3. 코드 스타일/명백한 불용 코드 정리
4. Rope 시뮬레이션 책임 분리
5. Rope 카메라 피드백 분리
6. Hunt Scene Manager 결합 완화
7. Team 편성 책임 분리
8. Result UI와 보상 적용 분리
9. 네이밍/Inspector를 기능 영역별로 개선
10. 캐릭터 이동 구조와 죽은 코드 처리 여부 결정

각 단계 완료 시 다음 내용을 `Report.md`에 누적한다.

- 변경 내용과 이유
- 영향받은 클래스, Scene, Prefab, 데이터 에셋
- 추가/이동/삭제된 구조
- 기능 보존 검증 결과
- 수정하지 않았지만 개선을 권장하는 항목

## 9. 결론

현재 프로젝트는 1~3분 단위의 작은 캐주얼 헌팅 루프를 구현하기에 충분히 직접적인 구조이며, ScriptableObject와 Rope 인터페이스, 공통 UI 애니메이션 등 좋은 분리 기반이 이미 있다. 가장 먼저 손볼 부분은 새로운 계층을 많이 추가하는 것이 아니라 핵심 `Rope`의 계산 책임을 제한적으로 추출하고, Scene 흐름 객체 사이의 전역 직접 참조를 줄이며, 폴더와 이름을 역할 기준으로 통일하는 것이다.

승인 전에는 위 리팩토링을 실행하지 않는다.
