using UnityEngine;

namespace _RD3.CommandPattern
{
    public class MovementCommand : PlayerCommand
    {
        private Vector3 _movementDirection;
        
        public MovementCommand(PlayerMovement playerMovement, Vector3 movementDirection) : base(playerMovement)
        {
            _movementDirection = movementDirection;
        }
        
        public override void Execute()
        {
            _playerMovement.Move(_movementDirection);
        }
        
        public override void Undo()
        {
            _playerMovement.Move(-_movementDirection);
        }
        
    }
}
