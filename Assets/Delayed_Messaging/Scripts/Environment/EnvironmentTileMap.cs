using System;
using System.Collections.Generic;
using UnityEngine;

namespace Delayed_Messaging.Scripts.Environment
{
    public class EnvironmentTileMap : MonoBehaviour
    {
        [Serializable] public struct Tile
        {
            public GameObject tileModel;
            public Vector3 tilePosition;
            public Vector2 tileIndex;
        }

        public List<Tile> tileMap = new List<Tile>();

        public void GenerateTileMap(EnvironmentController.Environment environment, float[,] noiseMap)
        {
            /*
            for (int i = 0; i < tileMap.Count; i++)
            {
                Destroy(tileMap[i].tileModel);
            }
            
            tileMap = EnvironmentGenerator.GenerateTileMap(environment, noiseMap);

            foreach (Tile t in tileMap)
            {
                Tile tile = t;
                
                tile.tileModel = Instantiate(tile.tileModel, transform);
                tile.tileModel.transform.position = new Vector3(tile.tileIndex.x, 0, tile.tileIndex.y);
            }
            */
        }
    }
}
