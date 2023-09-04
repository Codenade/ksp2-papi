#if UNITY_EDITOR

using UnityEditor;

public static class BuildAssets
{
    [MenuItem("Build/Addressables")]
    public static void PerformBuild()
    {
        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.CleanPlayerContent();
        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent();
    }
}

#endif