namespace _RD3._Universal.Scripts.GOAP
{
    public class Patient : GAgent
    {
        public new void Start()
        {
            base.Start();
            var s1 = new SubGoal("isWaiting", 1, true);
            goals.Add(s1, 3);
        }
    }
    
}