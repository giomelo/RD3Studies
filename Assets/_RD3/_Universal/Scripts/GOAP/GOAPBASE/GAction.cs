using System;
using System.Collections.Generic;
using System.Linq;
using _RD3._Universal.Scripts.GOAP.GOAPBASE;
using UnityEngine;
using UnityEngine.AI;

namespace _RD3._Universal.Scripts.GOAP
{
    public abstract class GAction : MonoBehaviour
    {
        public string actionName = "Action";
        public float cost = 1.0f;
        public GameObject target;
        public string targetTag;
        public float duration;
        public WorldStateData[] preConditions;
        public WorldStateData[] afterEffects;
        public NavMeshAgent agent;
        
        public Dictionary<string, int> preconditions = new();
        public Dictionary<string, int> effects = new();

        public WorldState agentBeliefs;
        
        public bool isRunning;
        public GIventory inventory;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();

            if (preConditions == null) return;
            foreach (var w in preConditions)
                preconditions.Add(w.key, w.value);
            
            
            if (afterEffects == null) return;
            foreach (var w in afterEffects)
                effects.Add(w.key, w.value);

            inventory = this.GetComponent<GAgent>().inventory;
            agentBeliefs = GetComponent<GAgent>().beliefs;

        }

        public bool IsAchievable()
        {
            return true;
        }


        public bool IsAchievableGiven(Dictionary<string, int> conditions)
        {
            return preconditions.All(p => conditions.ContainsKey(p.Key));
        }

        public abstract bool PrePerform();
        public abstract bool PostPerform();
        
    }
}
