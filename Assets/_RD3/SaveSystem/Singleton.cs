using System;
using UnityEngine;

namespace _RD3._Universal._Scripts.Utilities
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (_instance == null)
                    {
                        Debug.LogError("[Singleton] No instance of " + typeof(T) + " found in the scene.");
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                Debug.LogWarning("[Singleton] Another instance of " + typeof(T) + " already exists. Destroying this one.");
                _instance = this as T;
              //  Destroy(gameObject);userd
            }
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }
}