using System.Collections;
using UnityEngine;

namespace BTree.BehaviourTree
{
    public class PatronBehaviour : global::BTree.BehaviourTree.BTAgent
    {
        [SerializeField]private GameObject[] art;
        [SerializeField]private GameObject frontDoor;
        [SerializeField]private GameObject home;
        
        [Range(0,1000)]
        [SerializeField]private int boredomLevel = 0;

        public bool hasTicked = false;
        public bool isWaitingForTicket = false;
        
        public override void Start()
        {
            base.Start();
            // Initialize the behavior tree here
            
            RSelector selectobj = new RSelector("Select art to view");

            for (int i = 0; i < art.Length; i++)
            {
                Leaf gta = new Leaf("Go to " + art[i].name, i, GoToArt, () =>
                {
                    boredomLevel = Mathf.Clamp(boredomLevel - 150, 0, 1000);
                });
                selectobj.AddChild(gta);
            }

            Leaf goToFrontDoor = new Leaf("Go to frontDoor", GoToFrontDoor);
            Leaf goToHome = new Leaf("Go to home", GoToHome, () =>
            {
                
            });
            Leaf isBorded = new Leaf("Is bored?", IsBored);
            Leaf isOpen = new Leaf("Is open?", IsOpen);
            
            
            Sequence viewArt = new Sequence("Vier art");
            viewArt.AddChild(isOpen);
            viewArt.AddChild(isBorded);
            viewArt.AddChild(goToFrontDoor);
            
            Leaf noTicket = new Leaf("Waiting for ticket", NoTicke);
            Leaf isWaiting = new Leaf("Is waiting", IsWaiting);

            BehaviourTree waitForTicket = new BehaviourTree();
            waitForTicket.AddChild(noTicket);
            
            Loop getTicket = new Loop("Get ticket", waitForTicket);
            getTicket.AddChild(isWaiting);
            
            viewArt.AddChild(getTicket);
            
            BehaviourTree whileBored = new BehaviourTree();
            whileBored.AddChild(isBorded);
            
            Loop lookAtPaintings = new Loop("Look at paitings", whileBored);
            lookAtPaintings.AddChild(selectobj);
            
            viewArt.AddChild(lookAtPaintings);
            viewArt.AddChild(goToHome);

            BehaviourTree galleryOpenCondition = new BehaviourTree();
            galleryOpenCondition.AddChild(isOpen);
            
            DepSequence bePatron = new DepSequence("Be a patron", galleryOpenCondition, agent);
            bePatron.AddChild(viewArt);
           
            Selector viewArtWithFallback = new Selector("View art with fallback");
            viewArtWithFallback.AddChild(bePatron);
            viewArtWithFallback.AddChild(goToHome);
            
            tree.AddChild(viewArtWithFallback);
            
            StartCoroutine(IncreaseBoredom());
        }
        IEnumerator IncreaseBoredom()
        {
            while (true)
            {
                boredomLevel = Mathf.Clamp(boredomLevel + 30, 0, 1000);
                yield return new WaitForSeconds(Random.Range(1,5));
            }
        }
        
        public Node.Status GoToFrontDoor()
        {
            Node.Status s = GoToDoor(frontDoor);
            return s;
        }
        public Node.Status GoToArt(int i)
        {
            if(!art[i].activeInHierarchy)
            {
                return Node.Status.FAILURE;
            }

            Node.Status s = GoToLocation(art[i].transform.position);
            return s;
        }
        
        public Node.Status GoToHome()
        {
            isWaitingForTicket = false;
            Node.Status s = GoToLocation(home.transform.position);
            return s;
        }

        public Node.Status IsBored()
        {
            if (boredomLevel < 100)
                return Node.Status.FAILURE;

            return Node.Status.SUCCESS;
        }
        
        public Node.Status NoTicke()
        {
            if (hasTicked || IsOpen() == Node.Status.FAILURE)
                return Node.Status.FAILURE;

            return Node.Status.SUCCESS;
        }

        public Node.Status IsWaiting()
        {
            if (BlackBoard.Instance.RegisterPatron(this))
            {
                isWaitingForTicket = true;
                return Node.Status.SUCCESS;
            }
            return Node.Status.FAILURE;
        }
    }
}
