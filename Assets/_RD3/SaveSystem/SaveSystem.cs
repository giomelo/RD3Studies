using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
        [SerializeField] private List<Variable> variablesToSave = new List<Variable>();
        public string path;

        [SerializeField] private CryptSystem _cryptSystem;
        [SerializeField] private SaveTypes _defaultSaveType = SaveTypes.TXT;

        StringBuilder sb = new StringBuilder();
        List<JsonObject> jsonObjects = new List<JsonObject>();
        public int currentSave;
        private List<ISavedObject> _savedObjects = new List<ISavedObject>();


        #region Monobeheviour

        private void Start()
        {
            path = $"{Application.persistentDataPath}/save_{0}.save";
            //  LoadGame(0);

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

        private bool _hasDeleted = false;

        private void SaveObjectState(object obj)
        {
            jsonObjects.Clear();
           
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
            FieldInfo[] fields = obj.GetType().GetFields()
                .Where(field => field.IsDefined(typeof(SaveVariableAttribute), true)).ToArray();

            //var fields = TypeCache.GetFieldsWithAttribute(typeof(SaveVariableAttribute)).ToArray();
            decryptedData = string.Empty;
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
                    listValues.Append(item).Append(",");
                }

                if (listValues.Length > 0)
                    listValues.Length--;
                WriteOnFile($"{field.Name}:[{listValues}]");
            }
            else
                WriteOnFile($"{field.Name}:{value}");

            Debug.Log($"Field {field.Name} has SaveVariableAttribute value: {value}");
        }
        private void LoadFormatTxt(FieldInfo field, object obj)
        {
            Debug.Log($"Field {obj} has SaveVariableAttribute value");
            string savedData = GetFromFile(field.Name);

            Debug.Log(savedData);

            if (savedData.StartsWith("[") && savedData.EndsWith("]")) // Caso seja uma lista
            {
                string listData = savedData.Substring(1, savedData.Length - 2); // Remove os colchetes
                string[] items = Regex.Matches(listData, @"\((.*?)\)")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ToArray();

                IList list = (IList)Activator.CreateInstance(field.FieldType);
                Type elementType = field.FieldType.GetGenericArguments()[0]; // Tipo da lista (ex: Vector3, int, float)

                foreach (string item in items)
                {
                    object convertedItem;

                    if (elementType == typeof(Vector3))
                    {
                        convertedItem = ParseVector3(item);
                    }
                    else if (elementType == typeof(Vector2))
                    {
                        convertedItem = ParseVector2(item);
                    }
                    else if (elementType == typeof(Vector4))
                    {
                        convertedItem = ParseVector4(item);
                    }
                    else
                    {
                        convertedItem = Convert.ChangeType(item, elementType, CultureInfo.InvariantCulture);
                    }

                    list.Add(convertedItem);
                }

                field.SetValue(obj, list);
            }
            else if (field.FieldType == typeof(Vector3)) field.SetValue(obj, ParseVector3(savedData.Trim('(', ')')));
            
            else if (field.FieldType == typeof(Vector2)) field.SetValue(obj, ParseVector2(savedData.Trim('(', ')')));
            
            else if (field.FieldType == typeof(Vector4)) field.SetValue(obj, ParseVector4(savedData.Trim('(', ')')));
            
            else
            {
                object value = Convert.ChangeType(savedData, field.FieldType, CultureInfo.InvariantCulture);
                field.SetValue(obj, value);
            }

            Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
        }
        
        private Vector3 ParseVector3(string input)
        {
            string[] components = input.Split(',');
            if (components.Length == 3)
            {
                float x = float.Parse(components[0], CultureInfo.InvariantCulture);
                float y = float.Parse(components[1], CultureInfo.InvariantCulture);
                float z = float.Parse(components[2], CultureInfo.InvariantCulture);
                return new Vector3(x, y, z);
            }

            throw new FormatException("Formato inválido para Vector3: " + input);
        }

        private Vector2 ParseVector2(string input)
        {
            string[] components = input.Split(',');
            if (components.Length == 2)
            {
                float x = float.Parse(components[0], CultureInfo.InvariantCulture);
                float y = float.Parse(components[1], CultureInfo.InvariantCulture);
                return new Vector2(x, y);
            }

            throw new FormatException("Formato inválido para Vector2: " + input);
        }

        private Vector4 ParseVector4(string input)
        {
            string[] components = input.Split(',');
            if (components.Length == 4)
            {
                float x = float.Parse(components[0], CultureInfo.InvariantCulture);
                float y = float.Parse(components[1], CultureInfo.InvariantCulture);
                float z = float.Parse(components[2], CultureInfo.InvariantCulture);
                float w = float.Parse(components[3], CultureInfo.InvariantCulture);
                return new Vector4(x, y, z, w);
            }

            throw new FormatException("Formato inválido para Vector4: " + input);
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

            public JsonObject()
            {
            }
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
                if (field.Name != jsonObject.Name) continue;

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
                return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, Formatting.Indented, settings),
                    targetType);

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
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(value, Formatting.Indented, settings),
                targetType);
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

        string decryptedData = string.Empty;

        private string GetFromFile(string fieldName)
        {
            if (string.IsNullOrEmpty(decryptedData))
            {
                decryptedData = ReadAndDecryptFile();
            }

            return ExtractFieldValue(decryptedData, fieldName);
        }

        /// <summary>
        /// Lê e descriptografa o arquivo, dependendo do método de criptografia utilizado.
        /// </summary>
        private string ReadAndDecryptFile()
        {
            if (!File.Exists(path))
            {
                Debug.LogError("Arquivo não encontrado: " + path);
                return string.Empty;
            }

            switch (_cryptSystem)
            {
                case CryptSystem.None:
                    return File.ReadAllText(path);

                case CryptSystem.AES:
                    return ReadEncryptedFile();

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Lê um arquivo criptografado e retorna o conteúdo descriptografado.
        /// </summary>
        private string ReadEncryptedFile()
        {
            List<string> decryptedList = new List<string>();

            try
            {
                using (FileStream fss = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                using (BinaryReader reader = new BinaryReader(fss))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        int dataSize = reader.ReadInt32(); 
                        byte[] encryptedData = reader.ReadBytes(dataSize); 

                        if (encryptedData.Length != dataSize)
                        {
                            Debug.LogError("Erro ao ler o arquivo: dados incompletos.");
                            return string.Empty;
                        }

                        string decryptedText = EncryptSystem.Instance.DecryptData(encryptedData);
                        decryptedList.Add(decryptedText);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Erro ao ler ou descriptografar o arquivo: " + ex.Message);
                return string.Empty;
            }

            return string.Join("\n", decryptedList);
        }

        /// <summary>
        /// Procura um campo específico dentro do texto descriptografado.
        /// </summary>
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
                    return line.Substring(fieldName.Length + 1).Trim();
                }
            }

            return null;
        }


        #endregion

        #region WriteMethods

        private void WriteOnFile(string lineToAppend)
        {
            sb.AppendLine(lineToAppend);

            using FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.None);

            switch (_cryptSystem)
            {
                case CryptSystem.None:
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine(lineToAppend); // Agora escreve apenas a nova linha
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

        private void WriteOnFileJson()
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented, settings);
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

        #endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SaveSystem)), CanEditMultipleObjects]
    public class SceneLoaderControllerEditor : Editor
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
                data.SaveGame();
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