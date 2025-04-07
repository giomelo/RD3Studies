using UnityEditor;
using UnityEngine;

namespace _RD3.SaveSystem
{
    public abstract class AbstractedSavableClass : MonoBehaviour, ISavableObject
    {
        private void Awake()
        {
            SaveSystemManager.Instance.AddObjectToList(this);
        }

        public void SaveObject()
        {
            SaveSystemManager.Instance.SaveObjectState(this, Name);
        }
        
        public void LoadObject()
        {
            SaveSystemManager.Instance.LoadObjectState(this, Name);
        }

        public string Name { get => gameObject.name; set{} }
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