
namespace _RD3.SaveSystem.Scripts
{
    public class TestClassNonMonoBehaviour : ISavableObject
    {
        [SaveVariable(SaveTypes.Json)]
        public float myFloat = 24.3f;
        [SaveVariable(SaveTypes.Json)]
        public float myFloat2 = 5f;

        public TestClassNonMonoBehaviour() {}

        public TestClassNonMonoBehaviour(float f, float f2, string name)
        {
            myFloat = f;
            myFloat2 = f2;
            Name = name;
        }
        
        public string Name { get; set; } = "TestClassNonMonoBehaviour";
    }
}