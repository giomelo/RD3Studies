using UnityEditor;
using UnityEngine;

namespace _RD3.SaveSystem
{
    public abstract class SavableSo : ScriptableObject, ISavableObject
    {
        public void SaveObject()
        {
            SaveSystemManager.Instance.SaveObjectState(this, name);
        }
        
        public void LoadObject()
        {
            SaveSystemManager.Instance.LoadObjectState(this,name);
        }

        public string Name { get => name; set {} }
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