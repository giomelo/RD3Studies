using UnityEditor;
using UnityEngine;

// Atributo personalizado que usaremos para identificar o campo
public class ProgressBarAttribute : PropertyAttribute
{
    public float min;
    public float max;

    public ProgressBarAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}

#if UNITY_EDITOR
// PropertyDrawer associado ao ProgressBarAttribute
[CustomPropertyDrawer(typeof(ProgressBarAttribute))]
public class ProgressBarDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ProgressBarAttribute progressBar = (ProgressBarAttribute)attribute;

        // Calcula o valor normalizado entre min e max
        float value = property.floatValue;
        float fillAmount = Mathf.InverseLerp(progressBar.min, progressBar.max, value);

        // Desenha uma caixa de fundo
        EditorGUI.ProgressBar(position, value, $"{label.text}: {value}");
    }

    // Define a altura necessária para desenhar o campo
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
#endif