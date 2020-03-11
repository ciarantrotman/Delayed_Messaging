using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Utilities;
using Pathfinding;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Environment
{
    public class EnvironmentController : MonoBehaviour
    {
        [Serializable] public struct Environment
        {
            [Serializable] public struct EnvironmentDefinition
            {
                [Header("Environment Size")] 
                [Range(1, 500)] public int size;
                [Range(1, 100)] public float noiseScale;
                
                [Header("Fractional Brownian Motion Setup")] 
                [Range(1, 10)] public int octaves; 
                [Range(0, 1)] public float persistence;
                [Range(1, 10)]public float lacunarity;

                [Header("Environment Regions")] 
                public List<EnvironmentRegions> environmentRegions;

                [Header("Noise Seed Reference")] 
                [Range(-100, 100)] public int seed;
                public Vector2 offset;
            }
            
            [Header("Environment Dimensions")]
            public EnvironmentDefinition environmentDefinition;
            public enum DrawMode
            {
                NOISE, 
                COLOUR
            };
            public DrawMode drawMode;

            [Header("References")] 
            public MeshRenderer environmentRenderer;
            
            [Header("Generate Environment")] 
            public bool generateEnvironment;
            public bool generateRandomEnvironment;
        }
        [Serializable] public struct EnvironmentRegions
        {
            public enum EnvironmentRegion
            {
                PLAIN,
                WATER,
                MOUNTAIN
            }
            public EnvironmentRegion environmentRegion;
            [Range(0, 1)] public float height;
            public Color regionColourReference;
        }
        public Environment environment;
        private void Start()
        {
            GenerateEnvironment();
        }
        private void GenerateEnvironment() 
        {
            float[,] noiseMap = Draw.Noise.GenerateFractionalBrownianNoise(environment.environmentDefinition);
            Color[] colourMap = Draw.ColourMap(environment.environmentDefinition, noiseMap);

            switch (environment.drawMode)
            {
                case Environment.DrawMode.NOISE:
                    EnvironmentGenerator.DrawTexture(environment.environmentRenderer, EnvironmentGenerator.TextureFromHeightMap(noiseMap));
                    break;
                case Environment.DrawMode.COLOUR:
                    EnvironmentGenerator.DrawTexture(environment.environmentRenderer, EnvironmentGenerator.TextureFromColourMap(colourMap, noiseMap));
                    break;
                default:
                    break;
            }
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
        private void OnValidate()
        {
            if (environment.generateEnvironment)
            {
                GenerateEnvironment();
                environment.generateEnvironment = false;
                return;
            }
            
            if (environment.generateRandomEnvironment)
            {
                environment.environmentDefinition.seed = UnityEngine.Random.Range(-100, 100);
                GenerateEnvironment();
                environment.generateRandomEnvironment = false;
                return;
            }

            GenerateEnvironment();
        }
    }
}
