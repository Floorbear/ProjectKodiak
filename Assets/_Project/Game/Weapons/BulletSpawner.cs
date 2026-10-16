using UnityEngine;

namespace Kodiak.Game.Weapons
{
    // PlayerWeapon.Fired를 받아 총구에서 총알을 쏜다. 쏠 수 있는지는 PlayerWeapon, 무엇을 쏘는지는 여기서 담당.
    public class BulletSpawner : MonoBehaviour
    {
        [SerializeField] private PlayerWeapon weapon;
        [SerializeField, Tooltip("총구. 위치에서 +X 방향으로 발사")] private Transform muzzle;
        [SerializeField, Tooltip("씬의 BulletPool. 프리팹에서는 연결할 수 없으므로 씬 인스턴스에서 연결")]
        private BulletPool pool;

        // 에디터 전용. 컴포넌트를 처음 붙일 때, 또는 인스펙터 ⋮ 메뉴의 Reset을 누를 때 호출된다 (빌드에서는 호출 안 됨).
        private void Reset()
        {
            weapon = GetComponent<PlayerWeapon>();
        }

        // 컴포넌트가 활성화될 때마다 호출. 생성 직후(Awake 다음, Start 전) 1회 + enabled=true / SetActive(true) 때마다.
        // 언리얼의 델리게이트 AddDynamic처럼 여기서 구독하고 OnDisable에서 해제한다.
        private void OnEnable()
        {
            if (weapon == null || muzzle == null || pool == null)
            {
                Debug.LogError($"{nameof(BulletSpawner)}: weapon / muzzle / pool 중 연결되지 않은 참조가 있습니다.", this);
                enabled = false;
                return;
            }

            weapon.Fired += SpawnBullet;
        }

        // 컴포넌트가 비활성화될 때마다 호출. enabled=false / SetActive(false) / 파괴 직전(OnDestroy 전).
        private void OnDisable()
        {
            if (weapon != null)
            {
                weapon.Fired -= SpawnBullet;
            }
        }

        // 에디터 전용. 이 오브젝트를 선택했을 때 Scene 뷰에 그린다. 총구 위치·방향을 조정할 때 보조선으로 쓴다.
        private void OnDrawGizmosSelected()
        {
            if (muzzle == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(muzzle.position, 0.03f);
            Gizmos.DrawLine(muzzle.position, muzzle.position + muzzle.right * 0.5f);
        }

        private void SpawnBullet()
        {
            WeaponData data = weapon.Data;
            Bullet bullet = pool.Get();
            // 쏜 사람 = 이 컴포넌트가 붙은 루트. 총알이 자기 몸에 맞지 않게 넘긴다
            bullet.Launch(muzzle.position, muzzle.right, data.BulletSpeed, data.BulletLifetime, transform);
        }
    }
}
