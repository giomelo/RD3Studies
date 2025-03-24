using System;
using _RD3._Universal._Scripts.Utilities;
using UnityEditor;
using UnityEngine;

namespace _RD3.SaveSystem
{
    public abstract class AbstractedSavableClass : MonoBehaviour, ISavedObject
    {
        private void Awake()
        {
            SaveSystem.Instance.AddObjectToList(this);
        }

        public void SaveObject()
        {
            SaveSystem.Instance.SaveObjectState(this);
        }
        
        public void LoadObject()
        {
            SaveSystem.Instance.LoadObjectState(this);
        }
    }
    
        
#if UNITY_EDITOR

    [CustomEditor(typeof(AbstractedSavableClass))]
    public abstract class AbstractedSavableClassEditor : Editor
    {
        private AbstractedSavableClass _targetClass;
        private void OnEnable()
        {
            _targetClass = (AbstractedSavableClass) target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Save"))
                _targetClass.SaveObject();
            

            if (GUILayout.Button("Load"))
                _targetClass.LoadObject();
            
        }
    }
    
#endif
}