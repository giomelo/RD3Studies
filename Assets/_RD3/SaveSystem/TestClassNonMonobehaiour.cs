using _RD3._Universal._Scripts.Utilities;

namespace _RD3.SaveSystem
{
    public class TestClassNonMonobehaiour : ISavedObject
    {
        [SaveVariable]
        public float myFloat = 24.3f;
        
        public void Save()
        {
            throw new System.NotImplementedException();
        }

        public void Load()
        {
            throw new System.NotImplementedException();
        }
    }
}