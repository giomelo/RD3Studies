using System;
using System.Collections.Generic;
using UnityEngine;

namespace _RD3._Universal.Scripts.GOAP
{
    [Serializable]
    public class WorldStateData
    {
        public string key;
        public int value;
        
    }
    
    
    public class WorldState
    {
        private Dictionary<string, int> _states = new();

        public bool HasState(string key)
        {
            return _states.ContainsKey(key);
        }

        public Dictionary<string, int> GetStates()
        {
            return _states;
        }
        public int GetState(string key)
        {
            return _states.GetValueOrDefault(key, 0);
        }
        
        public void AddState(string key, int value)
        {
            _states.Add(key, value);
        }

        public void ModifyState(string key, int value)
        {
            if (_states.ContainsKey(key))
            {
                _states[key] += value;
                if (_states[key] <= 0)
                    RemoveState(key);
            }
            else
            {
                AddState(key, value);
            }
        }

        public void RemoveState(string key)
        {
            _states.Remove(key);
        }

        public void SetState(string key, int value)
        {
            if (_states.ContainsKey(key))
                _states[key] = value;
            else
                AddState(key, value);
        }
    }
}
