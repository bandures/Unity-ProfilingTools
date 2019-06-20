using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace Unity.NativeProfiling
{
    /// <summary>
    /// WizardPhase is a phase of multi-stage wizard setup process
    /// </summary>
    public class WizardPhase
    {
        private int m_PhaseId = 0;
        private string m_Name = "";
        private VisualTreeAsset m_PhaseTemplate;

        public string Name
        {
            get
            {
                return m_Name;
            }
        }

        public WizardPhase(string _name)
        {
            m_Name = _name;
            m_PhaseTemplate = Resources.Load<VisualTreeAsset>("wizardphase-template");
        }

        public void SetPhase(int _id)
        {
            m_PhaseId = _id;
        }

        public virtual void Update(VisualElement root)
        {
            root.Clear();
#if UNITY_2019_1_OR_NEWER            
            m_PhaseTemplate.CloneTree(root);
#else
            m_PhaseTemplate.CloneTree(root, null);
#endif

            root.Q<Label>("header").text = m_Name;
            root.Q<Label>("phase").text = m_PhaseId.ToString();
        }
    }

    /// <summary>
    /// Text-only wizard phase, useful for general descriptions and links to documentation
    /// </summary>
    public class TextWizardPhase : WizardPhase
    {
        string m_Description;

        public TextWizardPhase(string _name, string _description) : base(_name)
        {
            m_Description = _description;
        }

        public override void Update(VisualElement root)
        {
            base.Update(root);

            var content = root.Q("content");
            WizardUtils.SimpleRichText(content, m_Description);
        }
    }

    /// <summary>
    /// Table wizard phase. Displays provided number of rows
    /// </summary>
    public class TableWizardPhase : WizardPhase
    {
        public TableWizardPhase(string _name) : base(_name)
        {
        }

        protected VisualElement AddTable(VisualElement root)
        {
            var table = new VisualElement();
            table.AddToClassList("wizardTable");
            table.AddToClassList("verticalGroup");
            root.Add(table);
            return table;
        }

        protected VisualElement AddTableRow(VisualElement root)
        {
            var separator = new VisualElement();
            separator.name = "separator";
            separator.AddToClassList("horizontalSeparator");
            root.Add(separator);

            var rowGroup = new VisualElement();
            rowGroup.AddToClassList("wizardTableRow");
            rowGroup.AddToClassList("horizontalGroup");
            root.Add(rowGroup);

            return rowGroup;
        }

        protected VisualElement MakeRow(VisualElement table, string name, Func<bool> check, Action fix)
        {
            var rowRoot = AddTableRow(table);

            var nameLabel = new Label(name);
            nameLabel.AddToClassList("nameLabel");

            var statusGroup = new VisualElement();
            statusGroup.AddToClassList("stretchContent");

            var statusLabel = new Label("Good");
            var statusFixButton = new Button(null);

            statusLabel.name = "status";
            statusFixButton.text = "Fix";
            statusFixButton.name = "statusFix";
            statusFixButton.AddToClassList("compactButton");
            
#if UNITY_2019_1_OR_NEWER            
            statusLabel.style.position = Position.Absolute;
            statusFixButton.style.position = Position.Absolute;
#else
            statusLabel.style.positionType = PositionType.Absolute;
            statusFixButton.style.positionType = PositionType.Absolute;
#endif
            
            statusFixButton.clickable.clicked += () => {
                fix();
                UpdateStatus(check, statusGroup);
            };

            statusGroup.Add(statusLabel);
            statusGroup.Add(statusFixButton);

            rowRoot.Add(nameLabel);
            rowRoot.Add(statusGroup);

            UpdateStatus(check, statusGroup);

            return rowRoot;
        }

        private void UpdateStatus(Func<bool> check, VisualElement root)
        {
            var status = check();
            root.Q("status").visible = status;
            root.Q("statusFix").visible = !status;
        }
    }

    /// <summary>
    /// Wizard interface is a base class for all wizard setups
    /// </summary>
    public interface Wizard
    {
        /// <summary>
        /// Wizard name shown as caption in UI
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Array of folders or files needs to be copied for specific integration to work
        /// </summary>
        string[] RequiredFiles { get; }

        /// <summary>
        /// Source of all phases this wizard provides to complete integration setup
        /// </summary>
        IEnumerable<WizardPhase> GetPhases();
    }
}