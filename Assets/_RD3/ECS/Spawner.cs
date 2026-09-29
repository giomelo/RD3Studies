using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _RD3.ECS
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField]private GameObject sheepPrefab;
        [SerializeField]private int numberOfSheepToSpawn = 1000;

        private void Start()
        {
            for (int i = 0; i < numberOfSheepToSpawn; i++)
            {
                var pos = new Vector3(Random.Range(-50,50), 0, Random.Range(-50,50));
                Instantiate(sheepPrefab, pos, Quaternion.identity);
            }
        }
    }
}
