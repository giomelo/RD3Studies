using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _RD3.RNA._Scripts
{
    public class Creature : MonoBehaviour
    {
        public bool isDead;
        [SerializeField]private float rotateSpeed = 100;
        [SerializeField] private Vector3 playerVelocity;
        private const float GravityValue = -9.81f;
        [SerializeField]private float speed = 10.0F;
        [SerializeField]private float energy;
        [SerializeField]private float reproductionEnergy;
        [SerializeField]private float elapsedTime;
        [SerializeField]private CharacterController controller;

        public Action<GameObject> onReproduced;
        private void Update()
        {
            ManageEnergy();
        }

        private void Die()
        {
            isDead = true;
            GameManager.Instance.onCreatureDied?.Invoke();
            Destroy(gameObject);
        }

        public void Move(float x, float z)
        {
            Debug.Log("MOVE");
            //clamp the values of LR and FB
            x = Mathf.Clamp(x, -1, 1);
            z = Mathf.Clamp(z, 0, 1);
            //move the agent
            if (!isDead)
            {
                // Rotate around y - axis
                transform.Rotate(0, z * rotateSpeed, 0);

                // Move forward / backward
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                controller.SimpleMove(forward * (speed * x));
            }


            //Checks to see if the agent is grounded, if it is, don't apply gravity
            if (controller.isGrounded && playerVelocity.y < 0)
                playerVelocity.y = 0f;
            else
            {
                // Gravity
                playerVelocity.y += GravityValue * Time.deltaTime;
                controller.Move(playerVelocity * Time.deltaTime);
            }
        }

        public void SetEnergy(float newValue)
        {
            energy = newValue;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.CompareTag("Food"))
            {
                energy += GameManager.Instance.energyToGain;
                reproductionEnergy += GameManager.Instance.reproductionEnergyToGain;
                Destroy(other.gameObject);
            }
        }

        private void ManageEnergy()
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= 1)
            {
                elapsedTime = 0;
                // subtract 1 energy per second
                energy -= 1;
            }
            
            // check if energy is less than 0
            var y = transform.position.y;
            if (energy <= 0 || y < -10)
                Die();
            
            //if the agent has energy to reproduce
            if(reproductionEnergy >= GameManager.Instance.reproductionEnergyThreshold)
            {
                Reproduce();
            }
        }
        
        private void Reproduce()
        {
            for (int i = 0; i < 1; i++)
            {
                // create a new agent and set its position to the parents position + a random offset int eh x and z directions
                
                var newCreature = GameManager.Instance.creatureSpawner.Spawn(
                    new Vector3(transform.position.x + Random.Range(-1f, 1f), 0, transform.position.z + Random.Range(-1f, 1f)));
                
                onReproduced?.Invoke(newCreature);
            }
            reproductionEnergy = 0;
        }
    }
}