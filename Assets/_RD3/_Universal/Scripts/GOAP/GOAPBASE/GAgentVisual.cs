using UnityEngine;

namespace _RD3._Universal.Scripts.GOAP.GOAPBASE
{
    [ExecuteInEditMode]
    public class GAgentVisual : MonoBehaviour
    {
        public GAgent thisAgent;

        // Start is called before the first frame update
        void Start()
        {
            thisAgent = this.GetComponent<GAgent>();
        }
        
    }
}
