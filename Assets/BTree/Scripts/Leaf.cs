using System;

namespace BTree.Scripts
{
    public class Leaf : Node
    {
        public delegate Status Tick();

        public Tick ProcessMethod;

        public Action onComplete;
        
        public Leaf(string name, Tick pm, Action action = null)
        {
            this.name = name;
            ProcessMethod = pm;
            onComplete = action;
        }
        
        public Leaf() { }

        public override Status Process()
        {
            if (ProcessMethod != null)
            {
                var status = ProcessMethod();
                
                if(status == Status.SUCCESS)
                    onComplete?.Invoke();
                
                return status;
            }
           
            return Status.FAILURE;
        }
    }
}
