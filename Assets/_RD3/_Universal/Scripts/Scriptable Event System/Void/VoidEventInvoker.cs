using UnityEditor;
using UnityEngine;

public class VoidEventInvoker : MonoBehaviour
{
    #region Variables

    [SerializeField] private SO_VoidEvent voidEvent;

    #endregion

    #region Methods
    

    public void InvokeEvent()
    {
        Debug.LogWarning("EVENT INVOKE");
        voidEvent.Invoke();
    }

    #endregion
}

#if UNITY_EDITOR

[CustomEditor(typeof(VoidEventInvoker))]
public class InvokeEventEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        VoidEventInvoker invoker = (VoidEventInvoker)target;
        
        if (GUILayout.Button("InvokeEvent"))
        {
            invoker.InvokeEvent();    
        }
    }
}

#endif
