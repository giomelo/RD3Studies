using UnityEditor;
using UnityEngine;

// Atributo para associar ao campo
public class DynamicRangeAttribute : PropertyAttribute
{
    public string minProperty;
    public string maxProperty;

    public DynamicRangeAttribute(string minProperty, string maxProperty)
    {
        this.minProperty = minProperty;
        this.maxProperty = maxProperty;
    }
}
#if UNITY_EDITOR

// PropertyDrawer para o DynamicRangeAttribute
[CustomPropertyDrawer(typeof(DynamicRangeAttribute))]
public class DynamicRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DynamicRangeAttribute range = (DynamicRangeAttribute)attribute;

        SerializedProperty minProp = property.serializedObject.FindProperty(range.minProperty);
        SerializedProperty maxProp = property.serializedObject.FindProperty(range.maxProperty);

        if (minProp != null && maxProp != null)
        {
            float minValue = minProp.floatValue;
            float maxValue = maxProp.floatValue;
            
            property.floatValue = EditorGUI.Slider(position, label, property.floatValue, minValue, maxValue);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Error: Min or Max property not found.");
        }
    }
}

#endif