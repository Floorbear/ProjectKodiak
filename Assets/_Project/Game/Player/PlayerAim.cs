using UnityEngine;
using UnityEngine.InputSystem;

namespace Kodiak.Game.Player
{
    // 마우스 방향으로 조준. 픽셀 아트는 임의 각도 회전에 약하므로 몸은 좌우 뒤집기만 하고, 무기만 회전한다 (Zero Sievert 방식).
    // 앞쪽은 +X(transform.right) 기준. 루트(Rigidbody2D)는 회전시키지 않는다.
    public class PlayerAim : MonoBehaviour
    {
        // 마우스가 무기 피벗에 너무 가까우면 방향이 튀므로 회전하지 않는다
        private const float MinAimDistanceSqr = 0.0001f;

        [SerializeField] private InputActionReference aimAction;
        [SerializeField] private Camera aimCamera;   // 씬 오브젝트라 프리팹이 아니라 씬 인스턴스에서 연결
        [SerializeField, Tooltip("좌우로 뒤집을 몸 스프라이트")] private SpriteRenderer body;
        [SerializeField, Tooltip("손잡이 위치. 조준 방향으로 회전하고, 왼쪽을 볼 때는 좌우 대칭 위치로 옮겨 상하 뒤집는다")]
        private Transform weaponPivot;

        private Vector3 weaponPivotRightPosition;   // 오른쪽을 볼 때의 피벗 로컬 위치 (프리팹에서 정한 값)

        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public bool IsFacingLeft { get; private set; }

        // 오브젝트가 생성(로드)된 직후 1회. 활성화 여부와 무관하게 OnEnable보다 먼저 호출된다.
        // 언리얼의 PostInitializeComponents에 가깝다. 프리팹에서 정한 피벗 위치를 기억해 둔다.
        private void Awake()
        {
            if (weaponPivot != null)
            {
                weaponPivotRightPosition = weaponPivot.localPosition;
            }
        }

        // 컴포넌트가 활성화될 때마다 호출. 생성 직후(Awake 다음, Start 전) 1회 + enabled=true / SetActive(true) 때마다.
        private void OnEnable()
        {
            if (aimAction == null || aimCamera == null || body == null || weaponPivot == null)
            {
                Debug.LogError($"{nameof(PlayerAim)}: aimAction / aimCamera / body / weaponPivot 중 연결되지 않은 참조가 있습니다.", this);
                enabled = false;
                return;
            }

            aimAction.action.Enable();
        }

        // 컴포넌트가 비활성화될 때마다 호출. enabled=false / SetActive(false) / 파괴 직전(OnDestroy 전).
        private void OnDisable()
        {
            if (aimAction != null)
            {
                aimAction.action.Disable();
            }
        }

        // 매 렌더 프레임 1회 (언리얼의 Tick). 회전은 물리가 아니므로 FixedUpdate가 아니라 여기서 한다.
        private void Update()
        {
            Vector2 screenPosition = aimAction.action.ReadValue<Vector2>();

            // 언리얼의 DeprojectMousePositionToWorld. 직교 카메라라 z는 무시하고 xy만 쓴다
            Vector2 worldPosition = aimCamera.ScreenToWorldPoint(screenPosition);

            // 방향은 몸 중심(루트) 기준으로 정한다. 피벗 기준이면 뒤집을 때 피벗이 옮겨져 경계에서 왔다 갔다 할 수 있다
            SetFacing(worldPosition.x < transform.position.x);

            Vector2 toMouse = worldPosition - (Vector2)weaponPivot.position;
            if (toMouse.sqrMagnitude < MinAimDistanceSqr)
            {
                return;
            }

            AimDirection = toMouse.normalized;

            float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void SetFacing(bool facingLeft)
        {
            if (facingLeft == IsFacingLeft)
            {
                return;
            }

            IsFacingLeft = facingLeft;
            body.flipX = facingLeft;

            // 왼쪽을 볼 때: 손 위치를 좌우 대칭으로 옮기고, 무기를 상하 뒤집어 180° 근처로 회전해도 총이 거꾸로 보이지 않게 한다
            Vector3 position = weaponPivotRightPosition;
            if (facingLeft)
            {
                position.x = -position.x;
            }

            weaponPivot.localPosition = position;
            weaponPivot.localScale = new Vector3(1f, facingLeft ? -1f : 1f, 1f);
        }
    }
}
