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

        var editorPath = Path.GetDirectoryName(Path.GetDirectoryName(InternalEditorUtility.GetEditorAssemblyPath()));
        var projecPath = Directory.GetCurrentDirectory();
        var buildPath = Path.Combine(path, PlayerSettings.productName);

        Debug.LogWarning("Build: post-process your build to include debug '.so' files");

        var libIl2cppTarget = Path.Combine(buildPath, "src/main/jniLibs/armeabi-v7a/libil2cpp.so");
        var libIl2cppSource = Path.Combine(projecPath, "Temp/StagingArea/symbols/armeabi-v7a/libil2cpp.so.debug");
        //        Debug.Log(libIl2cppSource + " -> " + libIl2cppTarget);
        FileUtil.ReplaceFile(libIl2cppSource, libIl2cppTarget);

        var libUnityTarget = Path.Combine(buildPath, "src/main/jniLibs/armeabi-v7a/libunity.so");
        var libUnitySource = Path.Combine(editorPath, "PlaybackEngines/AndroidPlayer/Variations/il2cpp/Development/Libs/armeabi-v7a/libunity.so");
        //        Debug.Log(libUnitySource + " -> " + libUnityTarget);
        FileUtil.ReplaceFile(libUnitySource, libUnityTarget);
    }
}
