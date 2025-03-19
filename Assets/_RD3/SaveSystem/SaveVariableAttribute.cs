
using System;
using _RD3.SaveSystem;


namespace _RD3._Universal._Scripts.Utilities
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    // all attributes from the same class have to share the same save type
    public class SaveVariableAttribute : Attribute
    {
        public SaveTypes saveType { get; }
        
        public SaveVariableAttribute(SaveTypes saveType = default)
        {
            this.saveType = saveType;
        }
    }
}

