using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _RD3.SaveSystem.Scripts
{
    [CreateAssetMenu(fileName = "New SavableSO", menuName = "RD3/SaveSystem/SavableSO")]
    public class TestSo : SavableSo
    {
        [SaveVariable]
        public List<int> myInt = new List<int>();
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(TestSo))]
    public class TestSoEditor : SavableSoEditor
    {
        
    }
#endif
}