# 우가우가 리팩토링 결과 보고서

## 1. 작업 개요

`Markdown/Analysis.md`에서 승인된 범위에 따라 게임 플레이, 밸런스, 연출, UI 디자인과 신규 기능을 변경하지 않고 프로젝트 기반 구조를 정리했다.

주요 목표는 다음과 같다.

- 스크립트 폴더를 역할 기준으로 재구성
- 핵심 `Rope` 클래스의 책임 축소
- Manager와 캐릭터/UI 사이의 전역 참조 완화
- 팀 편성 배치와 결과 보상 처리의 책임 분리
- 기획 데이터 캡슐화 및 Inspector 사용성 개선
- 네이밍, 접근 제한자, 들여쓰기와 멤버 구성 정리
- Unity 직렬화 참조와 기존 수치 보존

## 2. 변경 요약

### 프로젝트 구조

기존의 `Character`, `Component`, `Manager`, `Object`, `Util`, `Weapons` 중심 구조를 실제 역할에 맞게 재배치했다.

```text
Assets/Scripts
├─ Characters
│  ├─ Animals
│  ├─ Common
│  ├─ Player
│  └─ Teammates
├─ Combat
│  ├─ Projectiles
│  └─ Weapons
├─ Core
│  ├─ GameSession
│  ├─ SceneFlow
│  └─ Values
├─ Data
├─ Interaction
├─ Presentation
├─ Rope
│  ├─ Core
│  ├─ Input
│  └─ Presentation
├─ Stages
│  ├─ Home
│  ├─ Hunt
│  └─ Result
├─ UI
│  ├─ Animation
│  ├─ Common
│  └─ Screens
└─ Utilities
```

이동한 기존 C# 파일은 원래 `.meta`를 함께 이동했다. 따라서 기존 MonoScript GUID와 Scene/Prefab 참조는 유지된다.

### 추가된 클래스

| 클래스 | 역할 |
|---|---|
| `GameSession` | 고기, 확정 팀, 최대 팀원 수의 런타임 접근 경계 |
| `RopeSimulation` | RopePoint 생성, Verlet 적분, 길이 제약 계산 |
| `RopeCameraFeedback` | 로프 부착/리셋 이벤트에 따른 카메라 Priority 처리 |
| `CandidateLineup` | 팀 후보 줄 배치, Cut Line, 대기 위치 계산 |

### 제거된 클래스

| 클래스 | 제거 이유 | 복구 방법 |
|---|---|---|
| `CharacterMovement` | 내용이 없는 MonoBehaviour이며 코드와 Unity 에셋 참조가 모두 없음 | Git에서 기존 파일과 `.meta` 복구 가능 |

정적 분석만으로 향후 사용 의도를 확정하기 어려운 다른 미사용 후보는 삭제하지 않았다.

## 3. 영역별 변경 내용

### 3.1 Rope

#### 변경 전

`Rope` 한 클래스가 다음 책임을 모두 담당했다.

- 상태 전환
- 캐스팅
- RopePoint 관리
- Verlet 적분과 제약 계산
- 텐션과 힘 분배
- 내구도
- 카메라 Priority

#### 변경 후

- `Rope`는 상태 전환, 외부 API, 텐션/힘/내구도 흐름을 조정한다.
- `RopeSimulation`이 포인트 목록, 길이, 적분과 제약 계산을 담당한다.
- `RopeCameraFeedback`이 `RopeAttached`, `RopeReset` 이벤트를 구독해 카메라를 제어한다.
- `Rope.Points`는 변경 가능한 `List` 대신 `IReadOnlyList`로 노출한다.
- 카메라 활성/비활성 Priority와 로프 색상 전환 속도를 Inspector 값으로 노출했다.
- 기존 물리 수치와 `FixedUpdate` 호출 순서는 유지했다.

영향 클래스:

- `Rope`
- `RopePoint`
- `RopeVisual`
- `RopeProjectile`
- `RopeActionController`
- 신규 `RopeSimulation`
- 신규 `RopeCameraFeedback`

### 3.2 런타임 게임 상태

`GameManager.Instance`를 여러 Manager와 UI가 직접 조회하던 구조에서 `GameSession`을 런타임 상태 접근 경계로 추가했다.

