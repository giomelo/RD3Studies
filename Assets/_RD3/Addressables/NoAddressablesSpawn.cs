using UnityEngine;

namespace _RD3
{
    public class NoAddressablesSpawn : MonoBehaviour
    {
        [SerializeField]private GameObject _objectToSpawn;
        private 
        void Start()
        {
            Instantiate(_objectToSpawn);
        }
    
    }
}
