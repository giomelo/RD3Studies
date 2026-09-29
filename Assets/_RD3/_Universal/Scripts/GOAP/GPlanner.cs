using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace _RD3._Universal.Scripts.GOAP
{
    public class Node
    {
        public Node parent;
        public float cost;
        public Dictionary<string, int> states;
        public GAction action;

        public Node(Node parent, float cost, Dictionary<string, int> allStates, GAction action)
        {
            this.parent = parent;
            this.cost = cost;
            this.states = new Dictionary<string, int>(allStates);
            this.action = action;
        }
    }
    
    public class GPlanner
    {
        public Queue<GAction> Plan(List<GAction> actions, Dictionary<string, int> goal, WorldState states)
        {
            var usableActions = new List<GAction>();
            foreach (var action in actions)
            {
                if(action.IsAchievable())
                    usableActions.Add(action);
            }

            List<Node> leaves = new List<Node>();
            Node start = new Node(null, 0, GWorld.instance.GetWorld.GetStates(), null);
            bool success = BuildGraph(start, leaves, usableActions, goal);

            if (!success)
            {
                Debug.Log("NO PLAN");
                return null;
            }

            Node cheapest = null;

            foreach (var leaf in leaves)
            {
                if (cheapest == null || leaf.cost < cheapest.cost)
                    cheapest = leaf;
            }

            List<GAction> resultPlan = new List<GAction>();
            Node n = cheapest;

            while (n != null)
            {
                if(n.action != null)
                    resultPlan.Insert(0, n.action);

                n = n.parent;
            }

            Queue<GAction> queue = new Queue<GAction>();
            foreach (var gAction in resultPlan)
            {
                queue.Enqueue(gAction);
            }
            Debug.Log("The plan is: " );
            foreach (var gAction in queue)
            {
                Debug.Log("Q: " + gAction.actionName);
            }
            return queue;
        }

        private bool BuildGraph(Node parent, List<Node> leaves, List<GAction> usableActions,
            Dictionary<string, int> goal)
        {
            bool foundPath = false;

            foreach (var usableAction in usableActions)
            {
                if (usableAction.IsAchievableGiven(parent.states))
                {
                    Dictionary<string, int> currentState = new Dictionary<string, int>(parent.states);
                    foreach (var usableActionEffect in usableAction.effects)
                    {
                        if(!currentState.ContainsKey(usableActionEffect.Key))
                            currentState.Add(usableActionEffect.Key, usableActionEffect.Value);
                    }

                    Node node = new Node(parent, parent.cost + usableAction.cost, currentState, usableAction);

                    if (GoalAchieved(goal, currentState))
                    {
                        leaves.Add(node);
                        foundPath = true;
                    }
                    else
                    {
                        List<GAction> subSet = ActionSubset(usableActions, usableAction);
                        bool found = BuildGraph(node, leaves, subSet, goal);
                        if (found)
                            foundPath = true;
                    }
                }
            }

            return foundPath;
        }

        private bool GoalAchieved(Dictionary<string, int> goal, Dictionary<string, int> state)
        {
            foreach (var g in goal)
            {
                if (!state.ContainsKey(g.Key))
                    return false;
            }

            return true;
        }
        
        private List<GAction> ActionSubset(List<GAction> actions, GAction removeMe)
        {
            List<GAction> subset = new List<GAction>();
            foreach (var gAction in actions)
            {
                if(!gAction.Equals(removeMe))
                    subset.Add(gAction);
            }

            return subset;
        }
    }
}
