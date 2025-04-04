using System.Collections.Generic;
using _RD3._Universal._Scripts.Utilities;
using UnityEditor;
using UnityEngine;

namespace _RD3.SaveSystem
{
    [CreateAssetMenu(fileName = "New SavableSO", menuName = "RD3/SaveSystem/SavableSO")]
    public class TestSO : SavableSo
    {
        [SaveVariable]
        public List<int> myInt = new List<int>();
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(TestSO))]
    public class TestSOEditor : SavableSoEditor
    {
        
    }
#endif
}