namespace BTree.BehaviourTree
{
    public class Loop : Node
    {
        private BehaviourTree _tree;
        
        public Loop(string n, BehaviourTree d) 
        {
            name = n;
            _tree = d;
        }

        public override Status Process()
        {
            if (_tree.Process() == Status.FAILURE)
            {
                return Status.SUCCESS;
            }

            Status childstatus = children[currentChild].Process();
            if (childstatus == Status.RUNNING) return Status.RUNNING;
            if (childstatus == Status.FAILURE)
            {
                return childstatus;
            }

            currentChild++;
            if (currentChild >= children.Count)
            {
                currentChild = 0;
            }

            return Status.RUNNING;
        }

    }
}