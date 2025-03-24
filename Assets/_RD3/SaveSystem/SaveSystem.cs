using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using _RD3._Universal._Scripts.Utilities;
using Newtonsoft.Json;
using Refactor.Data.Variables;
using UnityEditor;
using UnityEngine;
using Formatting = Newtonsoft.Json.Formatting;

namespace _RD3.SaveSystem
{
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
    [XmlInclude(typeof(TestStruct))]
    [XmlInclude(typeof(List<TestStruct>))]
    [Serializable]
    public class JsonObject
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public JsonObject(string name, object value)
        {
            Name = name;
            // Value = JsonConvert.SerializeObject(value);
            Value = value;
        }

        public JsonObject()
        {
        }
    }
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
        #region Serialized Variables

        [SerializeField] private List<Variable> variablesToSave = new List<Variable>();
        public string path;
        [SerializeField] private CryptSystem _cryptSystem;
        [SerializeField] private SaveTypes _defaultSaveType = SaveTypes.TXT;
        public int currentSave;

        #endregion

        #region Private Variables

        private StringBuilder _sb = new StringBuilder();
        private List<JsonObject> _jsonObjects = new List<JsonObject>();
        private List<ISavedObject> _savedObjects = new List<ISavedObject>();
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        string _decryptedData = string.Empty;

        #endregion
        
        
        #region Monobeheviour

        private void Start()
        {
            path = $"{Application.persistentDataPath}/save_{0}.save";
            GetAllSavedObjects();
        }

        #endregion


        #region SaveMethods

        private bool HasSave(int slot)
        {
            var path = GetSavePath(slot);
            bool hasSave = Directory.Exists(GetSavePath(slot));
            Debug.Log(path);
            Debug.Log(hasSave ? "Save exists" : "Save does not exist");
            return hasSave;
        }

        public void SaveGame()
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
            Directory.Delete(path, true);
        }
        
        private void DeleteFile(string path)
        {
            if (File.Exists(path)) File.Delete(path);
            
        }
        public void LoadGame(int slot)
        {
            if (HasSave(slot))
                Load();
        }

        public void AddObjectToList(ISavedObject obj)
        {
            Debug.Log("Adding object to list " + obj.GetType().Name);
            _savedObjects.Add(obj);
        }

        private void Save()
        {
            var currentSaveDirectory = GetSavePath(currentSave);
            Directory.CreateDirectory(currentSaveDirectory);
            foreach (var savableObject in _savedObjects)
            {
                _sb.Clear();
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
                _sb.Clear();
                LoadObjectState(savableObject);
            }
        }

        private bool _hasDeleted = false;

        private void SaveObjectState(object obj)
        {
            _jsonObjects.Clear();
           
            FieldInfo[] fields = obj.GetType().GetFields()
                .Where(field => field.IsDefined(typeof(SaveVariableAttribute), true)).ToArray();
            //var fields = TypeCache.GetFieldsWithAttribute(typeof(SaveVariableAttribute)).ToArray();
            if (fields.Length == 0) fields = obj.GetType().GetFields();
            _hasDeleted = false;
            foreach (FieldInfo field in fields)
            {
                var saveType = GetSaveType(field);
                path = GetPath(GetFileType(saveType), obj);
                
                if(!_hasDeleted) DeleteFile(path);
                _hasDeleted = true;
                
                switch (saveType)
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
                        WriteOnFileXml();
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
            FieldInfo[] fields = obj.GetType().GetFields()
                .Where(field => field.IsDefined(typeof(SaveVariableAttribute), true)).ToArray();

            //var fields = TypeCache.GetFieldsWithAttribute(typeof(SaveVariableAttribute)).ToArray();
            _decryptedData = string.Empty;
            if (fields.Length == 0) fields = obj.GetType().GetFields();

            foreach (FieldInfo field in fields)
            {
                var saveType = GetSaveType(field);
                path = GetPath(GetFileType(saveType), obj);
                switch (saveType)
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

        private SaveTypes GetSaveType(FieldInfo field)
        {
            var saveType = _defaultSaveType;
            var attribute = (SaveVariableAttribute)field.GetCustomAttribute(typeof(SaveVariableAttribute), true);
            if (attribute != null)
                if (attribute.saveType != default)
                    saveType = attribute.saveType;

            return saveType;
        }

        private string GetFileType(SaveTypes saveType)
        {
            var fileType = saveType switch
            {
                SaveTypes.Binary => "bin",
                SaveTypes.Json => "json",
                SaveTypes.XML => "xml",
                SaveTypes.TXT => "txt",
                _ => throw new ArgumentOutOfRangeException()
            };

            if (_cryptSystem == CryptSystem.AES) fileType = "aes";
            return fileType;
        }

        private string GetPath(string fileType, object obj)
        {
            var currentSaveDirectory = GetSavePath(currentSave);
            return Path.Combine(currentSaveDirectory, $"{obj.GetType().Name}.{fileType}");
        }
        
        #endregion

        #region TXT

        private void SaveFormatTxt(FieldInfo field, object obj)
        {
            object value = field.GetValue(obj);
            
            if (value is IList list)
            {
                StringBuilder listValues = new StringBuilder();
                foreach (var item in list)
                {
                    var type = item.GetType();
                    if ((type.IsClass || type.IsValueType && !type.IsPrimitive))
                        listValues.Append(JsonConvert.SerializeObject(item,_settings)).Append(",");
                    else
                        listValues.Append(item).Append(",");
                }

                if (listValues.Length > 0)
                    listValues.Length--;
                WriteOnFile($"{field.Name}:[{listValues}];");
            }
            else if ((field.FieldType.IsClass || field.FieldType.IsValueType && !field.FieldType.IsPrimitive))
            {
                string jsonValue = JsonConvert.SerializeObject(value,_settings);
                WriteOnFile($"{field.Name}:{jsonValue};");
            }
            else
            {
                WriteOnFile($"{field.Name}:{value};");
            }

            Debug.Log($"Field {field.Name} has SaveVariableAttribute value: {value}");
        }

        private void LoadFormatTxt(FieldInfo field, object obj)
        {
            Debug.Log($"Field {obj} has SaveVariableAttribute value");
            string savedData = GetFromFile(field.Name);

            Debug.Log(savedData);

            if (savedData.StartsWith("[") && savedData.EndsWith("]"))
            {
                string listData = savedData.Substring(1, savedData.Length - 2);
                string[] items = listData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                IList list = (IList)Activator.CreateInstance(field.FieldType);
                Type elementType = field.FieldType.GetGenericArguments()[0];

                foreach (string item in items)
                {
                    object convertedItem;
                    Debug.Log(item);
                    if (elementType == typeof(Vector3) || elementType == typeof(Vector2) || elementType == typeof(Vector4))
                        convertedItem = JsonConvert.DeserializeObject(item, elementType);
                    else if ((elementType.IsClass || elementType.IsValueType && !elementType.IsPrimitive))
                        convertedItem = JsonConvert.DeserializeObject(item, elementType);
                    else
                        convertedItem = Convert.ChangeType(item, elementType);

                    list.Add(convertedItem);
                }

                field.SetValue(obj, list);
            }
            else if ((field.FieldType.IsClass || field.FieldType.IsValueType && !field.FieldType.IsPrimitive))
            {
                object value = JsonConvert.DeserializeObject(savedData, field.FieldType);
                field.SetValue(obj, value);
            }
            else
            {
                object value = Convert.ChangeType(savedData, field.FieldType);
                Debug.Log("Converting " + value);
                field.SetValue(obj, value);
            }

            Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
        }

        #endregion

        #region JSON

        private void SaveFormatJson(FieldInfo field, object obj)
        {
            object value = field.GetValue(obj);
            JsonObject wrapper = new JsonObject(field.Name, value);
            _jsonObjects.Add(wrapper);
            Debug.Log($"Field {field.Name} has SaveVariableAttribute value: {value}");
        }

        private void LoadFormatJson(FieldInfo field, object obj)
        {
            string json = ReadAndDecryptFile(false);
            List<JsonObject> jsonObjects = JsonConvert.DeserializeObject<List<JsonObject>>(json);
        
            foreach (var jsonObject in jsonObjects)
            {
                if (field.Name != jsonObject.Name) continue;

                object convertedValue = ConvertValue(jsonObject.Value, field.FieldType);
                field.SetValue(obj, convertedValue);
            }

            Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
        }

        private object ConvertValue(object value, Type targetType)
        {
            if (value == null) return null;
           
            if (targetType == typeof(int)) return Convert.ToInt32(value);
            if (targetType == typeof(float)) return Convert.ToSingle(value);
            if (targetType == typeof(bool)) return Convert.ToBoolean(value);
            if (targetType == typeof(string)) return value.ToString();

          
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
                return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, Formatting.Indented, _settings),
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
            
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, Formatting.Indented, _settings),
                targetType);
        }


        #endregion

        #region Binary

        private void LoadFormatBinary(FieldInfo field, object obj)
        {
            string json = ReadAndDecryptFile(true);
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
            string xmlContent = ReadAndDecryptFile(false);

            if (string.IsNullOrEmpty(xmlContent))
            {
                return;
            }
            try
            {
                using StringReader stringReader = new StringReader(xmlContent);
                XmlSerializer serializer = new XmlSerializer(typeof(List<JsonObject>));
                List<JsonObject> jsonObjects = (List<JsonObject>)serializer.Deserialize(stringReader);

                foreach (var jsonObject in jsonObjects)
                {
                    if (field.Name != jsonObject.Name) continue;

                    object convertedValue = ConvertValue(jsonObject.Value, field.FieldType);
                    field.SetValue(obj, convertedValue);
                }

                Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro ao desserializar o XML: {ex.Message}");
            }
        }

        #endregion

        #region GetMethods

        private void GetAllSavedObjects()
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (typeof(ISavedObject).IsAssignableFrom(type) && !typeof(ScriptableObject).IsAssignableFrom(type) &&
                    !typeof(MonoBehaviour).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
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
        

        private string GetFromFile(string fieldName)
        {
            if (string.IsNullOrEmpty(_decryptedData))
            {
                _decryptedData = ReadAndDecryptFile(false);
            }
            Debug.Log(_decryptedData);
            
            if(_cryptSystem == CryptSystem.AES)
            {
                _decryptedData = _decryptedData.Replace(";","\n");
            }
            return ExtractFieldValue(_decryptedData, fieldName);
        }
    
        
        /// <summary>
        /// Read the file and decrypt it if it is encrypted
        /// </summary>
        /// <param name="readBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
   
        private string ReadAndDecryptFile(bool readBytes)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("Arquivo não encontrado: " + path);
                return string.Empty;
            }

            switch (_cryptSystem)
            {
                case CryptSystem.None:
                    if (readBytes)
                    {
                        using FileStream fs = new FileStream(path, FileMode.Open);
                        using GZipStream gzip = new GZipStream(fs, CompressionMode.Decompress);
                        using StreamReader reader = new StreamReader(gzip);
                        return reader.ReadToEnd();
                    }
                    return File.ReadAllText(path);

                case CryptSystem.AES:
                    return DecryptFile(readBytes);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Read a file and decrypt it if it is encrypted
        /// </summary>
        /// <param name="readBytes"></param>
        /// <returns></returns>
        private string DecryptFile(bool readBytes)
        {
            try
            {
                using (FileStream fss = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                using (BinaryReader reader = new BinaryReader(fss))
                {
                    List<byte> decryptedBytes = new List<byte>();

                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        int dataSize = reader.ReadInt32();
                        byte[] encryptedData = reader.ReadBytes(dataSize);

                        if (encryptedData.Length != dataSize)
                        {
                            Debug.LogError("Erro ao ler o arquivo: dados incompletos.");
                            return string.Empty;
                        }

                        byte[] decryptedData = EncryptSystem.Instance.DecryptDataToBytes(encryptedData);
                        decryptedBytes.AddRange(decryptedData);
                    }

                    byte[] finalDecryptedBytes = decryptedBytes.ToArray();

                    if (readBytes)
                    {
                        string debugString = Encoding.UTF8.GetString(finalDecryptedBytes);
                        using MemoryStream ms = new MemoryStream(finalDecryptedBytes);
                        using GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress);
                        using StreamReader readerGzip = new StreamReader(gzip, Encoding.UTF8);

                        string json = readerGzip.ReadToEnd();
                        return json;
                    }
                    
                    return Encoding.UTF8.GetString(finalDecryptedBytes);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// For txt object extract the field value based on the filed name
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private string ExtractFieldValue(string data, string fieldName)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            using StringReader stringReader = new StringReader(data);
            string line;
            Debug.Log(data);
            Debug.Log("field name = " + fieldName);

            while ((line = stringReader.ReadLine()) != null)
            {
                if (line.StartsWith($"{fieldName}:"))
                {
                    string fieldValue = line.Substring(fieldName.Length + 1).Trim();
                    Debug.Log(fieldValue);
                    if (fieldValue.EndsWith(";"))
                    {
                        fieldValue = fieldValue.Substring(0, fieldValue.Length - 1);
                    }
                    Debug.Log("field value = " + fieldName + fieldValue);
                    return fieldValue;
                }
            }

            return null;
        }


        #endregion

        #region WriteMethods
        
        /// <summary>
        /// Write a line into the file for txt objects
        /// </summary>
        /// <param name="lineToAppend"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>

        private void WriteOnFile(string lineToAppend)
        {
            _sb.AppendLine(lineToAppend);

            using FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);

            switch (_cryptSystem)
            {
                case CryptSystem.None:
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine(lineToAppend); 
                    }
                    break;

                case CryptSystem.AES:
                    byte[] encryptedData = EncryptSystem.Instance.EncryptDataAes(lineToAppend);
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        writer.Write(encryptedData.Length);
                        writer.Write(encryptedData);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// Write json objects in a json file
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>

        private void WriteOnFileJson()
        {
            string json = JsonConvert.SerializeObject(_jsonObjects, Formatting.Indented, _settings);
            
            switch (_cryptSystem)
            {
                case CryptSystem.None:
                    File.WriteAllText(path, json);
                    break;

                case CryptSystem.AES:
                    FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                    byte[] encryptedData = EncryptSystem.Instance.EncryptDataAes(json);
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        writer.Write(encryptedData.Length);
                        writer.Write(encryptedData);
                    }
                    fs.Close();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// Write the json objects on xml file
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void WriteOnFileXml()
        {
            string xml;
            
            using (StringWriter stringWriter = new StringWriter())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<JsonObject>));
                serializer.Serialize(stringWriter, _jsonObjects);
                xml = stringWriter.ToString();
            }

            switch (_cryptSystem)
            {
                case CryptSystem.None:
                    File.WriteAllText(path, xml);
                    break;

                case CryptSystem.AES:
                    byte[] encryptedData = EncryptSystem.Instance.EncryptDataAes(xml);
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    using (BinaryWriter binaryWriter = new BinaryWriter(fs))
                    {
                        binaryWriter.Write(encryptedData.Length);
                        binaryWriter.Write(encryptedData);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// Write the jsonObjects list to a binary file
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>

        private void WriteOnFileJsonBinary()
        {
            string json = JsonConvert.SerializeObject(_jsonObjects, Formatting.None, _settings);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            switch (_cryptSystem)
            {
                case CryptSystem.None:
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    using (GZipStream gzip = new GZipStream(fs, CompressionMode.Compress))
                    using (BinaryWriter writer = new BinaryWriter(gzip))
                    {
                        writer.Write(jsonBytes); 
                    }
                    break;

                case CryptSystem.AES:
                    byte[] compressedData;
       
                    using (MemoryStream ms = new MemoryStream())
                    using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
                    {
                        gzip.Write(jsonBytes, 0, jsonBytes.Length);
                        gzip.Close();
                        compressedData = ms.ToArray();
                    }

                    byte[] encryptedData = EncryptSystem.Instance.EncryptDataAes(compressedData);

                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    using (BinaryWriter binaryWriter = new BinaryWriter(fs))
                    {
                        binaryWriter.Write(encryptedData.Length);
                        binaryWriter.Write(encryptedData);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        #endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SaveSystem)), CanEditMultipleObjects]
    public class SceneLoaderControllerEditor : Editor
    {
        private SaveSystem _data;

        private void OnEnable()
        {
            _data = (SaveSystem)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _data.path = $"{Application.persistentDataPath}/save_{_data.currentSave}";
            if (GUILayout.Button("Save"))
            {
                _data.SaveGame();
            }

            if (GUILayout.Button("Load"))
            {
                _data.LoadGame(0);
            }

            if (GUILayout.Button("Delete Save"))
            {
                _data.path = $"{Application.persistentDataPath}/save_{_data.currentSave}";
                _data.DeleteSave(0);
            }

        }
    }
#endif
}