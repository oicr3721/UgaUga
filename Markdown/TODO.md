# Refactoring Project : Teammate / Weapon Architecture

## 목적

현재 프로젝트에서는 Teammate가

- 이동
- AI
- 상태
- 공격
- 무기 정보
- 공격 방식

까지 모두 담당하고 있습니다.

이 구조는 현재 게임에서는 동작하지만,

앞으로 추가될

- 무기 제작
- 무기 교체
- 다양한 무기 종류
- 팀원 개성 시스템
- 팀원과 무기의 자유로운 조합

을 구현하기에는 확장성이 부족합니다.

이번 리팩토링의 목표는

**"팀원은 캐릭터이고, 무기는 전투 방식을 정의한다."**

라는 구조를 만드는 것입니다.

---

# 최종 목표

앞으로는

같은 팀원도

장착한 무기에 따라

- 근거리
- 원거리
- 차징
- 투척
- 연속공격

등을 자유롭게 수행할 수 있어야 합니다.

즉,

공격 방식은 팀원이 아니라

Weapon이 결정해야 합니다.

---

# 작업 범위

이번 작업은

현재 존재하는

- MeleeTeammate
- RangedTeammate

구조를

Weapon 기반 구조로 변경하는 리팩토링입니다.

UI,

무기 제작,

무기 교체 기능은

이번 작업 범위가 아닙니다.

다만

앞으로 쉽게 추가할 수 있도록

구조를 만들어 주세요.

---

# 작업 순서

반드시 아래 순서대로 진행해 주세요.

1. 현재 Teammate 구조 분석

2. 현재 구조의 문제점 간단히 정리

3. 새로운 구조 제안

4. 변경될 클래스 구조 간단히 설명

5. 실제 리팩토링 진행

6. 기존 기능 검증

현재 구조보다 더 적합한 설계가 있다면

먼저 이유를 설명한 뒤

제안해 주세요.

---

# 새로운 구조

## Teammate

Teammate는

캐릭터 자체를 의미합니다.

공격 방식은 알지 않습니다.

다음 역할만 담당합니다.

- 이동
- AI
- 상태(State)
- 애니메이션
- 타겟 추적
- 장착 중인 Weapon
- 공격 요청

즉

```
CurrentWeapon.Attack(...)
```

만 호출합니다.

공격의 실제 구현은

Weapon이 담당합니다.

---

## BaseWeapon

새로운 추상 클래스

BaseWeapon

을 생성합니다.

Weapon은

공격 방식을 정의하는 객체입니다.

공통 데이터

Identity

- Weapon Name
- Weapon Icon (확장 고려)
- Weapon Rarity

Attack

- Base Attack Damage
- Base Attack Range

Attack Timing

- Attack Windup
- Attack Cooldown

Offsets

- Windup Offset
- Cooldown Offset
- AttackRange Offset

등

모든 무기가 공통으로 사용하는 데이터를 관리합니다.

또한

공격 실행 역시

Weapon이 담당합니다.

---

## MeleeWeapon

현재

MeleeTeammate

가 가지고 있는

근거리 공격 관련 책임을

MeleeWeapon으로 이동합니다.

예시

- HitBox
- HitBox Size
- HitBox Offset
- Target Layer

근거리 공격 판정 역시

Weapon이 수행합니다.

---

## RangedWeapon

현재

RangedTeammate

가 가지고 있는

원거리 공격 관련 책임을

RangedWeapon으로 이동합니다.

예시

- Projectile Prefab
- Fire Point
- Aim Offset

Projectile 생성 역시

Weapon이 담당합니다.

---

# 공격 구조

현재처럼

Teammate가

공격 방식을 직접 구현하지 않습니다.

대신

Weapon이

공격을 수행합니다.

예시

```
Teammate

↓

CurrentWeapon

↓

Attack(attacker, target)
```

와 같은 구조가 이상적입니다.

---

# TeammateData 변경

현재

TeammateData는

