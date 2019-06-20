using UnityEditor;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace Unity.NativeProfiling
{
    public class AndroidValidationPhase : TableWizardPhase
    {
        public AndroidValidationPhase() : base("Unity project setup")
        {
        }

        public override void Update(VisualElement root)
        {
            base.Update(root);

            var content = root.Q("content");
            var table = AddTable(content);
            MakeRow(table, "Active target - Android", () => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android, () => { EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android); });
            MakeRow(table, "Gradle Export", () => EditorUserBuildSettings.exportAsGoogleAndroidProject, () => { EditorUserBuildSettings.exportAsGoogleAndroidProject = true; });
            MakeRow(table, "Minification mode", () => EditorUserBuildSettings.androidDebugMinification == AndroidMinification.Proguard, () => { EditorUserBuildSettings.androidDebugMinification = AndroidMinification.Proguard; });
            MakeRow(table, "Development mode", () => EditorUserBuildSettings.development == false, () => { EditorUserBuildSettings.development = false; });
            MakeRow(table, "Scripting Backend", () => PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) == ScriptingImplementation.IL2CPP, () => { PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP); });
            MakeRow(table, "Internet permissions", () => PlayerSettings.Android.forceInternetPermission, () => { PlayerSettings.Android.forceInternetPermission = true; });
            MakeRow(table, "Force SD Card permissions", () => PlayerSettings.Android.forceSDCardPermission, () => { PlayerSettings.Android.forceSDCardPermission = true; });
            MakeRow(table, "Installation location - external", () => PlayerSettings.Android.preferredInstallLocation == AndroidPreferredInstallLocation.PreferExternal, () => { PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal; });
#if UNITY_2017_3_OR_NEWER
            MakeRow(table, "Limit to ARM v7 target", () => PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARMv7, () => { PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7; });
#else
            MakeRow(table, "Limit to ARM v7 target", () => { return PlayerSettings.Android.targetDevice == AndroidTargetDevice.ARMv7; }, () => { PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARMv7; } );
#endif
#if UNITY_2018_3_OR_NEWER
            MakeRow(table, "Stripping level", () => PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android) == ManagedStrippingLevel.Low, () => { PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low); });
#else
            MakeRow(table, "Stripping level", () => PlayerSettings.strippingLevel == StrippingLevel.Disabled, () => { PlayerSettings.strippingLevel = StrippingLevel.Disabled; } );
#endif
            MakeRow(table, "Engine code stripping", () => !PlayerSettings.stripEngineCode, () => { PlayerSettings.stripEngineCode = false; });
        }
    }
}