using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _RD3.RNA._Scripts
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;

        public float energyToGain;
        public float reproductionEnergyToGain;
        public float reproductionEnergyThreshold;
        
        public Spawner creatureSpawner;
        
        [SerializeField]private Spawner foodSpawner;
        [SerializeField]private float initialFoodCount = 100;
        [SerializeField]private float timeElapsed;
        [SerializeField]private float spawnRate = 1;

        public Action onCreatureDied;

        private void Awake()
        {
            if(_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
        }

        private void Start()
        {
            for(int i = 0; i < initialFoodCount; i++)
                foodSpawner.SpawnRandom();
        }

        private void OnEnable()
        {
            onCreatureDied += () =>
            {
                creatureSpawner.SpawnRandom();
            };
        }
        
        private void OnDisable()
        {
            onCreatureDied -= () =>
            {
                creatureSpawner.SpawnRandom();
            };
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;
            if (!(timeElapsed >= spawnRate)) return;
            
            timeElapsed = 0;
            foodSpawner.SpawnRandom();
        }
    }
}