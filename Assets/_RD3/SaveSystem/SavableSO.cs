using UnityEngine;

namespace _RD3.SaveSystem
{
    public abstract class SavableSo<T> : ScriptableObject, ISavedObject where T : ScriptableObject
    {
    }
}