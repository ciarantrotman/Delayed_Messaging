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
            [Header("Noise Settings")]
            public Noise terrainNoise;
            public Noise vegetationNoise;
            
            [Header("Environment Settings")]
            public EnvironmentRegions environmentRegions;
            public EnvironmentResources environmentResources;
            public enum DrawMode
            {
                NOISE, 
                COLOUR
            };
            [Space(10)] public DrawMode drawMode;

            [Header("References")] 
            public MeshRenderer terrainRenderer;
            public MeshRenderer resourcesRenderer;
            public EnvironmentTileMap environmentTileMap;
            
            [Header("Generate Environment")] 
            public bool generateEnvironment;
        }
        [Serializable] public struct Noise
        {
            [Header("Noise Settings")] 
            [Range(1, 500)] public int size;
            [Range(1, 1000)] public float noiseScale;
            
            [Header("Noise Seed Reference")] 
            [Range(0, 500)] public int seed;
            public Vector2 offset;

            [Header("Fractional Brownian Motion Settings")] 
            [Range(1, 10)] public int octaves;
            [Range(0, 1)] public float persistence;
            [Range(1, 10)] public float lacunarity;
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
        [Serializable] public struct EnvironmentResources
        {
            [Header("Vegetation Settings")]
            [Range(0f, 1f)] public float vegetationBorder;
            [Range(.5f, 0f)] public float vegetationDensity;
            public Color vegetationColour;

        }
        public Environment environment;
        private void Start()
        {
            GenerateEnvironment();
        }
        private void GenerateEnvironment() 
        {
            float[,] terrainHeightMap = Draw.Noise.GenerateFractionalBrownianNoise(environment.terrainNoise);
            
            float[,] resourceHeightMap = Draw.Noise.GenerateFractionalBrownianNoise(environment.vegetationNoise);
            resourceHeightMap = Draw.Noise.MaskedNoise(terrainHeightMap, resourceHeightMap, 
                environment.environmentRegions.landHeight - environment.environmentResources.vegetationBorder,
                environment.environmentRegions.seaLevel + environment.environmentRegions.shoreDepth + environment.environmentResources.vegetationBorder);
            resourceHeightMap = Draw.Noise.MaskedNoise(resourceHeightMap, resourceHeightMap, 
                1 - environment.environmentResources.vegetationDensity, 
                environment.environmentResources.vegetationDensity);

            Color[] terrainColourMap = Draw.ColourMap(environment.terrainNoise, environment.environmentRegions, terrainHeightMap);
            Color[] resourceColourMap = Draw.ColourMap(environment.vegetationNoise, environment.environmentResources, resourceHeightMap);

            switch (environment.drawMode)
            {
                case Environment.DrawMode.NOISE:
                    EnvironmentGenerator.DrawTexture(environment.resourcesRenderer, EnvironmentGenerator.TextureFromHeightMap(resourceHeightMap));
                    EnvironmentGenerator.DrawTexture(environment.terrainRenderer, EnvironmentGenerator.TextureFromHeightMap(terrainHeightMap));
                    break;
                case Environment.DrawMode.COLOUR:
                    EnvironmentGenerator.DrawTexture(environment.resourcesRenderer, EnvironmentGenerator.TextureFromColourMap(resourceColourMap, resourceHeightMap));
                    EnvironmentGenerator.DrawTexture(environment.terrainRenderer, EnvironmentGenerator.TextureFromColourMap(terrainColourMap, terrainHeightMap));
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
            if (environment.environmentRegions.shallowsDepth > environment.environmentRegions.seaLevel)
            {
                environment.environmentRegions.shallowsDepth = environment.environmentRegions.seaLevel;
            }
            if (environment.environmentRegions.landHeight < environment.environmentRegions.seaLevel)
            {
                environment.environmentRegions.landHeight = environment.environmentRegions.seaLevel;
            }
            if (environment.environmentRegions.shoreDepth > environment.environmentRegions.landHeight)
            {
                environment.environmentRegions.shoreDepth = environment.environmentRegions.landHeight;
            }
            if (environment.environmentRegions.footHillHeight > environment.environmentRegions.landHeight - environment.environmentRegions.seaLevel)
            {
                environment.environmentRegions.footHillHeight = environment.environmentRegions.landHeight - environment.environmentRegions.seaLevel;
            }

            if (environment.generateEnvironment)
            {
                GenerateEnvironment();
                environment.generateEnvironment = false;
                return;
            }

            GenerateEnvironment();
        }
    }
}
