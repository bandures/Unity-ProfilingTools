﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
#if UNITY_2018_2_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace Unity.NativeProfiling
{
#if UNITY_2018_2_OR_NEWER
    public class AndroidIncludeSymbolsPostprocessor : IPostprocessBuildWithReport
#else
    public class AndroidIncludeSymbolsPostprocessor : IPostprocessBuild
#endif
    {
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

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

        void PatchBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.Android || !EditorUserBuildSettings.exportAsGoogleAndroidProject)
                return;
            if (!EditorPrefs.GetBool(AndroidStudioIntegration.kAndroidDebugInfoPostprocessorKey, false))
                return;

            Debug.Log("Build: post-processing your build for profiling. Disable it for non-profiling builds!");

            // Target folder
            var buildPath = Path.Combine(path, PlayerSettings.productName);
            var buildVariant = EditorUserBuildSettings.development ? "Development" : "Release";

            var buildABIs = GetActiveABIs();  
            foreach (var i in buildABIs)
                PatchSpecificABI(buildPath, buildVariant, i);
        }

        void PatchSpecificABI(string buildPath, string buildVariant, string buildABI)
        {
            // Target folder for libs
            var targetBase = Path.Combine(buildPath, string.Format("src/main/jniLibs/{0}/", buildABI));
            
            // Copy unstripped libil2cpp from stagin area
            var projectPath = Directory.GetCurrentDirectory();
            var libIl2cppTarget = Path.Combine(targetBase, "libil2cpp.so");
            var libIl2cppSource = string.Format("Temp/StagingArea/symbols/{0}/libil2cpp.so.debug", buildABI);
            CopyFiles(new[] { projectPath }, libIl2cppSource, libIl2cppTarget);

            // Copy unstripped libunity.so from Unity Editor folder
            // Source folder can be different, depends on platform and they way Unity was built
            var pbeLocations = new[]
            {
                Directory.GetParent(EditorApplication.applicationPath).ToString(),
                EditorApplication.applicationPath,
                EditorApplication.applicationContentsPath,
            };

            var libUnityTarget = Path.Combine(targetBase, "libunity.so");
            var libUnitySource = string.Format("PlaybackEngines/AndroidPlayer/Variations/il2cpp/{0}/Libs/{1}/libunity.so", buildVariant, buildABI);
            CopyFiles(pbeLocations, libUnitySource, libUnityTarget);
        }

        static IEnumerable<string> GetActiveABIs()
        {
#if UNITY_2017_3_OR_NEWER
            int selectedABIs = (int)PlayerSettings.Android.targetArchitectures;
            if ((selectedABIs & (int)AndroidArchitecture.ARM64) != 0)
                yield return "arm64-v8a";
            if ((selectedABIs & (int)AndroidArchitecture.ARMv7) != 0)
                yield return "armeabi-v7a";
            if ((selectedABIs & (int)AndroidArchitecture.X86) != 0)
                yield return "x86";
#else
            var selectedABIs = PlayerSettings.Android.targetDevice;
            if (selectedABIs == AndroidTargetDevice.ARMv7)
                yield return "arm64-v8a";
            else if (selectedABIs == AndroidTargetDevice.x86)
                yield return "x86";
            else if (selectedABIs == AndroidTargetDevice.FAT)
            {
                yield return "arm64-v8a";
                yield return "x86";
            }
#endif
        }
        
        static void CopyFiles(string[] baseSrc, string src, string dst)
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
                Debug.LogErrorFormat("Failed to copy {0} -> {1}", src, dst);
            }
        }
    }
}