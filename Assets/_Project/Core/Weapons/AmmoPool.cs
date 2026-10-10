using System;

namespace Kodiak.Core.Weapons
{
    // 인벤토리의 예비 탄약 (낱개 탄 수). 나중에 인벤토리가 구경별로 소유한다.
    public sealed class AmmoPool
    {
        public AmmoPool(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), count, "0 이상이어야 합니다.");

            Count = count;
        }

        public int Count { get; private set; }

        // 최대 maxCount발을 꺼낸다. 남은 게 적으면 남은 만큼만 꺼내고, 실제로 꺼낸 수를 돌려준다.
        public int Take(int maxCount)
        {
            if (maxCount < 0) throw new ArgumentOutOfRangeException(nameof(maxCount), maxCount, "0 이상이어야 합니다.");

            int taken = Math.Min(maxCount, Count);
            Count -= taken;
            return taken;
        }

        public void Add(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), count, "0 이상이어야 합니다.");

            Count += count;
        }
    }
}
