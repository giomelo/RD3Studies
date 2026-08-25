using System;
using System.Collections.Generic;
using UnityEngine;

namespace _RD3.ServiceLocator
{
    [DefaultExecutionOrder(-1)]
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField]private List<MonoBehaviour> _services = new List<MonoBehaviour>();
        
        [SerializeField]private bool _installFromlist;
        private void Awake()
        {
           

            if (_installFromlist)
            {
                foreach (var monoBehaviour in _services)
                {
                    if (monoBehaviour is not IService service)
                        continue;

                    ServiceLocator.Register(service);
                }
            }
            else
            {
                ServiceLocator.Register(new AudioSystem());
            }
        }
    }
}