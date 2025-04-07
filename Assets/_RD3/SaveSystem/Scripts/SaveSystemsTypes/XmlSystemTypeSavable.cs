using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace _RD3.SaveSystem.Scripts.SaveSystemsTypes
{
    public class XmlSystemTypeSavable : SavableSystemType
    {
        public override void WriteOnFile()
        {
            string xml;
    
            using (StringWriter stringWriter = new StringWriter())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<JsonObject>));
                serializer.Serialize(stringWriter, JsonObjects);
                xml = stringWriter.ToString();
            }

            switch (SaveSystemManager.Instance.CryptSystem)
            {
                case CryptSystem.None:
                    File.WriteAllText(SaveSystemManager.Instance.path, xml);
                    break;

                case CryptSystem.Aes:
                    var iv = EncryptSystem.Instance.GenerateRandomIV();
                    byte[] xmlBytes = Encoding.UTF8.GetBytes(xml);
                    byte[] encryptedData = EncryptSystem.Instance.EncryptDataAes(xmlBytes,iv);

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
            string xmlContent = SaveSystemManager.Instance.ReadAndDecryptFile(false);

            if (string.IsNullOrEmpty(xmlContent))
            {
                Debug.LogError("File is null.");
                return;
            }

            try
            {
                using StringReader stringReader = new StringReader(xmlContent);
                XmlSerializer serializer = new XmlSerializer(typeof(List<JsonObject>));
                List<JsonObject> jsonObjects = (List<JsonObject>)serializer.Deserialize(stringReader);
                var stringToCompare = string.IsNullOrEmpty(variableName) ? field.Name : variableName; 

                foreach (var jsonObject in jsonObjects)
                {
                    if (stringToCompare != jsonObject.Name) continue;

                    object convertedValue = ConvertValue(jsonObject.Value, field.FieldType);
                    field.SetValue(obj, convertedValue);
                }

                Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error: {ex.Message}");
            }
        }


    }
}