using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System;

public class AndroidProfiling : EditorWindow
{
    [MenuItem("Window/Analysis/Profiling Tools")]
    public static void ShowWindow()
    {
        var wnd = EditorWindow.GetWindow(typeof(AndroidProfiling)) as AndroidProfiling;
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

    readonly BuildParam[] buildParams = new BuildParam[] {
        new BuildParam("Active target - Android", () => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android, () => { EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android); } ),
        new BuildParam("Gradle Export", () => EditorUserBuildSettings.exportAsGoogleAndroidProject, () => { EditorUserBuildSettings.exportAsGoogleAndroidProject = true; } ),
        new BuildParam("Minification mode", () => EditorUserBuildSettings.androidDebugMinification == AndroidMinification.Proguard, () => { EditorUserBuildSettings.androidDebugMinification = AndroidMinification.Proguard; } ),
        new BuildParam("Development mode", () => EditorUserBuildSettings.development == false, () => { EditorUserBuildSettings.development = false; } ),
        new BuildParam("Scripting Backend", () => PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP, () => { PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP); } ),
        new BuildParam("Internet permissions", () => PlayerSettings.Android.forceInternetPermission, () => { PlayerSettings.Android.forceInternetPermission = true; } ),
        new BuildParam("Force SD Card permissions", () => PlayerSettings.Android.forceSDCardPermission, () => { PlayerSettings.Android.forceSDCardPermission = true; } ),
        new BuildParam("Installation location - external", () => PlayerSettings.Android.preferredInstallLocation == AndroidPreferredInstallLocation.PreferExternal, () => { PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal; } ),
#if UNITY_2017_4_OR_NEWER
        new BuildParam("Limit to ARM v7 target", () => PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARMv7, () => { PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7; } ),
        new BuildParam("Stripping level", () => PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android) == ManagedStrippingLevel.Disabled, () => { PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Disabled); } )
#else
        new BuildParam("Limit to ARM v7 target", () => { return PlayerSettings.Android.targetDevice == AndroidTargetDevice.ARMv7; }, () => { PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARMv7; } ),
        new BuildParam("Stripping level", () => PlayerSettings.strippingLevel == StrippingLevel.Disabled, () => { PlayerSettings.strippingLevel = StrippingLevel.Disabled; } )
#endif
    };

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("UTools helps you setup your project for profiling on Android with Android Studio.", MessageType.Info);
        EditorGUILayout.Space();

        GUILayout.Space(3);

        foreach (var i in buildParams)
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
            foreach (var i in buildParams)
                i.fix();
        }
    }
}
