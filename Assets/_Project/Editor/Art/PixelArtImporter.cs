using UnityEditor;
using UnityEngine;

namespace Kodiak.EditorTools.Art
{
    // Art/ 아래에 새로 들어온 이미지에 픽셀 아트 임포트 설정을 자동 적용한다.
    // 언리얼에서 텍스처마다 Texture Group "2D Pixels (unfiltered)"·압축 설정을 바꾸던 것을 임포트 훅으로 자동화한 것.
    public class PixelArtImporter : AssetPostprocessor
    {
        // 텍스처 임포트 직전에 호출 (임포트 설정을 바꿀 수 있는 시점). 언리얼의 임포트 전 에디터 스크립트 훅에 해당.
        private void OnPreprocessTexture()
        {
            if (!PixelArtImportRules.IsArtPath(assetPath))
            {
                return;
            }

            // .meta가 없는 처음 임포트일 때만 적용. 이후 사람이 바꾼 피벗·시트 분할은 재임포트해도 덮어쓰지 않는다
            if (!assetImporter.importSettingsMissing)
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            PixelArtImportRules.Rule rule = PixelArtImportRules.GetRule(assetPath);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = rule.Mode;
            importer.spritePixelsPerUnit = PixelArtImportRules.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;

            if (rule.SetPivot)
            {
                // 피벗 정렬은 TextureImporter에 직접 프로퍼티가 없어 설정 묶음으로 읽고 써야 한다
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)rule.Alignment;
                importer.SetTextureSettings(settings);
            }

            Debug.Log($"[PixelArtImporter] {rule.Label} 규칙 적용: {assetPath}");
        }
    }
}
