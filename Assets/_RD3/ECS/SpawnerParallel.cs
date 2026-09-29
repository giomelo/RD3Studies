using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace _RD3.ECS
{
    public class SpawnerParallel : MonoBehaviour
    {
        [SerializeField]private GameObject sheepPrefab;
        [SerializeField]private int numberOfSheepToSpawn = 1000;

        private Transform[] _allSheep;
        
        struct MoveParallel : IJobParallelForTransform
        {
            public void Execute(int index, TransformAccess transform)
            {
                transform.position += 0.1f * (transform.rotation * new Vector3(0, 0, 1f));
                if (transform.position.z > 50)
                    transform.position = new Vector3(transform.position.x, 0, -50);
            }
        }

        private MoveParallel _moveJob;
        private JobHandle _moveJobHandle;
        private TransformAccessArray _transformAccessArray;
        private void Start()
        {
            _allSheep = new Transform[numberOfSheepToSpawn];
            for (int i = 0; i < numberOfSheepToSpawn; i++)
            {
                var pos = new Vector3(Random.Range(-50,50), 0, Random.Range(-50,50));
                _allSheep[i] = Instantiate(sheepPrefab, pos, Quaternion.identity).transform;
            }

            _transformAccessArray = new TransformAccessArray(_allSheep);
        }

        private void Update()
        {
            // cant create ranodm numbers in ecs parallel job, so we just move the sheep forward and reset them when they reach a certain point
            /*for (int i = 0; i < _allSheep.Length; i++)
            {
                _allSheep[i].Translate(0,0,0.1f);
                if(_allSheep[i].position.z > 50)
                    _allSheep[i].position = new Vector3(_allSheep[i].position.x, 0, -50);
            }*/
            
            _moveJob = new MoveParallel();
            _moveJobHandle = _moveJob.Schedule(_transformAccessArray);
        }

        private void LateUpdate()
        {
            _moveJobHandle.Complete();
        }

        /*private void OnDestroy()
        {
            transform.Dispose();
        }*/
    }
}
