using UnityEngine;

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
            int width = heightMap.GetLength (0);
            int height = heightMap.GetLength (1);
            
            Texture2D texture = new Texture2D(width, height)
            {
                filterMode = FilterMode.Point, 
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels (colourMap);
            texture.Apply ();
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
                    colourMap [y * width + x] = Color.Lerp (Color.black, Color.white, heightMap [x, y]);
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
    }
}