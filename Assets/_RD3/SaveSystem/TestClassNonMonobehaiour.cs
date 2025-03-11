using _RD3._Universal._Scripts.Utilities;

namespace _RD3.SaveSystem
{
    public class TestClassNonMonobehaiour : ISavedObject
    {
        [SaveVariable]
        public float myFloat = 24.3f;
        [SaveVariable]
        public float myFloat2 = 5f;
    }
}