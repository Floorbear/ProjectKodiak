using System;

namespace Kodiak.Core.Weapons
{
    // 무기 종류별 고정 수치. Game의 WeaponData(ScriptableObject)가 이 값을 만들어 WeaponState에 넘긴다.
    public sealed class WeaponSpec
    {
        public WeaponSpec(float fireInterval, int magazineCapacity, float reloadTime, float chamberTime)
        {
            if (fireInterval < 0f) throw new ArgumentOutOfRangeException(nameof(fireInterval), fireInterval, "0 이상이어야 합니다.");
            if (magazineCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(magazineCapacity), magazineCapacity, "1 이상이어야 합니다.");
            if (reloadTime < 0f) throw new ArgumentOutOfRangeException(nameof(reloadTime), reloadTime, "0 이상이어야 합니다.");
            if (chamberTime < 0f) throw new ArgumentOutOfRangeException(nameof(chamberTime), chamberTime, "0 이상이어야 합니다.");

            FireInterval = fireInterval;
            MagazineCapacity = magazineCapacity;
            ReloadTime = reloadTime;
            ChamberTime = chamberTime;
        }

        public float FireInterval { get; }      // 연사 간격(초)
        public int MagazineCapacity { get; }    // 탄창 용량 (약실 제외)
        public float ReloadTime { get; }        // 탄창 교체 시간(초)
        public float ChamberTime { get; }       // 장전손잡이 당기는 시간(초). 약실이 비었을 때만 추가
    }
}
