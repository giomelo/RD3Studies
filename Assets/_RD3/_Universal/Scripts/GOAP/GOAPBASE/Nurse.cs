namespace _RD3._Universal.Scripts.GOAP.GOAPBASE
{
    public class Nurse : GAgent
    {
        private new void Start()
        {
            base.Start();
            var s1 = new SubGoal("treatPatient", 1, true);
            goals.Add(s1, 3);
        }
    }
}
