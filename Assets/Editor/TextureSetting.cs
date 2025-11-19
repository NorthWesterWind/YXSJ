using UnityEditor;

public class TextureSetting : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.isReadable = true;
        textureImporter.mipmapEnabled = false;
    }
}
