using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _RD3.RNA
{
    public class Layer
    {
        public float[] NodesArray;
        public float[] BiasesArray;
        public float[,] WeightsArray;

        private int _numberOfNodes;
        private int _numberOfInputs;
            
        public Layer(int numberInputs, int numberNodes)
        {
            _numberOfNodes = numberNodes;
            _numberOfInputs = numberInputs;
                
            WeightsArray = new float[numberNodes, numberInputs];
            BiasesArray = new float[numberNodes];
            NodesArray = new float[numberNodes];
        }
        /// <summary>
        /// execute the neural network y = sum(weight * input) = bias
        /// </summary>
        /// <param name="inputsArray"></param>
        public void Forward(float[] inputsArray)
        {
            NodesArray = new float[_numberOfNodes];

            for(int i = 0; i< _numberOfNodes; i++)
            {
                for (int j = 0; j < _numberOfInputs; j++)
                {
                    // sum the weights and inputs (i = nodes, j = inputs)
                    NodesArray[i] += WeightsArray[i, j] * inputsArray[j];
                }
                
                //add the bias 
                NodesArray[i] += BiasesArray[i];
            }
        }
        
        /// <summary>
        /// ReLU activation function
        /// </summary>
        public void Activation()
        {
            for(int i = 0; i < _numberOfNodes; i++)
            {
                NodesArray[i] = Mathf.Max(0, NodesArray[i]);
            }
        }

        public void MutateLayer(float mutationAmount, float mutationChance)
        {
            for (int i = 0; i < _numberOfNodes; i++)
            {
                for (int j = 0; j < _numberOfInputs; j++)
                {
                    if (Random.value < mutationChance)
                    {
                        WeightsArray[i, j] += Random.Range(-1.0f, 1.0f) * mutationAmount;
                    }
                }

                if (Random.value < mutationChance)
                {
                    BiasesArray[i] += Random.Range(-1.0f, 1.0f) * mutationAmount;
                }
            }
        }
            
    }
    public class Nn : MonoBehaviour
    {
        public Layer[] Layers;
        public int[] networkShape = {5,32,2};
   
        private void Awake()
        {
            Layers = new Layer[networkShape.Length - 1];
            
            for(int i = 0; i < Layers.Length; i++)
                Layers[i] = new Layer(networkShape[i], networkShape[i + 1]);
        }

        

        public Layer[] CopyLayers()
        {
            var tempLayers = new Layer[networkShape.Length - 1];
            for (int i = 0; i < Layers.Length; i++)
            {
                tempLayers[i] = new Layer(networkShape[i], networkShape[i + 1]);
                Array.Copy(Layers[i].WeightsArray, tempLayers[i].WeightsArray, 
                    Layers[i].WeightsArray.GetLength(0) * Layers[i].WeightsArray.GetLength(1));
                Array.Copy(Layers[i].BiasesArray, tempLayers[i].BiasesArray, Layers[i].BiasesArray.Length);
            }

            return tempLayers;
        }

        /// <summary>
        /// Neural Network Brain
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        public float[] Brain(float[] inputs)
        {
            for (int i = 0; i < Layers.Length; i++)
            {
                if (i == 0)
                {
                    Layers[i].Forward(inputs);
                    Layers[i].Activation();
                    
                }else if (i != Layers.Length - 1)
                {
                    Layers[i].Forward(Layers[i - 1].NodesArray);
                    Layers[i].Activation();
                }
                else
                {
                    // final output dont need activation, is optional (it is going to turn everything to positive int this case)
                    Layers[i].Forward(Layers[i - 1].NodesArray);
                }
            }
            
            return Layers[^1].NodesArray;
        }

        public void MutateNetwork(float mutationAmount, float mutationsChance)
        {
            foreach (var layer in Layers)
                layer.MutateLayer(mutationAmount, mutationsChance);
            
        }
    }
}
