using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _RD3._Universal.Scripts.GOAP
{

    public class SubGoal
    {
        public Dictionary<string, int> subGoals;
        public bool shouldBeRemoved;

        public SubGoal(string s, int i, bool r)
        {
            subGoals = new Dictionary<string, int>();
            subGoals.Add(s, i);
            shouldBeRemoved = r;
        }
    }
    
    public class GAgent : MonoBehaviour
    {
        public List<GAction> actions = new() { };
        public Dictionary<SubGoal, int> goals = new();

        private GPlanner _planner;
        private Queue<GAction> _actionsQueue;

        public GAction currentAction;
        private SubGoal _currentSubGoal;
        public void Start()
        {
            GAction[] acts = GetComponents<GAction>();

            foreach (var gAction in acts)
            {
                actions.Add(gAction);
            }
        }

        private bool _invoked;
        private void LateUpdate()
        {
            
            if (currentAction != null && currentAction.isRunning)
            { 
                if (currentAction.agent.hasPath && currentAction.agent.remainingDistance < 1f)
                {
                    if (!_invoked)
                    {
                        Invoke("CompleteAction", currentAction.duration);
                        _invoked = true;
                    }
                }
                
                return;
            }
            
            if (_planner == null || _actionsQueue == null)
            {
                _planner = new GPlanner();

                var sortedGoals = from entry in goals orderby entry.Value descending select entry;

                foreach (var sg in sortedGoals)
                {
                    _actionsQueue = _planner.Plan(actions, sg.Key.subGoals, null);

                    if (_actionsQueue != null)
                    {
                        _currentSubGoal = sg.Key;
                    }
                }
            }

            if (_actionsQueue != null && _actionsQueue.Count == 0)
            {
                if (_currentSubGoal.shouldBeRemoved)
                {
                    goals.Remove(_currentSubGoal);
                }

                _planner = null;
            }

            if (_actionsQueue != null && _actionsQueue.Count > 0)
            {
                currentAction = _actionsQueue.Dequeue();
                if (currentAction.PrePerform())
                {
                    if (currentAction.target == null && currentAction.targetTag != "")
                    {
                        currentAction.target = GameObject.FindWithTag(currentAction.targetTag);
                    }
                    

                    if (currentAction.target == null) return;
                    currentAction.isRunning = true;
                    currentAction.agent.SetDestination(currentAction.target.transform.position);
                }else
                {
                    _actionsQueue = null;
                }
            }
        }

        private void CompleteAction()
        {
            currentAction.isRunning = false;
            currentAction.PostPerform();
            _invoked = false;
        }
    }
}
