using System;
using UnityEngine;

namespace _RD3.Disposable
{
    public class DisposableLockKeyboardInput : IDisposable
    {
       
        public DisposableLockKeyboardInput()
        {
            //KeyboardInputService.LockInput();
        }
        
        public void Dispose()
        {
            //KeyboardInputService.UnLockInput();
        }
    }
}
