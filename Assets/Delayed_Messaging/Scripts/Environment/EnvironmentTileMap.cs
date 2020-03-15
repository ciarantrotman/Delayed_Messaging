using System;
using System.Collections.Generic;
using Delayed_Messaging.Scripts.Objects.Resources;
using Delayed_Messaging.Scripts.Utilities;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Environment
{
    public class EnvironmentTileMap : MonoBehaviour
    {
        [Header("Tile Map Settings")]
        [SerializeField] private float tileWidth;
        private const float TileMapHeight = -.05f;
        
        [Serializable] public class Tile
        {
            // Path finding Information
            public int penalty;
            public bool walkable;

            // Terrain Information
            public Material tileMaterial;
            public EnvironmentController.ResourceRegions.ResourceRegion region;
            public Resource resource;
            public float height;
            //public TileObject tileObject;
            
            // GameObject Information
            public Vector3 tilePosition;
            public GameObject tileGameObject;
            public Mesh mesh;
            public MeshRenderer meshRenderer;
        }
        public Tile[,] tileMap;

        public void GenerateTileMap(EnvironmentController.ResourceRegions[,] resourceRegions, EnvironmentController.EnvironmentRegions environmentRegions)
        {
            GameObject tileMapParent = new GameObject("[TileMap Parent]");
            
            tileMap = EnvironmentGenerator.GenerateTileMap(resourceRegions, environmentRegions, tileWidth);

            int width = tileMap.GetLength(0);
            int height = tileMap.GetLength(1);

            float yPosition = -((height * tileWidth) * .5f);
            float xPosition = -((width * tileWidth) * .5f);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Tile tile = tileMap[x, y];
                    tile.tileGameObject = new GameObject(tile.region.ToString(), typeof(MeshFilter), typeof(MeshRenderer));
                    tile.tileGameObject.transform.SetParent(tileMapParent.transform);
                    tile.tileGameObject.transform.position = new Vector3(xPosition, TileMapHeight, yPosition);
                    
                    tile.meshRenderer = tile.tileGameObject.GetComponent<MeshRenderer>();
                    tile.meshRenderer.material = tile.tileMaterial;
                    
                    tile.mesh = Draw.CubeMesh(tileWidth, tile.height, tile.tileGameObject.transform.position);
                    tile.tileGameObject.GetComponent<MeshFilter>().mesh = tile.mesh;

                    xPosition += tileWidth;
                }

                yPosition += tileWidth;
                xPosition = -((width * tileWidth) * .5f);
            }
        }
    }
}
