using Unity.Entities;
using UnityEngine;

namespace _RD3.ECS
{
    public class ECSManager : MonoBehaviour
    {
        [SerializeField] private GameObject _sheepPrefab;
        [SerializeField] private int _numberOfSheep = 10;

        public class Baker : Baker<ECSManager>
        {
            public override void Bake(ECSManager authoring)
            {
                Debug.Log("=== BAKING ECS MANAGER ===");

                Entity sheepEntity = GetEntity(
                    authoring._sheepPrefab,
                    TransformUsageFlags.Dynamic
                );

                Debug.Log($"Sheep Entity: {sheepEntity}");

                Entity configEntity = GetEntity(
                    TransformUsageFlags.None
                );

                AddComponent(configEntity, new SheepSpawnConfig
                {
                    Prefab = sheepEntity,
                    Count = authoring._numberOfSheep
                });
            
            }
        }
    }

    public struct SheepSpawnConfig : IComponentData
    {
        public Entity Prefab;
        public int Count;
    }
}