# 프로젝트 코디악 (Unity)

## 개요
- 2D 탑뷰 슈팅 + A-Life 팩션 워 (레퍼런스: 스토커 클리어스카이 팩션 워, Zero Sievert). 목표는 6주 안에 스팀 데모 수준
- 설계 문서 (Notion MCP로 참조)
  - Notion DB "유니티 - Project Kodiak": https://app.notion.com/p/2377ffc6a187800e8970d2183434b48a
  - 게임 설계: https://app.notion.com/p/2387ffc6a18780d79476d427297b3110
  - AI 활용 개발 가이드라인: https://app.notion.com/p/3f07ffc6a1878158b009d79cbb814caa
- 현재 단계: 프로토타입 (세부 설계는 프로토타입 이후 확정)
- 개발자는 언리얼/C++ 경험자, 유니티 입문자. 유니티 개념은 언리얼과 비교해서 설명할 것
  - 유니티 메시지 함수(Awake, OnEnable, Start, Update, FixedUpdate, LateUpdate, OnDisable, OnDestroy, Reset, OnTriggerEnter2D 등)를 처음 쓰는 파일에는 위에 호출 시점을 한두 줄 주석으로 달 것 (언리얼 대응 함수가 있으면 함께)
- 응답은 한국어로
- 아트는 전부 AI 생성. 프레임 애니메이션 대신 회전 스프라이트, 레이어드 캐릭터, 코드 기반 연출을 쓴다

## 환경 (변경 금지)
- Unity 6.3 LTS (6000.3.x), URP 2D (Universal 2D 템플릿)
- 설치된 패키지: Input System, Test Framework, 2D Tilemap Extras
- Cinemachine은 사용 예정, 아직 미설치. 필요하면 설치 전에 먼저 물어볼 것
- 구 Input Manager(Input.GetAxis 등) 사용 금지
- Asset Serialization = Force Text

## 프로젝트 구조
```
Assets/_Project/
├─ Core/   Kodiak.Core.asmdef     순수 C# (noEngineReferences). ALife / Factions / NPC / Rules
├─ Game/   Kodiak.Game.asmdef     MonoBehaviour, 뷰, 어댑터. → Core, Unity.InputSystem 참조
├─ Data/                          ScriptableObject 에셋 (Input Actions 포함)
├─ Art/                           게임용으로 가공된 스프라이트만 (AssetPostprocessor 대상)
├─ Prefabs/                       프리팹 (Characters, Projectiles, Props)
├─ Scenes/                        씬
├─ Editor/ Kodiak.Editor.asmdef   에디터 전용 툴, 임포터. 네임스페이스는 Kodiak.EditorTools. → Core, Game 참조
└─ Tests/EditMode/ Kodiak.Tests.EditMode.asmdef   Core 대상 NUnit 테스트. → Core 참조
```
- 새 코드는 반드시 위 어셈블리 중 하나에 둘 것 (Assets 루트나 asmdef 밖에 스크립트 생성 금지)
- 어셈블리 추가·참조 변경은 승인받을 것
- Core / Game 판단 기준: "화면, 물리, 입력 없이 숫자와 데이터만으로 돌아가는가?" → 그렇다면 Core
- 네임스페이스는 어셈블리 rootNamespace를 따른다 (`Kodiak.Core.*`, `Kodiak.Game.*`, `Kodiak.EditorTools.*`, `Kodiak.Tests.EditMode.*`)
- 네임스페이스에 Editor, Object, Debug, Random 등 유니티 핵심 클래스 이름 사용 금지
  - 예: 에디터 코드의 네임스페이스를 `Kodiak.Editor`로 바꾸면 `Kodiak` 하위에서 `Editor`가 네임스페이스로 해석되어 `UnityEditor.Editor`를 상속하는 커스텀 인스펙터가 깨진다. 어셈블리 이름 `Kodiak.Editor`는 그대로 둔다

## 아키텍처 규칙
- Core 어셈블리에는 UnityEngine 참조 금지 (순수 C#)
- MonoBehaviour는 얇은 뷰/어댑터로만 사용, 게임 로직은 Core에
- NPC 등 시뮬레이션 대상은 "데이터가 본체, GameObject는 뷰" 원칙
- FindObjectOfType / FindFirstObjectByType / GameObject.Find 계열 사용 금지. 참조는 직렬화 필드 또는 주입
- 싱글턴은 현재 없음. 추가가 필요하면 이유와 함께 승인받을 것
- 이벤트는 C# event로 통일
- 대량 데이터(무기 스탯, NPC 특성 등)는 CSV/JSON → ScriptableObject 임포터로 관리

## 작업 절차
1. 구현 전에 계획(변경할 파일, 클래스 구조)을 먼저 제시하고 승인받을 것
2. Core 로직은 EditMode 테스트를 먼저 작성
3. 코드 수정 후 Unity MCP로 재컴파일 → 콘솔 에러·경고 확인
4. Core 수정 시 EditMode 테스트 실행 후 결과 보고
5. 작업이 끝나면 변경 내용 요약과 함께, 내가 직접 확인할 것(플레이 테스트 항목)을 알려줄 것

## MCP 검증 주의
- MCP로 만든 시각 에셋(프리팹, 스프라이트)은 수치 검증만으로 끝내지 말 것. 실제 크기를 `Renderer.bounds` / `Sprite.bounds`로 확인하고, 카메라 캡처로 화면에 보이는지 확인
- 플레이 모드를 끈 뒤 콘솔은 종료가 끝난 것(`isPlayingOrWillChangePlaymode == false`)을 확인하고 읽을 것. 종료 중 발생하는 에러(OnDestroy, 풀 정리 등)를 놓치기 쉬움
- 플레이 모드 입력 주입(`InputSystem.QueueStateEvent`)은 Unity 창에 포커스가 있어야 액션까지 전달됨. 필요하면 Game 뷰 클릭을 요청

## Git
- 커밋·푸시는 내가 요청할 때만
- 커밋 메시지는 한국어, 한 작업 단위로 작게

## 금지
- .meta 파일 직접 생성·수정 금지 (유니티가 생성)
- .unity / .prefab YAML 직접 수정 금지 (MCP 도구나 에디터 스크립트 사용)
- AI 생성 원본 이미지를 Assets에 넣지 말 것 (가공된 최종 스프라이트만)
- ProjectSettings, Packages/manifest.json 변경은 승인받을 것

## 유니티 함정 체크리스트
- UnityEngine.Object에 ?. / ?? 사용 금지 (파괴 체크 우회됨, 언리얼 IsValid()와 비슷한 함정)
- Update 안에서 GetComponent, LINQ, 문자열 조합 등 GC 할당 금지 (참조는 직렬화 필드나 Awake에서 캐싱)
- 필드 이름 변경 시 [FormerlySerializedAs] 추가
- 프로퍼티는 직렬화되지 않음. 인스펙터 노출은 [SerializeField] private 필드로
- 다형성 직렬화는 [SerializeReference]
- Unity 6에서 바뀐 API 사용 (예: Rigidbody2D.linearVelocity, FindAnyObjectByType. FindObjectOfType은 폐기)
- Awake → OnEnable → Start 사이의 초기화 순서 의존성 주의
