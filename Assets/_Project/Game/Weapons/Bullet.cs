using UnityEngine;

namespace Kodiak.Game.Weapons
{
    // 풀에서 꺼내 쓰는 총알. Launch로 속도를 한 번 주면 물리가 날려 보내고, 수명이 끝나면 풀로 돌아간다.
    // 언리얼의 투사체 액터 + ProjectileMovement에 해당. Destroy 대신 비활성화해서 재사용한다.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;

        private BulletPool pool;
        private float remainingLifetime;

        // 에디터 전용. 컴포넌트를 처음 붙일 때, 또는 인스펙터 ⋮ 메뉴의 Reset을 누를 때 호출된다 (빌드에서는 호출 안 됨).
        private void Reset()
        {
            body = GetComponent<Rigidbody2D>();
        }

        // 풀이 생성 직후 한 번 알려준다
        public void SetPool(BulletPool owner)
        {
            pool = owner;
        }

        public void Launch(Vector2 position, Vector2 direction, float speed, float lifetime)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 재사용 총알이므로 이전 위치에서 보간되지 않도록 Transform과 Rigidbody2D를 모두 옮긴다
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle));
            body.position = position;
            body.rotation = angle;
            body.linearVelocity = direction.normalized * speed;

            remainingLifetime = lifetime;
        }

        // 매 렌더 프레임 1회 (언리얼의 Tick). 비활성화(풀에 반납)된 동안에는 호출되지 않는다.
        private void Update()
        {
            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                ReturnToPool();
            }
        }

        // ⑥에서 피격 시에도 호출한다
        public void ReturnToPool()
        {
            pool.Release(this);
        }
    }
}
