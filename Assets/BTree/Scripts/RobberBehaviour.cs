using System;
using UnityEngine;
using UnityEngine.AI;

namespace BTree.Scripts
{
    public enum ActionState
    {
        Idle,
        Working
    }
    
    public class RobberBehaviour : BTAgent
    {
        
        [SerializeField]private GameObject diamond;
        [SerializeField]private GameObject van;
        [SerializeField]private DoorLock door;
        [SerializeField]private DoorLock frontDoor;

        [Range(0, 1000)] [SerializeField]private int money = 800;

        private new void Start()
        {
            base.Start();
            
            Sequence steal = new Sequence("Steal something");
            Leaf hasMoney = new Leaf("Has Money", HasMoney);
            Leaf goToDiamond = new Leaf("Go to diamond", GoToDiamond, (() => {diamond.transform.parent= this.transform;}));
            Leaf goToVan = new Leaf("Go to van",GoToVan, () =>
            {
                money += 300;
                diamond.gameObject.SetActive(false);
            });
            
            Inverter inverterMoney = new Inverter("Inverter Money");
            inverterMoney.AddChild(hasMoney);
            
            Leaf goToDoor = new Leaf("Go to door",GoToDoor);
            Leaf goToFrontDoor = new Leaf("Go to frontDoor",GoToFrontDoor);
            
            Selector openDoor = new Selector("Open door");
            openDoor.AddChild(goToFrontDoor);
            openDoor.AddChild(goToDoor);
            
            steal.AddChild(inverterMoney);
             
            steal.AddChild(openDoor);
            steal.AddChild(goToDiamond);
            steal.AddChild(openDoor);
            steal.AddChild(goToVan);
            tree.AddChild(steal);
            
            tree.PrintTree(); 
        }
        
        private Node.Status HasMoney()
        {
            return money < 500 ? Node.Status.FAILURE : Node.Status.SUCCESS;
        }
        private Node.Status GoToDoor()
        {
            return GoToDoor(door);
        } 
        
        private Node.Status GoToFrontDoor()
        {
            return GoToDoor(frontDoor);
        }
        private Node.Status GoToDiamond()
        {
            return GoToLocation(diamond.transform.position);
        }
        private Node.Status GoToVan()
        {
            return GoToLocation(van.transform.position);
        }

        public Node.Status GoToDoor(DoorLock d)
        {
            Node.Status s = GoToLocation(d.transform.position);
            
            if(s==Node.Status.SUCCESS)
            {
                if (!d.isLocked)
                {
                    d.gameObject.SetActive(false);
                    return Node.Status.SUCCESS;
                }

                return Node.Status.FAILURE;
            }

            return s;
        }
        
    }
}
