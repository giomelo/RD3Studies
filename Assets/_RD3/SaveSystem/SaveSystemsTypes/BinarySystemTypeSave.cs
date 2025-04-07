using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace _RD3.SaveSystem.SaveSystemsTypes
{
    public class BinarySystemTypeSave : SaveSystemType
    {
        
        public override void WriteOnFile()
        {
            string json = JsonConvert.SerializeObject(JsonObjects, Formatting.None, Settings);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            switch (SaveSystemManager.Instance.CryptSystem)
            {
                case CryptSystem.None:
                    using (FileStream fs = new FileStream(SaveSystemManager.Instance.path, FileMode.Create)) 
                    using (var gzip = new GZipStream(fs, CompressionMode.Compress))
                    {
                        using (BinaryWriter writer = new BinaryWriter(gzip))
                        {
                            writer.Write(jsonBytes.Length);
                            writer.Write(jsonBytes);
                        }   
                    }
                    break;

                case CryptSystem.AES:
                    var iv = EncryptSystem.Instance.GenerateRandomIV();
                    byte[] encryptedData = EncryptSystem.Instance.EncryptDataAes(jsonBytes,iv);

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
            string json = SaveSystemManager.Instance.ReadAndDecryptFile(true);
            Debug.Log(json);
            List<JsonObject> jsonObjects = JsonConvert.DeserializeObject<List<JsonObject>>(json, Settings);
            var stringToCompare = string.IsNullOrEmpty(variableName) ? field.Name : variableName; 

            foreach (var jsonObject in jsonObjects)
            {
                if (stringToCompare != jsonObject.Name) continue;
                
                object convertedValue = ConvertValue(jsonObject.Value, field.FieldType);
                field.SetValue(obj, convertedValue);
            }

            Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
        }
    }
}