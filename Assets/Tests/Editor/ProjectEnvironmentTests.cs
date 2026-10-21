using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.Build;

public class ProjectEnvironmentTests
{
    [Test]
    public void Test_TargetPlatform_IsDesktopStandalone()
    {
        BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;

        bool isDesktop = currentTarget == BuildTarget.StandaloneWindows ||
                         currentTarget == BuildTarget.StandaloneWindows64 ||
                         currentTarget == BuildTarget.StandaloneOSX ||
                         currentTarget == BuildTarget.StandaloneLinux64;

        Assert.IsTrue(isDesktop, $"Active build target is set to {currentTarget}. Project requires a Desktop Standalone target.");
    }

    [Test]
    public void VerifyCoreFoldersExist()
    {
        string[] requiredFolders = new string[]
        {
            "Assets/Scenes",
            "Assets/Scripts",
            "Assets/Prefabs"
        };

        foreach (string folder in requiredFolders)
        {
            bool exists = Directory.Exists(Path.Combine(Application.dataPath, "..", folder));
            Assert.IsTrue(exists, $"Required project folder missing: {folder}");
        }
    }

    [Test]
    public void Test_ScriptingBackend_Configuration()
    {
        ScriptingImplementation backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone);
        Debug.Log($"Current Standalone Scripting Backend: {backend}");
        Assert.AreNotEqual(ScriptingImplementation.WinRTDotNET, backend, "Invalid scripting backend selected for desktop build.");
    }
}