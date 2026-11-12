using UnityEngine;

namespace _RD3._Universal.Scripts.GOAP
{
    public class GetPatient : GAction
    {
        private GameObject _resource;
        public override bool PrePerform()
        {
            target = GWorld.instance.RemovePatient();
            if (target == null)
            {
                Debug.LogWarning("Target is null");
                return false;
            }

            _resource = GWorld.instance.RemoveCubicle();

            if (_resource == null)
            {
                Debug.LogWarning("Resource is null");
                GWorld.instance.AddPatient(target);
                target = null;
                return false;
            }
            
            inventory.AddItem(_resource);
            GWorld.instance.GetWorld.ModifyState("FreeCubicles", -1);
            Debug.Log("Go");
            return true;
        }

        public override bool PostPerform()
        {
            GWorld.instance.GetWorld.ModifyState("Waiting", -1);
            if(target)
                target.GetComponent<GAgent>().inventory.AddItem(_resource);
         
            return true;
        }
    }
}