using _RD3._Universal._Scripts.Utilities;
using UnityEditor;
using UnityEngine;

namespace _RD3.SaveSystem
{
    public abstract class SavableSO<T> : ScriptableObject, ISavedObject where T : ScriptableObject
    {
    }
}