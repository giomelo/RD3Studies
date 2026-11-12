namespace _RD3._Universal.Scripts.GOAP
{
    public class GoToCubicle : GAction
    {
        public override bool PrePerform()
        {
            target = inventory.FindItemWithTag("Cubicle");
            if (target == null)
                return false;
            return true;
        }

        public override bool PostPerform()
        {
            GWorld.instance.GetWorld.ModifyState("TreatingPatient", 1);
            GWorld.instance.AddCubicles(target);
            inventory.RemoveItem(target);
            GWorld.instance.GetWorld.ModifyState("FreeCubicles", 1);
            return true;
        }
    }
}