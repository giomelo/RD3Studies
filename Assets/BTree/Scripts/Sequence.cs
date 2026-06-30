using System.Security.Permissions;
using UnityEngine;

namespace BTree.Scripts
{
    public class Sequence : Node
    {
        public Sequence(string n)
        {
            name = n;
        }

        public override Status Process()
        {
            Debug.Log("Processing Sequence: " + name + ", Current Child Index: " + children[currentChildIndex].name);
            Status childStatus = children[currentChildIndex].Process();

            if (childStatus == Status.RUNNING) return Status.RUNNING;

            if (childStatus == Status.FAILURE)
                return childStatus;
            
            currentChildIndex++;
            
            if(currentChildIndex >= children.Count)
            {
                currentChildIndex = 0;
                return Status.SUCCESS;
            }
            return Status.RUNNING;
        }
        
    }
}
