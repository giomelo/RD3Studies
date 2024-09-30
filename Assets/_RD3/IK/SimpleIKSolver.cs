
using UnityEditor;
using UnityEngine;

public class SimpleIKSolver : MonoBehaviour
{

    public Transform pivot, upper, lower, effector, tip;
    public Vector3 target = Vector3.forward;
    public Vector3 normal = Vector3.up;
    float upperLength, lowerLength, effectorLength, pivotLength;
    Vector3 effectorTarget, tipTarget;
    
    
    void Awake()
    {
        upperLength = (lower.position - upper.position).magnitude;
        lowerLength = (effector.position - lower.position).magnitude;
        effectorLength = (tip.position - effector.position).magnitude;
        pivotLength = (upper.position - pivot.position).magnitude;
    }
    void Update()
    {
        var pivotDir = effectorTarget - pivot.position;
        pivot.rotation = Quaternion.LookRotation(pivotDir);
        var upperToTarget = (effectorTarget - upper.position);
        var a = upperLength;
        var b = lowerLength;
        var c = upperToTarget.magnitude;
        if (!float.IsNaN(c))
        {
            {
                var upperRotation = Quaternion.AngleAxis((-b), Vector3.right);
                upper.localRotation = upperRotation;
                var lowerRotation = Quaternion.AngleAxis(180 - c, Vector3.right);
                lower.localRotation = lowerRotation;
            }
            var effectorRotation = Quaternion.LookRotation(tipTarget - effector.position);
            effector.rotation = effectorRotation;
        }

        tipTarget = target;
        effectorTarget = target + normal * effectorLength;
        Solve();
    }
    
    void Solve()
    {
        var pivotDir = effectorTarget - pivot.position;
        pivot.rotation = Quaternion.LookRotation(pivotDir);


        var upperToTarget = (effectorTarget - upper.position);
        var a = upperLength;
        var b = lowerLength;
        var c = upperToTarget.magnitude;


        var B = Mathf.Acos((c * c + a * a - b * b) / (2 * c * a)) * Mathf.Rad2Deg;
        var C = Mathf.Acos((a * a + b * b - c * c) / (2 * a * b)) * Mathf.Rad2Deg;


        if (!float.IsNaN(C))
        {
            var upperRotation = Quaternion.AngleAxis((-B), Vector3.right);
            upper.localRotation = upperRotation;
            var lowerRotation = Quaternion.AngleAxis(180 - C, Vector3.right);
            lower.localRotation = lowerRotation;
        }
        var effectorRotation = Quaternion.LookRotation(tipTarget - effector.position);
        effector.rotation = effectorRotation;
    }
    void Reset()
    {
        {
            pivot = transform;
            try
            {
                upper = pivot.GetChild(0);
                lower = upper.GetChild(0);
                effector = lower.GetChild(0);
                tip = effector.GetChild(0);
            }
            catch (UnityException)
            {
                Debug.Log("Could not find required transforms, please assign manually.");
            }
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(SimpleIKSolver))]
public class SimpleIKSolverEditor : Editor
{
    static GUIStyle errorBox;

    [DrawGizmo(GizmoType.Selected)]
    static void OnDrawGizmosSelected(SimpleIKSolver siks, GizmoType gizmoType)
    {
        Handles.color = Color.blue;
        if (siks.pivot == null)
        {
            Handles.Label(siks.transform.position, "Pivot is not assigned", errorBox);
            return;
        }
        if (siks.upper == null)
        {
            Handles.Label(siks.pivot.position, "Upper is not assigned", errorBox);
            return;
        }
        if (siks.lower == null)
        {
            Handles.Label(siks.upper.position, "Lower is not assigned", errorBox);
            return;
        }
        if (siks.effector == null)
        {
            Handles.Label(siks.lower.position, "Effector is not assigned", errorBox);
            return;
        }
        if (siks.tip == null)
        {
            Handles.Label(siks.effector.position, "Tip is not assigned", errorBox);
            return;
        }
        Handles.DrawPolyLine(siks.pivot.position, siks.upper.position, siks.lower.position, siks.effector.position, siks.tip.position);
        Handles.DrawDottedLine(siks.tip.position, siks.target, 3);
        Handles.Label(siks.upper.position, "Upper");
        Handles.Label(siks.effector.position, "Effector");
        Handles.Label(siks.lower.position, "Lower");
        Handles.Label(siks.target, "Target");
        var distanceToTarget = Vector3.Distance(siks.target, siks.tip.position);
        var midPoint = Vector3.Lerp(siks.target, siks.tip.position, 0.5f);
        Handles.Label(midPoint, string.Format("Distance to Target: {0:0.00}", distanceToTarget));
    }
    void OnEnable()
    {
        errorBox = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).box);
        errorBox.normal.textColor = Color.red;
    }
    public override void OnInspectorGUI()
    {
        var s = target as SimpleIKSolver;
        if (s.pivot == null || s.upper == null || s.lower == null | s.effector == null || s.tip == null)
            EditorGUILayout.HelpBox("Please assign Pivot, Upper, Lower, Effector and Tip transforms.", MessageType.Error);
        base.OnInspectorGUI();
    }

    public void OnSceneGUI()
    {
        
            var siks = target as SimpleIKSolver;
            RotationHandle(siks.effector);
            RotationHandle(siks.lower);
            RotationHandle(siks.upper);
            siks.target = Handles.PositionHandle(siks.target, Quaternion.identity);
            var normalRotation = Quaternion.LookRotation(Vector3.forward, siks.normal);
            normalRotation = Handles.RotationHandle(normalRotation, siks.tip.position);
            siks.normal = normalRotation * Vector3.up;
        }
    void RotationHandle(Transform transform)
    {
        if (transform != null)
        {
            EditorGUI.BeginChangeCheck();
            var rotation = Handles.RotationHandle(transform.rotation, transform.position);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(transform, "Rotate");
                transform.rotation = rotation;
            }
        }
    }
}
#endif