- `GameManager`는 영속 객체 수명과 초기 세션 연결을 담당한다.
- `GameSession`은 고기, 팀 데이터와 최대 팀원 수를 제공한다.
- `TeamManager`, `TeammateSpawner`, `ResultSceneManager`, `PlayerMeatCountUI`는 더 이상 `GameManager.Instance`를 직접 참조하지 않는다.
- 기존 Scene마다 존재하는 `GameManager` Prefab과 중복 처리 방식은 기능 보존을 위해 유지했다.

영향 클래스:

- `GameManager`
- 신규 `GameSession`
- `TeamManager`
- `TeammateSpawner`
- `ResultSceneManager`
- `PlayerMeatCountUI`

### 3.3 Hunt Scene 흐름

#### 변경 전

- `Animal`이 `HuntingStageManager.Instance`를 직접 호출했다.
- `HuntingStageManager`가 `StageEndingManager.Instance`를 직접 호출했다.
- `TeammateSpawner`가 `StageEndingManager.Instance`에 생성 객체를 등록했다.
- `PositionTrack`이 `FindObjectsByType<Teammate>`로 팀원을 탐색했다.

#### 변경 후

- `Animal`은 `Captured`, `ReachedEndPoint` 이벤트만 발생시킨다.
- `TeammateSpawner`는 `TeammateSpawned` 이벤트를 발생시킨다.
- `HuntingStageManager`가 이벤트를 구독하여 결과 계산, 종료 연출 등록과 성공/실패 흐름을 조정한다.
- `PositionTrack`은 `TeammateSpawned` 이벤트로 Marker를 생성한다.
- `HuntingStageManager`, `Animal`, `TeammateSpawner`, `StageEndingManager`, `PositionTrack`의 관계는 Hunt Scene의 직렬화 참조로 명시했다.
- 기존 결과 데이터의 `TeammateCount = 3`은 기능 보존을 위해 `resultTeammateCount` Inspector 값으로 옮기고 기본값을 3으로 유지했다.

영향 클래스 및 에셋:

- `Animal`
- `HuntingStageManager`
- `StageEndingManager`
- `TeammateSpawner`
- `PositionTrack`
- `HuntScene.unity`

### 3.4 Home Scene과 팀 편성

- 후보 줄 배치, Cut Line 위치, 대기 위치 계산을 `CandidateLineup`으로 추출했다.
- `TeamManager`는 선택 목록, 유효성, 비용과 팀 확정 흐름을 담당한다.
- `TeammateCandidate`가 `TeamManager.Instance`를 직접 호출하는 대신 `RecruitmentChanged` 이벤트를 발생시킨다.
- `TeamManager` Singleton을 제거했다.
- `HomeExit`은 같은 Scene의 `TeamManager`를 Inspector 참조로 받는다.
- 필요 고기 필드를 private 직렬화 필드로 변경하고 읽기 전용 프로퍼티를 제공한다.
- 기존 `RequiredMeat` 직렬화 이름은 `FormerlySerializedAs`로 보존했다.

영향 클래스 및 에셋:

- `TeamManager`
- `TeammateCandidate`
- `HomeExit`
- 신규 `CandidateLineup`
- `HomeScene.unity`

### 3.5 Result Scene과 보상 적용

#### 변경 전

`ResultPanel`이 결과 UI, 애니메이션뿐 아니라 `GameManager.PlayerMeat`까지 직접 변경했다.

#### 변경 후

- `ResultPanel`은 고기 흡수 애니메이션 완료 시 `RewardCollected` 이벤트를 발생시킨다.
- `ResultSceneManager`가 이벤트를 받아 `GameSession.PlayerMeat`를 변경한다.
- 고기 하나의 흡수 완료마다 1씩 반영되는 기존 시각적 타이밍을 유지했다.
- `ResultPanel`은 표현과 사용자 확인 이벤트에 집중한다.

영향 클래스:

- `ResultPanel`
- `ResultSceneManager`

### 3.6 기획 데이터와 Inspector

`TeammateData`의 public 필드를 private 직렬화 필드와 읽기 전용 프로퍼티로 변경했다.

