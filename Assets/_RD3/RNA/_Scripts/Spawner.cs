using UnityEngine;
using Random = UnityEngine.Random;

namespace _RD3.RNA._Scripts
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField]private int floorScale = 4;
        [SerializeField]private GameObject objectToSpawn;
        
        private Transform _objectsParent;

        public GameObject SpawnRandom()
        {
            var x = Random.Range(-5, 5) * floorScale;
            var z = Random.Range(-5, 5) * floorScale;
            return Spawn(new Vector3(x,0,z));
        }

        public GameObject Spawn(Vector3 position)
        {
            if (_objectsParent == null) _objectsParent = new GameObject($"{objectToSpawn} Parent").transform;
            return Instantiate(objectToSpawn, position, Quaternion.identity, _objectsParent);
        }
    }
}
