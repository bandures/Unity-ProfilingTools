using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditorInternal;
using UnityEditor.Build.Reporting;

#if UNITY_2018_2_OR_NEWER
public class AndroidNativeProfilingBasePostprocessor : IPostprocessBuildWithReport
#else
public class AndroidNativeProfilingPostprocessor : IPostprocessBuild
#endif
{
    public int callbackOrder { get; } = 0;

#if UNITY_2018_2_OR_NEWER
    public void OnPostprocessBuild(BuildReport report)
    {
        PatchBuild(report.summary.platform, report.summary.outputPath);
    }
#else
    public void OnPostprocessBuild(BuildTarget target, string outputPath)
    {
        PatchBuild(target, outputPath);
    }
#endif

    protected void PatchBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.Android)
            return;

        var editorPath = EditorApplication.applicationContentsPath;
        //var alternativePath = FileUtil.CombinePaths(Directory.GetParent(EditorApplication.applicationPath).ToString(), "PlaybackEngines");
        var projectPath = Directory.GetCurrentDirectory();
        var buildPath = Path.Combine(path, PlayerSettings.productName);

        Debug.LogWarning("Build: post-process your build to include debug '.so' files");

        var libIl2cppTarget = Path.Combine(buildPath, "src/main/jniLibs/armeabi-v7a/libil2cpp.so");
        var libIl2cppSource = Path.Combine(projectPath, "Temp/StagingArea/symbols/armeabi-v7a/libil2cpp.so.debug");
        CopyFile(libIl2cppSource, libIl2cppTarget);

        var libUnityTarget = Path.Combine(buildPath, "src/main/jniLibs/armeabi-v7a/libunity.so");
        var libUnitySource = Path.Combine(editorPath, "PlaybackEngines/AndroidPlayer/Variations/il2cpp/Development/Libs/armeabi-v7a/libunity.so");
        CopyFile(libUnitySource, libUnityTarget);
    }

    private bool CopyFile(string src, string dst)
    {
        //        Debug.Log(src + " -> " + dst);
        try
        {
            //File.Copy(src, dst, true);
            FileUtil.CopyFileOrDirectory(src, dst);
        }
        catch (Exception)
        {
            Debug.LogError(string.Format("Failed to copy {0} -> {1}", src, dst));
            return false;
        }

        return true;
    }
}
