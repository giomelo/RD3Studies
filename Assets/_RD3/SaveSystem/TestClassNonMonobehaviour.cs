
namespace _RD3.SaveSystem
{
    public class TestClassNonMonobehaviour : ISavableObject
    {
        [SaveVariable(SaveTypes.Json)]
        public float myFloat = 24.3f;
        [SaveVariable(SaveTypes.Json)]
        public float myFloat2 = 5f;

        public TestClassNonMonobehaviour() {}

        public TestClassNonMonobehaviour(float f, float f2, string name)
        {
            myFloat = f;
            myFloat2 = f2;
            Name = name;
        }
        
        public string Name { get; set; } = "TestClassNonMonobehaviour";
    }
}