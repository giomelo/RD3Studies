/*
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace _RD3.Scripts.UI
{
    [RequireComponent(typeof(Toggle))]
    public class DisplayTogglePostProcessing : MonoBehaviour
    {
        [SerializeField] private string effectToToggle;

        public Volume postProcessData;
        private Toggle _toggle;
        private Text _label;

        private void Start()
        {
            postProcessData = FindObjectOfType<Volume>();
            _toggle = GetComponent<Toggle>();
            _label = GetComponentInChildren<Text>();
            _label.text = effectToToggle;
            _toggle.onValueChanged.AddListener(OnToggleChanged);
        }

        protected virtual void OnToggleChanged(bool value)
        {
            if (TryGetEffect(effectToToggle, out VolumeComponent effect))
                effect.active = value;
        }

        public bool TryGetEffect(string effectName, out VolumeComponent effect)
        {
            string namespaceName = "UnityEngine.Rendering.Universal";
            string assemblyName = "Unity.RenderPipelines.Universal.Runtime";

            string fullTypeName = $"{namespaceName}.{effectName}, {assemblyName}";

            var type = Type.GetType(fullTypeName);

            if (type != null)
            {
                if (postProcessData.profile.TryGet(type, out effect)) return true;

                 Debug.LogWarning($"Effect {effectName} not found in post processing profile.");
            }
            else
                Debug.LogError($"Type {fullTypeName} not found.");
            

            effect = null;
            return false;
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(DisplayTogglePostProcessing))]
    public class DebugPostProcessingEditor : Editor
    {
        private SerializedProperty _effectToToggleProperty;
        private string _message;
        private MessageType _messageType;
        private DisplayTogglePostProcessing _targetClass;

        private void OnEnable()
        {
            _effectToToggleProperty = serializedObject.FindProperty("effectToToggle");
            _targetClass = (DisplayTogglePostProcessing)target;
        }

        public override void OnInspectorGUI()
        {
            _targetClass.postProcessData = FindObjectOfType<Volume>();

            if (_targetClass.postProcessData == null)
            {
                _message = "No post processing detected in scene";
                _messageType = MessageType.Error;
            }
            else
            {
                EditorGUILayout.LabelField("Toggle Effect", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(_effectToToggleProperty, new GUIContent("Effect to toggle"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                    if (string.IsNullOrEmpty(_effectToToggleProperty.stringValue))
                    {
                        _message = "Effect cannot be empty";
                        _messageType = MessageType.Warning;
                    }
                    else
                    {
                        bool effectFound = _targetClass.TryGetEffect(_effectToToggleProperty.stringValue, out _);
                        if (!effectFound)
                        {
                            _message = "Effect not found in post processing profile";
                            _messageType = MessageType.Error;
                        }
                        else
                        {
                            _message = "Effect found.";
                            _messageType = MessageType.Info;
                        }
                    }
                    
                    Undo.RecordObject(_targetClass, "Change Effect To Toggle");
                    EditorUtility.SetDirty(_targetClass);

                    GameObject selectedObject = Selection.activeGameObject;
                    if (selectedObject != null)
                    {
                        Undo.RecordObject(selectedObject, "Change Effect To Toggle");
                        EditorUtility.SetDirty(selectedObject);
                    }

                    Repaint();
                }
            }
            
            if (!string.IsNullOrEmpty(_message))
                EditorGUILayout.HelpBox(_message, _messageType);
        }
    }
    #endif
}
*/
