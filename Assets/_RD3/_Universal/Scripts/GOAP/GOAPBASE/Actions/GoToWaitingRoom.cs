using UnityEngine;

namespace _RD3._Universal.Scripts.GOAP
{
    public class GoToWaitingRoom : GAction
    {
        public override bool PrePerform()
        {
           Debug.Log("Go");
           return true;
        }

        public override bool PostPerform()
        {
          GWorld.instance.GetWorld.ModifyState("Waiting", 1);
          GWorld.instance.AddPatient(gameObject);
          agentBeliefs.ModifyState("atHospital", 1);
          return true;
        }
    }
}