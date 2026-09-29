using Unity.Entities;
using Unity.Mathematics;

namespace _RD3.ECS
{
    public struct SheepData : IComponentData
    {
        public float speed;
        public float3 direction;
    }
}