using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace BTree.Scripts
{
    
    public class BTAgent : MonoBehaviour
    {
        public BehaviourTree tree;
        public NavMeshAgent agent;

        public ActionState actionState = ActionState.Idle;
        public Node.Status treeStatus = Node.Status.RUNNING;
        private WaitForSeconds _waitForSeconds;
        
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            _waitForSeconds = new WaitForSeconds(Random.Range(0.1f, 1f));
        }

        public void Start()
        {
            tree = new BehaviourTree();
            StartCoroutine(Behave());
        }

        private IEnumerator Behave()
        {
            while (true)
            {
                treeStatus = tree.Process();
                yield return _waitForSeconds;
            }
        }
        
        public Node.Status GoToLocation(Vector3 destination)
        {
            if (actionState == ActionState.Idle)
            {
                agent.SetDestination(destination);
                actionState = ActionState.Working;
            }
            else if (Vector3.Distance(agent.pathEndPosition, destination) >=2)
            {
                actionState = ActionState.Idle;
                return Node.Status.FAILURE;
            }else if (Vector3.Distance(transform.position, destination) < 2)
            {
                actionState = ActionState.Idle;
                return Node.Status.SUCCESS;
            }

            return Node.Status.RUNNING;
        }

        
    }
}
