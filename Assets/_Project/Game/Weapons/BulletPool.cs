using UnityEngine;
using UnityEngine.Pool;

namespace Kodiak.Game.Weapons
{
    // 씬에 하나 두고 플레이어와 NPC가 함께 쓰는 총알 풀. UnityEngine.Pool.ObjectPool 래퍼.
    // 꺼낼 때 SetActive(true), 반납할 때 SetActive(false). 풀이 비어 있을 때만 새로 Instantiate한다.
    public class BulletPool : MonoBehaviour
    {
        [SerializeField] private Bullet bulletPrefab;

        [SerializeField, Min(1), Tooltip("내부 목록의 처음 크기 (미리 만들어 두지는 않음)")]
        private int defaultCapacity = 32;

        [SerializeField, Min(1), Tooltip("반납된 총알을 보관하는 최대 개수. 넘치면 파괴")]
        private int maxSize = 256;

        private ObjectPool<Bullet> pool;

        // 디버그·검증용
        public int CountAll => pool.CountAll;
        public int CountActive => pool.CountActive;

        // 오브젝트가 생성(로드)된 직후 1회. 활성화 여부와 무관하게 OnEnable보다 먼저 호출된다.
        // 언리얼의 PostInitializeComponents에 가깝다. 다른 컴포넌트의 OnEnable·Update보다 먼저라서 풀은 여기서 만든다.
        private void Awake()
        {
            if (bulletPrefab == null)
            {
                Debug.LogError($"{nameof(BulletPool)}: bulletPrefab이 연결되지 않았습니다.", this);
                return;
            }

            pool = new ObjectPool<Bullet>(
                CreateBullet,
                bullet => bullet.gameObject.SetActive(true),
                bullet => bullet.gameObject.SetActive(false),
                DestroyBullet,
                collectionCheck: true,   // 같은 총알을 두 번 반납하면 에디터에서 예외
                defaultCapacity,
                maxSize);
        }

        // 오브젝트가 파괴될 때 1회 (씬 종료, 플레이 모드 종료 포함). 언리얼의 EndPlay / BeginDestroy에 해당.
        // 풀을 여기서 비워 둬야 Unity가 플레이 모드 종료 후 남은 풀을 정리(PoolManager)하면서 파괴된 총알을 다시 건드리지 않는다.
        private void OnDestroy()
        {
            if (pool != null)
            {
                pool.Clear();
            }
        }

        public Bullet Get()
        {
            return pool.Get();
        }

        public void Release(Bullet bullet)
        {
            pool.Release(bullet);
        }

        private Bullet CreateBullet()
        {
            // 풀 오브젝트 밑에 만든다. 플레이어 자식으로 만들면 플레이어를 따라 움직이게 됨
            Bullet bullet = Instantiate(bulletPrefab, transform);
            bullet.SetPool(this);
            return bullet;
        }

        private static void DestroyBullet(Bullet bullet)
        {
            // 씬 종료 때는 총알(자식)이 풀보다 먼저 파괴될 수 있다. == null은 파괴 여부까지 확인한다 (?. 는 안 됨)
            if (bullet != null)
            {
                Destroy(bullet.gameObject);
            }
        }
    }
}
