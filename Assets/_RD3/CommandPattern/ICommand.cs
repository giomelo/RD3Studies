namespace _RD3.CommandPattern
{
    public interface ICommand
    {
        public void Execute();
        public void Undo();
    }
}
