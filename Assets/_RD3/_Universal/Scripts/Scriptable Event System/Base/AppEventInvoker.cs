using UnityEngine;

public class AppEventInvoker<T> : MonoBehaviour
{
    #region Variables

    [SerializeField] private AppEvent<T> appEvent;

    #endregion

    #region Methods

    public void InvokeEvent(T param)
    {
        appEvent.Invoke(param);
    }

    #endregion
}