#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildAssets
{
    private static readonly string Eol = Environment.NewLine;

    [MenuItem("Build/Addressables")]
    public static void PerformBuild()
    {
        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.CleanPlayerContent();
        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent();
        ReportSummary(new BuildSummary());
    }

    private static void ReportSummary(BuildSummary summary)
    {
        Console.WriteLine(
            $"{Eol}" +
            $"###########################{Eol}" +
            $"#      Build results      #{Eol}" +
            $"###########################{Eol}" +
            $"{Eol}" +
            $"Duration: {summary.totalTime}{Eol}" +
            $"Warnings: {summary.totalWarnings}{Eol}" +
            $"Errors: {summary.totalErrors}{Eol}" +
            $"Size: {summary.totalSize} bytes{Eol}" +
            $"{Eol}"
        );
    }
}

#endif