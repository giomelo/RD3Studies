using UnityEngine;
using UnityEngine.Events;

public class VoidEventListener : MonoBehaviour
{
    #region Variables

    [SerializeField] private SO_VoidEvent voidEvent;

    #endregion

    #region Events

    public UnityEvent OnEvent;

    #endregion

    #region Monobehaviour

    private void OnEnable()
    {
        SubscribeToEvent();
    }

    private void OnDisable()
    {
        UnsubscribeToEvent();
    }

    #endregion

    #region Methods

    private void SubscribeToEvent()
    {
        voidEvent.AddListener(HandleEvent);
    }
    
    private void UnsubscribeToEvent()
    {
        voidEvent.RemoveListener(HandleEvent);
    }

    private void HandleEvent()
    {
        OnEvent?.Invoke();
    }

    #endregion
}