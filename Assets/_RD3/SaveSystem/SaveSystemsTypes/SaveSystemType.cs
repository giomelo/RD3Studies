using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Newtonsoft.Json;
using UnityEngine;
using Formatting = Newtonsoft.Json.Formatting;

namespace _RD3.SaveSystem.SaveSystemsTypes
{
    public class SaveSystemType
    {
        protected readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        protected List<JsonObject> JsonObjects = new List<JsonObject>();
        protected string DecryptedData = string.Empty;

        public virtual void SaveFormat(FieldInfo field, object obj)
        {
            object value = field.GetValue(obj);
            JsonObject wrapper = new JsonObject(field.Name, value);
            JsonObjects.Add(wrapper);
            Debug.Log($"Field {field.Name} has SaveVariableAttribute value: {value}");
        }

        public virtual void WriteOnFile(){}
        public virtual void Load(FieldInfo field, object obj){}
        
        public SaveSystemType()
        {
            JsonObjects.Clear();
            DecryptedData = string.Empty;
        }
        
        protected object ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;
           
            if (targetType == typeof(int)) return Convert.ToInt32(value);
            if (targetType == typeof(float)) return Convert.ToSingle(value);
            if (targetType == typeof(bool)) return Convert.ToBoolean(value);
            if (targetType == typeof(string)) return value.ToString();

          
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
                return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, Formatting.Indented, Settings),
                    targetType);

 
            if (targetType == typeof(Vector3))
            {
                if (value is XmlNode node)
                {
                    float x = float.Parse(node["x"].InnerText);
                    float y = float.Parse(node["y"].InnerText);
                    float z = float.Parse(node["z"].InnerText);
                    return new Vector3(x, y, z);
                }
            }
            
            if (targetType == typeof(Vector2))
            {
                if (value is XmlNode node)
                {
                    float x = float.Parse(node["x"].InnerText);
                    float y = float.Parse(node["y"].InnerText);
                    return new Vector2(x, y);
                }
            }
            
            if (targetType == typeof(Vector4))
            {
                if (value is XmlNode node)
                {
                    float x = float.Parse(node["x"].InnerText);
                    float y = float.Parse(node["y"].InnerText);
                    float z = float.Parse(node["z"].InnerText);
                    float w = float.Parse(node["w"].InnerText);
                    return new Vector4(x, y, z, w);
                }
            }
            
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, Formatting.Indented, Settings),
                targetType);
        }
    }
}