| 기존 필드 | 새 프로퍼티 |
|---|---|
| `type` | `Type` |
| `meatCount` | `MeatCost` |
| `speed` | `MoveSpeed` |
| `attackDamage` | `AttackDamage` |
| `attackRange` | `AttackRange` |
| `attackWindup` | `AttackWindup` |
| `attackCooldown` | `AttackCooldown` |

`RewardTable`과 `RewardRule`도 private 직렬화 필드와 읽기 전용 프로퍼티를 사용하도록 변경했다.

기존 ScriptableObject YAML 필드명은 모두 `FormerlySerializedAs`로 보존했다. 데이터 값과 보상 계산 순서는 변경하지 않았다.

주요 Inspector 개선:

- 시간, 거리, 속도, 힘에 `[Min]` 적용
- 입력 deadzone에 `[Range]` 적용
- Rope, 팀 배치와 결과 값에 Header/Tooltip 보강
- 상호작용 Prompt의 `-50` 화면 오프셋 노출
- Rope Projectile의 `-1` 회수 높이 노출
- Rope 카메라 Priority `20/0` 노출
- Rope 색상 보간 속도 `10` 노출
- 기존 하드코딩 값은 모두 현재 값과 같은 기본값 사용

### 3.7 네이밍과 코드 스타일

- `rb`, `cam`, `lt`, `tm`, `tmc`, `dist`, `dir` 등 축약 변수를 역할 중심 이름으로 변경했다.
- `UnLockMovement`를 `UnlockMovement`로 변경했다.
- `UpdateMarkerPos` 역할을 `UpdateMarkerPositions`로 명확히 했다.
- Unity 메시지와 private 멤버의 접근 제한자를 명시했다.
- 전체 `Assets/Scripts`에 동일한 C# whitespace 포맷을 적용했다.
- 빈 메서드와 사용되지 않는 `using` 일부를 정리했다.
- Animation Event가 연결된 `Anim_PerformAttack`, `Anim_EndAttack`은 이름을 유지했다.

## 4. 직렬화 보존 조치

- 이동한 기존 스크립트 55개의 `.meta`를 함께 이동했다.
- 기존 C# GUID 집합과 이동 후 GUID 집합을 비교했다.
- 제거한 `CharacterMovement` GUID를 제외한 기존 GUID가 모두 유지됨을 확인했다.
- 제거된 GUID를 참조하는 Scene, Prefab, ScriptableObject가 없음을 확인했다.
- 신규 스크립트는 고유 GUID를 가진 `.meta`를 추가했다.
- 전체 Assets에서 중복 GUID가 없음을 확인했다.
- 이름을 바꾼 직렬화 필드에는 `FormerlySerializedAs`를 적용했다.
- Scene에는 기존 객체의 fileID를 사용해 새 Inspector 참조를 연결했다.

## 5. 검증 결과

### 자동 및 정적 검증

| 검증 | 결과 |
|---|---|
| 변경 전 `dotnet build UgaUga.sln --no-restore` | 오류 0, 기존 경고 1 |
| 변경 후 동일 빌드 | 오류 0, 경고 0 |
| 현재 C# 파일과 `.cs.meta` 수 | 각각 73개, 일치 |
| 전체 Assets 중복 GUID | 0건 |
| 기존 프로젝트 스크립트 중 미해결 Unity 참조 | 0건 |
| `FindObject...` 계열 사용 | 0건 |
| 런타임의 `GameManager.Instance` 직접 소비 | 0건 |
| `TeamManager`, `HuntingStageManager`, `StageEndingManager` Singleton | 제거 |
| Teammate/Weapon 외 Prefab/ScriptableObject 내용 변경 | 없음 |

Unity Editor가 프로젝트를 연 상태여서 별도의 BatchMode Editor는 동시에 실행할 수 없었다. 열린 Editor가 폴더 이동 및 중간 변경을 임포트하고 Domain Reload를 성공적으로 수행한 기록은 확인했다. 최종 C# 상태는 생성된 Unity 프로젝트 파일을 사용한 전체 빌드로 검증했다.

### 수동 확인이 필요한 플레이 시나리오

이 환경에서는 Play Mode 조작 검증을 수행하지 않았다. 다음 항목은 Unity Editor에서 최종 확인해야 한다.

