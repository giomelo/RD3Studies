using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace _RD3.SaveSystem.Scripts.SaveSystemsTypes
{
    public class JsonSystemTypeSavable : SavableSystemType
    {
        public override void WriteOnFile()
        {
            string json = JsonConvert.SerializeObject(JsonObjects, Formatting.Indented, Settings);
            
            switch (SaveSystemManager.Instance.CryptSystem)
            {
                case CryptSystem.None:
                    File.WriteAllText(SaveSystemManager.Instance.path, json);
                    break;

                case CryptSystem.Aes:

                    var iv = EncryptSystem.Instance.GenerateRandomIV();
                    byte[] encryptedData = EncryptSystem.Instance.EncryptDataAes(json, iv);
                   
                    using (FileStream fs = new FileStream(SaveSystemManager.Instance.path, FileMode.Create))
                    using (BinaryWriter binaryWriter = new BinaryWriter(fs))
                    {
                        binaryWriter.Write(encryptedData.Length); 
                        binaryWriter.Write(iv); 
                        binaryWriter.Write(encryptedData);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Load(FieldInfo field, object obj, string variableName = null)
        {
            string json = SaveSystemManager.Instance.ReadAndDecryptFile(false);
            List<JsonObject> jsonObjects = JsonConvert.DeserializeObject<List<JsonObject>>(json,Settings);
            var stringToCompare = string.IsNullOrEmpty(variableName) ? field.Name : variableName; 
            foreach (var jsonObject in jsonObjects)
            {
                if (stringToCompare != jsonObject.Name) continue;

                object convertedValue = ConvertValue(jsonObject.Value, field.FieldType);
                Debug.Log("CONVERTED VALUE "+ convertedValue);
                Debug.Log(jsonObject.Value);
                Debug.Log(field.FieldType);
                field.SetValue(obj, convertedValue);
            }

            Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
        }
    }
}