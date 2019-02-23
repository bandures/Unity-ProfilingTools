using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

// Add Gradle project check
// - doNotStrip should be present in packagingOptions
// Add Android device check:
// - root-ed device
// - kernel naming access
// - systrace buffer (?)
// Add SO check
// - read symbols and scan for UnityLoop or any other signature function name (http://elfsharp.hellsgate.pl/qanda.shtml)

namespace Unity.Android.Profiling
{
    public class AndroidProfilingWindow : EditorWindow
    {
        [MenuItem("Window/Analysis/Profiling Tools")]
        public static void ShowWindow()
        {
            var wnd = EditorWindow.GetWindow(typeof(AndroidProfilingWindow)) as AndroidProfilingWindow;
            wnd.minSize = new Vector2(200, 300);
            wnd.Show();
        }

        public struct BuildParam
        {
            public string name;
            public Func<bool> check;
            public Action fix;

            public BuildParam(string _name, Func<bool> _check, Action _fix)
            {
                name = _name;
                check = _check;
                fix = _fix;
            }
        }

        readonly BuildParam[] m_BuildParams = new BuildParam[] {
            new BuildParam("Active target - Android", () => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android, () => { EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android); } ),
            new BuildParam("Gradle Export", () => EditorUserBuildSettings.exportAsGoogleAndroidProject, () => { EditorUserBuildSettings.exportAsGoogleAndroidProject = true; } ),
            new BuildParam("Minification mode", () => EditorUserBuildSettings.androidDebugMinification == AndroidMinification.Proguard, () => { EditorUserBuildSettings.androidDebugMinification = AndroidMinification.Proguard; } ),
            new BuildParam("Development mode", () => EditorUserBuildSettings.development == false, () => { EditorUserBuildSettings.development = false; } ),
            new BuildParam("Scripting Backend", () => PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP, () => { PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP); } ),
            new BuildParam("Internet permissions", () => PlayerSettings.Android.forceInternetPermission, () => { PlayerSettings.Android.forceInternetPermission = true; } ),
            new BuildParam("Force SD Card permissions", () => PlayerSettings.Android.forceSDCardPermission, () => { PlayerSettings.Android.forceSDCardPermission = true; } ),
            new BuildParam("Installation location - external", () => PlayerSettings.Android.preferredInstallLocation == AndroidPreferredInstallLocation.PreferExternal, () => { PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal; } ),
#if UNITY_2017_3_OR_NEWER
            new BuildParam("Limit to ARM v7 target", () => PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARMv7, () => { PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7; } ),
#else
            new BuildParam("Limit to ARM v7 target", () => { return PlayerSettings.Android.targetDevice == AndroidTargetDevice.ARMv7; }, () => { PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARMv7; } ),
#endif
#if UNITY_2018_3_OR_NEWER
            new BuildParam("Stripping level", () => PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android) == ManagedStrippingLevel.Low, () => { PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low); } )
#else
            new BuildParam("Stripping level", () => PlayerSettings.strippingLevel == StrippingLevel.Disabled, () => { PlayerSettings.strippingLevel = StrippingLevel.Disabled; } )
#endif
        };

        private ADB m_Adb = new ADB();
        private Dictionary<string, AndroidDeviceInfo> m_Devices = new Dictionary<string, AndroidDeviceInfo>();

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("UTools helps you setup your project for profiling on Android with Android Studio.", MessageType.Info);
            EditorGUILayout.Space();

            GUILayout.Space(3);

            foreach (var i in m_BuildParams)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i.name, GUILayout.Width(250));
                if (i.check())
                    GUILayout.Label("Good");
                else
                {
                    if (GUILayout.Button("Fix"))
                        i.fix();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(3);

            if (GUILayout.Button("Fix All"))
            {
                foreach (var i in m_BuildParams)
                    i.fix();
            }

            GUILayout.Space(3);

            foreach (var i in m_Devices)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(i.Value.Model, GUILayout.Width(100));
                GUILayout.Label(i.Value.Id, GUILayout.Width(250));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(" - SDK Version", GUILayout.Width(150));
                GUILayout.Label(i.Value.GetProperty("ro.build.version.sdk"), GUILayout.Width(100));
                if (Int32.Parse(i.Value.GetProperty("ro.build.version.sdk")) >= 27)
                    GUILayout.Label("Good");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(" - Perf access blocked", GUILayout.Width(150));
                GUILayout.Label(i.Value.GetProperty("security.perf_harden"), GUILayout.Width(100));
                if (Int32.Parse(i.Value.GetProperty("security.perf_harden")) == 0)
                    GUILayout.Label("Good");
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(3);

            if (GUILayout.Button("Refresh"))
            {
                var list = m_Adb.RetrieveConnectDevicesIDs();
                m_Devices.Clear();
                foreach (var i in list)
                    m_Devices[i] = m_Adb.RetriveDeviceInfo(i);
            }
        }
    }
}
