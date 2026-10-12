using System;
using Kodiak.Core.Weapons;
using NUnit.Framework;

namespace Kodiak.Tests.EditMode.Weapons
{
    // WeaponState 명세. 설계 근거는 노션 "건플레이 설계" 페이지.
    // 시간 값은 float 오차가 없도록 2의 거듭제곱 분수(0.25, 0.5 등)를 쓴다.
    public class WeaponStateTests
    {
        private const float FireInterval = 0.25f;
        private const int Capacity = 3;          // 탄창 3발 + 약실 1발 = 최대 4발
        private const float ReloadTime = 1f;     // 탄창 교체
        private const float ChamberTime = 0.5f;  // 장전손잡이
        private const int Reserve = 10;

        private static WeaponState CreateWeapon(out AmmoPool reserve, int reserveCount = Reserve)
        {
            reserve = new AmmoPool(reserveCount);
            return new WeaponState(new WeaponSpec(FireInterval, Capacity, ReloadTime, ChamberTime), reserve);
        }

        private static WeaponState CreateWeapon(int reserveCount = Reserve)
        {
            return CreateWeapon(out _, reserveCount);
        }

        // 쏘고 연사 간격만큼 기다린다
        private static void FireAndWait(WeaponState weapon, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                weapon.TryFire();
                weapon.Tick(FireInterval);
            }
        }

        // 약실까지 전부 쏜 빈 총
        private static WeaponState CreateEmptyWeapon(out AmmoPool reserve, int reserveCount = Reserve)
        {
            var weapon = CreateWeapon(out reserve, reserveCount);
            FireAndWait(weapon, Capacity + 1);
            return weapon;
        }

        // ── 생성 ──────────────────────────────────────────

        [Test]
        public void 생성직후_탄창이_가득차고_약실에_한발이_있다()
        {
            var weapon = CreateWeapon();

            Assert.AreEqual(Capacity, weapon.MagazineAmmo);
            Assert.IsTrue(weapon.IsChambered);
            Assert.AreEqual(Capacity + 1, weapon.LoadedAmmo);
            Assert.IsFalse(weapon.IsReloading);
        }

        [Test]
        public void 생성직후_바로_발사할수있다()
        {
            var weapon = CreateWeapon();

            Assert.IsTrue(weapon.TryFire());
        }

        [Test]
        public void 생성할때_예비탄약을_쓰지_않는다()
        {
            CreateWeapon(out var reserve);

            Assert.AreEqual(Reserve, reserve.Count);
        }

