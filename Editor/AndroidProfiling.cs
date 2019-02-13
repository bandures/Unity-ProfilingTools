using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;

public class AndroidProfiling : EditorWindow
{
    [MenuItem("Test/Profiling")]
    public static void ShowWindow()
    {
        var wnd = EditorWindow.GetWindow(typeof(AndroidProfiling)) as AndroidProfiling;
        wnd.minSize = new Vector2(200, 300);
        wnd.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox("UTools helps you setup your project for profiling on Android with Android Studio.", MessageType.Info);
        EditorGUILayout.Space();

        GUILayout.Space(3);

        if (GUILayout.Button("Setup"))
        {
            SetupProject();
        }

        if (GUILayout.Button("Check"))
        {
            Debug.Log(Path.GetFullPath(InternalEditorUtility.GetEditorFolder()));
            Debug.Log(Path.GetFullPath(InternalEditorUtility.GetAssetsFolder()));
            Debug.Log(InternalEditorUtility.GetEditorAssemblyPath());
            Debug.Log(Directory.GetCurrentDirectory());
            Debug.Log(PlayerSettings.productName);
//            Debug.Log(InternalEditorUtility.GetEngineAssemblyPath());
//            Debug.Log(InternalEditorUtility.GetEngineCoreModuleAssemblyPath());
//            Debug.Log(InternalEditorUtility.unityPreferencesFolder);

            Debug.Log(Path.GetPathRoot(InternalEditorUtility.GetEditorAssemblyPath()));
            Debug.Log(Path.GetDirectoryName(Path.GetDirectoryName(InternalEditorUtility.GetEditorAssemblyPath())));
        }
    }

    private void SetupProject()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            Debug.LogWarning("Current target isn't Android!");

        // Build Settings
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
//        EditorUserBuildSettings.androidBuildType = AndroidBuildType.Development; //?
        EditorUserBuildSettings.androidDebugMinification = AndroidMinification.Proguard;
        EditorUserBuildSettings.development = false;
        //        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        // Backend settings
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        // Android Player settings
        PlayerSettings.Android.forceInternetPermission = true;
        PlayerSettings.Android.forceSDCardPermission = true;
        PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal;
        //PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
        PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARMv7;
        PlayerSettings.strippingLevel = StrippingLevel.Disabled;

        AndroidNativeProfilingPostprocessor.enabled = true;
    }
}
