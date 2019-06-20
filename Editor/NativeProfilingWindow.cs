using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Linq;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif



namespace Unity.NativeProfiling
{
    /// <summary>
    /// NativeProfilingWindow implements wizard setup UI 
    /// </summary>
    public class NativeProfilingWindow : EditorWindow
    {
        static readonly string kPluginPackageName = "com.unity.profilingtools";
        static readonly string kNativeProfilingToolKey = "NativeProfilingToolKey";
        static readonly string kPluginsSourceFolder = "Plugins~/";
        static readonly string kPluginsTargetFolder = "Runtime/Plugins";

        Button m_ToolSelector;
        List<VisualElement> m_Phases = new List<VisualElement>();

        Wizard m_ActiveTool;
        Wizard[] m_Tools = {
            new AndroidStudioIntegration(),
            new StreamlineAnalyzerIntegration(),
            new SnapdragonProfilerIntegration(),
            new VTuneAmplifierIntegration()
        };


        [MenuItem("Window/Analysis/Profiling Tools")]
        public static void ShowWindow()
        {
            var wnd = GetWindow(typeof(NativeProfilingWindow)) as NativeProfilingWindow;
            wnd.titleContent = new GUIContent("Profiling Tools");
            wnd.minSize = new Vector2(200, 300);
        }

        void OnEnable()
        {
#if UNITY_2019_1_OR_NEWER
            var root = this.rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;
            root.styleSheets.Add(Resources.Load<StyleSheet>("nativeprofiling-style"));
            var template = Resources.Load<VisualTreeAsset>("nativeprofiling-template");
            template.CloneTree(root);
#else
            var root = this.GetRootVisualContainer();
            root.style.flexDirection = FlexDirection.Row;
            root.AddStyleSheetPath("nativeprofiling-style");
            var template = Resources.Load<VisualTreeAsset>("nativeprofiling-template");
            template.CloneTree(root, null);
#endif

            m_ToolSelector = root.Q("toolSelector").Q<Button>("selector");
            m_ToolSelector.clickable.clicked += OnToolSelectorMouseDown;

            // Restore last selected tool from Editor settings
            var savedToolKey = EditorPrefs.GetString(kNativeProfilingToolKey);
            SetActiveTool(m_Tools.TakeWhile(tool => { return tool.Name == savedToolKey; }).FirstOrDefault());
        }

        void OnToolSelectorMouseDown()
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

        /// <summary>
        /// Set currently active tool and updates integration plugin
        /// </summary>
        void SetActiveTool(Wizard wizard)
        {
            // Clear old wizard UI
            foreach (var phase in m_Phases)
            {
                phase.RemoveFromHierarchy();
            }
            m_Phases.Clear();
            
            // Clear plugins folder (this can fail for Windows plugins, as Editor will hold reference to loaded plugins)
            var packagePath = FindPackage(kPluginPackageName);
            if (packagePath != null)
            {
                var dstDir = Path.Combine(packagePath, kPluginsTargetFolder);
                FileUtil.DeleteFileOrDirectory(dstDir);
            }

            // Set active tool
            m_ActiveTool = wizard;
            m_ToolSelector.text = (m_ActiveTool == null ? "None" : m_ActiveTool.Name) + " ▾";
            EditorPrefs.SetString(kNativeProfilingToolKey, m_ActiveTool != null ? m_ActiveTool.Name : "" );

            if (m_ActiveTool == null)
                return;

            // Copy plugin files
            if (packagePath != null)
            {
                foreach (var dirName in m_ActiveTool.RequiredFiles)
                {
                    var srcDir = Path.Combine(Path.Combine(packagePath, kPluginsSourceFolder), dirName).Replace('\\', '/');
                    var dstDir = Path.Combine(packagePath, kPluginsTargetFolder).Replace('\\', '/');
                    try
                    {
                        FileUtil.CopyFileOrDirectory(srcDir, dstDir);
                    }
                    catch (Exception)
                    {
                        Debug.LogFormat("Failed to copy plugin {0} -> {1}", srcDir, dstDir);
                    }
                }
            }
            
            // Generate UI for wizard phases
#if UNITY_2019_1_OR_NEWER
            var root = this.rootVisualElement.Q("phasesView");
#else
            var root = this.GetRootVisualContainer().Q("phasesView");
#endif
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

        
        /// <summary>
        /// Fake package structure to read Unity package JSON files 
        /// </summary>
        [Serializable]
        class UnityPackage
        {
            public string name;
        }
        
        /// <summary>
        /// Find package by ID in package JSON file 
        /// </summary>
        static string FindPackage(string packageId)
        {
            // Look for all 'package.json' files
            var packages = AssetDatabase.FindAssets("package");
            foreach (var pkg in packages)
            {
                var path = AssetDatabase.GUIDToAssetPath(pkg);
                if (Path.GetExtension(path).ToLower() != ".json")
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
