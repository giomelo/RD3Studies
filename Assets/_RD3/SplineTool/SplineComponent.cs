using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _RD3.SplineTool
{
    public class SplineComponent : MonoBehaviour, ISpline
    {
        public bool closed = false;
        public List<Vector3> points = new List<Vector3>();
        public float? length;
        public int ControlPointCount => points.Count;

        public Vector3 GetNonUniformPoint(float t)
        {
            switch (points.Count)
            {
                case 0:
                    return Vector3.zero;
                case 1:
                    return transform.TransformPoint(points[0]);
                case 2:
                    return transform.TransformPoint(Vector3.Lerp(points[0], points[1], t));
                case 3:
                    return transform.TransformPoint(points[1]);
                default:
                    return Hermite(t);
            }
        }

        public void InsertControlPoint(int index, Vector3 position)
        {
            ResetIndex();
            if (index >= points.Count)
                points.Add(position);
            else
                points.Insert(index, position);
        }
    


        public void RemoveControlPoint(int index)
        {
            ResetIndex();
            points.RemoveAt(index);
        }

        /// <summary>
        /// Index is used to provide uniform point searching.
        /// </summary>
        SplineIndex uniformIndex;
        SplineIndex Index
        {
            get
            {
                if (uniformIndex == null) uniformIndex = new SplineIndex(this);
                return uniformIndex;
            }
        }

        public void ResetIndex()
        {
            uniformIndex = null;
            length = null;
        }

        public Vector3 GetPoint(float t) => Index.GetPoint(t);
    
        public Vector3 GetControlPoint(int index)
        {
            return points[index];
        }

        public void SetControlPoint(int index, Vector3 position)
        {
            ResetIndex();
            points[index] = position;
        }
    
        public Vector3 GetRight(float t)
        {
            var A = GetPoint(t - 0.001f);
            var B = GetPoint(t + 0.001f);
            var delta = (B - A);
            return new Vector3(-delta.z, 0, delta.x).normalized;
        }


        public Vector3 GetForward(float t)
        {
            var A = GetPoint(t - 0.001f);
            var B = GetPoint(t + 0.001f);
            return (B - A).normalized;
        }


        public Vector3 GetUp(float t)
        {
            var A = GetPoint(t - 0.001f);
            var B = GetPoint(t + 0.001f);
            var delta = (B - A).normalized;
            return Vector3.Cross(delta, GetRight(t));
        }
        public float GetLength(float step = 0.001f)
        {
            var D = 0f;
            var A = GetNonUniformPoint(0);
            for (var t = 0f; t < 1f; t += step)
            {
                var B = GetNonUniformPoint(t);
                var delta = (B - A);
                D += delta.magnitude;
                A = B;
            }
            return D;
        }

        public Vector3 GetDistance(float distance)
        {
            if (length == null) length = GetLength();
            return uniformIndex.GetPoint(distance / length.Value);
        }


        public Vector3 GetLeft(float t) => -GetRight(t);


        public Vector3 GetDown(float t) => -GetUp(t);


        public Vector3 GetBackward(float t) => -GetForward(t);
        public Vector3 FindClosest(Vector3 worldPoint)
        {
            var smallestDelta = float.MaxValue;
            var step = 1f / 1024;
            var closestPoint = Vector3.zero;
            for (var i = 0; i <= 1024; i++)
            {
                var p = GetPoint(i * step);
                var delta = (worldPoint - p).sqrMagnitude;
                if (delta < smallestDelta)
                {
                    closestPoint = p;
                    smallestDelta = delta;
                }
            }
            return closestPoint;
        }
        void Reset()
        {
            points = new List<Vector3>() {
                Vector3.forward * 3,
                Vector3.forward * 6,
                Vector3.forward * 9,
                Vector3.forward * 12
            };
        }
    
        void OnValidate()
        {
            if (uniformIndex != null) uniformIndex.ReIndex();
        }

        //This is the function which looks up the correct control points
        //for a position along the spline then performs and return the interpolated world position.
        Vector3 Hermite(float t)
        {
            var count = points.Count - (closed ? 0 : 3);
            var i = Mathf.Min(Mathf.FloorToInt(t * (float)count), count - 1);
            var u = t * (float)count - (float)i;
            var a = GetPointByIndex(i);
            var b = GetPointByIndex(i + 1);
            var c = GetPointByIndex(i + 2);
            var d = GetPointByIndex(i + 3);
            return transform.TransformPoint(Interpolate(a, b, c, d, u));
        }

        Vector3 GetPointByIndex(int i)
        {
            if (i < 0) i += points.Count;
            return points[i % points.Count];
        }

        /// <summary>
        /// This is a hermite spline interpolation function. It takes 4 vectors (a and d are control points,
        /// b and c are the start and end points) and a u parameter which specifies the interpolation position.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="u"></param>
        /// <returns></returns>
        internal static Vector3 Interpolate(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u)
        {
            return (
                0.5f *
                (
                    (-a + 3f * b - 3f * c + d) *
                    (u * u * u) +
                    (2f * a - 5f * b + 4f * c - d) *
                    (u * u) +
                    (-a + c) *
                    u + 2f * b
                )
            );
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SplineComponent))]
    public class SplineComponentEditor : Editor
    {    
        int hotIndex = -1;
        int removeIndex = -1;
        public override void OnInspectorGUI()
        {
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("points"));

            EditorGUILayout.HelpBox("Hold Shift and click to append and insert curve points. Backspace to delete points.", MessageType.Info);
            var spline = target as SplineComponent;
            GUILayout.BeginHorizontal();
            var closed = GUILayout.Toggle(spline.closed, "Closed", "button");
            if (spline.closed != closed)
            {
                spline.closed = closed;
                spline.ResetIndex();
            }
            if (GUILayout.Button("Flatten Y Axis"))
            {
                Undo.RecordObject(target, "Flatten Y Axis");
                Flatten(spline.points);
                spline.ResetIndex();
            }
            if (GUILayout.Button("Center around Origin"))
            {
                Undo.RecordObject(target, "Center around Origin");
                CenterAroundOrigin(spline.points);
                spline.ResetIndex();
            }
            GUILayout.EndHorizontal();
        }

        void OnSceneGUI()
        {
            var spline = target as SplineComponent;


            var e = Event.current;
            GUIUtility.GetControlID(FocusType.Passive);


            var mousePos = (Vector2)Event.current.mousePosition;
            var view = SceneView.currentDrawingSceneView.camera.ScreenToViewportPoint(Event.current.mousePosition);
            var mouseIsOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
            if (mouseIsOutside) return;

            var points = serializedObject.FindProperty("points");
            if (Event.current.shift)
            {
                if (spline.closed)
                    ShowClosestPointOnClosedSpline(points);
                else
                    ShowClosestPointOnOpenSpline(points);
            }
        
            for (int i = 0; i < spline.points.Count; i++)
            {
                var prop = points.GetArrayElementAtIndex(i);
                var point = prop.vector3Value;
                var wp = spline.transform.TransformPoint(point);
                if (hotIndex == i)
                {
                    var newWp = Handles.PositionHandle(wp, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : spline.transform.rotation);
                    var delta = spline.transform.InverseTransformDirection(newWp - wp);
                    if (delta.sqrMagnitude > 0)
                    {
                        prop.vector3Value = point + delta;
                        spline.ResetIndex();
                    }
                    HandleCommands(wp);
                }
                Handles.color = i == 0 | i == spline.points.Count - 1 ? Color.red : Color.white;
                var buttonSize = HandleUtility.GetHandleSize(wp) * 0.1f;
                if (Handles.Button(wp, Quaternion.identity, buttonSize, buttonSize, Handles.SphereHandleCap))
                    hotIndex = i;
                var v = SceneView.currentDrawingSceneView.camera.transform.InverseTransformPoint(wp);
                var labelIsOutside = v.z < 0;
                if (!labelIsOutside) Handles.Label(wp, i.ToString());

            }
            if (removeIndex >= 0 && points.arraySize > 4)
            {
                points.DeleteArrayElementAtIndex(removeIndex);
                spline.ResetIndex();
            }
        
            removeIndex = -1;
            serializedObject.ApplyModifiedProperties();
        
        }
    
        void HandleCommands(Vector3 wp)
        {
            if (Event.current.type == EventType.ExecuteCommand)
            {
                if (Event.current.commandName == "FrameSelected")
                {
                    SceneView.currentDrawingSceneView.Frame(new Bounds(wp, Vector3.one * 10), false);
                    Event.current.Use();
                }
            }
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Backspace)
                {
                    removeIndex = hotIndex;
                    Event.current.Use();
                }
            }
        }

        [DrawGizmo(GizmoType.NonSelected)]
        static void DrawGizmosLoRes(SplineComponent spline, GizmoType gizmoType)
        {
            Gizmos.color = Color.white;
            DrawGizmo(spline, 64);
        }

        [DrawGizmo(GizmoType.Selected)]
        static void DrawGizmosHiRes(SplineComponent spline, GizmoType gizmoType)
        {
            Gizmos.color = Color.white;
            DrawGizmo(spline, 1024);
        }

        static void DrawGizmo(SplineComponent spline, int stepCount)
        {
            if (spline.points.Count > 0)
            {
                var P = 0f;
                var start = spline.GetNonUniformPoint(0);
                var step = 1f / stepCount;
                do
                {
                    P += step;
                    var here = spline.GetNonUniformPoint(P);
                    Gizmos.DrawLine(start, here);
                    start = here;
                } while (P + step <= 1);
            }
        }
        void ShowClosestPointOnClosedSpline(SerializedProperty points)
        {
            var spline = target as SplineComponent;
            var plane = new Plane(spline.transform.up, spline.transform.position);
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float center;
            if (plane.Raycast(ray, out center))
            {
                var hit = ray.origin + ray.direction * center;
                Handles.DrawWireDisc(hit, spline.transform.up, 5);
                var p = SearchForClosestPoint(Event.current.mousePosition);
                var sp = spline.GetNonUniformPoint(p);
                Handles.DrawLine(hit, sp);


                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift)
                {
                    var i = (Mathf.FloorToInt(p * spline.points.Count) + 2) % spline.points.Count;
                    points.InsertArrayElementAtIndex(i);
                    points.GetArrayElementAtIndex(i).vector3Value = spline.transform.InverseTransformPoint(sp);
                    serializedObject.ApplyModifiedProperties();
                    hotIndex = i;
                }
            }
        }


        void ShowClosestPointOnOpenSpline(SerializedProperty points)
        {
            var spline = target as SplineComponent;
            var plane = new Plane(spline.transform.up, spline.transform.position);
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            float center;
            if (plane.Raycast(ray, out center))
            {
                var hit = ray.origin + ray.direction * center;
                var discSize = HandleUtility.GetHandleSize(hit);
                Handles.DrawWireDisc(hit, spline.transform.up, discSize);
                var p = SearchForClosestPoint(Event.current.mousePosition);


                if ((hit - spline.GetNonUniformPoint(0)).sqrMagnitude < 25) p = 0;
                if ((hit - spline.GetNonUniformPoint(1)).sqrMagnitude < 25) p = 1;


                var sp = spline.GetNonUniformPoint(p);


                var extend = Mathf.Approximately(p, 0) || Mathf.Approximately(p, 1);


                Handles.color = extend ? Color.red : Color.white;
                Handles.DrawLine(hit, sp);
                Handles.color = Color.white;


                var i = 1 + Mathf.FloorToInt(p * (spline.points.Count - 3));


                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift)
                {
                    if (extend)
                    {
                        if (i == spline.points.Count - 2) i++;
                        points.InsertArrayElementAtIndex(i);
                        points.GetArrayElementAtIndex(i).vector3Value = spline.transform.InverseTransformPoint(hit);
                        hotIndex = i;
                    }
                    else
                    {
                        i++;
                        points.InsertArrayElementAtIndex(i);
                        points.GetArrayElementAtIndex(i).vector3Value = spline.transform.InverseTransformPoint(sp);
                        hotIndex = i;
                    }
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }


        float SearchForClosestPoint(Vector2 screenPoint, float A = 0f, float B = 1f, float steps = 1000)
        {
            var spline = target as SplineComponent;
            var smallestDelta = float.MaxValue;
            var step = (B - A) / steps;
            var closestI = A;
            for (var i = 0; i <= steps; i++)
            {
                var p = spline.GetNonUniformPoint(i * step);
                var gp = HandleUtility.WorldToGUIPoint(p);
                var delta = (screenPoint - gp).sqrMagnitude;
                if (delta < smallestDelta)
                {
                    closestI = i;
                    smallestDelta = delta;
                }
            }
            return closestI * step;
        }
        void Flatten(List<Vector3> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = Vector3.Scale(points[i], new Vector3(1, 0, 1));
            }
        }


        void CenterAroundOrigin(List<Vector3> points)
        {
            var center = Vector3.zero;
            for (int i = 0; i < points.Count; i++)
            {
                center += points[i];
            }
            center /= points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                points[i] -= center;
            }
        }
    }


#endif
}