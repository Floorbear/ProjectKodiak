using Kodiak.Game.Combat;
using UnityEngine;

namespace Kodiak.Game.Weapons
{
    // 풀에서 꺼내 쓰는 총알. Launch로 속도를 한 번 주면 물리가 날려 보내고, 수명이 끝나거나 무언가에 맞으면 풀로 돌아간다.
    // 언리얼의 투사체 액터 + ProjectileMovement에 해당. Destroy 대신 비활성화해서 재사용한다.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;

        private BulletPool pool;
        private Transform owner;         // 쏜 사람. 언리얼의 Instigator. 자기 총알에 맞지 않도록 무시한다
        private float remainingLifetime;
        private bool isFlying;           // 한 물리 틱에 여러 콜라이더와 겹쳐도 한 번만 반납하기 위한 플래그

        // 에디터 전용. 컴포넌트를 처음 붙일 때, 또는 인스펙터 ⋮ 메뉴의 Reset을 누를 때 호출된다 (빌드에서는 호출 안 됨).
        private void Reset()
        {
            body = GetComponent<Rigidbody2D>();
        }

        // 풀이 생성 직후 한 번 알려준다
        public void SetPool(BulletPool owningPool)
        {
            pool = owningPool;
        }

        public void Launch(Vector2 position, Vector2 direction, float speed, float lifetime, Transform shooter)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 재사용 총알이므로 이전 위치에서 보간되지 않도록 Transform과 Rigidbody2D를 모두 옮긴다
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle));
            body.position = position;
            body.rotation = angle;
            body.linearVelocity = direction.normalized * speed;

            owner = shooter;
            remainingLifetime = lifetime;
            isFlying = true;
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

        // Is Trigger 콜라이더가 다른 콜라이더와 겹치기 시작한 물리 틱에 호출 (언리얼의 OnComponentBeginOverlap).
        // 어떤 레이어끼리 겹침을 감지할지는 Physics 2D 충돌 매트릭스가 정한다.
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isFlying)
            {
                return;
            }

            // 쏜 사람 자신(또는 그 자식 콜라이더)은 무시. 총구에서 몸과 겹친 채 생성되므로 필요하다
            if (owner != null && other.transform.IsChildOf(owner))
            {
                return;
            }

            // 레이어드 캐릭터는 콜라이더가 자식에 있을 수 있으므로 부모 방향으로 찾는다 (Update가 아니라 피격 시에만 호출)
            IHittable hittable = other.GetComponentInParent<IHittable>();
            if (hittable != null)
            {
                hittable.OnHit(other.ClosestPoint(body.position), body.linearVelocity.normalized);
            }

            // 맞힌 대상이 IHittable이 아니어도(벽 등) 총알은 멈춘다
            ReturnToPool();
        }

        public void ReturnToPool()
        {
            if (!isFlying)
            {
                return;
            }

            isFlying = false;
            owner = null;
            pool.Release(this);
        }
    }
}
