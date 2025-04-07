using System;

namespace _RD3.SaveSystem.Scripts
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SaveVariableAttribute : Attribute
    {
        // all attributes from the same class have to share the same save type
        public SaveTypes saveType { get; }
        
        public SaveVariableAttribute(SaveTypes saveType = default)
        {
            this.saveType = saveType;
        }
    }
}

