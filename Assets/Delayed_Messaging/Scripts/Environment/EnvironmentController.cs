using System;
using System.Collections.Generic;
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

            [Header("Environment Settings")]
            public EnvironmentRegions environmentRegions;
            public List<EnvironmentResources> environmentResources;
            public enum DrawMode
            {
                NOISE, 
                COLOUR
            };
            [Space(10)] public DrawMode drawMode;

            [Header("References")] 
            public MeshRenderer terrainRenderer;
            public EnvironmentTileMap environmentTileMap;
            
            [Header("Generate Environment")] 
            public bool generateEnvironment;
        }
        [Serializable] public struct Noise
        {
            [Header("Noise Seed Reference")] 
            [Range(0, 500)] public int seed;
            public Vector2 offset;
            
            [Header("Noise Settings")] 
            [Range(1, 500)] public int size;
            [Range(1, 1000)] public float noiseScale;

            [Header("Fractional Brownian Motion Settings")] 
            [Range(1, 10)] public int octaves;
            [Range(0, 1)] public float persistence;
            [Range(1, 10)] public float lacunarity;
        }
        public struct ResourceRegions
        {
            public enum ResourceRegion
            {
                SEA,
                SHALLOWS,
                SHORE,
                LAND,
                FOOTHILLS,
                MOUNTAIN,
                SNOW
            }
            public ResourceRegion resourceRegion;
            public float height;
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
        [Serializable] public class EnvironmentResources
        {
            [SerializeField] private string resourceName;
            
            [Header("Resource Noise Settings")]
            public ResourceRegions.ResourceRegion resourceRegion;
            public Noise resourceNoise;
            public float[,] resourceNoiseMap;
            [HideInInspector] public Color[] resourceColourMap;

            [Header("Resource Settings")] 
            public bool contiguous;
            [Range(0f, 1f)] public float borderThickness;
            [Range(.5f, 0f)] public float resourceDensity;
            public Color resourceColour;
        }
        public Environment environment;
        private void Start()
        {
            GenerateEnvironment();
        }
        private void GenerateEnvironment() 
        {
            // Create base terrain height maps and colour maps from textures
            float[,] terrainHeightMap = Draw.Noise.GenerateFractionalBrownianNoise(environment.terrainNoise);
            ResourceRegions[,] environmentRegions = Draw.Noise.GenerateEnvironmentResources(environment.terrainNoise, terrainHeightMap, environment.environmentRegions);
            Color[] terrainColourMap = Draw.ColourMap(environmentRegions, environment.environmentRegions);

            // Loop through all of the available resources to create individual height maps and colour maps
            foreach (EnvironmentResources environmentResource in environment.environmentResources)
            {
                // Generate height map based on the resource specific noise
                environmentResource.resourceNoiseMap = Draw.Noise.GenerateFractionalBrownianNoise(environmentResource.resourceNoise);
                // Mask that based on the region that it appears in
                environmentResource.resourceNoiseMap = Draw.Noise.MaskedNoise(environmentResource, environmentRegions, environment.environmentRegions);
                // Mask that again based on how dense that resource is, change values based on whether they are contiguous or not
                float upperLimit = environmentResource.contiguous ? 1 - environmentResource.resourceDensity : 1;
                float lowerLimit = environmentResource.contiguous ? environmentResource.resourceDensity : 1 - environmentResource.resourceDensity;
                environmentResource.resourceNoiseMap = Draw.Noise.MaskedNoise(environmentResource.resourceNoiseMap, environmentResource.resourceNoiseMap, upperLimit, lowerLimit);
                // Create a colour map based on that noise
                environmentResource.resourceColourMap = Draw.ColourMap(environmentResource);
                // Mask that over the terrain map
                terrainColourMap = Draw.OverlayColourMap(environmentResource.resourceNoise, 
                    terrainColourMap,
                    environmentResource.resourceColourMap,
                    new Color(0,0,0,0));
            }

            switch (environment.drawMode)
            {
                case Environment.DrawMode.NOISE:
                    EnvironmentGenerator.DrawTexture(environment.terrainRenderer, EnvironmentGenerator.TextureFromHeightMap(terrainHeightMap));
                    break;
                case Environment.DrawMode.COLOUR:
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
            if (environment.environmentRegions.footHillHeight > 1 - environment.environmentRegions.landHeight)
            {
                environment.environmentRegions.footHillHeight = 1 - environment.environmentRegions.landHeight;
            }
            if (environment.environmentRegions.snowHeight > environment.environmentRegions.footHillHeight)
            {
                environment.environmentRegions.snowHeight = environment.environmentRegions.footHillHeight;
            }

            if (environment.generateEnvironment)
            {
                GenerateEnvironment();
                environment.generateEnvironment = false;
                return;
            }
        }
    }
}
