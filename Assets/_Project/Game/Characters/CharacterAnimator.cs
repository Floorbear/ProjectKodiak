using UnityEngine;

namespace Kodiak.Game.Characters
{
    // 이동 속도를 Animator의 Speed 파라미터로 넘긴다 (대기 ↔ 걷기 전환). 플레이어와 NPC가 함께 쓴다.
    // 언리얼로 치면 Anim BP Event Graph에서 매 틱 Speed 변수를 갱신하는 부분.
    public class CharacterAnimator : MonoBehaviour
    {
        // 문자열 대신 해시로 접근해 매 프레임 문자열 비교·할당을 피한다
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody2D body;

        // 에디터 전용. 컴포넌트를 처음 붙일 때, 또는 인스펙터 ⋮ 메뉴의 Reset을 누를 때 호출된다 (빌드에서는 호출 안 됨).
        private void Reset()
        {
            animator = GetComponentInChildren<Animator>();
            body = GetComponent<Rigidbody2D>();
        }

        // 오브젝트가 생성(로드)된 직후 1회. 활성화 여부와 무관하게 OnEnable보다 먼저 호출된다.
        private void Awake()
        {
            if (animator == null || body == null)
            {
                Debug.LogError($"{nameof(CharacterAnimator)}: animator / body 중 연결되지 않은 참조가 있습니다.", this);
                enabled = false;
                return;
            }

            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning($"{nameof(CharacterAnimator)}: Animator에 Controller가 없어 애니메이션을 끕니다.", this);
                enabled = false;
            }
        }

        // 매 렌더 프레임 1회 (언리얼의 Tick). 물리 속도는 FixedUpdate에서 바뀌지만 표시용이라 프레임마다 읽으면 충분하다.
        private void Update()
        {
            animator.SetFloat(SpeedHash, body.linearVelocity.magnitude);
        }
    }
}