1. Home 후보 모집/퇴출, 줄 배치와 필요 고기 표시
2. 팀 확정 후 재화 차감과 Hunt 팀원 생성
3. 로프 발사, 부착, 당김, 해제, 텐션/내구도/스태미나
4. 로프 부착/리셋 시 카메라 전환
5. 근거리/원거리 팀원 이동 및 공격
6. Position Track의 동적 팀원 Marker
7. 성공 Timeline, Result 표시, 고기 흡수와 재화 반영
8. 실패 Timeline과 Home 복귀
9. Scene 반복 전환 시 영속 GameManager 동작

## 6. 기능 보존을 위해 수정하지 않은 권고 항목

다음 항목은 `Analysis.md`에서 확인했지만 버그 수정 또는 플레이 결과 변경 가능성이 있어 이번 리팩토링에서 수정하지 않았다.

- 원거리 무기의 `aimOffset` 계산 결과 미사용
- `Teammate.AttackRange`를 읽을 때마다 Random 값을 다시 계산하는 구조
- 결과 데이터의 팀원 수 기본값 3
- 빈 `RewardTable`에서 `RewardCalculator` 예외 가능성
- Result 데이터가 없을 때 `ResultSceneManager`의 null 처리
- `GameManager` 중복 시 컴포넌트만 제거하는 기존 방식
- Result 표시 팀원 수가 표시 오브젝트 수보다 클 때 범위 초과 가능성
- 스태미나 고갈 시 Player/Rope 해제 콜백 순서
- Rope Projectile 재발사 시 기존 Rigidbody 선속도 미초기화

정적 참조가 없지만 삭제하지 않은 코드:

- `CharacterVisual` / `RigidbodyMover`
- `RopeData`
- `StraightProjectile`
- `HoverUI`
- Scene에 있으나 현재 호출이 연결되지 않은 `DamageFlash`

향후 사용 계획을 확인한 뒤 별도 승인으로 삭제하거나 활성 구조에 통합하는 것을 권장한다.

## 7. 작업 범위에서 제외한 항목

- 게임 플레이 및 밸런스 변경
- 연출 및 UI 디자인 변경
- 신규 기능 추가
- 영속 SaveData와 무기 제작 시스템 구현
- namespace 및 Assembly Definition 도입
- 위 코드 리뷰 항목의 버그 수정

## 8. Teammate / Weapon 아키텍처 리팩토링

`TODO.md`의 승인된 구조에 따라 팀원과 공격 방식을 분리했다. 기존 `GameManager`의 초기 팀 목록 직렬화는 유지하고, 런타임 편성에는 팀원과 장착 무기를 함께 보관하도록 확장했다.

### 변경된 구조

```text
TeammateData ── DefaultWeapon ──> WeaponData
     │                              │
     └── Teammate Prefab            └── BaseWeapon Prefab
              │                              │
              └── WeaponHolder ─────────────┘
                          │
                  MeleeWeapon / RangedWeapon
```

| 클래스 | 변경 내용과 이유 |
|---|---|
| `Teammate` | 구체 캐릭터 클래스로 변경했다. 공격 준비·쿨다운·애니메이션 상태만 관리하고 실제 공격은 현재 무기에 위임한다. |
| `BaseWeapon` | 무기 전략의 공통 계약과 공격 수치·타이밍 계산을 제공한다. |
| `MeleeWeapon` | 기존 `MeleeTeammate`의 HitBox 판정과 피해 적용을 담당한다. 최종 피해는 기본 피해와 Strength의 합이다. |
| `RangedWeapon` | 기존 `RangedTeammate`의 투사체 준비와 발사를 담당한다. 최종 사거리는 기본 사거리와 Strength의 합이다. |
| `WeaponHolder` | WeaponData에 지정된 런타임 무기 프리팹을 장착하고 현재 무기 수명을 관리한다. |
| `WeaponData` | 무기 ID, 표시 정보, 희귀도, 기본 공격 수치와 공격 타이밍을 관리하는 ScriptableObject다. |
| `TeammateData` | 공격 데이터를 제거하고 캐릭터 정체성, 모집 정보, Speed, Strength, 기본 무기와 캐릭터 프리팹만 보관한다. |
| `TeammateLoadout` | 팀원과 현재 장착 무기를 함께 전달하는 직렬화 가능한 데이터 구조다. |
| `GameSession` | 기존 초기 `List<TeammateData>`를 기본 무기 Loadout으로 변환하며, Home/Hunt/Result에는 Loadout 목록을 제공한다. |
| `TeammateSpawner` | `TeammateType` 분기를 제거하고 TeammateData의 캐릭터 프리팹과 Loadout의 무기를 조합해 생성한다. |

