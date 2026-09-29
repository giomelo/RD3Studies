using UnityEngine;

namespace _RD3.CommandPattern
{
    public class PlayerMovement : MonoBehaviour
    {
        public void Move(Vector3 direction)
        {
            transform.Translate(direction * Time.deltaTime);
        }
        
        public void Scale(Vector3 scale)
        {
            transform.localScale += scale * Time.deltaTime;
        }
    }
}
