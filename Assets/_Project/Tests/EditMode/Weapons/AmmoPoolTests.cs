using System;
using Kodiak.Core.Weapons;
using NUnit.Framework;

namespace Kodiak.Tests.EditMode.Weapons
{
    public class AmmoPoolTests
    {
        [Test]
        public void 꺼내면_줄고_넣으면_는다()
        {
            var pool = new AmmoPool(10);

            Assert.AreEqual(3, pool.Take(3));
            Assert.AreEqual(7, pool.Count);

            pool.Add(5);
            Assert.AreEqual(12, pool.Count);
        }

        [Test]
        public void 요청보다_적게_남아있으면_남은_만큼만_꺼낸다()
        {
            var pool = new AmmoPool(2);

            Assert.AreEqual(2, pool.Take(30));
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void 음수_개수는_허용하지_않는다()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AmmoPool(-1));

            var pool = new AmmoPool(10);
            Assert.Throws<ArgumentOutOfRangeException>(() => pool.Take(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => pool.Add(-1));
        }
    }
}
