using UnityEditor;
using UnityEngine;

// Enum com descrições customizadas
public enum Mood
{
    [Description("Feeling very happy!")]
    Happy,
    [Description("Feeling okay.")]
    Neutral,
    [Description("Feeling sad.")]
    Sad
}

// Atributo personalizado para associar ao enum
public class DescriptionAttribute : PropertyAttribute
{
    public string Description { get; private set; }

    public DescriptionAttribute(string description)
    {
        Description = description;
    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(DescriptionAttribute))]
public class DescriptionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Exibe a enumeração
        EditorGUI.BeginProperty(position, label, property);

        // Converte o índice do enum para um objeto enum real
        Mood moodValue = (Mood)property.enumValueIndex;

        // Recupera a descrição
        var description = GetDescription(moodValue);
        EditorGUI.LabelField(position, label.text, description);

        EditorGUI.EndProperty();
    }

    private string GetDescription(Mood mood)
    {
        var fieldInfo = mood.GetType().GetField(mood.ToString());
        var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        // Usando o seu DescriptionAttribute
        return attributes.Length > 0 ? attributes[0].Description : mood.ToString();
    }
}
#endif