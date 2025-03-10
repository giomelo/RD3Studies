using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _RD3.CachedEditor
{
    public class CachedEditor : MonoBehaviour
    {
        public ScriptableObject so;
        [HideInInspector]public bool foldOut;
    }
    
    #if UNITY_EDITOR
    
    [CustomEditor(typeof(CachedEditor))]
    public class CachedEditorEditor : UnityEditor.Editor
    {
        private CachedEditor _targetClass;
        private Editor _cachedEditor;
        private bool _showOptions;
        private void OnEnable()
        {
            _targetClass = (CachedEditor)target;
        }

        public override void OnInspectorGUI()
        {
            DrawSettingsEditor(_targetClass.so, ref _targetClass.foldOut, ref _cachedEditor);
  
            _showOptions = EditorGUILayout.Foldout(_showOptions, "Advanced Options");
            if (_showOptions)
            {
                EditorGUILayout.LabelField("Option 1");
                EditorGUILayout.LabelField("Option 2");
            }
            base.OnInspectorGUI();
        }

        public void DrawSettingsEditor(Object settings, ref bool foldout, ref Editor editor)
        {
            if (settings == null) return;
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                foldout =  EditorGUILayout.InspectorTitlebar(foldout, settings);
                if (!foldout) return;
                
                CreateCachedEditor(settings, null, ref editor);
                editor.OnInspectorGUI();
            }
          
        }

    }
    
    #endif
    
}