using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using _RD3._Universal._Scripts.Utilities;
using Newtonsoft.Json;
using Refactor.Data.Variables;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Formatting = Newtonsoft.Json.Formatting;

namespace _RD3.SaveSystem
{
    [Serializable]
    public enum SaveTypes
    {
        Binary,
        Json,
        XML,
        TXT
    }
    [Serializable]
    public enum CryptSystem
    {
        None,
        AES,
    }
    
    public class SaveSystem : Singleton<SaveSystem>
    {
        [SerializeField]private List<Variable> variablesToSave = new List<Variable>();
        public string path;
        private static string saveKey ="";

        [SerializeField]private CryptSystem _cryptSystem;
        [SerializeField]private SaveTypes _defaultSaveType = SaveTypes.TXT;

        public int currentSave;
        
        #region Monobeheviour

        private void Start()
        {
            path = $"{Application.persistentDataPath}/save_{0}.save";
            if(saveKey=="")
                saveKey = EncryptSystem.GenerateRandomKey();
            //  LoadGame(0);
            
            GetAllSavedObjects();
        }

        #endregion
        
        
        #region SaveMethods

        private bool HasSave(int slot)
        {
            var path = GetSavePath(slot);
            bool hasSave =  Directory.Exists(GetSavePath(slot));
            Debug.Log(path);
            Debug.Log(hasSave ? "Save exists" : "Save does not exist");
            return hasSave;
        }
        public void SaveGame(int slot)
        {
            Save();
        }

        public void DeleteSave(int slot)
        {
            if (HasSave(slot))
                DeleteSave(GetSavePath(slot));
        }

        private void DeleteSave(string path)
        {
            Directory.Delete(path,true);
        }

        public void LoadGame(int slot)
        {
            if(HasSave(slot))
                Load();
        }

