using System;
using System.Collections.Generic;
using _RD3._Universal._Scripts.Utilities;
using UnityEngine;

namespace _RD3.SaveSystem
{
    [Serializable]
    public class TestSavableClass : ISavedObject
    {
        
    }
    
    [Serializable]
    public class TestStruct : TestSavableClass
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
    }
    
    public class TestScript : AbstractedSavableClass<MonoBehaviour>
    {
        [SaveVariable]
        public Vector3 myVector;
        [SaveVariable]
        public List<TestStruct> myStringList = new List<TestStruct>()
        {
            new(3), new(4), new(5)
        };
        [SaveVariable]
        public TestStruct myStruct = new TestStruct(4);
    }
}