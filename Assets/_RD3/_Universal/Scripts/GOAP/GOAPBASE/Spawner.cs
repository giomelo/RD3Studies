using System;
using UnityEngine;
using Random = UnityEngine.Random;


namespace _RD3._Universal.Scripts.GOAP.GOAPBASE
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField]private GameObject _patientPrefab;
        [SerializeField]private int _numOfPatients = 10;

        private void Start()
        {
            for (int i = 0; i < _numOfPatients; i++)
            {
                Invoke("SpawnPatient", 5);
            }
        }
        
        void SpawnPatient()
        {
            Instantiate(_patientPrefab, transform.position, Quaternion.identity);
            Invoke("SpawnPatient", Random.Range(2,10));
        }
    }
}
