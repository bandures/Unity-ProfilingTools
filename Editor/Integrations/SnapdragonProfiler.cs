using System.Collections.Generic;

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
            yield return new TextWizardPhase("Instructions",
                "Open exported Gradle project in Android Studio and start profiler.\nFor Android Studio setup and guides click on _this link_",
                "https://docs.google.com/document/d/17WJQZyT4PSSumEZvyvDlpAfC0qZER_vRqmkhrelU6k4/edit?usp=sharing");
        }
    }
}
