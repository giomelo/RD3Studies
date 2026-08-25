using System;
using System.Collections.Generic;
using System.Linq;

namespace _RD3.ServiceLocator
{
    public interface IService
    {
    }
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> _services = new();

        public static void Register<T>(T service) where T : class, IService
        {
            var serviceType = service.GetType();

            var serviceInterface = serviceType
                .GetInterfaces()
                .FirstOrDefault(i =>
                    i != typeof(IService) &&
                    typeof(IService).IsAssignableFrom(i));

            if (serviceInterface != null)
            {
                _services[serviceInterface] = service;
            }
            else
            {
                _services[serviceType] = service;
            }
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
