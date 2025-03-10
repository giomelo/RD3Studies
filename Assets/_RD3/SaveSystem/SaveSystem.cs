using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using _RD3._Universal._Scripts.Utilities;
using Refactor.Data.Variables;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace _RD3.SaveSystem
{
    [Serializable]
    public enum SaveType
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
        [SerializeField]private SaveType _saveType;

        [SerializeField]private int _currentSave;
        
        #region Monobeheviour

        private void Start()
        {
            path = $"{Application.persistentDataPath}/save_{0}.save";
            if(saveKey=="")
                saveKey = GenerateRandomKey();
            //  LoadGame(0);
            
            GetAllSavedObjects();
        }

        #endregion
        
        #region Encryption

        private static string GenerateRandomKey()
        {
            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }
        private byte[] EncryptData(string plainText, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16]; // You should use a different IV each time for security

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return msEncrypt.ToArray();
                }
            }
        }
        private string DecryptData(byte[] encryptedData, string key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = new byte[16]; // You should use a different IV each time for security

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
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
            Save(_currentSave);
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
                Load(path);
        }

        private void SaveObjectState(object obj)
        {
            if (obj == null)
            {
                Debug.LogError("The object passed to SaveObjectState is null.");
                return;
            }

            FieldInfo[] fields = obj.GetType().GetFields().Where(field => field.IsDefined(typeof(SaveVariableAttribute), true)).ToArray();
            foreach (FieldInfo field in fields)
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
                    SerializeToJson($"{field.Name}:[{listValues}]");
                }
                else
                {
                    SerializeToJson($"{field.Name}:{value}");
                }
                Debug.Log($"Field {field.Name} has SaveVariableAttribute value: {value}");
            }
        }

        private void LoadObjectState(object obj)
        {
            if (obj == null)
            {
                Debug.LogError("The object passed to LoadObjectState is null.");
                return;
            }

            FieldInfo[] fields = obj.GetType().GetFields().Where(field => field.IsDefined(typeof(SaveVariableAttribute), true)).ToArray();
            foreach (FieldInfo field in fields)
            {
                string savedData = DeserializeFromJson(field.Name);
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
                else
                {
                    object value = Convert.ChangeType(savedData, field.FieldType);
                    field.SetValue(obj, value);
                }
                Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
            }
        }

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
        private void Save(int slot)
        {
            var currentSaveDirectory = GetSavePath(slot);
            Directory.CreateDirectory(currentSaveDirectory);
        
            foreach (var savableObject in _savedObjects)
            {
                sb.Clear();
                path = Path.Combine(currentSaveDirectory, $"{savableObject.GetType().Name}.save");
                SaveObjectState(savableObject);
            }
        }
        private void Load(string filePath)
        {
            var currentSaveDirectory = GetSavePath(_currentSave);
            if (!Directory.Exists(currentSaveDirectory))
            {
                Debug.LogError("No save found at " + currentSaveDirectory);
                return;
            }

            foreach (var savableObject in _savedObjects)
            {
                sb.Clear();
                path = Path.Combine(currentSaveDirectory, $"{savableObject.GetType().Name}.save");
                LoadObjectState(savableObject);
            }
        }
    
    
        
        
        StringBuilder sb = new StringBuilder(); 
        private void SerializeToJson(string lineToAppend)
        {
            
          //  string[] data = new string[variablesToSave.Count];

            /*for(int i = 0; i < data.Length; i ++)
            {    
                data[i] = variablesToSave[i].ToJson();
                sb.AppendLine(data[i]);
            }*/
            sb.AppendLine(lineToAppend);
            using FileStream fs = new FileStream(path, FileMode.Create);
            //using BinaryWriter writer = new BinaryWriter(fs);
            using StreamWriter writer = new StreamWriter(fs);
            //  byte[] encryptedData = EncryptData(SerializeToJson(), saveKey);
            writer.Write(sb.ToString());
        }
        private string DeserializeFromJson(string fieldName)
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
            data.path = $"{Application.persistentDataPath}/save_{0}.save";
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
                data.path = $"{Application.persistentDataPath}/save_{0}.save";
                data.DeleteSave(0);
            }
            
        }
    }
#endif
}