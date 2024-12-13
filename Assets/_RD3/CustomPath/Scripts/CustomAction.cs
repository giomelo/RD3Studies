using System;
using System.Threading.Tasks;
using _RD3.CustomPath.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public enum ActionType
{
    EventType,
    Move
}

[Serializable]
public enum EventsType
{
    SO,
    UnityEvent
}


interface IAction
{
    public void ExecuteAction();
}

public interface IMovable
{
    public void SetMovePosition(Vector3 finalPos, Action onReached = null);
}

public abstract class ActionParameters : IAction
{
    public GameObject gameObject;
    [HideInInspector]public CustomAction onCompleteAction;
    [HideInInspector]public CustomAction lastAction;
    public virtual void ExecuteAction()
    {
        throw new NotImplementedException();
    }
}
[Serializable]
public class ScriptableAction : ActionParameters
{
    public SO_VoidEvent eventToInvoke;
    public UnityEvent unityEventToInvoke;
    public EventsType eventsType;
    public int delayStart;
    public int delayNext;
    
    public override async void ExecuteAction()
    {
        await Task.Delay(delayStart * 1000);
        eventToInvoke.Invoke();
        unityEventToInvoke?.Invoke();
        await Task.Delay(delayNext * 1000);
        if(onCompleteAction != null) onCompleteAction.Execute();
    }
}

[Serializable]
public class MoveParams : ActionParameters
{
    public Vector3 movePosition;

    
    public override void ExecuteAction()
    {
        gameObject.GetComponent<IMovable>().SetMovePosition(movePosition, () =>
        {
            if(onCompleteAction != null) onCompleteAction.Execute();
        });
    }
}

public class CustomAction : MonoBehaviour
{
    public ActionType actionType;
    public ScriptableAction eventParams;
    public MoveParams moveParams;
    [HideInInspector]public RunCustomAction customAction;
    public void Execute()
    {
        GetCurrentAction().ExecuteAction();
    }

    public ActionParameters GetCurrentAction()
    {
        ActionParameters actionParameters = actionType switch
        {
            ActionType.EventType => eventParams,
            ActionType.Move => moveParams,
            _ => throw new ArgumentOutOfRangeException()
        };
        return actionParameters;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosLine(Vector3 toPos, Vector3 fromPos, Color lineColor, string text = null,
        Color? textColor = null)
    {
        Handles.color = lineColor;
        Handles.DrawAAPolyLine(5, toPos, fromPos);
        var dir = (fromPos - toPos).normalized;
        var distance = Vector3.Distance(toPos, fromPos);

        for (int i = 0; i < distance; i += 1)
        {
            Quaternion rotation = Quaternion.AngleAxis(Time.realtimeSinceStartup * 360f, dir);

            Handles.DrawAAPolyLine(
                5f,
                toPos + dir * i,
                toPos + dir * (i + 0.15f) + rotation * Vector3.up * 0.1f
            );

            Handles.DrawAAPolyLine(
                5f,
                toPos + dir * i,
                toPos + dir * (i + 0.15f) + rotation * Vector3.down * 0.1f
            );
        }

        if (textColor == null) return;
        DrawText(toPos + (dir * distance * .5f), text, (Color)textColor);
    }

    private void DrawDottedLine(Vector3 from, Vector3 to, Color color, string text = null, Color? textColor = null)
    {
        Handles.color = color;
        Handles.DrawDottedLine(from, to, 5);
        var dir = (from - to).normalized;
        var distance = Vector3.Distance(to, from);
        if (textColor == null) return;
        DrawText(to + (dir * distance * .5f), text, (Color)textColor);
    }

    private void DrawText(Vector3 pos, string text, Color color, int size = 15)
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = color;
        style.fontSize = size;
        Handles.Label(pos, text, style);
    }

    public Vector3 lastPosition;

    private void OnDrawGizmos()
    {
        var sceneCamera = SceneView.currentDrawingSceneView.camera;

        if (Vector3.Distance(sceneCamera.transform.position, transform.position) > 50)
            return;

        DrawText(transform.position + new Vector3(0, 0.5f, 0), actionType.ToString(), Color.white);

        ActionParameters current = GetCurrentActionParameters();

        DrawCurrentActionGizmos(current);

        if (current.onCompleteAction != null)
        {
            DrawOnCompleteActionGizmos(current);
        }
    }

