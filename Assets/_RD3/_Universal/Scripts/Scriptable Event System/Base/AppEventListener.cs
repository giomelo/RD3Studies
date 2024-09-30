using UnityEngine;
using UnityEngine.Events;

public class AppEventListener<T> : MonoBehaviour
{
    #region Variables

    [SerializeField] private AppEvent<T> appEvent;

    #endregion

    #region Events

    public UnityEvent<T> OnEvent;

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

    protected virtual void SubscribeToEvent()
    {
        appEvent.AddListener(HandleEvent);
    }
    
    protected virtual void UnsubscribeToEvent()
    {
        appEvent.RemoveListener(HandleEvent);
    }

    protected virtual void HandleEvent(T value)
    {
        OnEvent.Invoke(value);
    }

    #endregion
}