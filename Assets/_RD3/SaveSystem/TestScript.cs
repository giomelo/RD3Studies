using System;
using System.Collections.Generic;
using _RD3._Universal._Scripts.Utilities;
using UnityEngine;

namespace _RD3.SaveSystem
{
    public class TestStruct : ISavedObject
    {
        [SaveVariable]
        public int myStructVariable;
    }
    
    public class TestScript : AbstractedSavableClass<MonoBehaviour>
    {
        [SaveVariable]
        public Vector3 myVector;
        [SaveVariable]
        public List<Vector3> myStringList;
    }
}