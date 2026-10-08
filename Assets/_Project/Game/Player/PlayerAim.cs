using UnityEngine;
using UnityEngine.InputSystem;

namespace Kodiak.Game.Player
{
    // 마우스 방향으로 자식 Visual을 회전. 앞쪽은 +X(transform.right) 기준.
    // 루트(Rigidbody2D)는 회전시키지 않으므로 물리와 무관하게 렌더 프레임마다 회전한다.
    public class PlayerAim : MonoBehaviour
    {
        // 마우스가 캐릭터 중심에 너무 가까우면 방향이 튀므로 회전하지 않는다
        private const float MinAimDistanceSqr = 0.0001f;

        [SerializeField] private InputActionReference aimAction;
        [SerializeField] private Camera aimCamera;   // 씬 오브젝트라 프리팹이 아니라 씬 인스턴스에서 연결
        [SerializeField] private Transform visual;

        public Vector2 AimDirection { get; private set; } = Vector2.right;

        // 컴포넌트가 활성화될 때마다 호출. 생성 직후(Awake 다음, Start 전) 1회 + enabled=true / SetActive(true) 때마다.
        private void OnEnable()
        {
            if (aimAction == null || aimCamera == null || visual == null)
            {
                Debug.LogError($"{nameof(PlayerAim)}: aimAction / aimCamera / visual 중 연결되지 않은 참조가 있습니다.", this);
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
            Vector2 toMouse = worldPosition - (Vector2)visual.position;

            if (toMouse.sqrMagnitude < MinAimDistanceSqr)
            {
                return;
            }

            AimDirection = toMouse.normalized;

            float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
            visual.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
