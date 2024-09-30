using UnityEditor;
using UnityEngine;

namespace _RD3.LaucherHandles
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [HideInInspector] new public Rigidbody rigidbody;
        public float damageRadius = 1;

        void Reset()
        {
            rigidbody = GetComponent<Rigidbody>();
        }
    }
    
    #if UNITY_EDITOR
    
    [CustomEditor(typeof(Projectile))]
    public class ProjectileEditor : Editor
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmosSelected(Projectile projectile, GizmoType gizmoType)
        {
            Gizmos.DrawSphere(projectile.transform.position, 0.125f);
        }

        void OnSceneGUI()
        {
            var projectile = target as Projectile;
            var transform = projectile.transform;
            projectile.damageRadius = Handles.RadiusHandle(
                transform.rotation, 
                transform.position, 
                projectile.damageRadius);
        }
    }
    #endif
}