### 에셋과 Inspector

- 근거리·원거리 무기 프리팹과 `WeaponData` 에셋을 추가했다.
- 기존 두 팀원 프리팹은 동일한 `Teammate + WeaponHolder` 구성으로 변경했다.
- 기존 유효 공격 수치와 타이밍을 무기 데이터로 이동했다.
- 최초 Strength를 0으로 설정해 기존 피해량과 사거리를 유지했다.
- Teammate와 Weapon에 변경되지 않는 ID 필드를 추가해 향후 저장 데이터가 에셋 이름에 의존하지 않도록 준비했다.
- `ShareRate`는 후속 팀원 구성 UX 작업에서 0.1~1.0 실수 범위로 전환했다.
- `TeammateCandidate`와 `GameSession`이 장착 무기를 보존하므로 HomeScene과 HuntScene 사이에서 장비가 유지된다.

### 검증

| 검증 | 결과 |
|---|---|
| 생성된 Unity 프로젝트 파일 기반 `dotnet build` | 오류 0, 경고 0 |
| 제거된 `MeleeTeammate`, `RangedTeammate`, `TeammateType` 참조 검색 | 0건 |
| 신규 `.meta` GUID 중복 검사 | 0건 |
| 기존 Melee/Ranged 피해·사거리·선딜레이·쿨다운 데이터 비교 | 동일 |
| Rope, Enemy, Scene Flow 코드 변경 | 없음 |

Unity Editor가 프로젝트를 연 상태이므로 별도 BatchMode 플레이 검증은 실행하지 않았다. Home에서 팀 편성 후 Hunt 진입, 근거리 판정, 원거리 투사체 발사와 Result 복귀는 Play Mode에서 최종 확인이 필요하다.

### 수정하지 않은 개선 권고

- 기존 원거리 조준 오프셋 계산 결과가 실제 목표 위치에 적용되지 않는 동작은 기능 보존을 위해 그대로 유지했다.
- 공격 사거리를 조회할 때마다 Random Offset을 다시 계산하는 동작도 플레이 변경 가능성이 있어 유지했다.
- 취소된 원거리 공격의 장전 투사체를 명시적으로 파괴하지 않는 기존 동작은 별도 버그 수정 대상으로 남겼다.
- 실제 무기 교체 UI, 제작, 랜덤 무기와 영속 SaveData는 이번 작업 범위에서 제외했다.

## 9. 팀원 구성 UX 시스템

`TODO.md`의 팀원 구성 UX 요구사항에 따라 HomeScene의 기존 상호작용·후보·팀 편성·Loadout 구조를 확장했다. 별도 Manager는 추가하지 않았으며, `TeamManager`는 편성 규칙, `CampfireCouncilController`는 회의 상태 흐름, View 클래스는 UI 표현만 담당한다.

### 변경된 파일

- Scene/Prefab/Data: `HomeScene.unity`, `GameManager.prefab`, `Teammate_Test.asset`, `Ranged Teammate Data.asset`
- Character/Input: `Character.cs`, `PlayerMoveController.cs`, `Teammate.cs`, `TeammateCandidate.cs`
- Session/Data: `GameManager.cs`, `GameSession.cs`, `TeammateData.cs`, `TeammateLoadout.cs`, 신규 `WeaponStack.cs`, `WeaponInventory.cs`
- Home 흐름: `TeamManager.cs`, 신규 `CampfireCouncilController.cs`, `CampfireInteractable.cs`, `CampfireSlotLayout.cs`
- UI: `TeammateDataUI.cs`, 신규 `TeamManagementView.cs`, `TeamManagementPanelView.cs`, `WeaponInventoryItemView.cs`
- Prefab: 신규 `Team Management Panel.prefab`, `Weapon Slot.prefab`, `Weapon Drag Ghost.prefab`, `Hover Info.prefab`; 수정 `Teammate Candidate.prefab`

