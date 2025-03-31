using UnityEditor;
using UnityEngine;

namespace _RD3.SaveSystem
{
    public abstract class SavableSo : ScriptableObject, ISavedObject
    {
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

    [CustomEditor(typeof(SavableSo))]
    public abstract class SavableSoEditor : Editor
    {
        private SavableSo _targetClass;
        private void OnEnable()
        {
            _targetClass = (SavableSo) target;
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