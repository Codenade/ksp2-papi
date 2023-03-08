using UnityEngine;
using UnityEditor;

public class CreateAssetBundle : MonoBehaviour
{
    [MenuItem("KSP_PAPI/Build Asset Bundles")]
    static void BuildAssetBundles()
    {
        AssetBundleBuild[] buildMap = 
        {
            new AssetBundleBuild()
            {
                assetBundleName = "ksp_papi.assetbundle",
                assetNames = new string[]
                {
                    "Assets/KSP_PAPI/KSP_PAPI.fbx",
                    "Assets/KSP_PAPI/KSP_PAPI.png",
                    "Assets/KSP_PAPI/KSP_PAPI.mat",
                    "Assets/KSP_PAPI/KSP_PAPI.shader",
                    "Assets/KSP_PAPI/KSP_PAPI.prefab"
                }
            }
        };
        BuildPipeline.BuildAssetBundles("AssetBundles", buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);        
    }
}