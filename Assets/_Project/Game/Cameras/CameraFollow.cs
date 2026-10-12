using UnityEngine;

namespace Kodiak.Game.Cameras
{
    // 타깃(플레이어)을 부드럽게 따라가는 탑뷰 카메라. 언리얼의 Spring Arm + Camera Lag에 해당.
    // 네임스페이스를 Camera로 하면 UnityEngine.Camera와 충돌하므로 복수형 Cameras를 쓴다.
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        [SerializeField, Min(0f), Tooltip("목표에 도달하는 대략적인 시간(초). 0이면 딱 붙어서 따라감")]
        private float smoothTime = 0.12f;

        private Vector3 velocity;   // SmoothDamp가 프레임 사이에 기억하는 현재 속도

        // 첫 Update 직전 1회 (Awake, OnEnable 다음). 언리얼의 BeginPlay에 해당.
        // 다른 오브젝트의 Awake가 모두 끝난 뒤라 타깃 위치를 믿을 수 있다.
        private void Start()
        {
            if (target == null)
            {
                Debug.LogError($"{nameof(CameraFollow)}: target이 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            // 씬 시작 시 카메라가 미끄러져 오지 않도록 바로 맞춘다
            transform.position = GetDesiredPosition();
        }

        // 모든 Update가 끝난 뒤 매 프레임 1회 (언리얼의 PostUpdateWork / TG_PostPhysics 틱 그룹).
        // 플레이어 이동(물리 + 보간)이 확정된 뒤에 따라가야 떨리지 않는다.
        private void LateUpdate()
        {
            Vector3 desired = GetDesiredPosition();

            if (smoothTime <= 0f)
            {
                transform.position = desired;
                return;
            }

            // Lerp(a, b, k * dt)와 달리 프레임레이트와 관계없이 같은 감속 느낌을 준다 (스프링-댐퍼)
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

        // 타깃의 XY + 카메라 자신의 Z (2D 카메라는 Z가 -10 등으로 떨어져 있어야 화면에 그려진다)
        private Vector3 GetDesiredPosition()
        {
            Vector3 targetPosition = target.position;
            return new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        }
    }
}
