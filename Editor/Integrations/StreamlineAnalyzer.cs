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
            yield return new TextWizardPhase("", "");
        }
    }
}
