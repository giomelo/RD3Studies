namespace BTree.Scripts
{
    public class Selector : Node
    {
        public Selector(string name)
        {
            this.name = name;
        }
        
        public Selector() { }

        public override Status Process()
        {
            Status childStatus = children[currentChildIndex].Process();
            if (childStatus == Status.RUNNING) return Status.RUNNING;
            
            if (childStatus == Status.SUCCESS)
            {
                currentChildIndex = 0;
                return Status.SUCCESS;
            }

            currentChildIndex++;
            if (currentChildIndex >= children.Count)
            {
                currentChildIndex = 0;
                return Status.FAILURE;
            }

            return Status.RUNNING;
        }
    }
}
