using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem("Assets/AssetBundles/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/AssetBundles";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    [MenuItem("Assets/AssetBundles/Configure AssetBundles")]
    static void ConfigureAssetBundles()
    {
        string bundlePrefix = "cardimages/";
        int bundleProgress = 0;
        string AssetBundleFolder = "Assets/Resources/cardimages";
        string[] folderGuids = AssetDatabase.FindAssets("t:defaultasset", new string[] { AssetBundleFolder });
        foreach (string folderGuid in folderGuids)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);
            string folderName = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath).name;
            string[] textureGuids = AssetDatabase.FindAssets("t:texture", new string[] { folderPath });
            foreach (string textureGuid in textureGuids)
            {
                EditorUtility.DisplayProgressBar("Assigning bundle names...", "", bundleProgress / 4000f);
                string texturePath = AssetDatabase.GUIDToAssetPath(textureGuid);
                TextureImporter importer = TextureImporter.GetAtPath(texturePath) as TextureImporter;
                importer.assetBundleName = bundlePrefix + folderName;

                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.maxTextureSize = 512;
                importer.crunchedCompression = true;
                importer.compressionQuality = 100;
                var settings = importer.GetPlatformTextureSettings("Standalone");
                settings.overridden = false;
                importer.alphaSource = TextureImporterAlphaSource.None;
                settings.maxTextureSize = 256;
                settings.crunchedCompression = true;
                settings.compressionQuality = 50;
                importer.SetPlatformTextureSettings(settings);

                AssetDatabase.SaveAssetIfDirty(importer);
                AssetDatabase.WriteImportSettingsIfDirty(texturePath);
                bundleProgress++;
            }
        }
        EditorUtility.ClearProgressBar();
    }
}
