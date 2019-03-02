using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
        public static readonly string kAndroidDebugInfoPostprocessorKey = "AndroidDebugInfoPostprocessorEnabled";

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

        public struct DeviceParam
        {
            public string name;
            public Func<AndroidDeviceInfo, string> value;
            public Func<AndroidDeviceInfo, string> resolution;

            public DeviceParam(string _name, Func<AndroidDeviceInfo, string> _value, Func<AndroidDeviceInfo, string> _resolution)
            {
                name = _name;
                value = _value;
                resolution = _resolution;
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
            new BuildParam("Stripping level", () => PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android) == ManagedStrippingLevel.Low, () => { PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low); } ),
#else
            new BuildParam("Stripping level", () => PlayerSettings.strippingLevel == StrippingLevel.Disabled, () => { PlayerSettings.strippingLevel = StrippingLevel.Disabled; } ),
#endif
            new BuildParam("Engine code stripping", () => !PlayerSettings.stripEngineCode, () => { PlayerSettings.stripEngineCode = false; } )
        };

        readonly DeviceParam[] m_DeviceParams = new DeviceParam[] {
            // Device API Level - 26 is minimum required
            new DeviceParam("SDK Version", (dev) => dev.GetProperty("ro.build.version.sdk"), (dev) => { return Int32.Parse(dev.GetProperty("ro.build.version.sdk")) >= 26 ? "Good" : "Failed"; } ),
            // Check that we can execute 'su' command
            new DeviceParam("Is Rooted", (dev) => dev.IsRooted.ToString(), (dev) => { return dev.IsRooted ? "Good" : ""; } ),
            // Check '/proc/sys/kernel/perf_event_paranoid' for perf mode
            new DeviceParam("Perf enabled", (dev) => dev.PerfLevel.ToString(), (dev) => { if (dev.PerfLevel == 3) return "Disabled"; if (dev.PerfLevel == -1) return "Full access"; return "Limited"; } ),
            // Check property 'security.perf_harden', is access to perf hardened
            new DeviceParam("Perf access hardened", (dev) => dev.GetProperty("security.perf_harden").ToString(), (dev) => { return Int32.Parse(dev.GetProperty("security.perf_harden")) == 0 ? "Good" : "Failed"; } ),
            // Check SE Linux default permission mode
            new DeviceParam("Kernel security policy", (dev) => dev.KernelPolicy, (dev) => { return dev.KernelPolicy.ToLower() == "permissive" ? "Good" : "Failed"; } ),
        };

        private AndroidADB m_Adb = new AndroidADB();
        private Dictionary<string, AndroidDeviceInfo> m_Devices = new Dictionary<string, AndroidDeviceInfo>();

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("UTools helps you setup your project for profiling on Android with Android Studio.", MessageType.Info);
            EditorGUILayout.Space();

            GUILayout.Space(3);

            var previousColor = GUI.backgroundColor;
            bool ppStatus = EditorPrefs.GetBool(kAndroidDebugInfoPostprocessorKey, false);
            GUI.backgroundColor = ppStatus ? Color.red : Color.green;
            if (GUILayout.Button((ppStatus ? "Disable" : "Enable") + " Android Debug Info postprocessor"))
                EditorPrefs.SetBool(kAndroidDebugInfoPostprocessorKey, !ppStatus);
            GUI.backgroundColor = previousColor;

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

                foreach (var par in m_DeviceParams)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(" - " + par.name, GUILayout.Width(150));
                    GUILayout.Label(par.value(i.Value), GUILayout.Width(100));
                    GUILayout.Label(par.resolution(i.Value));
                    GUILayout.EndHorizontal();
                }
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
