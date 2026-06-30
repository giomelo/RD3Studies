using System;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace _RD3.RNA._Scripts
{
    [RequireComponent(typeof(Nn))]
    [RequireComponent(typeof(Creature))]
    public class CreatureController : MonoBehaviour
    {
        private Nn _neuralNetWork;
        private Creature _creature;
        [SerializeField]private float viewDistance = 3;
        [SerializeField] private bool muteMutations = true;
        

        private void Awake()
        {
            _neuralNetWork = GetComponent<Nn>();
            _creature = GetComponent<Creature>();
        }

        private void Start()
        {
            MutateCreature();
            transform.localScale = new Vector3(1, 1, 1);
            _creature.SetEnergy(20);
        }

        private void OnEnable()
        {
            _creature.onReproduced += CopyLayers;
        }

        private void OnDisable()
        {
            _creature.onReproduced -= CopyLayers;
        }
        
        private void CopyLayers(GameObject newCreature)
        {
            if (!newCreature.TryGetComponent<Nn>(out var nn)) return;

            nn.Layers = _neuralNetWork.CopyLayers();
        }

        private float x;
        private float z;
        private void Update()
        {
            var distances = CreateRayCasts(5, 20);
            // set inputs for the neural network
            float [] inputsToNN = distances;
            
            // get outputs from the neural network
            float[] outputsFromNN = _neuralNetWork.Brain(inputsToNN);
            x = outputsFromNN[0];
            z = outputsFromNN[1];
            _creature.Move(x, z);
        }

        private float[] CreateRayCasts(int numRays, float angleBetweenRaycasts)
        {
            float[] distances = new float[numRays];

            RaycastHit hit;
            for (int i = 0; i < numRays; i++)
            {
                float angle = ((2 * i - 1 - numRays) * angleBetweenRaycasts) / 2;
                var rotation = Quaternion.AngleAxis(angle, Vector3.up);
                Vector3 direction = rotation * transform.forward;
                var rayStart = transform.position + Vector3.up * 0.1f;

                if (Physics.Raycast(rayStart, direction, out hit, viewDistance))
                {
                    // draw
                    Debug.DrawRay(rayStart,direction * hit.distance, Color.green);

                    if (hit.transform.gameObject.CompareTag("Food"))
                    {
                        // use the lengh of the raycast as the distance to te food object
                        distances[i] = hit.distance;
                    }
                    else
                    {
                        // if no food object is detected, set the distance to the maximum lenght of the raycast
                        distances[i] = viewDistance;
                    }
                }
                else
                {
                    Debug.DrawRay(rayStart,direction * viewDistance, Color.red);
                    // if no food object is detected, set the distance to the maximum lenght of the raycast
                    distances[i] = viewDistance;
                }
            }

            return distances;
        }

        private void MutateCreature()
        {
            float mutationAmount = 0;
            float mutationChance = 0;

            if (muteMutations)
            {
                mutationAmount = Random.Range(0.1f, 1f);
                mutationChance = Random.Range(0.1f, 1f);
            }else
            {
                mutationAmount = 0.5f;
                mutationChance = 1f;
            }
            
            // make sure mutation amount and chance are positive
            mutationAmount = Mathf.Max(mutationAmount, 0);
            mutationChance = Mathf.Max(mutationChance, 0);
            
            Debug.Log($"MutationAmount: {mutationAmount}, MutationChance: {mutationChance}");
            _neuralNetWork.MutateNetwork(mutationAmount, mutationChance);
        }
    }
}