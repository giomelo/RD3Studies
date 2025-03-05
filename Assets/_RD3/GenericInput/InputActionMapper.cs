using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace _RD3
{
    public enum InputType
    {
        InputAxis,
        KeyCode,
        InputAction
    }
    
    public class InputActionMapper : MonoBehaviour
    {
        [SerializeField]private InputType _inputType;

        #region Inputs

        [SerializeField]private InputActionProperty _inputAction;
        [SerializeField]private InputAxisEnum _inputAxis;
        [SerializeField]private KeyCode _keyCode;

        #endregion
        
        [Header("Events")]
        public UnityEvent _onPerformed;
        public UnityEvent _onUp;
        public UnityEvent _OnPressing;
        
        public UnityEvent<InputAction.CallbackContext>  _onPerformedCTX;
        public UnityEvent<InputAction.CallbackContext> _onCanceledCTX;

        public UnityEvent<float> _onPerformedVector2;

        private void OnEnable()
        {
            _inputAction.action.performed += OnPerformed;
            _inputAction.action.canceled += OnCancel;
        }

        private void OnDisable()
        {
            _inputAction.action.performed -= OnPerformed;
            _inputAction.action.canceled -= OnCancel;
        }

        private void Update()
        {
            switch (_inputType)
            {
                case InputType.InputAxis:
                    _onPerformedVector2?.Invoke(Input.GetAxis(_inputAxis.ToString()));
                    Debug.Log(Input.GetAxis(_inputAxis.ToString()));
                    break;
                case InputType.KeyCode:
                    
                    if (_onPerformed != null)
                    {
                        if(Input.GetKeyDown(_keyCode))
                            _onPerformed?.Invoke();
                    }

                    if (_onUp != null)
                    {
                        if(Input.GetKeyUp(_keyCode))
                            _onUp?.Invoke();
                    }

                    if (_OnPressing != null)
                    {
                        if(Input.GetKey(_keyCode))
                            _OnPressing?.Invoke();
                    }
                 
                    break;
            }
        }

        private void OnPerformed(InputAction.CallbackContext obj)
        {
            _onPerformedCTX?.Invoke(obj);
        }
        
        private void OnCancel(InputAction.CallbackContext obj)
        {
            _onCanceledCTX?.Invoke(obj);
        }
        
#if UNITY_EDITOR
        private void OnGUI()
        {
            if (GUILayout.Button("Generate InputAxis Enum"))
                GenerateInputAxisEnum();
            
        }

        public void GenerateInputAxisEnum()
        {
            var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            SerializedObject obj = new SerializedObject(inputManager);
            SerializedProperty axesProperty = obj.FindProperty("m_Axes");

            StringBuilder enumBuilder = new StringBuilder();
            enumBuilder.AppendLine("public enum InputAxisEnum");
            enumBuilder.AppendLine("{");
            
            // obs the enum is saved without the spaces, so you have to change the name in the input system to match the enum name
            for (int i = 0; i < axesProperty.arraySize; i++)
            {
                SerializedProperty axis = axesProperty.GetArrayElementAtIndex(i);
                string name = axis.FindPropertyRelative("m_Name").stringValue.Replace(" ", "");
                enumBuilder.AppendLine($" {name},");
            }

            enumBuilder.AppendLine("}");

            string filePath = "Assets/_RD3/GenericInput/InputAxisEnum.cs";
            File.WriteAllText(filePath, enumBuilder.ToString());
            AssetDatabase.Refresh();
            Debug.Log("Enum Generated");
        }
        #endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(InputActionMapper))]
    public class InputActionMapperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var inputType = serializedObject.FindProperty("_inputType");
            EditorGUILayout.PropertyField(inputType);

          //  serializedObject.Update();

            switch ((InputType)inputType.enumValueIndex)
            {
                case InputType.InputAction:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_inputAction"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_onPerformedCTX"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_onCanceledCTX"));
                    break;
                case InputType.InputAxis:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_inputAxis"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_onPerformedVector2"));
                    break;
                case InputType.KeyCode:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_keyCode"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_onPerformed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_onUp"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_OnPressing"));
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            
            if (GUILayout.Button("Generate InputAxis Enum"))
            {
                ((InputActionMapper)target).GenerateInputAxisEnum();
            }
        }
    }
    #endif
}
