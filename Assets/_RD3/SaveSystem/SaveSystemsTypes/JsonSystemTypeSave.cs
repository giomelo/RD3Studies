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
                    byte[] encryptedData = EncryptSystem.Instance.EncryptDataAes(json);

                    using (FileStream fs = new FileStream(SaveSystem.Instance.path, FileMode.Create))
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