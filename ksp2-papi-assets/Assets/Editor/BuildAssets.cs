#if UNITY_EDITOR

using System;
using System.Diagnostics;
using UnityEditor;

public static class BuildAssets
{
    private static readonly string Eol = Environment.NewLine;

    [MenuItem("Build/Addressables")]
    public static void PerformBuild()
    {
        var sw = Stopwatch.StartNew();
        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.CleanPlayerContent();
        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent(out var result);
        sw.Stop();
        Console.WriteLine($"Error: {result.Error}");
        bool error = (result.Error ?? "") != "";
        ReportSummary(sw.Elapsed, 0, error ? 255 : 0, 0);
        if (error)
            throw new Exception(result.Error);
    }

    private static void ReportSummary(TimeSpan duration, int warnings, int errors, ulong size)
    {
        Console.WriteLine(
            $"{Eol}" +
            $"###########################{Eol}" +
            $"#      Build results      #{Eol}" +
            $"###########################{Eol}" +
            $"{Eol}" +
            $"Duration: {duration}{Eol}" +
            $"Warnings: {warnings}{Eol}" +
            $"Errors: {errors}{Eol}" +
            $"Size: {size} bytes{Eol}" +
            $"{Eol}"
        );
    }
}

#endif