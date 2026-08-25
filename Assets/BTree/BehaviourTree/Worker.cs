using UnityEngine;

namespace BTree.BehaviourTree
{
    public class Worker : BTAgent
    {
        [SerializeField]private Transform office;
        
        public override void Start()
        {
            base.Start();

            Leaf goToPatron = new Leaf("Go To Patron", GoToPatron);
            Leaf goToOffice = new Leaf("Go To Office", GoToOffice);
            
            
            Leaf allocatePatron = new Leaf("Allocate Patron", AllocatePatron);
            Leaf patronWaiting = new Leaf("Patron waiting", PatronWaiting);
            
            Sequence sequence = new Sequence("Allocate Patron");
            sequence.AddChild(allocatePatron);
            
            
            BehaviourTree waiting = new BehaviourTree();
            waiting.AddChild(patronWaiting);
            DepSequence moveToPatron = new DepSequence("Move to Patron", waiting, agent);
            moveToPatron.AddChild(goToPatron);
            
            
            Selector work = new Selector("Work");
            work.AddChild(sequence);  
            work.AddChild(goToOffice);
            
            tree.AddChild(work);
            
        }

        public Node.Status PatronWaiting()
        {
            if (patron == null) return Node.Status.FAILURE;

            if (patron.isWaitingForTicket)
            {
                return Node.Status.SUCCESS;
            }
            
            return Node.Status.FAILURE;
        }
        
        private PatronBehaviour patron;

        public Node.Status AllocatePatron()
        {
            if (BlackBoard.Instance.patrons.Count == 0) return Node.Status.FAILURE;
            
            patron = BlackBoard.Instance.patrons.Pop();
            if (patron == null) return Node.Status.FAILURE;

            return Node.Status.SUCCESS;
        }
        public Node.Status GoToPatron()
        {
            if (patron == null) return Node.Status.FAILURE;
            var s = GoToLocation(patron.transform.position);
            if(s == Node.Status.SUCCESS)
            {
                patron.hasTicked = true;
                patron = null;
            }

            return s;

        }

        public Node.Status GoToOffice()
        {
            patron = null;
            return GoToLocation(office.position);
            
        }
    }
}
