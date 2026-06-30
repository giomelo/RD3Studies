using System;
using System.Collections.Generic;

namespace _RD3.ServiceLocator
{
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out object service))
            {
                return service as T;
            }

            throw new Exception($"Service {typeof(T).Name} not found");
        }

        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }
    
        
    }
}
