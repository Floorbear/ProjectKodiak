using UnityEngine;

namespace Kodiak.Game.Weapons
{
    // 개발용 탄약 표시. 실제 탄약 UI가 생기면 제거한다.
    // 문자열 조합(GC 할당)이 있으므로 에디터와 개발 빌드에서만 그린다.
    public class WeaponDebugHud : MonoBehaviour
    {
        [SerializeField] private PlayerWeapon weapon;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private GUIStyle style;

        // 즉시 모드 GUI. 매 프레임 여러 번(레이아웃/그리기 이벤트마다) 호출된다. 언리얼의 AHUD::DrawHUD에 해당.
        private void OnGUI()
        {
            if (weapon == null || weapon.State == null)
            {
                return;
            }

            // GUIStyle은 OnGUI 안에서만 만들 수 있다
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = 24 };
            }

            var state = weapon.State;
            string text = $"약실 {(state.IsChambered ? 1 : 0)} + 탄창 {state.MagazineAmmo} / 예비 {weapon.Reserve.Count}";
            if (state.IsReloading)
            {
                text += "  [재장전 중]";
            }

            GUI.Label(new Rect(16, 16, 600, 40), text, style);
        }
#endif
    }
}
