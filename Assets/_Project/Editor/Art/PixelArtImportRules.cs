using System;
using UnityEditor;   // SpriteImportMode
using UnityEngine;   // SpriteAlignment

namespace Kodiak.EditorTools.Art
{
    // Art/ 폴더별 픽셀 아트 임포트 규칙. 공통 값과 폴더 이름은 여기서만 정의한다.
    internal static class PixelArtImportRules
    {
        public const string ArtRoot = "Assets/_Project/Art/";
        public const float PixelsPerUnit = 32f;

        public readonly struct Rule
        {
            public readonly string Label;
            public readonly SpriteImportMode Mode;
            public readonly bool SetPivot;              // false면 피벗은 유니티 기본값(중앙) 그대로 두고 사람이 정한다
            public readonly SpriteAlignment Alignment;

            public Rule(string label, SpriteImportMode mode, bool setPivot, SpriteAlignment alignment)
            {
                Label = label;
                Mode = mode;
                SetPivot = setPivot;
                Alignment = alignment;
            }
        }

        // 위에서부터 먼저 맞는 폴더의 규칙을 쓴다
        private static readonly (string Folder, Rule Rule)[] FolderRules =
        {
            // 시트로 들어오므로 칸을 나눈 뒤 피벗을 정한다 (발이 칸 바닥보다 위에 있어 일괄 "아래 중앙"은 틀림)
            ("Character", new Rule("Character", SpriteImportMode.Multiple, false, SpriteAlignment.Center)),
            // 손잡이 위치는 사람이 정한다
            ("Weapons", new Rule("Weapons", SpriteImportMode.Single, false, SpriteAlignment.Center)),
            // 바닥에 놓이는 소품. Y 정렬 기준이 발밑이 되도록
            ("Props", new Rule("Props", SpriteImportMode.Single, true, SpriteAlignment.BottomCenter)),
            ("Tiles", new Rule("Tiles", SpriteImportMode.Single, true, SpriteAlignment.Center)),
            ("Decals", new Rule("Decals", SpriteImportMode.Single, true, SpriteAlignment.Center)),
        };

        private static readonly Rule DefaultRule = new Rule("Default", SpriteImportMode.Single, true, SpriteAlignment.Center);

        public static bool IsArtPath(string assetPath)
        {
            return assetPath.StartsWith(ArtRoot, StringComparison.Ordinal);
        }

        public static Rule GetRule(string assetPath)
        {
            foreach ((string folder, Rule rule) in FolderRules)
            {
                if (assetPath.StartsWith(ArtRoot + folder + "/", StringComparison.Ordinal))
                {
                    return rule;
                }
            }

            return DefaultRule;
        }
    }
}
