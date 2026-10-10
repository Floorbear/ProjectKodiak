using Kodiak.Core.Weapons;
using UnityEngine;

namespace Kodiak.Game.Weapons
{
    // 무기 종류별 수치 에셋 (언리얼의 DataAsset). 규칙은 Core의 WeaponState가 담당하고, 이 에셋은 수치만 가진다.
    [CreateAssetMenu(menuName = "Kodiak/Weapon Data", fileName = "WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField, Min(0f), Tooltip("연사 간격(초). 0.1 = 분당 600발")]
        private float fireInterval = 0.1f;

        [SerializeField, Min(1), Tooltip("탄창 용량 (약실 제외)")]
        private int magazineCapacity = 30;

        [SerializeField, Min(0f), Tooltip("탄창 교체 시간(초)")]
        private float reloadTime = 2f;

        [SerializeField, Min(0f), Tooltip("장전손잡이 시간(초). 약실이 비었을 때만 추가")]
        private float chamberTime = 0.6f;

        public WeaponSpec CreateSpec()
        {
            return new WeaponSpec(fireInterval, magazineCapacity, reloadTime, chamberTime);
        }
    }
}
