using System.Security.Permissions;
using UnityEngine;

namespace BTree.Scripts
{
    public class Inverter : Node
    {
        public Inverter(string n)
        {
            name = n;
        }

        public override Status Process()
        {
            Debug.Log("Processing Sequence: " + name + ", Current Child Index: " + children[currentChildIndex].name);
            Status childStatus = children[currentChildIndex].Process();

            if (childStatus == Status.RUNNING) return Status.RUNNING;

            if (childStatus == Status.FAILURE)
                return Status.SUCCESS;
            
            return Status.FAILURE;
        }
        
    }
}
