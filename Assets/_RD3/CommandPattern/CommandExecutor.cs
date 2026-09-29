using System;
using System.Collections.Generic;
using UnityEngine;

namespace _RD3.CommandPattern
{
    public class CommandExecutor : MonoBehaviour
    {
        [SerializeField] private PlayerMovement playerMovement;
        
        private Stack<ICommand> _undoStack = new Stack<ICommand>();
        private Stack<ICommand> _redoStack = new Stack<ICommand>();

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
                ExecuteCommand(new MovementCommand(playerMovement, Vector3.up));
            
            if(Input.GetKeyDown(KeyCode.DownArrow))
                ExecuteCommand(new MovementCommand(playerMovement, Vector3.down));
            
            if(Input.GetKeyDown(KeyCode.LeftArrow))
                ExecuteCommand(new MovementCommand(playerMovement, Vector3.left));
            
            if(Input.GetKeyDown(KeyCode.RightArrow))
                ExecuteCommand(new MovementCommand(playerMovement, Vector3.right));
        }

        private void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }
        
        public void Undo()
        {
            if (_undoStack.Count <= 0) return;
            ICommand command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }
        
        public void Redo()
        {
            if (_redoStack.Count <= 0) return;
            ICommand command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }
    }
}