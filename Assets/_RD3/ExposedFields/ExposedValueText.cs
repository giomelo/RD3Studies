using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace _RD3.ExposedFileds
{
    public struct FieldAndObject
    {
        public FieldInfo FieldInfo;
        public object Obj;

        public FieldAndObject(FieldInfo field, object obj)
        {
            FieldInfo = field;
            Obj = obj;
        }
    }
    
    #if UNITY_EDITOR
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ExposedValueText : MonoBehaviour
    {
        private TextMeshProUGUI _textField;
        [TextArea] public string textValue;
        public ExposedValueSelector[] exposedValues;
        private Dictionary<string, FieldAndObject> _fieldNameDictionary = new Dictionary<string, FieldAndObject>();
        private List<string> _instancedValues;

        private void Awake()
        {
            _textField = GetComponent<TextMeshProUGUI>();
            CreateDictionary();
        }

        private void Start()
        {
            _textField.text = GetFormattedString();
        }

        private string GetFormattedString()
        {
            _instancedValues = new List<string>();

            foreach (var t in exposedValues)
            {
                var field = _fieldNameDictionary[t.fieldName];
                string value = GetValue(field.FieldInfo, field.Obj);

                _instancedValues.Add(value);
            }

            return string.Format(textValue, _instancedValues.ToArray());
        }

        private string GetValue(FieldInfo field, object reference)
        {
            object obj;
            string value = "N/A";

            obj = field.GetValue(reference);
            if (obj != null) value = obj.ToString();

            return value;
        }



        private void CreateDictionary()
        {
            _fieldNameDictionary = new Dictionary<string, FieldAndObject>();

            var members = TypeCache.GetTypesWithAttribute<ExposedObjectAttribute>();

            foreach (var member in members)
            {
                var fields = member.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                              BindingFlags.Static);
                foreach (var field in fields)
                {
                    var attribute = field.GetCustomAttribute<ExposedFieldAttribute>();
                    if (attribute == null) continue;
                    FieldAndObject fieldAndObject = new FieldAndObject(field, Activator.CreateInstance(member));
                    _fieldNameDictionary.Add(attribute.DisplayName, fieldAndObject);
                }

            }
        }
    }
    
    #endif
}