#if UNITY_EDITOR
using System;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

namespace _RD3._Universal._Scripts.Utilities
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SaveVariableAttribute : Attribute
    {
        
    }
}

#endif