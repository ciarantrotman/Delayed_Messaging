using System;
using Delayed_Messaging.Scripts.Utilities;
using Pathfinding;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Environment
{
    public class EnvironmentController : MonoBehaviour
    {
        [Serializable] public struct Environment
        {
            [Serializable] public struct EnvironmentDimensions
            {
                [Header("Environment Size")] 
                [Range(0, 100)] public int x;
                [Range(0, 100)] public int z;
                [Range(.01f, 100)] public float noiseScale;
                
                [Header("Fractional Brownian Motion Setup")] 
                [Range(0, 10)] public int octaves; 
                [Range(0, 1)] public float persistence;
                public float lacunarity;
                
                [Header("Noise Seed Reference")] 
                [Range(-100, 100)] public int seed;
                public Vector2 offset;
            }
            [Header("Environment Dimensions")]
            public EnvironmentDimensions environmentDimensions;

            [Header("Environment References")] 
            public Renderer noiseRenderer;
            
            [Header("Generate Environment")] 
            public bool generateEnvironment;
        }

        public Environment environment;

        private void Start()
        {
            GenerateEnvironment();
        }
        private void OnValidate()
        {
            if (environment.generateEnvironment)
            {
                GenerateEnvironment();
                environment.generateEnvironment = false;
                return;
            }

            GenerateEnvironment();
        }
        /// <summary>
        /// Generates a map based on the generated Perlin noise
        /// </summary>
        private void GenerateEnvironment() 
        {
            float[,] noiseMap = Draw.Noise.GenerateFractionalBrownianMotion(environment.environmentDimensions);
            EnvironmentGeneration.DrawNoiseMap(noiseMap, environment.noiseRenderer);
        }
        /// <summary>
        /// Generates a path by decreasing the penalty of walking on a node that other units have walked on before
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="previousNode"></param>
        /// <param name="weight"></param>
        public static void GeneratePath(GraphNode currentNode, GraphNode previousNode, uint weight)
        {
            if (previousNode != null && currentNode != previousNode && currentNode.Penalty > 0)
            {
                currentNode.Penalty -= weight;
            }
        }
    }
}
