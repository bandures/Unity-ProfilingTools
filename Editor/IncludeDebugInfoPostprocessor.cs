﻿using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Unity.Android.Profiling
{
#if UNITY_2018_2_OR_NEWER
    interface IPostprocessBuildBase : IPostprocessBuildWithReport { }
#else
    interface IPostprocessBuildBase : IPostprocessBuild { }
#endif

    public class IncludeDebugInfoPostprocessor : IPostprocessBuildBase
    {
        public int callbackOrder { get; } = 0;

        // Unity 2018.2+
        public void OnPostprocessBuild(BuildReport report)
        {
            PatchBuild(report.summary.platform, report.summary.outputPath);
        }

        // Older Unity
        public void OnPostprocessBuild(BuildTarget target, string outputPath)
        {
            PatchBuild(target, outputPath);
        }

        protected void PatchBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.Android)
                return;

            Debug.Log("Build: post-processing your build for profiling. Disable it for non-profiling builds!");

            // Target folder
            var buildPath = Path.Combine(path, PlayerSettings.productName);

            // Copy unstripped libil2cpp from stagin area
            var projectPath = Directory.GetCurrentDirectory();
            var libIl2cppTarget = Path.Combine(buildPath, "src/main/jniLibs/armeabi-v7a/libil2cpp.so");
            var libIl2cppSource = "Temp/StagingArea/symbols/armeabi-v7a/libil2cpp.so.debug";
            CopyFile(new string[] { projectPath }, libIl2cppSource, libIl2cppTarget);

            // Copy unstripped libunity.so from Unity Editor folder
            // Source folder can be different, depends on platform and they way Unity was built
            var pbeLocations = new string[]
            {
            Directory.GetParent(EditorApplication.applicationPath).ToString(),
            EditorApplication.applicationPath,
            EditorApplication.applicationContentsPath,
            };

            var libUnityTarget = Path.Combine(buildPath, "src/main/jniLibs/armeabi-v7a/libunity.so");
            var libUnitySource = "PlaybackEngines/AndroidPlayer/Variations/il2cpp/Development/Libs/armeabi-v7a/libunity.so";
            CopyFile(pbeLocations, libUnitySource, libUnityTarget);
        }

        private bool CopyFile(string[] baseSrc, string src, string dst)
        {
            bool success = false;
            foreach (var i in baseSrc)
            {
                try
                {
                    File.Copy(Path.Combine(i, src), dst, true);
                    success = true;
                    break;
                }
                catch (Exception)
                {
                }
            }

            if (!success)
            {
                Debug.LogError(string.Format("Failed to copy {0} -> {1}", src, dst));
                return false;
            }

            return true;
        }
    }
}