using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace _RD3.SaveSystem.SaveSystemsTypes
{
    public class JsonSystemTypeSave : SaveSystemType
    {
        public override void WriteOnFile()
        {
            string json = JsonConvert.SerializeObject(JsonObjects, Formatting.Indented, Settings);
            
            switch (SaveSystem.Instance.CryptSystem)
            {
                case CryptSystem.None:
                    File.WriteAllText(SaveSystem.Instance.path, json);
                    break;

                case CryptSystem.AES:
                    FileStream fs = new FileStream(SaveSystem.Instance.path, FileMode.Append, FileAccess.Write, FileShare.None);
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

        public override void Load(FieldInfo field, object obj)
        {
            string json = SaveSystem.Instance.ReadAndDecryptFile(false);
            List<JsonObject> jsonObjects = JsonConvert.DeserializeObject<List<JsonObject>>(json,Settings);
        
            foreach (var jsonObject in jsonObjects)
            {
                if (field.Name != jsonObject.Name) continue;

                object convertedValue = ConvertValue(jsonObject.Value, field.FieldType);
                field.SetValue(obj, convertedValue);
            }

            Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
        }
    }
}