using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace _RD3.CustomPath.Scripts
{
    public class RunCustomAction : MonoBehaviour
    {
        [SerializeField]private CustomAction customAction;

        private Queue<CustomAction> actions = new();
        private int currentIndex;
    
        public void Execute()
        {
            customAction.Execute();
        }
    
        #if UNITY_EDITOR
        
        private void OnDrawGizmos()
        {
            var sceneCamera = SceneView.currentDrawingSceneView.camera;

            if (Vector3.Distance(sceneCamera.transform.position, transform.position) > 50)
                return;

            if (customAction != null)
                Handles.DrawDottedLine(transform.position, customAction.transform.position,5);
        }
        #endif

        public void SetActions()
        {
            if(actions!=null) actions.Clear();
            CustomAction currentAction = customAction;
            while (currentAction != null)
            {
                actions.Enqueue(currentAction);
                currentAction = currentAction.GetCurrentAction().onCompleteAction;
            } 
        }

        private GameObject _currentObject;
        #if UNITY_EDITOR
        public void AddAction()
        {
            EditorUtility.SetDirty(this);
            var action = Instantiate(Resources.Load("CustomAction")) as GameObject;
            CustomAction lastAction = null;
            var actionScript = action.GetComponent<CustomAction>();
            actionScript.customAction = this;
        
            if (actions.Count != 0)
                lastAction = actions.Last();
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
            actions.Enqueue(actionScript);
        }
    
        public void RemoveAction()
        {
            EditorUtility.SetDirty(this);
            var action = actions.Last();
            actions.Dequeue();
            DestroyImmediate(action.gameObject);
            if(actions.Count > 0) actions.Last().GetCurrentAction().onCompleteAction = null;
        }
    #endif
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
            
            serializedObject.Update();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("ADD ACTION"))
            {
                _customAction.AddAction();
                Repaint();
            }


            if (GUILayout.Button("REMOVE ACTION"))
            {
                _customAction.RemoveAction();
                Repaint();
            }
            
            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
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
}