using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace _RD3.ECS
{
    public class SheepAuthoring : MonoBehaviour
    {
        public float speed;
        public float3 direction;

        private class Baker : Baker<SheepAuthoring>
        {
            public override void Bake(SheepAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);



                AddComponent(entity, new SheepData
                {
                    speed = authoring.speed,
                    direction = authoring.direction

                });
            }
        }
    }
}