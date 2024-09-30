using UnityEngine;

public class RunCustomAction : MonoBehaviour
{
    [SerializeField]private CustomAction customAction;
    [SerializeField]private SO_VoidEvent eVoidEvent;
    
    
    private void Start()
    {
        customAction.Execute();
        eVoidEvent.AddListener(()=> Debug.Log("Event invoked")); 
    }
}
