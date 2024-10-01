using System;
using System.Threading.Tasks;
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
    public CustomAction onCompleteAction;
    public CustomAction lastMoveParams;
    public virtual async void ExecuteAction()
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
        onCompleteAction.Execute();
    }
}

[Serializable]
public class MoveParams : ActionParameters
{
    public Vector3 movePosition;
    public GameObject gameObject;
    public override void ExecuteAction()
    {
        gameObject.GetComponent<IMovable>().SetMovePosition(movePosition, () =>
        {
            onCompleteAction.Execute();
        });
    }
}

public class CustomAction : MonoBehaviour
{
    public ActionType actionType;
    public ScriptableAction eventParams;
    public MoveParams moveParams;

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

    private Vector3 lastPosition;
    private void OnDrawGizmos()
    {
        var sceneCamera = SceneView.currentDrawingSceneView.camera;

        if (Vector3.Distance(sceneCamera.transform.position, transform.position) > 50)
            return;

        DrawText(transform.position, actionType.ToString(), Color.white);
        ActionParameters current = moveParams;

        switch (actionType)
        {
            case ActionType.EventType:
                current = eventParams;
                if (current.onCompleteAction != null)
                {
                    OnDrawGizmosLine(current.onCompleteAction.transform.position, transform.position, Color.black,
                        "Next", Color.white);
                    if (current.lastMoveParams != null)
                        lastPosition = current.lastMoveParams.moveParams.movePosition;
                }

                break;
            case ActionType.Move:
                current = moveParams;
                lastPosition = moveParams.movePosition;
        
                if (moveParams.gameObject != null)
                {
                    DrawDottedLine(transform.position, moveParams.gameObject.transform.position, Color.red);
                    //draw the first movement line
                    if (moveParams.lastMoveParams == null)
                        OnDrawGizmosLine(moveParams.movePosition, moveParams.gameObject.transform.position, Color.green);
                    else
                        DrawDottedLine(moveParams.movePosition, moveParams.gameObject.transform.position, Color.cyan, "",
                            Color.white);
                }
                if (current.onCompleteAction != null)
                {
                    OnDrawGizmosLine(current.onCompleteAction.transform.position, transform.position, Color.black,
                        "Next", Color.white);
                    switch (current.onCompleteAction.actionType)
                    {
                        case ActionType.EventType:
                            lastPosition = moveParams.movePosition;
                            break;
                        case ActionType.Move:
                            if (current.lastMoveParams != null)
                                lastPosition = current.lastMoveParams.moveParams.movePosition;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    
                }
                
                break;
        }

        if (current.onCompleteAction == null) return;
        switch (current.onCompleteAction.actionType)
        {
            case ActionType.EventType:
                if(current.onCompleteAction.eventParams.eventToInvoke != null) DrawText(lastPosition, $"{current.onCompleteAction.eventParams.eventToInvoke.name}", Color.white);
                break;
            case ActionType.Move:
                OnDrawGizmosLine(current.onCompleteAction.moveParams.movePosition, lastPosition, Color.green, "",
                    Color.white);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

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
                // EditorGUILayout.PropertyField(serializedObject.FindProperty("chatBubbleParams").FindPropertyRelative("text"));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("eventParams"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventParams").FindPropertyRelative("lastMoveParams"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("eventParams").FindPropertyRelative("onCompleteAction"));
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