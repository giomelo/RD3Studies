using System;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace _RD3.Overlays
{
    [Overlay(typeof(SceneView), ID,"Teste")]
    [Icon("Assets/_RD3/Overlays/Sun.png")]
    public class TestOverlay : ToolbarOverlay
    {
        [EditorToolbarElement(ID, typeof(SceneView))]
        public class TesteTool : EditorToolbarButton
        {
            public const string ID = "TesteTool";

            public TesteTool()
            {
                text = "CREATE TOOL";
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_RD3/Overlays/Sun.png");
                tooltip = "Test tool";
                clicked += OnClickButton;
            }

            public void OnClickButton()
            {
                Debug.Log("OnClickButton");
            }
        } 
        private const string ID = "teste-id";
        public TestOverlay() : base(TesteTool.ID)
        {
            
        }
        public override VisualElement CreatePanelContent()
        {
           var root = new VisualElement();

           root.style.width = new StyleLength(new Length(250, LengthUnit.Pixel));
           
           var titleLabel = new Label(text: "Teste");
           root.Add(titleLabel);

           var timeField = new Slider("Time of day", 0f, 24f);
           timeField.style.flexGrow = 1;
           root.Add(timeField);
           
           
           var timeLabel = new Label();
           timeLabel.text = GetTimeAsString(timeField.value);
           root.Add(timeLabel);
           
           timeField.RegisterValueChangedCallback(ctx =>
           {
               timeLabel.text = GetTimeAsString(ctx.newValue);
               
           });

           return root;
        }
        private string GetTimeAsString(float seconds)
        {
            var span = TimeSpan.FromHours(seconds);
            return string.Format("{0:00} : {1:00}", span.Hours, span.Minutes);
        }
    }
 
}
