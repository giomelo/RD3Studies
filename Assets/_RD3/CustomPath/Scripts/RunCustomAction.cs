
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RunCustomAction : MonoBehaviour
{
    [SerializeField]private CustomAction customAction;
    [SerializeField]private SO_VoidEvent eVoidEvent;

    public List<CustomAction> actions;
    private int currentIndex;

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

    public void SetActions()
    {
        actions.Clear();
        CustomAction currentAction = customAction;
        while (currentAction != null)
        {
            actions.Add(currentAction);
            currentAction = currentAction.GetCurrentAction().onCompleteAction;
        } 
    }

    private GameObject _currentObject;
    public void AddAction()
    {
        var action = Instantiate(Resources.Load("CustomAction")) as GameObject;
        CustomAction lastAction = null;
        var actionScript = action.GetComponent<CustomAction>();
        if (actions.Count != 0)
            lastAction = actions[^1];
        else 
            customAction = actionScript;
        
        var position = lastAction ? lastAction.transform.position : transform.position;
        CustomAction currentAction = action.GetComponent<CustomAction>();
        action.transform.position = position + new Vector3(0,0,2);
        currentAction.moveParams.lastAction = lastAction;
        currentAction.eventParams.lastAction = lastAction;
        action.transform.SetParent(transform);
        
        if (lastAction)
        {
            lastAction.moveParams.onCompleteAction = currentAction;
            lastAction.eventParams.onCompleteAction = currentAction;
            if (lastAction.actionType == ActionType.Move)
                _currentObject = lastAction.moveParams.gameObject;
        }
        actionScript.moveParams.gameObject = _currentObject;
        actionScript.eventParams.gameObject = _currentObject;
        actions.Add(actionScript);
    }
    
    public void RemoveAction()
    {
        var action = actions[^1];
        actions.Remove(action);
        DestroyImmediate(action.gameObject);
        if(actions.Count > 0) actions[^1].GetCurrentAction().onCompleteAction = null;
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
        _customAction.SetActions();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ADD ACTION"))
        {
            _customAction.AddAction();
        }

        if (GUILayout.Button("REMOVE ACTION"))
        {
            _customAction.RemoveAction();
        }
        EditorGUILayout.EndHorizontal();
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
