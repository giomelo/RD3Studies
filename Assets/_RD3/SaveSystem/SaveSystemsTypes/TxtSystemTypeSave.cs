using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace _RD3.SaveSystem.SaveSystemsTypes
{
    public class TxtSystemTypeSave : SaveSystemType
    {
        private StringBuilder _sb = new StringBuilder();
        /// <summary>
        /// Write a line into the file for txt objects
        /// </summary>
        /// <param name="lineToAppend"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>

        public override void WriteOnFile()
        {
            using FileStream fs = new FileStream(SaveSystem.Instance.path, FileMode.Append, FileAccess.Write, FileShare.None);
        
            switch (SaveSystem.Instance.CryptSystem)
            {
                case CryptSystem.None:
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine(_sb.ToString()); 
                    }
                    break;

                case CryptSystem.AES:
                    byte[] encryptedData = EncryptSystem.Instance.EncryptDataAes(_sb.ToString());
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        writer.Write(encryptedData.Length);
                        writer.Write(encryptedData);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Load(FieldInfo field, object obj)
        {
            Debug.Log($"Field {obj} has SaveVariableAttribute value");
            string savedData = GetFromFile(field.Name);

            Debug.Log(savedData);

            if (savedData.StartsWith("[") && savedData.EndsWith("]"))
            {
                string listData = savedData.Substring(1, savedData.Length - 2);
                string[] items = listData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                IList list = (IList)Activator.CreateInstance(field.FieldType);
                Type elementType = field.FieldType.GetGenericArguments()[0];

                foreach (string item in items)
                {
                    object convertedItem;
                    Debug.Log(item);
                    if (elementType == typeof(Vector3) || elementType == typeof(Vector2) || elementType == typeof(Vector4))
                        convertedItem = JsonConvert.DeserializeObject(item, elementType);
                    else if ((elementType.IsClass || elementType.IsValueType && !elementType.IsPrimitive))
                        convertedItem = JsonConvert.DeserializeObject(item, elementType);
                    else
                        convertedItem = Convert.ChangeType(item, elementType);

                    list.Add(convertedItem);
                }

                field.SetValue(obj, list);
            }
            else if ((field.FieldType.IsClass || field.FieldType.IsValueType && !field.FieldType.IsPrimitive))
            {
                object value = JsonConvert.DeserializeObject(savedData, field.FieldType);
                field.SetValue(obj, value);
            }
            else
            {
                object value = Convert.ChangeType(savedData, field.FieldType);
                Debug.Log("Converting " + value);
                field.SetValue(obj, value);
            }

            Debug.Log($"Field {field.Name} loaded with value: {field.GetValue(obj)}");
        }
        
        #region TXT

        public override void SaveFormat(FieldInfo field, object obj)
        {
            object value = field.GetValue(obj);
            if (value is IList list)
            {
                StringBuilder listValues = new StringBuilder();
                foreach (var item in list)
                {
                    var type = item.GetType();
                    if ((type.IsClass || type.IsValueType && !type.IsPrimitive))
                        listValues.Append(JsonConvert.SerializeObject(item,Settings)).Append(",");
                    else
                        listValues.Append(item).Append(",");
                }

                if (listValues.Length > 0)
                    listValues.Length--;
                
                _sb.AppendLine($"{field.Name}:[{listValues}];");
                //WriteOnFile($"{field.Name}:[{listValues}];");
            }
            else if ((field.FieldType.IsClass || field.FieldType.IsValueType && !field.FieldType.IsPrimitive))
            {
                string jsonValue = JsonConvert.SerializeObject(value,Settings);
                _sb.AppendLine(($"{field.Name}:{jsonValue};"));
             //   WriteOnFile($"{field.Name}:{jsonValue};");
            }
            else
            {
                _sb.AppendLine(($"{field.Name}:{value};"));
             //   WriteOnFile($"{field.Name}:{value};");
            }

            Debug.Log($"Field {field.Name} has SaveVariableAttribute value: {value}");
        }
        private string GetFromFile(string fieldName)
        {
            if (string.IsNullOrEmpty(DecryptedData))
            {
                DecryptedData = SaveSystem.Instance.ReadAndDecryptFile(false);
            }
            Debug.Log(DecryptedData);
            
            if(SaveSystem.Instance.CryptSystem == CryptSystem.AES)
            {
                DecryptedData = DecryptedData.Replace(";","\n");
            }
            return ExtractFieldValue(DecryptedData, fieldName);
        }
        /// <summary>
        /// For txt object extract the field value based on the filed name
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private string ExtractFieldValue(string data, string fieldName)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            using StringReader stringReader = new StringReader(data);
            string line;
            Debug.Log(data);
            Debug.Log("field name = " + fieldName);

            while ((line = stringReader.ReadLine()) != null)
            {
                if (line.StartsWith($"{fieldName}:"))
                {
                    string fieldValue = line.Substring(fieldName.Length + 1).Trim();
                    Debug.Log(fieldValue);
                    if (fieldValue.EndsWith(";"))
                    {
                        fieldValue = fieldValue.Substring(0, fieldValue.Length - 1);
                    }
                    Debug.Log("field value = " + fieldName + fieldValue);
                    return fieldValue;
                }
            }

            return null;
        }

        #endregion
    }
}