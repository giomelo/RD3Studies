using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using _RD3._Universal._Scripts.Utilities;
using _RD3.SaveSystem.SaveSystemsTypes;
using Newtonsoft.Json;
using Refactor.Data.Variables;
using UnityEditor;
using UnityEngine;

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

        public List<Variable> variablesToSave = new List<Variable>();
        public string path;
        [SerializeField] private CryptSystem _cryptSystem;
        [SerializeField] private SaveTypes _defaultSaveType = SaveTypes.TXT;
        
        public CryptSystem CryptSystem => _cryptSystem;
        public int currentSave;
        
        #endregion

        #region Private Variables
        
        private List<ISavedObject> _savedObjects = new List<ISavedObject>();
        private bool _hasDeleted = false;
        
        #endregion
        
        
        #region Monobeheviour

        protected override void Awake()
        {
            base.Awake();
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
            if (HasSave(slot)) Load();
        }

        public void AddObjectToList(ISavedObject obj)
        {
            _savedObjects.Add(obj);
        }

        private void Save()
        {
            var currentSaveDirectory = GetSavePath(currentSave);
            Directory.CreateDirectory(currentSaveDirectory);
            foreach (var savableObject in _savedObjects)
            {
                SaveObjectState(savableObject);
            }
            path = GetPath(GetFileType(_defaultSaveType), "Variables");
            SaveSystemType saveSystemType;
            saveSystemType = _defaultSaveType switch
            {
                SaveTypes.Binary => new BinarySystemTypeSave(),
                SaveTypes.Json => new JsonSystemTypeSave(),
                SaveTypes.XML => new XmlSystemTypeSave(),
                SaveTypes.TXT => new TxtSystemTypeSave(),
                _ => throw new ArgumentOutOfRangeException()
            };
            foreach (var variable in variablesToSave)
                saveSystemType.SaveVariable(variable.name, variable.GetValue());
            
            
            saveSystemType.WriteOnFile();
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
                LoadObjectState(savableObject);
            }
            
            path = GetPath(GetFileType(_defaultSaveType), "Variables");
            SaveSystemType saveSystemType;
            saveSystemType = _defaultSaveType switch
            {
                SaveTypes.Binary => new BinarySystemTypeSave(),
                SaveTypes.Json => new JsonSystemTypeSave(),
                SaveTypes.XML => new XmlSystemTypeSave(),
                SaveTypes.TXT => new TxtSystemTypeSave(),
                _ => throw new ArgumentOutOfRangeException()
            };
            foreach (var variable in variablesToSave)
            {
                FieldInfo value = variable.GetFieldInfo();
                saveSystemType.Load(value, variable, variable.name);
            }
        }
        

        public void SaveObjectState(object obj)
        {
            var currentSaveDirectory = GetSavePath(currentSave);
            Directory.CreateDirectory(currentSaveDirectory);
            
            FieldInfo[] fields = obj.GetType().GetFields()
                .Where(field => field.IsDefined(typeof(SaveVariableAttribute), true)).ToArray();
            //var fields = TypeCache.GetFieldsWithAttribute(typeof(SaveVariableAttribute)).ToArray();
            if (fields.Length == 0) fields = obj.GetType().GetFields();
            _hasDeleted = false;
            
            if(fields.Length <= 0) return;
            
            SaveSystemType saveSystemType;
            var saveType = GetSaveType(fields[0]);
            path = GetPath(GetFileType(saveType), obj.GetType().Name);
            if(!_hasDeleted) DeleteFile(path);
            _hasDeleted = true;
            saveSystemType = saveType switch
            {
                SaveTypes.Binary => new BinarySystemTypeSave(),
                SaveTypes.Json => new JsonSystemTypeSave(),
                SaveTypes.XML => new XmlSystemTypeSave(),
                SaveTypes.TXT => new TxtSystemTypeSave(),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            foreach (FieldInfo field in fields)
            {
                saveSystemType.SaveFormat(field, obj);
                //saveSystemType.WriteOnFile();
            }
            saveSystemType.WriteOnFile();
        }

        public void LoadObjectState(object obj)
        {
            FieldInfo[] fields = obj.GetType().GetFields()
                .Where(field => field.IsDefined(typeof(SaveVariableAttribute), true)).ToArray();

            //var fields = TypeCache.GetFieldsWithAttribute(typeof(SaveVariableAttribute)).ToArray();
            if (fields.Length == 0) fields = obj.GetType().GetFields();
            
            if(fields.Length <= 0) return;
            
            SaveSystemType saveSystemType;
            var saveType = GetSaveType(fields[0]);
            
            path = GetPath(GetFileType(saveType), obj.GetType().Name);

            if (!File.Exists(path))
            {
                Debug.LogError("Arquivo não encontrado: " + path);
                return;
            }
            saveSystemType = saveType switch
            {
                SaveTypes.Binary => new BinarySystemTypeSave(),
                SaveTypes.Json => new JsonSystemTypeSave(),
                SaveTypes.XML => new XmlSystemTypeSave(),
                SaveTypes.TXT => new TxtSystemTypeSave(),
                _ => throw new ArgumentOutOfRangeException()
            };
       
            foreach (FieldInfo field in fields)
            {
                saveSystemType.Load(field, obj);
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

        private string GetPath(string fileType, string fileName)
        {
            var currentSaveDirectory = GetSavePath(currentSave);
            return Path.Combine(currentSaveDirectory, $"{fileName}.{fileType}");
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
        
        /// <summary>
        /// Read the file and decrypt it if it is encrypted
        /// </summary>
        /// <param name="readBytes"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
   
        public string ReadAndDecryptFile(bool readBytes)
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
                        using (FileStream fs = new FileStream(path, FileMode.Open))
                        using (BinaryReader reader = new BinaryReader(fs))
                        {
                            int length = reader.ReadInt32();
                            byte[] xmlBytes = reader.ReadBytes(length);
                            return Encoding.UTF8.GetString(xmlBytes);
                        }

                    }
                   
                    return File.ReadAllText(path);
                
                case CryptSystem.AES:
                    
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        int length = reader.ReadInt32();
                        byte[] encryptedData = reader.ReadBytes(length);
                        byte[] decryptedData = EncryptSystem.Instance.DecryptDataToBytes(encryptedData);
                        return Encoding.UTF8.GetString(decryptedData);
                    }

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
                     //   using GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress);
                        using StreamReader readerGzip = new StreamReader(ms, Encoding.UTF8);

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