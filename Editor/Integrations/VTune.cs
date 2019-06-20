using System.Collections.Generic;
using UnityEditor;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace Unity.NativeProfiling
{
    /// <summary>
    /// VTuneAmplifierIntegration class implements wizard for Intel VTune Amplifier
    /// </summary>
    public class VTuneAmplifierIntegration : Wizard
    {
        private static readonly string kSettingCopyPDBFiles = "CopyPDBFiles";

        public string Name
        {
            get { return "VTune Amplifier"; }
        }

        public string[] RequiredFiles
        {
            get { return new string[] { "vtune" }; }
        }

        public IEnumerable<WizardPhase> GetPhases()
        {
            yield return new ProjectSetupPhase();
            yield return new TextWizardPhase("Instructions",
                "Open built exe file in VTune Amplifier and start profiler.\nFor VTune setup and guides click on <link=https://docs.google.com/document/d/1dAlaMaHxMYtlaybiwldLk2PFJTg-DSYssYq8OoAhsCA/edit?usp=sharing>this link");
        }

        class ProjectSetupPhase : TableWizardPhase
        {
            public ProjectSetupPhase() : base("Unity project setup")
            {
            }

            public override void Update(VisualElement root)
            {
                base.Update(root);

                var content = root.Q("content");
                var table = AddTable(content);

                // Build target - Windows
                MakeRow(table, "Active target - Windows", 
                    () => (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows) || (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64), 
                    () => { EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64); });

                // Scripting backend - IL2CPP
                MakeRow(table, "Scripting Backend", 
                    () => PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone) == ScriptingImplementation.IL2CPP, 
                    () => { PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP); });

                // Copy PDB files
                MakeRow(table, "Copy PDB files",
                    () => EditorUserBuildSettings.GetPlatformSettings("Standalone", kSettingCopyPDBFiles).ToLower() == "true",
                    () => { EditorUserBuildSettings.SetPlatformSettings("Standalone", kSettingCopyPDBFiles, "true"); });
            }
        }
    }
}
