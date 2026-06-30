using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ToolsStudy.Editor
{
    [CustomPropertyDrawer(typeof(ExposedValueSelector))]
    public class ExposedValueSelectorPropertyDrawer : PropertyDrawer
    {
        private List<string> _fields;
        private List<string> _fieldNames = new List<string>();
        private bool _gotFields;
        private int _index;


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if(!_gotFields) GetFields();

            var fieldNameProperty = property.FindPropertyRelative("fieldName");

            _index = GetFieldName(fieldNameProperty.stringValue);
            _index = EditorGUI.Popup(position, _index, _fieldNames.ToArray());
            fieldNameProperty.stringValue = _fields[_index];
        }

        private void GetFields()
        {
            _fields = new List<string>();
            _fieldNames = new List<string>();
            var members = TypeCache.GetFieldsWithAttribute<ExposedFieldAttribute>()
                .Where(member => member.GetCustomAttribute<ExposedFieldAttribute>() != null);

            foreach (var member in members)
            {
                var attribute = member.GetCustomAttribute<ExposedFieldAttribute>();
                _fields.Add(attribute.DisplayName);
                _fieldNames.Add($"{member.ReflectedType}/{attribute.DisplayName}");
            }

            _gotFields = true;
        }

        private int GetFieldName(string value)
        {
            string fieldName = value;

            for (int i = 0; i < _fields.Count; i++)
            {
                if (_fields[i] == fieldName)
                    return i;
            }

            return 0;
        }
    }
}