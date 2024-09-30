using UnityEditor;
using UnityEngine;

public class MyEditorUtilityExample
{
    [MenuItem("Tools/Show Complex Dialog")]
    public static void ShowComplexDialog()
    {
        int option = EditorUtility.DisplayDialogComplex(
            "Complex Dialog",
            "Choose an option:",
            "Option 1",
            "Option 2",
            "Cancel"
        );

        switch (option)
        {
            case 0:
                Debug.Log("User chose Option 1");
                break;
            case 1:
                Debug.Log("User chose Option 2");
                break;
            case 2:
                Debug.Log("User pressed Cancel");
                break;
        }
    }
}