using System.Collections.Generic;
using UnityEngine;

namespace BTree.Scripts
{
    struct NodeLevel
    {
        public Node node;
        public int level;
    }
    public class BehaviourTree : Node
    {
        public BehaviourTree()
        {
            name = "Tree";
        }

        public BehaviourTree(string n)
        {
            name = n;
        }
        

        public override Status Process()
        {
            if (children.Count == 0) return Status.SUCCESS;
            return children[currentChildIndex].Process();
        }
        public void PrintTree()
        {
            PrintNode(this);

            string treePrintoOut = "";
            Stack<NodeLevel> nodeStack = new Stack<NodeLevel>();
            Node currentNode = this;
            nodeStack.Push(new NodeLevel { level = 0, node = currentNode });

            while (nodeStack.Count != 0)
            {
                NodeLevel nextNode = nodeStack.Pop();
                treePrintoOut += new string('-', nextNode.level) +  nextNode.node.name + " \n ";
                for(int i = nextNode.node.children.Count - 1; i >= 0; i--)
                {
                    nodeStack.Push(new NodeLevel{level = nextNode.level + 1, node = nextNode.node.children[i]});
                }
            }
            
            Debug.Log(treePrintoOut);
        }

        public void PrintNode(Node node)
        {
            node.PrintNode();
            if(node.children.Count > 0)
            {
                Debug.Log("Childs");
                foreach (var child in node.children)
                    PrintNode(child);
            }
        }
    }
}
