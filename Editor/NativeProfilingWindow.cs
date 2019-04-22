using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEditor.PackageManager;
using System.Linq;


namespace Unity.NativeProfiling
{
    /// <summary>
    /// NativeProfilingWindow implements wizard setup UI 
    /// </summary>
    public class NativeProfilingWindow : EditorWindow
    {
        private static readonly string kNativeProfilingToolKey = "NativeProfilingToolKey";

        private Button m_ToolSelector;
        private List<VisualElement> m_Phases = new List<VisualElement>();

        private Wizard m_ActiveTool = null;
        private Wizard[] m_Tools = {
            new AndroidStudioIntegration(),
            new StreamlineAnalyzerIntegration(),
            new SnapdragonProfilerIntegration(),
            new VTuneAmplifierIntegration()
        };


        [MenuItem("Window/Analysis/Profiling Tools")]
        public static void ShowWindow()
        {
            var wnd = EditorWindow.GetWindow(typeof(NativeProfilingWindow)) as NativeProfilingWindow;
            wnd.titleContent = new GUIContent("Native profiling wizard");
            wnd.minSize = new Vector2(200, 300);
        }

        private void OnEnable()
        {
            var root = this.GetRootVisualContainer();
            root.style.flexDirection = FlexDirection.Row;

            root.AddStyleSheetPath("nativeprofiling-style");
            var template = Resources.Load<VisualTreeAsset>("nativeprofiling-template");
            template.CloneTree(root, null);

            m_ToolSelector = root.Q("toolSelector").Q<Button>("selector");
            m_ToolSelector.clickable.clicked += OnToolSelectorMouseDown;

            var savedToolKey = EditorPrefs.GetString(kNativeProfilingToolKey);
            SetActiveTool(m_Tools.TakeWhile((tool) => { return tool.Name == savedToolKey; }).FirstOrDefault());
        }

        private void OnToolSelectorMouseDown()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("None"), m_ActiveTool == null, () => { SetActiveTool(null); });
            foreach (var tool in m_Tools)
            {
                menu.AddItem(new GUIContent(tool.Name), m_ActiveTool == tool, () => { SetActiveTool(tool); });
            }

            // Show dropdown menu
            var menuPosition = new Vector2(0, m_ToolSelector.layout.height);
            menuPosition = m_ToolSelector.LocalToWorld(menuPosition);
            var menuRect = new Rect(menuPosition, Vector2.zero);
            menu.DropDown(menuRect);
        }

        private void SetActiveTool(Wizard wizard)
        {
            // Clear old wizard UI
            foreach (var phase in m_Phases)
            {
                phase.RemoveFromHierarchy();
            }
            m_Phases.Clear();
            
            // Clear plugins folder
            var packagePath = FindPackage("com.unity.profilingtools");
            if (packagePath != null)
            {
                var dstDir = Path.Combine(packagePath, "Runtime/Plugins");
                FileUtil.DeleteFileOrDirectory(dstDir);
            }

            // Set active tool
            m_ActiveTool = wizard;
            m_ToolSelector.text = (m_ActiveTool == null ? "None" : m_ActiveTool.Name) + " ▾";
            EditorPrefs.SetString(kNativeProfilingToolKey, m_ActiveTool != null ? m_ActiveTool.Name : "" );

            if (m_ActiveTool == null)
                return;

            // Copy plugins files
            if (packagePath != null)
            {
                foreach (var dirName in m_ActiveTool.RequiredFiles)
                {
                    var srcDir = Path.Combine(packagePath, "Plugins~/" + dirName).Replace('\\', '/');
                    var dstDir = Path.Combine(packagePath, "Runtime/Plugins").Replace('\\', '/');
                    try
                    {
                        FileUtil.CopyFileOrDirectory(srcDir, dstDir);
                    }
                    catch (Exception e)
                    {
                        Debug.LogFormat("Failed to copy plugin {0} -> {1}. Reason: ", srcDir, dstDir, e.ToString());
                    }
                }
            }
            
            // Generate UI for wizard phases
            var root = this.GetRootVisualContainer().Q("phasesView");
            if (m_ActiveTool.GetPhases() != null)
            {
                int counter = 2;
                foreach (var phase in m_ActiveTool.GetPhases())
                {
                    var phaseGroup = new VisualElement();
                    phaseGroup.AddToClassList("wizardGroup");
                    phaseGroup.AddToClassList("horizontalGroup");
                    phase.SetPhase(counter);
                    phase.Update(phaseGroup);

                    counter++;

                    root.Add(phaseGroup);
                    m_Phases.Add(phaseGroup);
                }
            }
        }

        [Serializable]
        public class UnityPackage
        {
            public string name;
        }
        
        private string FindPackage(string packageId)
        {
            // Look for all 'package.json' files
            var packages = AssetDatabase.FindAssets("package");
            foreach (var pkg in packages)
            {
                var path = AssetDatabase.GUIDToAssetPath(pkg);
                if (Path.GetExtension(path) != ".json")
                    continue;
                
                // Look for identical package name id
                var fileData = File.ReadAllText(path);
                var pkgClass = JsonUtility.FromJson<UnityPackage>(fileData);
                if (pkgClass.name == packageId)
                    return Path.GetDirectoryName(path);
            }

            return null;
        }
    }
}
