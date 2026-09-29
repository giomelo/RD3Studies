using UnityEngine;

namespace _RD3.ECS
{
    public class Sheep : MonoBehaviour
    {
  
        // Update is called once per frame
        void Update()
        {
            transform.Translate(0,0,0.1f);
            if(transform.position.z > 50)
                transform.position = new Vector3(Random.Range(-50,50), 0, -50);
        }
    }
}
