using UnityEngine;

namespace _RD3._Universal.Scripts.GOAP
{
    public class GoToHospitalAction : GAction
    {
        public override bool PrePerform()
        {
           // throw new System.NotImplementedException();
           Debug.Log("Go");
           return true;
        }

        public override bool PostPerform()
        {
          //  throw new System.NotImplementedException();
          return true;
        }
    }
}