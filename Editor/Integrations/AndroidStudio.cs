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
    /// AndroidStudioIntegration class implements wizard for Google Android Studio
    /// </summary>
    public class AndroidStudioIntegration : Wizard
    {
        public static readonly string kAndroidDebugInfoPostprocessorKey = "AndroidDebugInfoPostprocessorEnabled";

        public string Name
        {
            get { return "Android Studio"; }
        }

        public string[] RequiredFiles
        {
            get { return new string[] { "androidsystrace" }; }
        }

        public IEnumerable<WizardPhase> GetPhases()
        {
            yield return new AndroidValidationPhase();
            yield return new BuildPostprocessPhase();
#if UNITY_2018_2_OR_NEWER
            yield return new AndroidDeviceCheckPhase();
#endif
            yield return new TextWizardPhase("Instructions", 
                "Open exported Gradle project in Android Studio and start profiler. " +
                "For Android Studio setup and guides click on " +
                "<style=link><link=https://docs.google.com/document/d/17WJQZyT4PSSumEZvyvDlpAfC0qZER_vRqmkhrelU6k4/edit?usp=sharing> this link for online documentation");
        }

        class BuildPostprocessPhase : WizardPhase
        {
            public BuildPostprocessPhase() : base("Build post processor")
            {
            }

            public override void Update(VisualElement root)
            {
                base.Update(root);

                var enablePP = new Button(null);
                enablePP.clickable.clicked += () => { UpdateStatus(!EditorPrefs.GetBool(kAndroidDebugInfoPostprocessorKey, false), enablePP); };
                root.Q("content").Add(enablePP);

                UpdateStatus(EditorPrefs.GetBool(kAndroidDebugInfoPostprocessorKey, false), enablePP);
            }

            private void UpdateStatus(bool status, Button button)
            {
                EditorPrefs.SetBool(kAndroidDebugInfoPostprocessorKey, status);
                button.text = status ? "Enabled" : "Disabled";
                button.AddToClassList(status ? "enabledButton" : "disabledButton");
                button.RemoveFromClassList(status ? "disabledButton" : "enabledButton");
            }
        }
    }
}
