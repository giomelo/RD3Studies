namespace _RD3.CommandPattern
{
    public abstract class PlayerCommand : ICommand
    {
        protected PlayerMovement _playerMovement;
        
        
        public PlayerCommand(PlayerMovement playerMovement)
        {
            _playerMovement = playerMovement;
        }
        
        public virtual void Execute()
        {
            throw new System.NotImplementedException();
        }

        public virtual void Undo()
        {
            throw new System.NotImplementedException();
        }
    }
}