공격 관련 데이터를 많이 가지고 있습니다.

이번 리팩토링에서는

공격 관련 데이터를 모두 제거합니다.

TeammateData는

"팀원 자체"

만 표현해야 합니다.

예시

Identity

- Name
- Portrait
- Description
- Animator

Recruitment

- ShareRate (향후 사용 예정. 사냥 성공 시 보상 Meat 분배 비율. 1~5까지의 정수형)

Stat

- Speed
- Strength

Equipment

- DefaultWeapon

정도의 데이터만 가지도록 변경해 주세요.

현재 존재하는

Attack 관련 데이터는

Weapon으로 이동합니다.

---

# Weapon 데이터

Weapon은

자신만의 기본 성능을 가집니다.

예시

- Name
- BaseDamage
- BaseRange
- Windup
- Cooldown
- HitBox
- ProjectilePrefab
- Sprite

등

공격에 필요한 모든 데이터는

Weapon이 관리합니다.

---

# 공격 계산 방식

Weapon은

공격자의 능력치를 이용하여

최종 성능을 계산합니다.

예시

근접 무기

```
FinalDamage

=

Weapon Base Damage

+

Attacker Strength
```

원거리 무기

```
FinalRange

=

Weapon Base Range

+

Attacker Strength
```

(현재는 Strength만 사용하지만,
앞으로 다른 능력치가 추가될 수 있도록 확장성을 고려해 주세요.)

중요한 점은

Weapon은

공격자의 능력치를 이용하여

최종 공격 성능을 계산한다는 것입니다.

---

# HomeScene

현재

Teammate에 대한 정보만 저장하고 있다면

Teammate가 장착한 Weapon 정보도 함께 저장할 수 있도록

구조를 변경합니다.

향후

HomeScene에서

무기를 교체할 예정입니다.

이번 작업에서는 우선

데이터 구조만 준비합니다.

UI는 구현하지 않습니다.

---

# HuntScene

사냥 씬에서 생성된 Teammate는

해당 TeammateData가 장착중인
Weapon의 공격방식에 따라 행동하게 합니다.

---

# 앞으로 추가될 시스템

이번 리팩토링은

다음 기능을 위한 기반입니다.

- 무기 제작
- 무기 교체
- 무기 희귀도
- 랜덤 무기
- 팀원 개성
- 팀원과 무기의 자유로운 조합

새로운 타입의 무기를 추가할 때는

가능하면

새로운 Weapon 클래스만 추가하면 되도록

설계해 주세요.

기존 Teammate 코드는

수정하지 않는 방향을 우선합니다.

---

# 객체지향 원칙

다음 원칙을 최대한 지켜 주세요.

- Single Responsibility Principle
- Open / Closed Principle
- Composition over Inheritance
- Strategy Pattern을 활용한 공격 방식 분리
- 중복 코드 제거
- Unity Inspector에서 관리하기 쉬운 구조
- ScriptableObject 적극 활용
- 확장성을 고려한 설계

불필요한 오버엔지니어링은 피하고,

Unity 프로젝트에서 유지보수하기 쉬운 구조를 우선합니다.

---

# 기존 시스템 유지

다음 시스템은

동작 방식이 변경되면 안 됩니다.

- Rope
- Enemy
- Save
- Scene Flow
- AI

현재 게임 플레이는

리팩토링 전과 동일하게 동작해야 합니다.

기존 SaveData도 깨지면 안 됩니다.

---

# 최종 목표

이번 작업은

"근거리 팀원"

"원거리 팀원"

을 만드는 것이 아니라,

**"팀원이 무기를 사용한다."**

라는 구조를 만드는 것이 목적입니다.

앞으로는

같은 팀원이

도끼를 들면 근접 공격,

창을 들면 원거리 공격,

다른 무기를 들면 새로운 공격 방식을 사용할 수 있어야 합니다.

이 구조를 기준으로

유지보수성과 확장성을 고려하여 리팩토링해 주세요.