using System;
using _RD3._Universal._Scripts.Utilities;
using UnityEngine;

namespace _RD3.SaveSystem
{
    public class AbstractedSavableClass<T> : MonoBehaviour, ISavedObject where T : MonoBehaviour
    {
        private void Awake()
        {
            SaveSystem.Instance.AddObjectToList(this);
        }
    }
}