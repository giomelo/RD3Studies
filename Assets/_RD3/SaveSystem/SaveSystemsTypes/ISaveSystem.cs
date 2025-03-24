using System.Reflection;

namespace _RD3.SaveSystem.SaveSystemsTypes
{
    public interface ISaveSystem
    {
        public void SaveFormat(FieldInfo field, object obj);
        public void WriteOnFile();
    }
}