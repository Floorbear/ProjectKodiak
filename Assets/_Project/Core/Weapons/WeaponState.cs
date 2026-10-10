using System;

namespace Kodiak.Core.Weapons
{
    // 무기 하나의 약실/탄창/재장전 상태. 시간은 Tick(deltaTime)으로 바깥에서 넣어준다 (Time.deltaTime 직접 사용 금지).
    // 재장전 = [탄창 교체 ReloadTime] + (교체 시점에 약실이 비어 있으면) [장전손잡이 ChamberTime]
    public sealed class WeaponState
    {
        private enum ReloadPhase
        {
            None,
            SwappingMagazine,   // 탄창이 빠져 있음. 약실 탄은 쏠 수 있음
            Chambering,         // 새 탄창은 꽂혔고 장전손잡이를 당기는 중
        }

        private readonly WeaponSpec spec;
        private readonly AmmoPool reserve;

        private float fireCooldown;          // 0 이하가 되면 다시 쏠 수 있음
        private ReloadPhase reloadPhase;
        private float reloadPhaseRemaining;

        // 생성 시 탄창 가득 + 약실 장전 상태로 시작한다 (예비 탄약은 쓰지 않음)
        public WeaponState(WeaponSpec spec, AmmoPool reserve)
        {
            this.spec = spec ?? throw new ArgumentNullException(nameof(spec));
            this.reserve = reserve ?? throw new ArgumentNullException(nameof(reserve));

            MagazineAmmo = spec.MagazineCapacity;
            IsChambered = true;
        }

        public int MagazineAmmo { get; private set; }
        public bool IsChambered { get; private set; }
        public int LoadedAmmo => MagazineAmmo + (IsChambered ? 1 : 0);   // 약실 + 탄창
        public bool IsReloading => reloadPhase != ReloadPhase.None;

        // 약실에 탄이 있고 연사 간격이 지났으면 발사 후 true. 탄창에 탄이 있으면 다음 탄을 약실로 보낸다
        public bool TryFire()
        {
            if (!IsChambered || fireCooldown > 0f)
            {
                return false;
            }

            IsChambered = false;
            fireCooldown = spec.FireInterval;   // += 가 아니라 = : 오래 쉬어도 연사가 모이지 않게

            // 재장전 중에는 탄창이 빠져 있어서 다음 탄이 들어오지 않는다
            if (reloadPhase == ReloadPhase.None && MagazineAmmo > 0)
            {
                MagazineAmmo--;
                IsChambered = true;
            }

            return true;
        }

        // 재장전을 시작했으면 true
        public bool TryStartReload()
        {
            if (IsReloading || reserve.Count == 0 || MagazineAmmo >= spec.MagazineCapacity)
            {
                return false;
            }

            // 탄창을 뽑는다. 남은 탄은 예비 탄약으로 돌아간다
            reserve.Add(MagazineAmmo);
            MagazineAmmo = 0;

            reloadPhase = ReloadPhase.SwappingMagazine;
            reloadPhaseRemaining = spec.ReloadTime;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f) throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "0 이상이어야 합니다.");

            fireCooldown = Math.Max(0f, fireCooldown - deltaTime);
            AdvanceReload(deltaTime);
        }

        // 한 Tick에 여러 단계가 끝날 수 있으므로 남은 시간을 다음 단계로 넘긴다
        private void AdvanceReload(float deltaTime)
        {
            while (reloadPhase != ReloadPhase.None && deltaTime >= reloadPhaseRemaining)
            {
                deltaTime -= reloadPhaseRemaining;
                CompleteReloadPhase();
            }

            if (reloadPhase != ReloadPhase.None)
            {
                reloadPhaseRemaining -= deltaTime;
            }
        }

        private void CompleteReloadPhase()
        {
            switch (reloadPhase)
            {
                case ReloadPhase.SwappingMagazine:
                    MagazineAmmo = reserve.Take(spec.MagazineCapacity);

                    // 교체가 끝난 시점에 약실을 본다. 재장전 중 약실 탄을 쐈다면 여기서 비어 있다
                    if (!IsChambered && MagazineAmmo > 0)
                    {
                        reloadPhase = ReloadPhase.Chambering;
                        reloadPhaseRemaining = spec.ChamberTime;
                    }
                    else
                    {
                        reloadPhase = ReloadPhase.None;
                    }
                    break;

                case ReloadPhase.Chambering:
                    MagazineAmmo--;
                    IsChambered = true;
                    reloadPhase = ReloadPhase.None;
                    break;
            }
        }
    }
}