    private ActionParameters GetCurrentActionParameters()
{
    return actionType switch
    {
        ActionType.EventType => eventParams,
        ActionType.Move => moveParams,
        _ => throw new ArgumentOutOfRangeException()
    };
}

private void DrawCurrentActionGizmos(ActionParameters current)
{
    switch (actionType)
    {
        case ActionType.EventType:
            DrawEventTypeGizmos(current);
            break;
        case ActionType.Move:
            DrawMoveTypeGizmos(current);
            break;
    }
}

private void DrawEventTypeGizmos(ActionParameters current)
{
    if (current.onCompleteAction != null)
    {
        OnDrawGizmosLine(current.onCompleteAction.transform.position, transform.position, Color.black, "Next", Color.white);
        if (current.lastAction != null)
            lastPosition = current.lastAction.moveParams.movePosition;
    }
}

private void DrawMoveTypeGizmos(ActionParameters current)
{
    lastPosition = moveParams.movePosition;

    if (moveParams.gameObject != null)
    {
        DrawDottedLine(transform.position, moveParams.gameObject.transform.position, Color.red);

        if (moveParams.lastAction == null)
            OnDrawGizmosLine(lastPosition, moveParams.gameObject.transform.position, Color.green);
        else
            DrawDottedLine(lastPosition, moveParams.gameObject.transform.position, Color.cyan, "", Color.white);
    }

    if (current.onCompleteAction != null)
    {
        OnDrawGizmosLine(current.onCompleteAction.transform.position, transform.position, Color.black, "Next", Color.white);
        UpdateLastPositionBasedOnActionType(current);
    }
}

private void UpdateLastPositionBasedOnActionType(ActionParameters current)
{
    switch (current.onCompleteAction.actionType)
    {
        case ActionType.EventType:
            lastPosition = moveParams.movePosition;
            break;
        case ActionType.Move:
            if (current.lastAction != null)
                lastPosition = current.lastAction.moveParams.movePosition;
            break;
        default:
            throw new ArgumentOutOfRangeException();
    }
}

private void DrawOnCompleteActionGizmos(ActionParameters current)
{
    switch (current.onCompleteAction.actionType)
    {
        case ActionType.EventType:
            DrawOnCompleteEventTypeGizmos(current);
            break;
        case ActionType.Move:
            DrawOnCompleteMoveTypeGizmos(current);
            break;
        default:
            throw new ArgumentOutOfRangeException();
    }
}

private void DrawOnCompleteEventTypeGizmos(ActionParameters current)
{
    if (current.onCompleteAction.eventParams.eventToInvoke != null)
    {
        Handles.color = Color.yellow;
        Handles.SphereHandleCap(0, lastPosition, transform.rotation * Quaternion.LookRotation(Vector3.right), 0.5f, EventType.Repaint);

        if (current.onCompleteAction.eventParams.eventsType == EventsType.SO)
        {
            DrawText(lastPosition + new Vector3(0, 0.5f, 0), $"{current.onCompleteAction.eventParams.eventToInvoke.name}", Color.white);
        }
    }
}

private void DrawOnCompleteMoveTypeGizmos(ActionParameters current)
{
    if (actionType == ActionType.EventType)
        lastPosition = GetLastMovePosition();
    else
        lastPosition = moveParams.movePosition;
    
    OnDrawGizmosLine(current.onCompleteAction.moveParams.movePosition, lastPosition, Color.green, "", Color.white);
}
   
    Vector3 position = Vector3.zero;
    private Vector3 GetLastMovePosition()
    {
        CustomAction currentAction = this;

        while (currentAction != null)
        {
            if (currentAction.actionType == ActionType.EventType)
            {
                position = currentAction.moveParams.gameObject.transform.position;
            }

            if (currentAction.GetCurrentAction().lastAction == null)
            {
                if (currentAction.actionType == ActionType.EventType)
                {
                    return position;
                }

                return currentAction.moveParams.gameObject.transform.position;

            }

            if (currentAction.GetCurrentAction().lastAction.actionType == ActionType.EventType)
            {
                currentAction = currentAction.GetCurrentAction().lastAction;
            }
            else
            {
                position = currentAction.GetCurrentAction().lastAction.moveParams.movePosition;
                return position;
            }

        }

        return position;
    }
#endif

}

#if UNITY_EDITOR
[CustomEditor(typeof(CustomAction))]
public class CustomActionEditor : Editor
{
    private CustomAction targetClass;

    private void OnEnable()
    {
        targetClass = (CustomAction)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("actionType"));

        switch (targetClass.actionType)
        {
            case ActionType.EventType:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventParams").FindPropertyRelative("delayStart"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventParams").FindPropertyRelative("delayNext"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventParams").FindPropertyRelative("eventsType"));

                EditorGUILayout.PropertyField(targetClass.eventParams.eventsType == EventsType.SO
                    ? serializedObject.FindProperty("eventParams").FindPropertyRelative("eventToInvoke")
                    : serializedObject.FindProperty("eventParams").FindPropertyRelative("unityEventToInvoke"));


                break;
            case ActionType.Move:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveParams"));
              
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

    

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        switch (targetClass.actionType)
        {
            case ActionType.EventType:
                break;
            case ActionType.Move:
                EditorGUI.BeginChangeCheck();
                var newPosition = Handles.PositionHandle(targetClass.moveParams.movePosition, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(targetClass, "Change move");
                    targetClass.moveParams.movePosition = newPosition;
                    serializedObject.Update();
                }

                break;
        }
    }
}

#endif