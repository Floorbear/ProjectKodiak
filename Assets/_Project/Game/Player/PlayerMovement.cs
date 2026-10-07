using UnityEngine;
using UnityEngine.InputSystem;

namespace Kodiak.Game.Player
{
    // 탑뷰 이동. 입력은 Update에서 읽고, 속도는 FixedUpdate(물리 틱)에서 적용한다.
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private float moveSpeed = 5f;

        private Vector2 moveInput;

        // 에디터 전용. 컴포넌트를 처음 붙일 때, 또는 인스펙터 ⋮ 메뉴의 Reset을 누를 때 호출된다 (빌드에서는 호출 안 됨).
        // 여기서 채운 값은 프리팹/씬에 저장되므로 런타임에 GetComponent를 부를 필요가 없다.
        private void Reset()
        {
            body = GetComponent<Rigidbody2D>();
        }

        // 컴포넌트가 활성화될 때마다 호출. 생성 직후(Awake 다음, Start 전) 1회 + enabled=true / SetActive(true) 때마다.
        // 언리얼의 BeginPlay와 달리 여러 번 불릴 수 있어서 OnDisable과 짝으로 구독/해제를 한다.
        private void OnEnable()
        {
            if (moveAction == null)
            {
                Debug.LogError($"{nameof(PlayerMovement)}: moveAction이 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            moveAction.action.Enable();
        }

        // 컴포넌트가 비활성화될 때마다 호출. enabled=false / SetActive(false) / 파괴 직전(OnDestroy 전).
        private void OnDisable()
        {
            if (moveAction != null)
            {
                moveAction.action.Disable();
            }

            moveInput = Vector2.zero;
        }

        // 매 렌더 프레임 1회 (언리얼의 Tick). 프레임레이트에 따라 간격이 달라진다.
        private void Update()
        {
            // 게임패드 스틱 등 길이가 1을 넘는 입력에 대비해 클램프
            moveInput = Vector2.ClampMagnitude(moveAction.action.ReadValue<Vector2>(), 1f);
        }

        // 고정 간격 물리 틱 (기본 0.02초 = 50Hz). 프레임이 느리면 한 프레임에 여러 번, 빠르면 0번 불릴 수 있다.
        private void FixedUpdate()
        {
            body.linearVelocity = moveInput * moveSpeed;
        }
    }
}
