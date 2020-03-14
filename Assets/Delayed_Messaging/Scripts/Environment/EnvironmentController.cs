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
            [Serializable] public struct EnvironmentDefinition
            {
                [Header("Environment Size")] 
                [Range(1, 500)] public int size;
                [Range(1, 100)] public float noiseScale;
                
                [Header("Fractional Brownian Motion Setup")] 
                [Range(1, 10)] public int octaves;
                [Range(0, 1)] public float persistence;
                [Range(1, 10)] public float lacunarity;

                [Header("Environment Regions")]
                public EnvironmentRegions environmentRegions;
                
                [Header("Noise Seed Reference")] 
                [Range(0, 500)] public int seed;
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
            public EnvironmentTileMap environmentTileMap;
            
            [Header("Generate Environment")] 
            public bool generateEnvironment;
            public bool generateRandomEnvironment;
        }
        [Serializable] public struct NoiseSettings
        {
            public enum Noise
            {
                TERRAIN,
                VEGETATION
            }
            public Noise noise;
        }
        [Serializable] public struct EnvironmentRegions
        {
            public enum EnvironmentRegion
            {
                LAND,
                WATER,
            }
            
            [Header("Land Settings")]
            [Range(0f, 1f)] public float landHeight;
            public Color landColour;
            
            [Header("Mountain Settings")]
            public Color mountainColour;
            [Space(5), Range(0f, 1f)] public float footHillHeight;
            public Color footHillColour;
            [Space(5),Range(0f, 1f)] public float snowHeight;
            public Color snowColour;

            [Header("Sea Settings")]
            [Range(0f, 1f)] public float seaLevel;
            public Color seaColour;
            [Space(5), Range(0f, 1f)] public float shoreDepth;
            public Color shoreColour;
            [Space(5), Range(0f, 1f)] public float shallowsDepth;
            public Color shallowsColour;
        }
        public Environment environment;
        private void Start()
        {
            GenerateEnvironment();
        }
        private void GenerateEnvironment(bool debug = false) 
        {
            float[,] terrainHeightMap = Draw.Noise.GenerateFractionalBrownianNoise(environment.environmentDefinition);
            float[,] vegetationHeightMap = Draw.Noise.GenerateFractionalBrownianNoise(environment.environmentDefinition);

            Color[] terrainColourMap = Draw.ColourMap(environment.environmentDefinition, terrainHeightMap);
            Color[] vegetationColourMap = Draw.ColourMap(environment.environmentDefinition, vegetationHeightMap);

            switch (environment.drawMode)
            {
                case Environment.DrawMode.NOISE:
                    EnvironmentGenerator.DrawTexture(environment.environmentRenderer, EnvironmentGenerator.TextureFromHeightMap(terrainHeightMap));
                    break;
                case Environment.DrawMode.COLOUR when !debug:
                    environment.environmentTileMap.GenerateTileMap(environment.environmentDefinition, terrainHeightMap);
                    EnvironmentGenerator.DrawTexture(environment.environmentRenderer, EnvironmentGenerator.TextureFromColourMap(terrainColourMap, terrainHeightMap));
                    break;
                case Environment.DrawMode.COLOUR:
                    EnvironmentGenerator.DrawTexture(environment.environmentRenderer, EnvironmentGenerator.TextureFromColourMap(terrainColourMap, terrainHeightMap));
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
            if (environment.environmentDefinition.environmentRegions.shallowsDepth > environment.environmentDefinition.environmentRegions.seaLevel)
            {
                environment.environmentDefinition.environmentRegions.shallowsDepth = environment.environmentDefinition.environmentRegions.seaLevel;
            }
            if (environment.environmentDefinition.environmentRegions.landHeight < environment.environmentDefinition.environmentRegions.seaLevel)
            {
                environment.environmentDefinition.environmentRegions.landHeight = environment.environmentDefinition.environmentRegions.seaLevel;
            }
            if (environment.environmentDefinition.environmentRegions.shoreDepth > environment.environmentDefinition.environmentRegions.landHeight)
            {
                environment.environmentDefinition.environmentRegions.shoreDepth = environment.environmentDefinition.environmentRegions.landHeight;
            }
            if (environment.environmentDefinition.environmentRegions.footHillHeight > environment.environmentDefinition.environmentRegions.landHeight - environment.environmentDefinition.environmentRegions.seaLevel)
            {
                environment.environmentDefinition.environmentRegions.footHillHeight = environment.environmentDefinition.environmentRegions.landHeight - environment.environmentDefinition.environmentRegions.seaLevel;
            }

            if (environment.generateEnvironment)
            {
                GenerateEnvironment(true);
                environment.generateEnvironment = false;
                return;
            }
            
            if (environment.generateRandomEnvironment)
            {
                environment.environmentDefinition.seed = UnityEngine.Random.Range(-100, 100);
                GenerateEnvironment(true);
                environment.generateRandomEnvironment = false;
                return;
            }

            GenerateEnvironment(true);
        }
    }
}
