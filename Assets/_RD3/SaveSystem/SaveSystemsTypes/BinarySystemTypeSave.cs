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

            switch (SaveSystem.Instance.CryptSystem)
            {
                case CryptSystem.None:
                    using (FileStream fs = new FileStream(SaveSystem.Instance.path, FileMode.Append))
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
            string json = SaveSystem.Instance.ReadAndDecryptFile(true);
            List<JsonObject> jsonObjects = JsonConvert.DeserializeObject<List<JsonObject>>(json);

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