### 상태 및 NPC 배치

- `CampfireCouncilState`에 `Normal`, `Gathering`, `TeamManagement` 흐름을 구현했다.
- HomeScene에 기존 `IInteractable`을 사용하는 모닥불을 추가했다.
- F 입력으로 회의를 시작하면 플레이어 이동 입력을 잠그고 지정 좌석으로 이동시킨다.
- `CampfireSlotLayout`이 전체 후보 수를 기준으로 모닥불 좌우 Slot을 매 회의마다 새로 계산한다.
- 후보 상태는 `Walking → ReachedPosition → Rotating → Ready` 순서로 진행한다.
- 위치 도착 후 기존 `Character.Flip`으로 모닥불을 바라본 후보만 Ready가 된다.
- 모든 후보와 플레이어가 준비된 뒤에만 팀 관리 UI를 표시한다.
- 회의 종료 시 비팀원은 원래 생활 위치로 복귀하고, 팀원은 모닥불 주변에 남아 착석 상태를 유지한다.
- 임시 착석 표현은 `Character.SetSittingState`로 캡슐화했으며 내부에서 `Moving` Animator Bool을 사용한다.

### 팀 편성 및 지분

- 후보 클릭과 기존 상호작용 호출은 동일한 팀 변경 요청으로 연결된다.
- `ShareRate`를 `float 0.1~1.0`으로 변경했다.
- 전체 팀 ShareRate가 1.0을 초과하는 추가 요청은 취소된다.
- 초과 시 팀 패널과 좌측 상단 총 지분 원형 UI가 빨간색으로 점멸한다.
- 기존 세 명의 초기 팀 지분은 `0.3 + 0.4 + 0.3 = 1.0`이 되도록 마이그레이션했다.
- 기존 최대 팀원 수, Meat 비용, HomeExit의 팀 유효성 검사와 Confirm 흐름은 유지했다.

### 월드 기반 무기 인벤토리와 Drag & Drop

- `WeaponStack`과 `WeaponInventory`를 추가해 `GameSession`이 무기별 수량을 보관한다.
- GameManager Inspector에서 초기 무기와 수량을 설정할 수 있다.
- 팀 관리 UI에는 플레이어가 보유한 무기를 수량만큼 아이콘 Slot으로 표시하며 팀원 목록 UI는 제거했다.
- 무기 아이콘을 월드의 후보 NPC로 드래그하면 장착되고, NPC가 들고 있던 기존 무기는 인벤토리로 자동 반환된다.
- 무장 NPC를 일정 시간 클릭 Hold하면 장착 무기를 직접 드래그할 수 있다. 다른 NPC에 놓으면 이전되고, 인벤토리나 잘못된 위치에 놓으면 플레이어 인벤토리로 반환된다.
- 드래그 시작 시 소유권을 인벤토리 또는 NPC에서 먼저 분리하고, 모든 종료 경로에서 NPC 장착 또는 인벤토리 반환 중 하나를 수행하므로 무기가 월드에 유실되지 않는다.
- 팀에서 제외해도 후보의 장착 무기는 유지된다. `GameSession`이 씬 NPC의 고유 `candidateId`별 장비를 팀 목록과 분리해 기억하므로, 같은 `TeammateData`를 쓰는 후보가 여러 명이어도 HomeScene 재구성 시 각 장비 상태를 복구한다.
- 후보 프리팹의 `Teammate HandPoint`에 미리 배치한 `SpriteRenderer`가 현재 `WeaponData.Icon`을 표시한다.
- 편성 확정 시 기존 `TeammateLoadout`이 현재 장착 무기를 HuntScene으로 전달한다.

### UI와 Prefab 구성

