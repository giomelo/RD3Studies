using UnityEngine;

namespace _RD3._Universal.Scripts.GOAP
{
    public sealed class GWorld
    {
        public static GWorld instance { get; } = new GWorld();
        private static WorldState _world;
        public WorldState GetWorld => _world;
        
        static GWorld()
        {
            _world = new WorldState();
        }
        
        private GWorld(){}
      
    }
}
