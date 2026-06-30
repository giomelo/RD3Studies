using System;
using UnityEngine;

namespace _RD3.ServiceLocator
{
    [DefaultExecutionOrder(-1)]
    public class Bootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            ServiceLocator.Register(new AudioSystem());
        }
    }
}