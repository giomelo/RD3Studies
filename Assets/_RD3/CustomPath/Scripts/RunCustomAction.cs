using System;
using UnityEditor;
using UnityEngine;

public class RunCustomAction : MonoBehaviour
{
    [SerializeField]private CustomAction customAction;
    [SerializeField]private SO_VoidEvent eVoidEvent;

    public void Execute()
    {
        customAction.Execute();
        eVoidEvent.AddListener(()=> Debug.Log("Event invoked"));
    }
    
    private void OnDrawGizmos()
    {
        var sceneCamera = SceneView.currentDrawingSceneView.camera;

        if (Vector3.Distance(sceneCamera.transform.position, transform.position) > 50)
            return;

        if (customAction != null)
            Handles.DrawDottedLine(transform.position, customAction.transform.position,5);
    }
}

#if UNITY_EDITOR
[CustomEditor((typeof(RunCustomAction)))]
public class RunCustomActionEditor : Editor
{
    private RunCustomAction _customAction;
    
    private void OnEnable()
    {
        _customAction = (RunCustomAction)target;
    }

    private void OnSceneGUI()
    {
        var sceneCamera = SceneView.currentDrawingSceneView.camera;

        if (Vector3.Distance(sceneCamera.transform.position, _customAction.transform.position) > 50)
            return;

        Handles.BeginGUI();
        var rectMin = Camera.current.WorldToScreenPoint(
            _customAction.transform.position);

        var rect = new Rect();
        rect.xMin = rectMin.x;
        rect.yMin = SceneView.currentDrawingSceneView.position.height - rectMin.y;
        rect.width = 64;
        rect.height = 18;

        GUILayout.BeginArea(rect);
        using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
        {
            if (GUILayout.Button("RUN"))
                _customAction.Execute();
        }
        GUILayout.EndArea();
        Handles.EndGUI();
    }
}

#endif
