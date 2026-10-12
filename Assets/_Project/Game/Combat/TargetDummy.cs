using UnityEngine;

namespace Kodiak.Game.Combat
{
    // 사격 연습용 표적. 맞으면 잠깐 flashColor로 바뀌었다 돌아온다.
    public class TargetDummy : MonoBehaviour, IHittable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField, Min(0f)] private float flashDuration = 0.1f;

        private Color baseColor;
        private float flashRemaining;

        // 디버그·검증용
        public int HitCount { get; private set; }

        // 에디터 전용. 컴포넌트를 처음 붙일 때, 또는 인스펙터 ⋮ 메뉴의 Reset을 누를 때 호출된다 (빌드에서는 호출 안 됨).
        private void Reset()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // 오브젝트가 생성(로드)된 직후 1회. 활성화 여부와 무관하게 OnEnable보다 먼저 호출된다.
        // 언리얼의 PostInitializeComponents에 가깝다. 인스펙터에서 틴트한 원래 색을 기억해 둔다.
        private void Awake()
        {
            if (spriteRenderer == null)
            {
                Debug.LogError($"{nameof(TargetDummy)}: spriteRenderer가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            baseColor = spriteRenderer.color;
        }

        public void OnHit(Vector2 point, Vector2 direction)
        {
            HitCount++;

            if (!enabled)
            {
                return;
            }

            flashRemaining = flashDuration;
            spriteRenderer.color = flashColor;
        }

        // 매 렌더 프레임 1회 (언리얼의 Tick). 깜빡임 시간만 센다.
        private void Update()
        {
            if (flashRemaining <= 0f)
            {
                return;
            }

            flashRemaining -= Time.deltaTime;
            if (flashRemaining <= 0f)
            {
                spriteRenderer.color = baseColor;
            }
        }
    }
}
