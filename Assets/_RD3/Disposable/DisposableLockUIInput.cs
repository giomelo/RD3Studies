using System;
using UnityEngine;

namespace _RD3.Disposable
{
    public class DisposableLockUIInput : IDisposable
    {
        public CanvasGroup _canvas;

        public DisposableLockUIInput(CanvasGroup uiElement)
        {
            _canvas = uiElement;
            _canvas.interactable = false;
        }
        
        public void Dispose()
        {
            _canvas.interactable = true;
        }
    }
}