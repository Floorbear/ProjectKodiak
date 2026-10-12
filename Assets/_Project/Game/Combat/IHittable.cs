using UnityEngine;

namespace Kodiak.Game.Combat
{
    // 총알 등에 맞을 수 있는 대상. 언리얼의 IDamageable / TakeDamage에 해당.
    // 피해량, 부위, 팩션 판정 등은 나중에 Core 규칙이 생기면 인자로 추가한다.
    public interface IHittable
    {
        void OnHit(Vector2 point, Vector2 direction);
    }
}
