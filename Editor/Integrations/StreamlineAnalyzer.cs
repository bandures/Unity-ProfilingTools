using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.UIElements;

namespace Unity.NativeProfiling
{
    /// <summary>
    /// StreamlineAnalyzerIntegration class implements wizard for Arm Mobile Studio
    /// </summary>
    public class StreamlineAnalyzerIntegration : Wizard
    {
        public string Name
        {
            get { return "Arm Streamline Analyzer"; }
        }

        public string[] RequiredFiles
        {
            get { return new string[] { "streamlineanalyzer" }; }
        }

        public IEnumerable<WizardPhase> GetPhases()
        {
            yield return new AndroidValidationPhase();
            yield return new AndroidDeviceCheckPhase();
            yield return new TextWizardPhase("Instructions", 
                "Open exported Gradle project in Android Studio and build project with debug build variant. " +
                "For Arm Mobile Studio and Android Studio setup and guides click on " +
                "<style=link><link=https://docs.google.com/document/d/17WJQZyT4PSSumEZvyvDlpAfC0qZER_vRqmkhrelU6k4/edit?usp=sharing> this link for online documentation");
        }
    }
}
