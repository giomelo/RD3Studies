using System;
using System.Collections.Generic;
using _RD3._Universal._Scripts.Utilities;
using UnityEngine;

namespace _RD3.SaveSystem
{
    public class TestScript : AbstractedSavableClass<MonoBehaviour>
    {
        [SaveVariable]
        public string myString;
        [SaveVariable]
        public List<string> myStringList;
    }
}