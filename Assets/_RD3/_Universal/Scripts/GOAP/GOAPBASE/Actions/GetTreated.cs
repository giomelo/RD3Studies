namespace _RD3._Universal.Scripts.GOAP
{
    public class GetTreated : GAction
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
            GWorld.instance.GetWorld.ModifyState("isTreated", 1);
            GWorld.instance.GetWorld.ModifyState("isCured", 1);
            inventory.RemoveItem(target);
            return true;
        }
    }
}