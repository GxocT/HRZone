using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundles
{
    [MenuItem("Tool/Build HRZone Bundle")]
    static void BuildAllAssetBundles()
    {
        var targetPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "HRZone", "Resources", "hrzone"));

        var assetBundleBuild = new AssetBundleBuild
        {
            assetBundleName = "hrzone",
            assetNames = new[] { Path.Combine("Assets", "HRZone.prefab") }
        };
        var manifest = BuildPipeline.BuildAssetBundles(Application.temporaryCachePath, new[] { assetBundleBuild }, BuildAssetBundleOptions.ForceRebuildAssetBundle,
            EditorUserBuildSettings.activeBuildTarget);

        if (manifest == null)
        {
            Debug.LogError("Failed to build AssetBundle!");
            return;
        }

        var fileName = manifest.GetAllAssetBundles()[0];
        File.Copy(Path.Combine(Application.temporaryCachePath, fileName), targetPath, true);
        Debug.Log($"AssetBundle exported to: {targetPath}");
    }
}
