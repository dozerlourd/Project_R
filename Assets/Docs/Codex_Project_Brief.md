# Project R - Codex 공통 브리핑

이 문서는 새 Codex 스레드나 새 작업자가 프로젝트를 열었을 때 공통적으로 알고 시작해야 할 내용을 정리한다.  
세부 기획은 개발 중 바뀔 수 있지만, 아래 항목은 별도 지시가 없는 한 기본 전제로 삼는다.

## 프로젝트 개요

- 장르: 2D 탑뷰 로그라이크 액션 게임.
- 엔진: Unity 6000.5.1f1.
- 렌더 파이프라인: Universal Render Pipeline, URP 17.5.0.
- 입력: Unity Input System 사용.
- 주 개발 방향: 빠른 반복 개발이 가능한 기능 단위 씬과 프리팹 중심 구조.
- 목표 감각: 플레이어가 짧은 런 안에서 이동, 전투, 아이템 획득, 성장, 맵 탐험을 반복하며 매번 다른 흐름을 경험하는 게임.

## 현재 저장소 상태

- Unity 프로젝트 루트는 `C:\GitRoot\Portfolio_Projects\Project_R`.
- 주요 Unity 폴더:
  - `Assets`: 게임 에셋, 씬, 스크립트, 프리팹이 들어갈 위치.
  - `Packages`: Unity 패키지 의존성.
  - `ProjectSettings`: Unity 프로젝트 설정.
- 현재 확인된 개발 씬:
  - `Assets/Scenes/Scene_1_CharactorDev.unity`: 캐릭터/플레이어 개발용 씬. 이름의 오탈자는 기존 파일명을 존중한다.
  - `Assets/Scenes/Scene_2_MapDev.unity`: 맵, 타일, 방 생성 개발용 씬.
  - `Assets/Scenes/Scene_3_ItemDev.unity`: 아이템, 드랍, 장비/강화 개발용 씬.
  - `Assets/Scenes/Scene_4_FinalDev.unity`: 기능 통합 및 최종 플레이 흐름 개발용 씬.

## 게임 핵심 방향

- 카메라는 탑뷰 또는 약간의 2.5D 느낌이 아닌 명확한 2D 탑뷰를 기본으로 한다.
- 플레이어 조작은 즉각적이고 읽기 쉬워야 한다.
- 핵심 루프는 다음 흐름을 기준으로 설계한다:
  1. 방 또는 구역 진입.
  2. 적과 장애물 대응.
  3. 보상 획득.
  4. 아이템/능력 선택 또는 성장.
  5. 다음 구역으로 이동.
- 로그라이크 요소는 랜덤 맵, 랜덤 보상, 런 기반 성장, 실패 후 재도전을 중심으로 한다.
- 구현 시 처음부터 거대한 시스템을 만들기보다, 작은 프로토타입을 플레이 가능하게 만든 뒤 확장한다.

## 우선 구현해야 할 시스템

- 플레이어:
  - 8방향 이동.
  - 기본 공격 또는 임시 테스트 공격.
  - 체력, 피격, 사망 처리.
- 카메라:
  - 플레이어 추적.
  - 탑뷰 화면에서 전투와 이동이 잘 읽히는 줌 레벨 유지.
- 적:
  - 플레이어 추적.
  - 접촉 또는 공격 판정.
  - 체력과 사망 처리.
- 전투:
  - 히트박스/허트박스 구분.
  - 데미지 이벤트 처리.
  - 넉백, 무적 시간 등은 기본 구조 이후 추가.
- 아이템:
  - 드랍 아이템.
  - 획득 처리.
  - 런 중 능력 변화.
  - 획득 탐색은 플레이어 중심으로 처리한다. 아이템은 기본적으로 대기하고, 플레이어가 `pickupMagnetRadius` 안의 아이템을 찾아 끌어오며 `pickupRadius` 안에서는 즉시 루팅 처리한다.
- 맵:
  - 테스트용 수동 배치 맵부터 시작.
  - 이후 방 단위 또는 타일 단위 절차 생성으로 확장.
