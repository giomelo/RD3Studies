using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace _RD3.LaucherHandles
{
    public class Launcher : MonoBehaviour
    {
        public Rigidbody projectile;
        public Vector3 offset = Vector3.forward;
        [Range(0, 100)] public float velocity = 10;

        [ContextMenu("Fire")]
        public void Fire()
        {
            var body = Instantiate(
                projectile,
                transform.TransformPoint(offset),
                transform.rotation);
            body.velocity = Vector3.forward * velocity;
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(Launcher))]
    public class LauncherEditor : Editor
    {
        [DrawGizmo(GizmoType.Pickable | GizmoType.Selected)]
        static void DrawGizmosSelected(Launcher launcher, GizmoType gizmoType)
        {
            {
                var offsetPosition = launcher.transform.TransformPoint(launcher.offset);
                Handles.DrawDottedLine(launcher.transform.position, offsetPosition, 3);
                Handles.Label(offsetPosition, "Offset");
                if (launcher.projectile != null)
                {
                    var positions = new List<Vector3>();
                    var velocity = launcher.transform.forward * 
                                   launcher.velocity / 
                                   launcher.projectile.mass;
                    var position = offsetPosition;
                    var physicsStep = 0.1f;
                    for (var i = 0f; i <= 1f; i += physicsStep)
                    {
                        positions.Add(position);
                        position += velocity * physicsStep;
                        velocity += Physics.gravity * physicsStep;
                    }
                    using (new Handles.DrawingScope(Color.yellow))
                    {
                        Handles.DrawAAPolyLine(positions.ToArray());
                        Gizmos.DrawWireSphere(positions[positions.Count - 1], 0.125f);
                        Handles.Label(positions[positions.Count - 1], "Estimated Position (1 sec)");
                    }
                }
            }
        }

        void OnSceneGUI()
        {
            var launcher = target as Launcher;
            var transform = launcher.transform;

            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                var newOffset = transform.InverseTransformPoint(

                    Handles.PositionHandle(
                        transform.TransformPoint(launcher.offset),
                        transform.rotation));

                if (cc.changed)
                {
                    Undo.RecordObject(launcher, "Offset Change");
                    launcher.offset = newOffset;
                }
            }

            Handles.BeginGUI();
            var rectMin = Camera.current.WorldToScreenPoint(
                launcher.transform.position +
                launcher.offset);
            var rect = new Rect();
            rect.xMin = rectMin.x;
            rect.yMin = SceneView.currentDrawingSceneView.position.height -
                        rectMin.y;
            rect.width = 64;
            rect.height = 18;
            GUILayout.BeginArea(rect);
            using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Fire"))
                    launcher.Fire();
            }

            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }

#endif
}