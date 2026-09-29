using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _RD3.ECS
{
    public partial struct SheepMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SheepData>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var job = new SheepJob
            {
                deltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
           // job.ScheduleParallel();
        }

        public partial struct SheepJob : IJobEntity
        {
            public float deltaTime;

            private void Execute(ref SheepData sheepData, ref LocalTransform transform)
            {
                UnityEngine.Debug.Log(
                    $"Direction: {sheepData.direction} | Speed: {sheepData.speed}"
                );
               // transform.Position += new float3(0, 10, 0) * deltaTime;
              //  transform.Position = new Unity.Mathematics.float3(0, 0, 100);
                // Atualiza a posição do carneiro com base na velocidade e no tempo decorrido
               // transform = transform.Translate(sheepData.direction * sheepData.speed * deltaTime);
            }
        }
    }
}