- 화면 상단 팀원 목록을 제거하고 플레이어 무기 인벤토리와 현재 총 지분만 남겼다.
- 후보 Mouse Over 정보는 지분·힘·속도를 좌측 Label과 가로 Fill bar로 표시하며, 무기를 드래그하는 동안에도 동일하게 유지된다.
- `HomeScene`의 기존 Canvas를 재사용하고 `Team Management Panel`만 Prefab으로 생성한다.
- 무기 Slot, 드래그 Ghost, Hover Info 역시 각각 Prefab을 `Instantiate`한다. UI 계층과 Component를 코드에서 `new GameObject`/`AddComponent`로 생성하던 경로는 제거했다.
- 기존 EventSystem과 Input System UI 모듈을 재사용한다.

### 추가된 클래스

| 클래스 | 역할 |
|---|---|
| `CampfireCouncilController` | 회의 상태, 플레이어 이동/착석, 후보 Gathering과 종료 흐름 조정 |
| `CampfireInteractable` | 기존 IInteractable을 통한 회의 시작점 |
| `CampfireSlotLayout` | 모닥불 좌우의 동적 Slot 위치 계산 |
| `WeaponStack` | Inspector 직렬화 가능한 무기·수량 데이터 |
| `WeaponInventory` | 무기 추가·차감·수량 조회 및 변경 이벤트 |
| `TeamManagementView` | Prefab UI 수명, 무기 Drag 상태, UI/월드 Drop 판정, 총 지분 경고 표시 |
| `TeamManagementPanelView` | Panel 내부의 총 지분 Fill·인벤토리 Drop 영역·Slot Container 참조 제공 |
| `WeaponInventoryItemView` | Prefab 무기 Slot 표현과 Drag 입력 전달 |
| `TeammateDataUI` | Prefab Hover의 지분·힘·속도 Label 및 Fill bar 갱신 |

### 기존 시스템 연결

- F 입력: 기존 `PlayerInteractor`와 `IInteractable`
- 회의 종료: `PlayerMoveController.MoveRequested`
- NPC 이동·회전·애니메이션: 기존 `Character.SetMoveInput`, `Flip`, Animator
- 팀 편성: 기존 `TeamManager`, `GameSession`, `TeammateLoadout`
- Hunt 생성: 기존 `TeammateSpawner`와 Weapon 기반 공격 구조
- UI 입력: 기존 HomeScene `EventSystem`

### 검증 및 추후 개선

- 생성된 Unity 프로젝트 파일 기반 빌드 결과: 오류 0, 경고 0
- 신규 UI/후보 Prefab의 로컬 fileID 참조 검사: 미해결 참조 0건
- 전체 `.meta` GUID 중복 검사: 0건
- 팀 구성 UI 코드의 `new GameObject`/`AddComponent` 및 제거된 `TeamMemberSlotView` 참조 검색: 0건
- C#과 `.cs.meta`: 각각 82개로 일치
- 전체 Assets 중복 GUID: 0건
- HomeScene 신규 내부 fileID: 미해결 및 중복 0건
- Sit 전용 애니메이션이 준비되면 `SetSittingState` 내부 Animator Parameter만 교체해야 한다.
- 무기 인벤토리는 현재 GameSession 런타임 데이터이며 영속 저장은 별도 범위다.
- Hover Info의 한국어 Label은 현재 Dynamic TMP Font Asset의 원본 폰트에서 런타임에 글리프를 채운다. 다른 언어를 지원할 때는 TMP Fallback Font 구성을 함께 정리하는 편이 좋다.
- 현재 월드 Drop은 `Physics2D.OverlapPointAll`로 후보 Collider를 판정한다. 후보 전용 Layer가 확정되면 LayerMask를 추가해 탐색 범위를 좁힐 수 있다.
- 능력치 미리보기, 장착 애니메이션, 무기 상세 비교 UI는 TODO의 후순위 범위이므로 추가하지 않았다.
- `CandidateLineup`은 기존 출구 대기열 구현 보존을 위해 삭제하지 않았으나 현재 모닥불 UX에서는 사용하지 않는다.
- 실제 Play Mode에서 F 상호작용, 후보 도착/회전, 클릭 편성, Drag & Drop, 이동키 종료와 Hunt 진입을 최종 확인해야 한다.

## 10. Scene Candidate 기반 TeamManager 개선

### 변경된 클래스와 데이터 구조

