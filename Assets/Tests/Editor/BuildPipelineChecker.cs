using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildPipelineChecker
{
    [MenuItem("Tools/Run Build Pipeline Smoke Test")]
    public static void RunBuildSmokeTest()
    {
        Debug.Log("Starting Build Pipeline Smoke Test. . . ");

        BuildPlayerOptions buildOptions = new BuildPlayerOptions();

        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length == 0)
        {
            Debug.LogError("Build Pipeline Error: No scenes are added to the Build Settings!");
            return;
        }

        string[] scenePaths = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenePaths[i] = scenes[i].path;
        }

        buildOptions.scenes = scenePaths;
        buildOptions.locationPathName = "Builds/TestBuild/Game.exe";
        buildOptions.target = BuildTarget.StandaloneWindows64;
        buildOptions.options = BuildOptions.Development; // Using valid Development flag

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"<color=green>Build Pipeline Test Passed!</color> Filesize: {summary.totalSize} bytes. Time: {summary.totalTime.TotalSeconds} seconds.");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError($"<color=red>Build Pipeline Test Failed!</color> Errors: {summary.totalErrors}");
        }
    }
}