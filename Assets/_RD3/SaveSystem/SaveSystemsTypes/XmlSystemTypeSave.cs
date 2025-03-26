using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace _RD3.SaveSystem.SaveSystemsTypes
{
    public class XmlSystemTypeSave : SaveSystemType
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

            byte[] xmlBytes = Encoding.UTF8.GetBytes(xml);

            switch (SaveSystem.Instance.CryptSystem)
            {
                case CryptSystem.None:
                    using (FileStream fs = new FileStream(SaveSystem.Instance.path, FileMode.Create))
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        writer.Write(xmlBytes.Length); // Armazena o tamanho do XML
                        writer.Write(xmlBytes);
                    }
                    break;

                case CryptSystem.AES:
                    byte[] encryptedData = EncryptSystem.Instance.EncryptDataAes(xmlBytes);

                    using (FileStream fs = new FileStream(SaveSystem.Instance.path, FileMode.Create))
                    using (BinaryWriter binaryWriter = new BinaryWriter(fs))
                    {
                        binaryWriter.Write(encryptedData.Length); // Armazena o tamanho dos dados criptografados
                        binaryWriter.Write(encryptedData);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Load(FieldInfo field, object obj)
        {
            string xmlContent = SaveSystem.Instance.ReadAndDecryptFile(true); // Agora sempre lê corretamente os bytes

            if (string.IsNullOrEmpty(xmlContent))
            {
                Debug.LogError("Arquivo XML está vazio ou corrompido.");
                return;
            }

            try
            {
                using StringReader stringReader = new StringReader(xmlContent);
                XmlSerializer serializer = new XmlSerializer(typeof(List<JsonObject>));
                List<JsonObject> jsonObjects = (List<JsonObject>)serializer.Deserialize(stringReader);

                foreach (var jsonObject in jsonObjects)
                {
                    if (field.Name != jsonObject.Name) continue;

                    object convertedValue = ConvertValue(jsonObject.Value, field.FieldType);
                    field.SetValue(obj, convertedValue);
                }

                Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro ao desserializar XML: {ex.Message}");
            }
        }

    }
}