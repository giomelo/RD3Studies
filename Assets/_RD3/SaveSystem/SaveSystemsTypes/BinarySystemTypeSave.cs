using System;
using System.Collections.Generic;
using System.IO;
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

            switch (SaveSystem.Instance.CryptSystem)
            {
                case CryptSystem.None:
                    using (FileStream fs = new FileStream(SaveSystem.Instance.path, FileMode.Create)) 
                    //using (GZipStream gzip = new GZipStream(fs, CompressionMode.Compress))
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                       // writer.Write(jsonBytes.Length);
                        writer.Write(jsonBytes);
                    }
                    break;

                case CryptSystem.AES:
                    byte[] encryptedData = EncryptSystem.Instance.EncryptDataAes(jsonBytes);

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

        public override void Load(FieldInfo field, object obj, string variableName = null)
        {
            string json = SaveSystem.Instance.ReadAndDecryptFile(true);
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