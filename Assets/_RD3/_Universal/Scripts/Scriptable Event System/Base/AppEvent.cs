using System;
using UnityEngine;

public class AppEvent<T> : ScriptableObject
{
    #region Event
    
    private event Action<T> _action = delegate { };

    #endregion

    #region Methods

    public virtual void Invoke(T value)
    {
        _action?.Invoke(value);
    }

    public virtual void AddListener(Action<T>  listener)
    {
        _action -= listener;
        _action += listener;
    }

    public virtual void RemoveListener(Action<T>  listener)
    {
        _action -= listener;
    }

    #endregion
}