        private void SaveObjectState(object obj)
        {
            if (obj == null)
            {
                Debug.LogError("The object passed to SaveObjectState is null.");
                return;
            }
            jsonObjects.Clear();
            FieldInfo[] fields = obj.GetType().GetFields().Where(field => field.IsDefined(typeof(SaveVariableAttribute), true)).ToArray();
            //var fields = TypeCache.GetFieldsWithAttribute(typeof(SaveVariableAttribute)).ToArray();
            if (fields.Length == 0) fields = obj.GetType().GetFields();
            
            foreach (FieldInfo field in fields)
            {
                var saveTpe = _defaultSaveType;
                var attribute = (SaveVariableAttribute)field.GetCustomAttribute(typeof(SaveVariableAttribute), true);
                
                if (attribute != null)
                    if(attribute.saveType != default) saveTpe = attribute.saveType;
                
                var currentSaveDirectory = GetSavePath(currentSave);
                string fileType = saveTpe switch
                {
                    SaveTypes.Binary => "bin",
                    SaveTypes.Json => "json",
                    SaveTypes.XML => "xml",
                    SaveTypes.TXT => "txt",
                    _ => throw new ArgumentOutOfRangeException()
                };
                path = Path.Combine(currentSaveDirectory, $"{obj.GetType().Name}.{fileType}");
                switch (saveTpe)
                {
                    case SaveTypes.Binary:
                        SaveFormatJson(field, obj);
                        WriteOnFileJsonBinary();
                        break;
                    case SaveTypes.Json:
                        SaveFormatJson(field, obj);
                        WriteOnFileJson();
                        break;
                    case SaveTypes.XML:
                        SaveFormatJson(field, obj);
                        WriteOnFileXmlBinary();
                        break;
                    case SaveTypes.TXT:
                        SaveFormatTxt(field, obj);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
               
            }
        }
        private void LoadObjectState(object obj)
        {
            if (obj == null)
            {
                Debug.LogError("The object passed to LoadObjectState is null.");
                return;
            }

            FieldInfo[] fields = obj.GetType().GetFields()
                .Where(field => field.IsDefined(typeof(SaveVariableAttribute), true)).ToArray();

            //var fields = TypeCache.GetFieldsWithAttribute(typeof(SaveVariableAttribute)).ToArray();
            
            if (fields.Length == 0) fields = obj.GetType().GetFields();
            
            foreach (FieldInfo field in fields)
            {
                var saveTpe = _defaultSaveType;
                var attribute = (SaveVariableAttribute)field.GetCustomAttribute(typeof(SaveVariableAttribute), true);
                if (attribute != null)
                    if(attribute.saveType != default) saveTpe = attribute.saveType;
                
                var currentSaveDirectory = GetSavePath(currentSave);
                string fileType = saveTpe switch
                {
                    SaveTypes.Binary => "bin",
                    SaveTypes.Json => "json",
                    SaveTypes.XML => "xml",
                    SaveTypes.TXT => "txt",
                    _ => throw new ArgumentOutOfRangeException()
                };
                path = Path.Combine(currentSaveDirectory, $"{obj.GetType().Name}.{fileType}");
                switch (saveTpe)
                {
                    case SaveTypes.Binary:
                        LoadFormatBinary(field, obj);
                        break;
                    case SaveTypes.Json:
                        LoadFormatJson(field, obj);
                        break;
                    case SaveTypes.XML:
                        LoadFormatXml(field, obj);
                        break;
                    case SaveTypes.TXT:
                        LoadFormatTxt(field, obj);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        #region TXT

         private void SaveFormatTxt(FieldInfo field, object obj)
        {
            object value = field.GetValue(obj);
            if (value is IList list)
            {
                StringBuilder listValues = new StringBuilder();
                foreach (var item in list)
                {
                    listValues.Append(item).Append(",");
                }
                if (listValues.Length > 0)
                    listValues.Length--; // Remove the last comma
                WriteOnFile($"{field.Name}:[{listValues}]");
            }
            else
            {
                WriteOnFile($"{field.Name}:{value}");
            }
            Debug.Log($"Field {field.Name} has SaveVariableAttribute value: {value}");
        }

        private void LoadFormatTxt(FieldInfo field, object obj)
        {
            string savedData = GetFromFile(field.Name);
            Debug.Log(savedData);
            if (savedData.StartsWith("[") && savedData.EndsWith("]"))
            {
                // Handle list deserialization
                string listData = savedData.Substring(1, savedData.Length - 2);
                string[] items = listData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                IList list = (IList)Activator.CreateInstance(field.FieldType);
                foreach (string item in items)
                    list.Add(Convert.ChangeType(item, field.FieldType.GetGenericArguments()[0]));

                field.SetValue(obj, list);
            }
            else if (field.FieldType == typeof(Vector3))
            {
                // Handle Vector3 deserialization
                string[] components = savedData.Trim('(', ')').Split(',');
                if (components.Length == 3)
                {
                    float x = float.Parse(components[0], CultureInfo.InvariantCulture);
                    float y = float.Parse(components[1], CultureInfo.InvariantCulture);
                    float z = float.Parse(components[2], CultureInfo.InvariantCulture);
                    Vector3 vector = new Vector3(x, y, z);
                    field.SetValue(obj, vector);
                }
            }
            else if (field.FieldType == typeof(Vector2))
            {
                // Handle Vector2 deserialization
                string[] components = savedData.Trim('(', ')').Split(',');
                if (components.Length == 2)
                {
                    float x = float.Parse(components[0], CultureInfo.InvariantCulture);
                    float y = float.Parse(components[1], CultureInfo.InvariantCulture);
                    Vector2 vector = new Vector2(x, y);
                    field.SetValue(obj, vector);
                }
            }
            else
            {
                object value = Convert.ChangeType(savedData, field.FieldType);
                field.SetValue(obj, value);
            }

            Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
        }

        #endregion

        #region JSON

        [XmlInclude(typeof(Vector3))]
        [XmlInclude(typeof(Vector2))]
        [XmlInclude(typeof(Vector4))]
        [XmlInclude(typeof(List<string>))]
        [XmlInclude(typeof(List<int>))]
        [XmlInclude(typeof(List<bool>))]
        [XmlInclude(typeof(List<float>))]
        [XmlInclude(typeof(List<Vector3>))]
        [XmlInclude(typeof(List<Vector2>))]
        [XmlInclude(typeof(List<Vector4>))]
        [XmlInclude(typeof(List<Vector4>))]
        public class JsonObject
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public JsonObject(string name, object value)
            {
                Name = name;
             //   Value = JsonConvert.SerializeObject(value);
                Value = value;
            }
            
            public JsonObject() { }
        }
        
        private void SaveFormatJson(FieldInfo field, object obj)
        {
            object value = field.GetValue(obj);
            JsonObject wrapper = new JsonObject(field.Name, value);
            jsonObjects.Add(wrapper);
            Debug.Log($"Field {field.Name} has SaveVariableAttribute value: {value}");
        }

        private void LoadFormatJson(FieldInfo field, object obj)
        {
            string json = File.ReadAllText(path);
            List<JsonObject> jsonObjects = JsonConvert.DeserializeObject<List<JsonObject>>(json);

            foreach (var jsonObject in jsonObjects)
            {
                if(field.Name != jsonObject.Name) continue;
                
                object convertedValue = ConvertValue(jsonObject.Value, field.FieldType);
                field.SetValue(obj, convertedValue);
            }
            Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
        }
        private object ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            // Conversões simples para tipos primitivos
            if (targetType == typeof(int)) return Convert.ToInt32(value);
            if (targetType == typeof(float)) return Convert.ToSingle(value);
            if (targetType == typeof(bool)) return Convert.ToBoolean(value);
            if (targetType == typeof(string)) return value.ToString();

            // Verifica se é uma lista genérica
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
                return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value,Formatting.Indented,settings), targetType);

            // Tratamento para Vector3 (esperando um XML com as tags <x>, <y>, <z>)
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

            // Tratamento para Vector2 (esperando <x>, <y>)
            if (targetType == typeof(Vector2))
            {
                if (value is XmlNode node)
                {
                    float x = float.Parse(node["x"].InnerText);
                    float y = float.Parse(node["y"].InnerText);
                    return new Vector2(x, y);
                }
            }

            // Tratamento para Vector4 (esperando <x>, <y>, <z>, <w>)
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

            // Caso genérico para outros tipos
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, Formatting.Indented,settings), targetType);
        }
        
        
        #endregion
       
        #region Binary
        
        private void LoadFormatBinary(FieldInfo field, object obj)
        {
            using FileStream fs = new FileStream(path, FileMode.Open);
            using GZipStream gzip = new GZipStream(fs, CompressionMode.Decompress);
            using StreamReader reader = new StreamReader(gzip);
    
            string json = reader.ReadToEnd();
    
            List<JsonObject> jsonObjects = JsonConvert.DeserializeObject<List<JsonObject>>(json);

            foreach (var jsonObject in jsonObjects)
            {
                if (field.Name != jsonObject.Name) continue;

                object convertedValue = ConvertValue(jsonObject.Value, field.FieldType);
                field.SetValue(obj, convertedValue);
            }

            Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
        }
        
        #endregion
        
        #region XML
        
        private void LoadFormatXml(FieldInfo field, object obj)
        {
            using FileStream fs = new FileStream(path, FileMode.Open);
            using StreamReader reader = new StreamReader(fs);

            XmlSerializer serializer = new XmlSerializer(typeof(List<JsonObject>));
            List<JsonObject> jsonObjects = (List<JsonObject>)serializer.Deserialize(reader);

            foreach (var jsonObject in jsonObjects)
            {
                if (field.Name != jsonObject.Name) continue;

                object convertedValue = ConvertValue(jsonObject.Value, field.FieldType);
                field.SetValue(obj, convertedValue);
            }

            Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
        }
        #endregion

        private void GetAllSavedObjects()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (typeof(ISavedObject).IsAssignableFrom(type) && !typeof(ScriptableObject).IsAssignableFrom(type) && !typeof(MonoBehaviour).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    ISavedObject instance = (ISavedObject)Activator.CreateInstance(type);
                    AddObjectToList(instance);
                }
            }
            
            ScriptableObject[] allObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>();

            foreach (ScriptableObject obj in allObjects)
            {
                if (obj is ISavedObject savedObject)
                {
                    Debug.Log(obj.name);
                    AddObjectToList(savedObject);
                }
            }
        }

        private string GetSavePath(int slot)
        {
            string savesDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            string currentSaveDirectory = Path.Combine(savesDirectory, $"save_{slot}");
            return currentSaveDirectory;
        }
        private List<ISavedObject> _savedObjects = new List<ISavedObject>();
        private void Save()
        {
            var currentSaveDirectory = GetSavePath(currentSave);
            Directory.CreateDirectory(currentSaveDirectory);
            foreach (var savableObject in _savedObjects)
            {
                sb.Clear();
                SaveObjectState(savableObject);
            }
        }
        private void Load()
        {
            var currentSaveDirectory = GetSavePath(currentSave);
            if (!Directory.Exists(currentSaveDirectory))
            {
                Debug.LogError("No save found at " + currentSaveDirectory);
                return;
            }

            foreach (var savableObject in _savedObjects)
            {
                sb.Clear();
                LoadObjectState(savableObject);
            }
        }
        
        
        StringBuilder sb = new StringBuilder(); 
        List<JsonObject> jsonObjects = new List<JsonObject>();
        private void WriteOnFile(string lineToAppend)
        {
            sb.AppendLine(lineToAppend);
            using FileStream fs = new FileStream(path, FileMode.Create);
            using StreamWriter writer = new StreamWriter(fs);
            writer.Write(sb.ToString());
        }
        
        private void WriteOnFileJson()
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented,settings);
            File.WriteAllText(path, json);
        }
        
        private void WriteOnFileXmlBinary()
        {
            using FileStream fs = new FileStream(path, FileMode.Create);
            using StreamWriter writer = new StreamWriter(fs);

            XmlSerializer serializer = new XmlSerializer(typeof(List<JsonObject>));
            serializer.Serialize(writer, jsonObjects);
        }

        private void WriteOnFileJsonBinary()
        {
            using FileStream fs = new FileStream(path, FileMode.Create);
            using GZipStream gzip = new GZipStream(fs, CompressionMode.Compress);
            using BinaryWriter writer = new BinaryWriter(gzip);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(jsonObjects, Formatting.None, settings);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            writer.Write(jsonBytes);
        }
        private string GetFromFile(string fieldName)
        {
            using FileStream fs = new FileStream(path, FileMode.Open);
            using StreamReader reader = new StreamReader(fs);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith($"{fieldName}:"))
                {
                    return line.Substring(fieldName.Length + 1);
                }
            }
            return null; 
        }

        public void AddObjectToList(ISavedObject obj)
        {
            Debug.Log("Adding object to list " + obj.GetType().Name);
            _savedObjects.Add(obj);
        }

        #endregion
       
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SaveSystem)), CanEditMultipleObjects]
    public class SceneLoaderControllerEditor : UnityEditor.Editor
    {
        private SaveSystem data;

        private void OnEnable()
        {
            data = (SaveSystem)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            data.path = $"{Application.persistentDataPath}/save_{data.currentSave}";
            if (GUILayout.Button("Save"))
            {
                data.SaveGame(0);
            }

            if (GUILayout.Button("Load"))
            {
                data.LoadGame(0);
            }

            if (GUILayout.Button("Delete Save"))
            {
                data.path = $"{Application.persistentDataPath}/save_{data.currentSave}";
                data.DeleteSave(0);
            }

        }
    }
#endif
}