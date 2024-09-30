using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class ScriptableObjectEditorWindow : EditorWindow
{
    private MyScriptableObject myData;

    [MenuItem("Examples/ScriptableObject Editor")]
    public static void ShowWindow()
    {
        GetWindow<ScriptableObjectEditorWindow>("ScriptableObject Editor");
    }

    private void OnGUI()
    {
        myData = (MyScriptableObject)EditorGUILayout.ObjectField("Data", myData, typeof(MyScriptableObject), false);

        if (myData != null)
        {
            myData.someValue = EditorGUILayout.IntField("Some Value", myData.someValue);
            myData.someValue2 = EditorGUILayout.TextField("Some Value2", myData.someValue2);
            EditorUtility.SetDirty(myData);
        }
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Column 1", EditorStyles.label);
        GUILayout.Label("Column 2", EditorStyles.label);
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < 5; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Row {i + 1} Col 1");
            GUILayout.Label($"Row {i + 1} Col 2");
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }
}
#endif

