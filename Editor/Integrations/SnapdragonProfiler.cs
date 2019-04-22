using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Experimental.UIElements;

namespace Unity.NativeProfiling
{
    /// <summary>
    /// SnapdragonProfilerIntegration class implements wizard for Qualcomm Snapdragon Profiler
    /// </summary>
    public class SnapdragonProfilerIntegration : Wizard
    {
        public string Name
        {
            get { return "Snapdragon Profiler"; }
        }

        public string[] RequiredFiles
        {
            get { return new string[] { "androidsystrace" }; }
        }

        public IEnumerable<WizardPhase> GetPhases()
        {
            yield return new ValidationPhase();
            yield return new TextWizardPhase("Instructions",
                "Open exported Gradle project in Android Studio and build project with debug build variant. " +
                "For Snapdragon Profiler and Android Studio setup and guides click on " +
                "<style=link><link=https://docs.google.com/document/d/17WJQZyT4PSSumEZvyvDlpAfC0qZER_vRqmkhrelU6k4/edit?usp=sharing> this link for online documentation");
        }

        class ValidationPhase : TableWizardPhase
        {
            public ValidationPhase()
                : base("Unity project setup") { }

            public override void Update(VisualElement root)
            {
                base.Update(root);

                var content = root.Q("content");
                var table = AddTable(content);
                MakeRow(table, "Active target - Android", () => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android, () => { EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android); });
                
                // We need to use Gradle project export and build project separately to make sure that application debug flag is set. By default in release mode Unity resets it to false 
                MakeRow(table, "Gradle Export", () => EditorUserBuildSettings.exportAsGoogleAndroidProject, () => { EditorUserBuildSettings.exportAsGoogleAndroidProject = true; });
                
                // SDP requires these settings
                MakeRow(table, "Internet permissions", () => PlayerSettings.Android.forceInternetPermission, () => { PlayerSettings.Android.forceInternetPermission = true; });
                MakeRow(table, "Force SD Card permissions", () => PlayerSettings.Android.forceSDCardPermission, () => { PlayerSettings.Android.forceSDCardPermission = true; });
                MakeRow(table, "Installation location - external", () => PlayerSettings.Android.preferredInstallLocation == AndroidPreferredInstallLocation.PreferExternal, () => { PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.PreferExternal; });

                // Limit to one target, to make sure that tool isn't confused which platform to inject
#if UNITY_2017_3_OR_NEWER
                MakeRow(table, "Limit to ARM v7 target", () => WizardUtils.CountBits((int)PlayerSettings.Android.targetArchitectures) == 1, () => { PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7; });
#else
                MakeRow(table, "Limit to ARM v7 target", () => { return PlayerSettings.Android.targetDevice == AndroidTargetDevice.ARMv7; }, () => { PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARMv7; } );
#endif
            }
        }
    }
}
