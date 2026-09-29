using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using System;
using Random = Unity.Mathematics.Random;

namespace _RD3.ECS
{
    public partial struct SheepSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SheepSpawnConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            SheepSpawnConfig config =
                SystemAPI.GetSingleton<SheepSpawnConfig>();

            
            Random random = new Random(12345);
            for (int i = 0; i < config.Count; i++)
            {
                Entity sheep = state.EntityManager.Instantiate(config.Prefab);
                
      
                float3 position = new float3(
                    random.NextFloat(-50f, 50f),
                    0f,
                    random.NextFloat(-50f, 50f)
                );

                SystemAPI.SetComponent(
                    sheep,
                    LocalTransform.FromPosition(position)
                );
            }

            // Remove a configuração depois do primeiro spawn
            Entity configEntity =
                SystemAPI.GetSingletonEntity<SheepSpawnConfig>();

            state.EntityManager.RemoveComponent<SheepSpawnConfig>(
                configEntity
            );
        }
    }
}