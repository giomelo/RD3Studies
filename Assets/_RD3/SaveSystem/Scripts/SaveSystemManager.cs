using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using _RD3.SaveSystem.Scripts.SaveSystemsTypes;
using Refactor.Data.Variables;
using UnityEditor;
using UnityEngine;

namespace _RD3.SaveSystem.Scripts
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
    [XmlInclude(typeof(TestStruct[]))]
    [Serializable]
    public class JsonObject : IXmlSerializable
    {
        public string Name { get; set; }
        public object Value { get; set; }
        

        public JsonObject(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public JsonObject()
        {
        }
        
        public XmlSchema GetSchema() => null;
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            Name = reader.GetAttribute("Name");
            var type = Type.GetType(reader.GetAttribute("Type") ?? string.Empty);
            reader.ReadStartElement();
            if (type != null)
            {
                var serializer = new XmlSerializer(type);
                Value = serializer.Deserialize(reader);
            }

            reader.ReadEndElement();
        
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            WriteXml(writer);
        }
        
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            var assemblyQualifiedName = Value.GetType().AssemblyQualifiedName;
            if (assemblyQualifiedName != null) writer.WriteAttributeString("Type", assemblyQualifiedName);
            var serializer = new XmlSerializer(Value.GetType());
            serializer.Serialize(writer, Value);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return GetSchema();
        }
    }
    [Serializable]
    public enum SaveTypes
    {
        Binary,
        Json,
        XML,
        Txt
    }

    [Serializable]
    public enum CryptSystem
    {
        None,
        Aes,
    }

    public class SaveSystemManager : Singleton<SaveSystemManager>
    {
        #region Serialized Variables
        
        public List<Variable> variablesToSave = new List<Variable>();
        public string path;
        [SerializeField] private CryptSystem _cryptSystem;
        [SerializeField] private SaveTypes _defaultSaveType = SaveTypes.Txt;
        
        public CryptSystem CryptSystem => _cryptSystem;
        public int currentSave;
        
        #endregion

        #region Private Variables
        
        private List<ISavableObject> _savedObjects = new List<ISavableObject>();
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
        
        public void DeleteSave(int slot)
        {
            if (HasSave(slot)) DeleteSave(GetSavePath(slot));
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

        /// <summary>
        /// add objects do save save list
        /// </summary>
        /// <param name="obj"></param>
        public void AddObjectToList(ISavableObject obj)
        {
            _savedObjects.Add(obj);
        }
        
        /// <summary>
        /// loop trough all objects and save them
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>

        public void Save()
        {
            var currentSaveDirectory = GetSavePath(currentSave);
            Directory.CreateDirectory(currentSaveDirectory);
            foreach (var savableObject in _savedObjects)
                SaveObjectState(savableObject, savableObject.Name);
            
            // save variables
            path = GetPath(GetFileType(_defaultSaveType), "Variables");
            SavableSystemType saveSystemType;
            saveSystemType = _defaultSaveType switch
            {
                SaveTypes.Binary => new BinarySystemTypeSavable(),
                SaveTypes.Json => new JsonSystemTypeSavable(),
                SaveTypes.XML => new XmlSystemTypeSavable(),
                SaveTypes.Txt => new TxtSystemTypeSavable(),
                _ => throw new ArgumentOutOfRangeException()
            };
            foreach (var variable in variablesToSave)
                saveSystemType.SaveVariable(variable.name, variable.GetValue());
            
            
            saveSystemType.WriteOnFile();
        }

        private void Load()
        {
            if (!HasSave(currentSave))
            {
                Debug.LogError("No save found");
                return;
            }

            foreach (var savableObject in _savedObjects)
                LoadObjectState(savableObject, savableObject.Name);
            
            path = GetPath(GetFileType(_defaultSaveType), "Variables");
            SavableSystemType saveSystemType;
            saveSystemType = _defaultSaveType switch
            {
                SaveTypes.Binary => new BinarySystemTypeSavable(),
                SaveTypes.Json => new JsonSystemTypeSavable(),
                SaveTypes.XML => new XmlSystemTypeSavable(),
                SaveTypes.Txt => new TxtSystemTypeSavable(),
                _ => throw new ArgumentOutOfRangeException()
            };
            foreach (var variable in variablesToSave)
            {
                FieldInfo value = variable.GetFieldInfo();
                saveSystemType.Load(value, variable, variable.name);
            }
        }
        

        public void SaveObjectState(object obj, string fileName)
        {
            var currentSaveDirectory = GetSavePath(currentSave);
            Directory.CreateDirectory(currentSaveDirectory);
            
            FieldInfo[] fields = obj.GetType().GetFields()
                .Where(field => field.IsDefined(typeof(SaveVariableAttribute), true)).ToArray();
 
            if (fields.Length == 0) fields = obj.GetType().GetFields();
            _hasDeleted = false;
            
            if(fields.Length <= 0) return;
            
            SavableSystemType savableSystemType;
            var saveType = GetSaveType(fields[0]);
            path = GetPath(GetFileType(saveType), fileName);
            
            if(!_hasDeleted) DeleteFile(path);
            _hasDeleted = true;
            
            savableSystemType = saveType switch
            {
                SaveTypes.Binary => new BinarySystemTypeSavable(),
                SaveTypes.Json => new JsonSystemTypeSavable(),
                SaveTypes.XML => new XmlSystemTypeSavable(),
                SaveTypes.Txt => new TxtSystemTypeSavable(),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            foreach (FieldInfo field in fields)
                savableSystemType.SaveFormat(field, obj);
            
            savableSystemType.WriteOnFile();
        }

        public void LoadObjectState(object obj, string fileName)
        {
            FieldInfo[] fields = obj.GetType().GetFields()
                .Where(field => field.IsDefined(typeof(SaveVariableAttribute), true)).ToArray();
            
            if (fields.Length == 0) fields = obj.GetType().GetFields();
            
            if(fields.Length <= 0) return;
            
            SavableSystemType savableSystemType;
            var saveType = GetSaveType(fields[0]);
            
            path = GetPath(GetFileType(saveType), fileName);

            if (!File.Exists(path))
            {
                Debug.LogError("File not found: " + path);
                return;
            }
            savableSystemType = saveType switch
            {
                SaveTypes.Binary => new BinarySystemTypeSavable(),
                SaveTypes.Json => new JsonSystemTypeSavable(),
                SaveTypes.XML => new XmlSystemTypeSavable(),
                SaveTypes.Txt => new TxtSystemTypeSavable(),
                _ => throw new ArgumentOutOfRangeException()
            };
       
            foreach (FieldInfo field in fields)
                savableSystemType.Load(field, obj);
            
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
                SaveTypes.Txt => "txt",
                _ => throw new ArgumentOutOfRangeException()
            };

            if (_cryptSystem == CryptSystem.Aes) fileType = "aes";
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
           ScriptableObject[] allObjects = Resources.FindObjectsOfTypeAll<ScriptableObject>();
       
           foreach (ScriptableObject obj in allObjects)
           {
               Debug.Log("Found object: " + obj.name);
               if (obj is ISavableObject savedObject)
                   AddObjectToList(savedObject);
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
                Debug.LogError("File not found: " + path);
                return string.Empty;
            }

            switch (_cryptSystem)
            {
                case CryptSystem.None:
                    if (readBytes)
                    {
                        using (FileStream fs = new FileStream(path, FileMode.Open))
                        using (var gzip = new GZipStream(fs, CompressionMode.Decompress))
                        {
                            using (BinaryReader reader = new BinaryReader(gzip))
                            {
                                int length = reader.ReadInt32();
                                byte[] xmlBytes = reader.ReadBytes(length);
                                return Encoding.UTF8.GetString(xmlBytes);
                            }
                        }
                    }
                   
                    return File.ReadAllText(path);
                
                case CryptSystem.Aes:
                    
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        int length = reader.ReadInt32();
                        byte[] iv = reader.ReadBytes(16);
                        byte[] encryptedData = reader.ReadBytes(length);
                        byte[] decryptedData = EncryptSystem.Instance.DecryptDataToBytes(encryptedData, iv);
                        return Encoding.UTF8.GetString(decryptedData);
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        
        #endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SaveSystemManager)), CanEditMultipleObjects]
    public class SceneLoaderControllerEditor : Editor
    {
        private SaveSystemManager _data;

        private void OnEnable()
        {
            _data = (SaveSystemManager)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _data.path = $"{Application.persistentDataPath}/save_{_data.currentSave}";
            if (GUILayout.Button("Save"))
            {
                _data.Save();
            }

            if (GUILayout.Button("Load"))
            {
                _data.LoadGame(_data.currentSave);
            }

            if (GUILayout.Button("Delete Save"))
            {
                _data.path = $"{Application.persistentDataPath}/save_{_data.currentSave}";
                _data.DeleteSave(_data.currentSave);
            }

        }
    }
#endif
}