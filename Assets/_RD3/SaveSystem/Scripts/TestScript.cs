using System;
using UnityEditor;
using UnityEngine;

namespace _RD3.SaveSystem.Scripts
{
    [Serializable]
    public class TestStruct : ISavableObject
    {
        [SaveVariable]
        public float myStructVariable;

        public TestStruct(int myStructVariable)
        {
            this.myStructVariable = myStructVariable;
        }
        public TestStruct()
        {
            
        }

        public string Name { get; set; } = "TestStruct";
    }
    
    public class TestScript : AbstractedSavableClass
    {
        [SaveVariable]
        public Vector3 myVector;
        [SaveVariable]
        public TestStruct[] myStringList = {
            new(3), new(4), new(5)
        };
    
        public TestStruct myStruct = new TestStruct(4);

        private void Start()
        {
            var test = new TestClassNonMonoBehaviour(2, 5, "test");
            SaveSystemManager.Instance.SaveObjectState(test, test.Name);
            test.myFloat = 10;
            Debug.Log(test.myFloat);
            SaveSystemManager.Instance.LoadObjectState(test, test.Name);
            Debug.Log(test.myFloat);
            
        }
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(TestScript))]
    public class TestScriptEditor : AbstractedSavableClassEditor
    {
        
    }
    #endif
}