using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace Unity.NativeProfiling
{
    public static class WizardUtils
    {
        public static int CountBits(int n)
        {
            int count = 0;
            while (n != 0)
            {
                count++;
                n &= (n - 1);
            }
            return count;
        }

        public static void SimpleRichText(VisualElement root, string text)
        {
            root.style.flexWrap = Wrap.Wrap;
            root.style.flexDirection = FlexDirection.Row;
            
#if UNITY_2018_1_OR_NEWER
            string styleId = "rich-text-default"; 
            string linkUrl = ""; 
            var sections = Regex.Split(text, @"(?=<)");
            foreach (var i in sections)
            {
                var labelText = i;
                
                if (i.StartsWith("<"))
                {
                    var tagData = i.Split('>');
                    if (tagData.Length == 2)
                    {
                        var tagParts = tagData[0].Trim().ToLower().Split('=');
                        if (tagParts.Length == 2)
                        {
                            if (tagParts[0].StartsWith("<style"))
                                styleId = "rich-text-" + tagParts[1];
                            else if (tagParts[0].StartsWith("<link"))
                                linkUrl = tagParts[1];
                        }

                        labelText = tagData[1];
                    }
                }

                labelText = labelText.Replace('\t', ' ');
                labelText = labelText.Replace('\n', ' ');
                var labelTextParts = labelText.Split(' ');
                foreach (var textPart in labelTextParts)
                {
                    if (textPart.Length == 0)
                        continue;

                    var label = MakeLabelText(textPart, styleId, linkUrl);
                    root.Add(label);
                    
                    var space = MakeLabelText(" ", styleId, linkUrl);
                    root.Add(space);
                }
            }
#else
            MakeLabelText(text, "rich-text-default", "");
#endif
        }

        static VisualElement MakeLabelText(string text, string style, string linkUrl)
        {
            var elem = new Label();
            elem.text = text;
            elem.AddToClassList(style);
            if (linkUrl != "")
                elem.RegisterCallback<MouseDownEvent>((clickEvent) => { Application.OpenURL(linkUrl); });
            return  elem;
        }
    }
}