- UI:
  - 체력 표시.
  - 런 진행 정보.
  - 아이템 선택 또는 획득 피드백.

## 개발 원칙

- 씬에 직접 붙인 임시 오브젝트보다 재사용 가능한 프리팹을 우선한다.
- MonoBehaviour는 Unity 생명주기와 씬 연결을 담당하고, 계산 가능한 규칙은 가능한 한 일반 C# 클래스로 분리한다.
- 초기 프로토타입에서는 과한 추상화를 피한다. 단, 다음 항목은 초반부터 분리한다:
  - 입력 처리.
  - 체력/데미지 처리.
  - 아이템 효과 적용.
  - 맵 생성 데이터.
- 하드코딩이 필요한 경우라도 테스트 목적임을 이름이나 주석으로 명확히 남긴다.
- 새 기능은 가능한 한 개발용 씬에서 먼저 검증한 뒤 `Scene_4_FinalDev`에 통합한다.
- 기존 파일명, 씬명, 설정은 특별한 이유 없이 변경하지 않는다.

## 권장 폴더 구조

아직 폴더가 없다면 아래 구조를 기준으로 만든다.

```text
Assets/
  Animations/
  Art/
    Characters/
    Enemies/
    Items/
    Tiles/
    UI/
  Audio/
  Materials/
  Prefabs/
    Player/
    Enemies/
    Items/
    Map/
    UI/
  Scenes/
  Scripts/
    Core/
    Player/
    Combat/
    Enemies/
    Items/
    Map/
    UI/
  ScriptableObjects/
    Items/
    Enemies/
    Map/
  Settings/
```

## 코딩 스타일

- 언어: C#.
- Unity 직렬화가 필요한 필드는 `private`와 `[SerializeField]`를 우선 사용한다.
- public 필드는 외부에서 직접 수정되어야 하는 명확한 이유가 있을 때만 사용한다.
- 클래스 이름과 파일 이름은 일치시킨다.
- Update 루프에는 무거운 탐색, 할당, 생성 로직을 넣지 않는다.
- 입력, 이동, 공격, 체력 같은 핵심 컴포넌트는 서로 직접 강결합하기보다 필요한 이벤트나 명확한 참조만 갖게 한다.
- 로그는 개발 중에는 허용하되, 반복 호출되는 로그는 정리하거나 조건부로 둔다.

## 데이터 설계 기준

- 아이템, 적, 방/맵 설정처럼 디자이너가 조정할 가능성이 높은 값은 ScriptableObject 후보로 본다.
- 런 중 변하는 상태와 원본 데이터는 분리한다.
  - 예: `ItemDefinition`은 고정 데이터, `RuntimeItemEffect`나 플레이어 스탯은 런타임 상태.
- 랜덤 요소는 가능한 한 시드 기반으로 확장할 수 있게 설계한다.
- 밸런스 값은 테스트하기 쉬운 위치에 모은다.

## 로그라이크 관련 설계 메모

- 한 런에서 얻는 성장은 런 종료 시 대부분 사라지는 것을 기본으로 한다.
- 영구 성장 시스템은 나중에 추가하되, 초반에는 핵심 액션 재미를 먼저 검증한다.
- 아이템 효과는 중첩, 교체, 일회성, 지속 효과를 구분할 수 있게 확장성을 남긴다.
- 맵 생성은 처음부터 완전 자동 생성으로 가지 않고, 다음 순서로 진행한다:
  1. 수동 테스트 방.
  2. 여러 방 프리팹을 연결.
  3. 랜덤 방 선택.
  4. 시드 기반 생성.
  5. 난이도와 보상 테이블 연동.

## Codex 작업 지침

새 Codex 스레드에서 작업할 때는 다음을 지킨다.

