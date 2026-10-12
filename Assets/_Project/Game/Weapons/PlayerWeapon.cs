using System;
using Kodiak.Core.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kodiak.Game.Weapons
{
    // 플레이어 입력을 Core의 WeaponState에 전달하는 어댑터. 규칙은 WeaponState에 있고 여기서는 Tick과 입력만 넘긴다.
    public class PlayerWeapon : MonoBehaviour
    {
        [SerializeField] private InputActionReference fireAction;
        [SerializeField] private InputActionReference reloadAction;
        [SerializeField] private WeaponData weaponData;

        [SerializeField, Min(0), Tooltip("인벤토리가 생기기 전 임시 예비 탄약")]
        private int startingReserveAmmo = 90;

        public WeaponData Data => weaponData;
        public WeaponState State { get; private set; }
        public AmmoPool Reserve { get; private set; }

        // 발사가 실제로 일어났을 때. ⑤에서 총알 생성이 구독한다
        public event Action Fired;

        // 오브젝트가 생성(로드)된 직후 1회. 활성화 여부와 무관하게 OnEnable보다 먼저 호출된다.
        // 언리얼의 PostInitializeComponents에 가깝다. 자기 자신의 초기화만 하고 다른 오브젝트에 의존하지 않는다.
        private void Awake()
        {
            if (weaponData == null)
            {
                Debug.LogError($"{nameof(PlayerWeapon)}: weaponData가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }

            Reserve = new AmmoPool(startingReserveAmmo);
            State = new WeaponState(weaponData.CreateSpec(), Reserve);
        }

        // 컴포넌트가 활성화될 때마다 호출. 생성 직후(Awake 다음, Start 전) 1회 + enabled=true / SetActive(true) 때마다.
        private void OnEnable()
        {
            if (fireAction == null || reloadAction == null || State == null)
            {
                Debug.LogError($"{nameof(PlayerWeapon)}: fireAction / reloadAction / weaponData 중 연결되지 않은 참조가 있습니다.", this);
                enabled = false;
                return;
            }

            fireAction.action.Enable();
            reloadAction.action.Enable();
        }

        // 컴포넌트가 비활성화될 때마다 호출. enabled=false / SetActive(false) / 파괴 직전(OnDestroy 전).
        private void OnDisable()
        {
            if (fireAction != null)
            {
                fireAction.action.Disable();
            }

            if (reloadAction != null)
            {
                reloadAction.action.Disable();
            }
        }

        // 매 렌더 프레임 1회 (언리얼의 Tick). 시간 경과를 먼저 반영한 뒤 입력을 처리한다.
        private void Update()
        {
            State.Tick(Time.deltaTime);

            if (reloadAction.action.WasPressedThisFrame())
            {
                State.TryStartReload();
            }

            // 연사: 누르고 있는 동안 연사 간격마다 발사
            if (fireAction.action.IsPressed() && State.TryFire())
            {
                Fired?.Invoke();
            }
        }
    }
}
