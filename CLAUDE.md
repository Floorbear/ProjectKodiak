# 프로젝트 코디악 (Unity)

## 개요
- 2D 탑뷰 슈팅 + A-Life 팩션 워. 목표는 6주 안에 스팀 데모 수준.
- 설계 문서: Notion DB "유니티 - Project Kodiak" https://app.notion.com/p/2377ffc6a187800e8970d2183434b48a
  - AI 활용 개발 가이드라인: https://app.notion.com/p/3f07ffc6a1878158b009d79cbb814caa
- 현재 단계: 프로토타입
- 개발자는 언리얼/C++ 경험자, 유니티 입문자. 유니티 개념은 언리얼과 비교해서 설명할 것
- 아트는 전부 AI 생성. 프레임 애니메이션 대신 회전 스프라이트, 레이어드 캐릭터, 코드 기반 연출을 쓴다

## 환경 (변경 금지)
- Unity 6.3 LTS (6000.3.x), URP 2D
- Input System (구 Input Manager 사용 금지), Test Framework, 2D Tilemap Extras
- Cinemachine: 사용 예정, 아직 미설치
- Asset Serialization = Force Text

## 프로젝트 구조
```
Assets/_Project/
├─ Core/   Kodiak.Core.asmdef     순수 C# (noEngineReferences). ALife / Factions / NPC / Rules
├─ Game/   Kodiak.Game.asmdef     MonoBehaviour, 뷰, 어댑터. Core, InputSystem 참조
├─ Data/                          ScriptableObject 에셋
├─ Art/                           AI 생성 이미지 (AssetPostprocessor 대상)
├─ Prefabs/                       프리팹 (Characters, Projectiles, Props)
├─ Scenes/                        씬
├─ Editor/ Kodiak.Editor.asmdef   에디터 전용 툴, 임포터. 네임스페이스는 Kodiak.EditorTools
└─ Tests/EditMode/ Kodiak.Tests.EditMode.asmdef   Core 대상 NUnit 테스트
```
- 네임스페이스는 어셈블리 rootNamespace를 따른다 (`Kodiak.Core.*`, `Kodiak.Game.*`, `Kodiak.EditorTools.*`, `Kodiak.Tests.EditMode.*`)
- 에디터 코드의 네임스페이스를 `Kodiak.Editor`로 바꾸지 말 것. `Kodiak` 하위에서 `Editor`가 네임스페이스로 해석되어 `UnityEditor.Editor`를 상속하는 커스텀 인스펙터가 깨진다. 어셈블리 이름 `Kodiak.Editor`는 그대로 둔다

## 아키텍처 규칙
- Core 어셈블리에는 UnityEngine 참조 금지 (순수 C#)
- MonoBehaviour는 얇은 뷰/어댑터로만 사용, 게임 로직은 Core에
- FindObjectOfType / Find 계열 사용 금지, 참조는 직렬화 필드 또는 주입
- 싱글턴은 지정된 매니저만 허용
- 이벤트는 C# event로 통일
- 대량 데이터(무기 스탯, NPC 특성 등)는 CSV/JSON → ScriptableObject 임포터로 관리

## 작업 절차
- 구현 전에 계획을 먼저 제시하고 승인받을 것
- Core 로직은 EditMode 테스트를 먼저 작성
- 코드 수정 후 반드시 재컴파일 → 콘솔 에러 확인 (Unity MCP의 콘솔 읽기 사용)
- Core 수정 시 EditMode 테스트 실행
- 작은 단위로 자주 커밋

## 금지
- .meta 파일 직접 수정 금지
- .unity / .prefab YAML 직접 수정 금지 (MCP 도구나 에디터 스크립트 사용)

## 유니티 함정 체크리스트
- UnityEngine.Object에 ?. / ?? 사용 금지 (파괴 체크 우회됨)
- Update 안에서 GetComponent, LINQ, 문자열 조합 등 GC 할당 금지
- 필드 이름 변경 시 [FormerlySerializedAs] 추가
- 다형성 직렬화는 [SerializeReference]
- Unity 6에서 바뀐 API 사용 (예: Rigidbody2D.linearVelocity, FindAnyObjectByType)
- Awake → OnEnable → Start 사이의 초기화 순서 의존성 주의