        [TestCase(-0.25f, 3, 1f, 0.5f)]
        [TestCase(0.25f, 0, 1f, 0.5f)]
        [TestCase(0.25f, 3, -1f, 0.5f)]
        [TestCase(0.25f, 3, 1f, -0.5f)]
        public void 잘못된_수치로는_무기스펙을_만들수없다(float fireInterval, int capacity, float reloadTime, float chamberTime)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponSpec(fireInterval, capacity, reloadTime, chamberTime));
        }

        // ── 발사와 약실 ──────────────────────────────────

        [Test]
        public void 발사하면_탄창의_다음탄이_약실로_들어간다()
        {
            var weapon = CreateWeapon();

            weapon.TryFire();

            Assert.IsTrue(weapon.IsChambered);
            Assert.AreEqual(Capacity - 1, weapon.MagazineAmmo);
            Assert.AreEqual(Capacity, weapon.LoadedAmmo);
        }

        [Test]
        public void 탄창이_비어도_약실의_마지막_한발은_쏠수있다()
        {
            var weapon = CreateWeapon();
            FireAndWait(weapon, Capacity);

            Assert.AreEqual(0, weapon.MagazineAmmo);
            Assert.IsTrue(weapon.IsChambered);
            Assert.IsTrue(weapon.TryFire());
        }

        [Test]
        public void 약실까지_비면_발사할수없다()
        {
            var weapon = CreateEmptyWeapon(out _);

            Assert.AreEqual(0, weapon.LoadedAmmo);
            Assert.IsFalse(weapon.IsChambered);
            Assert.IsFalse(weapon.TryFire());
        }

        [Test]
        public void 빈총으로_발사를_시도해도_자동으로_재장전하지_않는다()
        {
            var weapon = CreateEmptyWeapon(out _);

            weapon.TryFire();

            Assert.IsFalse(weapon.IsReloading);
        }

        // ── 연사 간격 ────────────────────────────────────

        [Test]
        public void 연사간격이_지나기_전에는_다시_쏠수없다()
        {
            var weapon = CreateWeapon();

            Assert.IsTrue(weapon.TryFire());
            weapon.Tick(FireInterval / 2);
            Assert.IsFalse(weapon.TryFire());
        }

        [Test]
        public void 연사간격이_정확히_지나면_다시_쏠수있다()
        {
            var weapon = CreateWeapon();

            weapon.TryFire();
            weapon.Tick(FireInterval / 2);
            weapon.Tick(FireInterval / 2);

            Assert.IsTrue(weapon.TryFire());
        }

        [Test]
        public void 쏠수없을때의_발사시도는_탄을_소모하지_않는다()
        {
            var weapon = CreateWeapon();

            weapon.TryFire();
            weapon.TryFire();   // 연사 간격 안이라 실패

            Assert.AreEqual(Capacity, weapon.LoadedAmmo);
        }

        [Test]
        public void 오래_쉬어도_연사가_모이지_않는다()
        {
            // 한 프레임이 길어도(렉) 여러 발이 한꺼번에 나가면 안 된다
            var weapon = CreateWeapon();

            weapon.TryFire();
            weapon.Tick(FireInterval * 10);

            Assert.IsTrue(weapon.TryFire());
            Assert.IsFalse(weapon.TryFire());
        }

        // ── 재장전 공통 ──────────────────────────────────

        [Test]
        public void 재장전을_시작하면_재장전중이_된다()
        {
            var weapon = CreateWeapon();
            FireAndWait(weapon);

            Assert.IsTrue(weapon.TryStartReload());
            Assert.IsTrue(weapon.IsReloading);
        }

        [Test]
        public void 재장전을_시작하면_기존_탄창의_탄은_예비탄약으로_돌아간다()
        {
            var weapon = CreateWeapon(out var reserve);
            FireAndWait(weapon);   // 약실 1 + 탄창 2

            weapon.TryStartReload();

            Assert.AreEqual(0, weapon.MagazineAmmo);
            Assert.IsTrue(weapon.IsChambered);
            Assert.AreEqual(Reserve + 2, reserve.Count);
        }

        [Test]
        public void 탄창이_가득차있으면_재장전하지_않는다()
        {
            var weapon = CreateWeapon();

            Assert.IsFalse(weapon.TryStartReload());
            Assert.IsFalse(weapon.IsReloading);
        }

        [Test]
        public void 예비탄약이_없으면_재장전하지_않는다()
        {
            var weapon = CreateWeapon(reserveCount: 0);
            FireAndWait(weapon);

            Assert.IsFalse(weapon.TryStartReload());
            Assert.IsFalse(weapon.IsReloading);
            Assert.AreEqual(Capacity - 1, weapon.MagazineAmmo);
        }

        [Test]
        public void 재장전중에_다시_재장전해도_시간이_처음부터_시작되지_않는다()
        {
            var weapon = CreateWeapon();
            FireAndWait(weapon);

            weapon.TryStartReload();
            weapon.Tick(ReloadTime / 2);
            Assert.IsFalse(weapon.TryStartReload());
            weapon.Tick(ReloadTime / 2);

            Assert.IsFalse(weapon.IsReloading);
            Assert.AreEqual(Capacity, weapon.MagazineAmmo);
        }

        // ── 택티컬 재장전 (약실에 탄이 있을 때) ──────────

        [Test]
        public void 택티컬재장전은_탄창교체_시간만_걸리고_약실탄은_유지된다()
        {
            var weapon = CreateWeapon();
            FireAndWait(weapon);

            weapon.TryStartReload();
            weapon.Tick(ReloadTime);

            Assert.IsFalse(weapon.IsReloading);
            Assert.AreEqual(Capacity, weapon.MagazineAmmo);
            Assert.IsTrue(weapon.IsChambered);
            Assert.AreEqual(Capacity + 1, weapon.LoadedAmmo);
        }

        [Test]
        public void 탄창교체가_끝나기_전에는_탄창이_차지_않는다()
        {
            var weapon = CreateWeapon();
            FireAndWait(weapon);

            weapon.TryStartReload();
            weapon.Tick(ReloadTime / 2);

            Assert.IsTrue(weapon.IsReloading);
            Assert.AreEqual(0, weapon.MagazineAmmo);
        }

        [Test]
        public void 재장전중에도_약실의_한발은_쏠수있다()
        {
            var weapon = CreateWeapon();
            FireAndWait(weapon);

            weapon.TryStartReload();

            Assert.IsTrue(weapon.TryFire());
            Assert.IsFalse(weapon.IsChambered);   // 탄창이 빠져 있어 다음 탄이 안 들어옴
        }

        [Test]
        public void 재장전중_약실탄을_쏜_뒤에는_더_쏠수없다()
        {
            var weapon = CreateWeapon();
            FireAndWait(weapon);

            weapon.TryStartReload();
            weapon.TryFire();
            weapon.Tick(FireInterval);

            Assert.IsFalse(weapon.TryFire());
        }

        [Test]
        public void 재장전중에_약실탄을_쏘면_재장전이_취소된다()
        {
            var weapon = CreateWeapon();
            FireAndWait(weapon);

            weapon.TryStartReload();
            weapon.TryFire();

            Assert.IsFalse(weapon.IsReloading);
            Assert.IsFalse(weapon.IsChambered);
            Assert.AreEqual(0, weapon.MagazineAmmo);   // 뽑은 탄창은 빠진 채로
        }

        [Test]
        public void 재장전이_취소되면_시간이_지나도_탄창이_차지_않는다()
        {
            var weapon = CreateWeapon();
            FireAndWait(weapon);

            weapon.TryStartReload();
            weapon.TryFire();
            weapon.Tick(ReloadTime + ChamberTime);

            Assert.AreEqual(0, weapon.MagazineAmmo);
            Assert.IsFalse(weapon.IsChambered);
        }

        [Test]
        public void 재장전이_취소된_뒤_다시_재장전하면_빈총_재장전이_된다()
        {
            var weapon = CreateWeapon();
            FireAndWait(weapon);

            weapon.TryStartReload();
            weapon.TryFire();
            weapon.Tick(FireInterval);

            Assert.IsTrue(weapon.TryStartReload());
            weapon.Tick(ReloadTime);

            Assert.IsTrue(weapon.IsReloading);   // 약실이 비었으니 장전손잡이 단계가 남음

            weapon.Tick(ChamberTime);

            Assert.IsFalse(weapon.IsReloading);
            Assert.IsTrue(weapon.IsChambered);
            Assert.AreEqual(Capacity - 1, weapon.MagazineAmmo);
        }

        [Test]
        public void 재장전_취소로_탄의_총량은_변하지_않는다()
        {
            var weapon = CreateWeapon(out var reserve);
            FireAndWait(weapon);
            int before = weapon.LoadedAmmo + reserve.Count;

            weapon.TryStartReload();
            weapon.TryFire();   // 1발 소모

            Assert.AreEqual(before - 1, weapon.LoadedAmmo + reserve.Count);
        }

        // ── 빈 총 재장전 (약실이 비었을 때) ──────────────

        [Test]
        public void 빈총은_탄창교체_후_장전손잡이_시간이_추가로_걸린다()
        {
            var weapon = CreateEmptyWeapon(out _);

            weapon.TryStartReload();
            weapon.Tick(ReloadTime);

            Assert.IsTrue(weapon.IsReloading);
            Assert.IsFalse(weapon.IsChambered);
            Assert.AreEqual(Capacity, weapon.MagazineAmmo);   // 탄창은 꽂혔지만 아직 약실이 빔

            weapon.Tick(ChamberTime);

            Assert.IsFalse(weapon.IsReloading);
            Assert.IsTrue(weapon.IsChambered);
            Assert.AreEqual(Capacity - 1, weapon.MagazineAmmo);   // 한 발이 약실로 이동
        }

        [Test]
        public void 장전손잡이를_당기는_중에는_발사할수없다()
        {
            var weapon = CreateEmptyWeapon(out _);

            weapon.TryStartReload();
            weapon.Tick(ReloadTime);

            Assert.IsFalse(weapon.TryFire());
            Assert.IsTrue(weapon.IsReloading);   // 쏘지 못했으니 취소되지도 않는다
        }

        [Test]
        public void 한번에_긴_시간이_지나도_탄창교체와_장전손잡이가_모두_끝난다()
        {
            // 남은 시간이 다음 단계로 이어져야 한다
            var weapon = CreateEmptyWeapon(out _);

            weapon.TryStartReload();
            weapon.Tick(ReloadTime + ChamberTime);

            Assert.IsFalse(weapon.IsReloading);
            Assert.IsTrue(weapon.IsChambered);
        }

        [Test]
        public void 재장전이_끝나면_바로_발사할수있다()
        {
            var weapon = CreateEmptyWeapon(out _);

            weapon.TryStartReload();
            weapon.Tick(ReloadTime + ChamberTime);

            Assert.IsTrue(weapon.TryFire());
        }

        // ── 예비 탄약 부족 ───────────────────────────────

        [Test]
        public void 예비탄약이_부족하면_있는_만큼만_탄창에_채운다()
        {
            var weapon = CreateEmptyWeapon(out var reserve, reserveCount: 2);

            weapon.TryStartReload();
            weapon.Tick(ReloadTime + ChamberTime);

            Assert.AreEqual(0, reserve.Count);
            Assert.IsTrue(weapon.IsChambered);
            Assert.AreEqual(1, weapon.MagazineAmmo);   // 2발 중 1발은 약실로
            Assert.AreEqual(2, weapon.LoadedAmmo);
        }

        [Test]
        public void 탄창에_넣은_만큼_예비탄약이_줄어든다()
        {
            var weapon = CreateEmptyWeapon(out var reserve);

            weapon.TryStartReload();
            weapon.Tick(ReloadTime + ChamberTime);

            Assert.AreEqual(Reserve - Capacity, reserve.Count);
        }

        [Test]
        public void 재장전을_해도_탄의_총량은_변하지_않는다()
        {
            // 기존 탄창 탄이 예비로 돌아갔다가 다시 채워지는 과정에서 탄이 사라지거나 늘어나면 안 된다
            var weapon = CreateWeapon(out var reserve, reserveCount: 1);
            FireAndWait(weapon, 2);   // 약실 1 + 탄창 1, 예비 1
            int before = weapon.LoadedAmmo + reserve.Count;

            weapon.TryStartReload();
            weapon.Tick(ReloadTime + ChamberTime);

            Assert.AreEqual(before, weapon.LoadedAmmo + reserve.Count);
            Assert.AreEqual(2, weapon.MagazineAmmo);
            Assert.AreEqual(0, reserve.Count);
        }
    }
}
