using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Delayed_Messaging.Scripts.Environment
{
    public static class EnvironmentGenerator
    {
        /// <summary>
        /// Generates a texture from a supplied colour array
        /// </summary>
        /// <param name="colourMap"></param>
        /// <param name="heightMap"></param>
        /// <returns></returns>
        public static Texture2D TextureFromColourMap(Color[] colourMap, float[,] heightMap) 
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            
            Texture2D texture = new Texture2D(width, height)
            {
                filterMode = FilterMode.Point, 
                wrapMode = TextureWrapMode.Clamp
            };
            
            texture.SetPixels(colourMap);
            texture.Apply();
            return texture;
        }
        /// <summary>
        /// Creates a texture from a supplied height map
        /// </summary>
        /// <param name="heightMap"></param>
        /// <returns></returns>
        public static Texture2D TextureFromHeightMap(float[,] heightMap) 
        {
            int width = heightMap.GetLength (0);
            int height = heightMap.GetLength (1);

            Color[] colourMap = new Color[width * height];
            
            for (int y = 0; y < height; y++) 
            {
                for (int x = 0; x < width; x++) 
                {
                    colourMap [y * width + x] = heightMap[x, y] > 0f ? Color.Lerp (Color.black, Color.white, heightMap [x, y]) : new Color(0,0,0,0);
                }
            }

            return TextureFromColourMap (colourMap,heightMap);
        }
        /// <summary>
        /// Sets a renderer's texture
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="texture"></param>
        public static void DrawTexture(Renderer renderer, Texture2D texture) 
        {
            renderer.sharedMaterial.mainTexture = texture;
        }

        /// <summary>
        /// Generates a tile map from a supplied colour map
        /// </summary>
        /// <param name="resourceRegions"></param>
        /// <param name="environmentRegions"></param>
        /// <param name="tileWidth"></param>
        /// <returns></returns>
        public static EnvironmentTileMap.Tile[,] GenerateTileMap(EnvironmentController.ResourceRegions[,] resourceRegions, EnvironmentController.EnvironmentRegions environmentRegions, float tileWidth)
        {
            int width = resourceRegions.GetLength(0);
            int height = resourceRegions.GetLength(1);
            
            EnvironmentTileMap.Tile[,] tiles = new EnvironmentTileMap.Tile[width, height];
            
            for (int y = 0; y < height; y++) 
            {
                for (int x = 0; x < width; x++) 
                {
                    EnvironmentTileMap.Tile tile = new EnvironmentTileMap.Tile
                    {
                        region = resourceRegions[x, y].resourceRegion,
                        height = resourceRegions[x, y].height
                    };

                    // Set region specific tile information
                    switch (resourceRegions[x,y].resourceRegion)
                    {
                        case EnvironmentController.ResourceRegions.ResourceRegion.SEA:
                            tile.tileMaterial = environmentRegions.seaMaterial;
                            break;
                        case EnvironmentController.ResourceRegions.ResourceRegion.SHALLOWS:
                            tile.tileMaterial = environmentRegions.shallowsMaterial;
                            break;
                        case EnvironmentController.ResourceRegions.ResourceRegion.SHORE:
                            tile.tileMaterial = environmentRegions.shoreMaterial;
                            break;
                        case EnvironmentController.ResourceRegions.ResourceRegion.LAND:
                            tile.tileMaterial = environmentRegions.landMaterial;
                            break;
                        case EnvironmentController.ResourceRegions.ResourceRegion.FOOTHILLS:
                            tile.tileMaterial = environmentRegions.footHillMaterial;
                            break;
                        case EnvironmentController.ResourceRegions.ResourceRegion.MOUNTAIN:
                            tile.tileMaterial = environmentRegions.mountainMaterial;
                            break;
                        case EnvironmentController.ResourceRegions.ResourceRegion.SNOW:
                            tile.tileMaterial = environmentRegions.snowMaterial;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    tiles[x,y] = tile;
                }
            }
            return tiles;
        }
    }
}