- 먼저 이 문서를 읽고 현재 요청이 어떤 시스템에 해당하는지 판단한다.
- 구현 전 `Assets`, `Packages/manifest.json`, 관련 씬/스크립트 구조를 확인한다.
- 기존 파일이나 씬 설정을 덮어쓰기 전에 변경 범위를 설명한다.
- Unity 메타 파일은 Unity가 생성한 파일이면 함께 유지하되, 불필요하게 손대지 않는다.
- `Library`, `Temp`, `Logs`, `UserSettings`는 일반적으로 수정 대상으로 보지 않는다.
- 기능을 추가할 때는 작은 단위로 끝까지 동작하게 만든다.
- 테스트가 가능한 순서로 작업한다:
  1. 컴파일 가능한 C# 코드 작성.
  2. 필요한 프리팹/씬 연결 지점 명시.
  3. Unity에서 확인해야 할 수동 테스트 절차 작성.
- Codex가 Unity Editor를 직접 실행하지 못하거나 테스트하지 못한 경우, 최종 답변에 검증하지 못한 부분을 분명히 적는다.

## 현재 확정되지 않은 항목

아래는 아직 프로젝트 기준으로 확정되지 않은 영역이다. 작업 중 필요하면 임시 결정을 문서나 코드에 남긴다.

- 최종 아트 스타일.
- 플레이어 캐릭터 콘셉트.
- 적 종류와 공격 패턴.
- 방 생성 방식.
- 아이템 등급, 시너지, 중첩 규칙.
- 저장/로드 및 영구 성장 여부.
- 플랫폼 목표.
- 해상도와 UI 기준.
- 사운드 방향.

## 다음 작업 추천 순서

1. `Assets/Scripts` 기본 폴더 구조 생성.
2. 플레이어 이동 프로토타입 구현.
3. 카메라 추적 구현.
4. 체력/데미지 공통 컴포넌트 구현.
5. 테스트 적 구현.
6. 기본 공격 구현.
7. 아이템 획득 프로토타입 구현.
8. 방 단위 맵 테스트 구현.
9. `Scene_4_FinalDev`에서 최소 플레이 루프 통합.

## 문서 갱신 규칙

- 프로젝트의 공통 전제가 바뀌면 이 문서를 함께 갱신한다.
- 단일 기능의 세부 구현 기록은 별도 문서로 분리해도 된다.
- 새 Codex 스레드에 전달해야 하는 내용은 이 문서에 짧게라도 남긴다.

## Prototype Movement Assumptions

- Prototype maps use `PrototypeWalkable` for floor/path trigger tilemap colliders and `PrototypeMapBounds` for blocking wall colliders.
- Prototype player movement should require a walkable surface by default, using `PlayerMovement2D`'s walkable layer mask.

## Item Layer Assumptions

- `AutoLootingItem` is reserved for items pulled by the player's pickup magnet and collected automatically inside `pickupRadius`.
- `ManiacLootingItem` is reserved for items that are not pulled by the pickup magnet and must be directly picked up or equipped by the player.

## Camera Follow Assumptions

- Prototype camera follow keeps a configurable dead zone, move speed, and curved Lerp response through `SmoothCameraFollow2D`.

## 플레이어/카메라 핵심 기능 기록

- 플레이어 이동은 8방향 입력을 지원하며 `PrototypeWalkable` 타일맵 위에서만 이동한다.
- 플레이어 대시는 스탯의 대시 속도, 대시 횟수, 대시 쿨타임을 기준으로 동작한다.
- 플레이어 스탯은 체력, 보호막, 이동속도, 대시, 공격, 루팅 반경, 임시 이동속도 버프를 관리한다.
- 자동 루팅 아이템은 `AutoLootingItem` 레이어를 사용하며 플레이어의 자석 반경 안에서 끌려오고 루팅 반경 안에서 획득된다.
- 수동 루팅/장비 아이템은 `ManiacLootingItem` 레이어를 사용하며 플레이어에게 자동으로 끌려가지 않는다.
- 카메라는 플레이어를 추적하되 데드존 안에서는 멈추고 곡선형 Lerp 응답으로 부드럽게 이동한다.
- 카메라 추적은 `followSharpness`, `cameraMoveSpeed`, `followLerpCurve`, `snapDistance`로 인스펙터에서 조절한다.
