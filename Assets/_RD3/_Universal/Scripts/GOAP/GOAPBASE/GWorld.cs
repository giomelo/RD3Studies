using System.Collections.Generic;
using UnityEngine;

namespace _RD3._Universal.Scripts.GOAP
{
    public sealed class GWorld
    {
        public static GWorld instance { get; } = new GWorld();
        private static WorldState _world;
        public WorldState GetWorld => _world;
        private static Queue<GameObject> _agents;
        private static Queue<GameObject> _cubicles;
        
        static GWorld()
        {
            _world = new WorldState();
            _agents = new Queue<GameObject>();
            _cubicles = new Queue<GameObject>();
            
            GameObject[] cubicles = GameObject.FindGameObjectsWithTag("Cubicle");
            foreach (var cubicle in cubicles)
            {
                _cubicles.Enqueue(cubicle);
            }
            
            
            if(_cubicles.Count > 0)
                _world.ModifyState("FreeCubicles", _cubicles.Count);
            
        }
        
        private GWorld(){}

        public void AddPatient(GameObject p)
        {
            _agents.Enqueue(p);
        }
        
        public void AddCubicles(GameObject p)
        {
            _cubicles.Enqueue(p);
        }
        public GameObject RemoveCubicle()
        {
            if (_cubicles.Count == 0) return null;
            return _cubicles.Dequeue();
        }
        
        public GameObject RemovePatient()
        {
            if (_agents.Count == 0) return null;
            return _agents.Dequeue();
        }
      
    }
}