- `TeamManager.candidates`는 더 이상 현재 팀 목록이 아니라 HomeScene에 미리 배치된 전체 후보 NPC 참조다.
- 실제 현재 팀은 `TeamManager.teamMembers` 런타임 목록으로 분리했다.
- `TeammateCandidate`에 씬 NPC 고유 식별자인 `candidateId`를 추가했다.
- `TeammateLoadout`은 `candidateId`, `TeammateData`, 장착 무기를 함께 보관한다.
- `GameSession`의 후보 장비 데이터는 `TeammateData + 등장 순번` 대신 `candidateId`를 Key로 사용한다.
- `TeammateCandidate.ActiveCandidates`, `TeamManager.candidatePrefab`, 후보 생성 및 Roster 보충 코드를 제거했다.

### TeamManager 초기화 방식

- HomeScene에 `candidate_1`부터 `candidate_4`까지 네 후보를 미리 배치하고 TeamManager Inspector 목록에 연결했다. 기존 초기 팀 3명과 후보 Pool을 분리해 네 번째 NPC는 비팀원 원래 위치에서 시작한다.
- `LoadConfirmedTeam`은 Scene 후보를 먼저 비팀원 상태로 초기화한 뒤 `GameSession.TeammateLoadouts`의 `candidateId`와 일치하는 NPC를 찾는다.
- 기존 초기 설정처럼 아직 `candidateId`가 없는 Loadout은 `TeammateData.TeammateId`로 순서 매칭하고, 매칭 직후 Scene ID가 포함된 Loadout으로 정규화한다.
- 동일한 `TeammateData`를 여러 NPC가 공유해도 이후에는 각 Scene NPC의 팀 포함 상태와 장비가 독립적으로 복원된다.
- 후보 ID가 비어 있거나 중복되면 초기화 시 명확한 오류를 출력한다.

### 원래 위치 및 Slot 복귀 버그

- `TeammateCandidate.originalPosition`은 각 Scene 인스턴스의 `Awake`에서 한 번만 저장하며 이후 덮어쓰지 않는다.
- 회의 Slot 이동과 원래 위치는 별도 상태로 관리한다. `ReturnHome`은 먼저 Council Slot 참조를 해제하고 현재 위치에서 `originalPosition`을 목적지로 설정한다.
- 복귀 시작 시 `transform.position`을 다른 Slot으로 변경하지 않으므로 비팀원 NPC가 다른 Slot으로 순간이동한 후 되돌아가던 경로를 제거했다.
- 버그의 구조적 원인은 전체 후보와 현재 팀이 혼재된 목록 및 런타임 후보 보충 때문에 회의 종료 시 Slot 대상이 안정적이지 않았던 것이다. Scene 후보 목록과 현재 팀 목록을 분리해 종료 시점의 대상 판정을 고정했다.

### 영향 범위와 검증

- 변경 파일: `TeamManager.cs`, `TeammateCandidate.cs`, `TeammateLoadout.cs`, `GameSession.cs`, `HomeScene.unity`, `HuntScene.unity`, `Teammate Candidate.prefab`.
- 삭제된 `Ranged Teammate Data`를 가리키던 HuntScene의 잔여 참조 2건은 현재 사용 중인 `Teammate_Test`로 교체했다. 삭제된 에셋 자체는 복구하지 않았다.
- 후보 런타임 생성 경로와 `ActiveCandidates` 참조 검색 결과: 0건.
- HomeScene 후보 수: 4개, 고유 ID: 4개, TeamManager 연결: 4개.
- HomeScene 내부 fileID 중복 및 미해결 참조: 0건.

## 11. 결론

핵심 Rope 물리 계산, Scene 흐름, 팀 편성과 결과 보상 처리의 책임 경계가 이전보다 명확해졌다. 프로젝트의 전역 `Instance` 의존은 Scene Transition과 GameManager 수명 관리만 남기고 실제 게임 로직의 직접 소비를 제거했다. 기획 데이터는 기존 값을 보존하면서 읽기 전용 API와 Inspector 제약을 갖추었고, 스크립트 위치는 역할 기준으로 예측할 수 있게 정리했다.

향후 기능 개발은 `GameSession`, 이벤트 기반 Hunt 흐름, 분리된 Rope 계산과 Stage별 폴더를 기반으로 작은 단위로 확장할 수 있다.
