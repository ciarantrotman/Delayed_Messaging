using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Delayed_Messaging.Scripts.Environment;
using Grapple.Scripts;
using Pathfinding;
using UnityEngine;
using UnityEngine.Rendering;

namespace Delayed_Messaging.Scripts.Utilities
{
	public static class Draw
	{
		public static void BezierLineRenderer(this LineRenderer lr, Vector3 p0, Vector3 p1, Vector3 p2, int segments = 40)
		{
			lr.positionCount = segments;
			lr.SetPosition(0, p0);
			lr.SetPosition(segments - 1, p2);

			for (var i = 1; i < segments; i++)
			{
				var point = GetPoint(p0, p1, p2, i / (float) segments);
				lr.SetPosition(i, point);
			}
		}
		private const float G = 9.81f;
		public static void BallisticTrajectory(this LineRenderer lineRenderer, BallisticTrajectory.BallisticTrajectoryData data, int segments = 40)
		{
			lineRenderer.positionCount = segments;
			lineRenderer.useWorldSpace = false;
			
			for (int i = 0; i < segments; i++)
			{
				float timeStep = GetTimeStep(data.flight, i / (float) segments);
				lineRenderer.SetPosition(i, new Vector3(
					0, 
					GetHeight(data, timeStep), 
					GetRange(data, timeStep)));
			}
		}
		private static float GetTimeStep(float flight, float step)
		{
			return Mathf.Lerp(0, flight, step);
		}
		private static float GetHeight(BallisticTrajectory.BallisticTrajectoryData data, float time)
		{
			return (data.height = data.initialHeight + (data.verticalComponent * time) - 0.5f * G * Mathf.Pow(time, 2)) - data.initialHeight;
		}
		private static float GetRange(BallisticTrajectory.BallisticTrajectoryData data, float time)
		{
			return Mathf.Lerp(0, data.range, time);
		}
		public static void DrawLineRenderer(this LineRenderer lr, GameObject focus, GameObject midpoint, Transform controller, GameObject target, int quality)
		{
			midpoint.transform.localPosition = new Vector3(0, 0, controller.Midpoint(target.transform));
            
			lr.LineRenderWidth(.001f, focus != null ? .01f : 0f);
            
			lr.BezierLineRenderer(controller.position,
				midpoint.transform.position, 
				target.transform.position,
				quality);
		}
		public static void DrawRectangularLineRenderer(this LineRenderer lineRenderer, Vector3 start, Vector3 end)
		{
			lineRenderer.SetPosition(0, start);
			lineRenderer.SetPosition(1, new Vector3(start.x, start.y, end.z));
			lineRenderer.SetPosition(2, end);
			lineRenderer.SetPosition(3, new Vector3(end.x, end.y, start.z));
			lineRenderer.SetPosition(4, start);
		}
		private static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
		{
			return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
		}
		public enum Orientation { FORWARD, RIGHT, UP }
		public static void CircleLineRenderer(this LineRenderer lr, float radius, Orientation orientation, int quality)
		{
			lr.positionCount = quality;
			lr.useWorldSpace = false;
			lr.loop = true;
			
			var angle = 0f;
			const float arcLength = 360f;
			
			for (var i = 0; i < quality; i++)
			{
				var x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
				var y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
				switch (orientation)
				{
					case Orientation.FORWARD:
						lr.SetPosition(i, new Vector3(x, y, 0));
						break;
					case Orientation.RIGHT:
						lr.SetPosition(i, new Vector3(x, 0, y));
						break;
					case Orientation.UP:
						lr.SetPosition(i, new Vector3(0, x, y));
						break;
					default:
						throw new ArgumentException();
				}

				angle += arcLength / quality;
			}
		}
		public static void DrawStraightLineRender(this LineRenderer lr, Transform start, Transform end)
		{
			lr.positionCount = 2;
			lr.SetPosition(0, start.position);
			lr.SetPosition(1, end.position);
		}
		public static void DrawStraightLineRender(this LineRenderer lr, Vector3 start, Vector3 end)
		{
			lr.positionCount = 2;
			lr.SetPosition(0, start);
			lr.SetPosition(1, end);
		}
		public static void DrawDestinationLineRender(this LineRenderer lineRenderer, Seeker seeker)
		{
			if (lineRenderer == null || seeker == null || seeker.lastCompletedNodePath == null) return;

			lineRenderer.positionCount = seeker.lastCompletedNodePath.Count;
			
			for (int i = 0; i < seeker.lastCompletedNodePath.Count - 1; i++) 
			{
				lineRenderer.SetPosition(i, (Vector3)seeker.lastCompletedNodePath[i].position);
			}
		}
		public static void ArcLineRenderer(this LineRenderer lr, float radius, float startAngle, float endAngle,
			Orientation orientation, int quality)
		{
			lr.positionCount = quality;
			lr.useWorldSpace = false;

			var angle = startAngle;
			var arcLength = endAngle - startAngle;

			for (var i = 0; i < quality; i++)
			{
				var x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
				var y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
				switch (orientation)
				{
					case Orientation.FORWARD:
						lr.SetPosition(i, new Vector3(x, y, 0));
						break;
					case Orientation.RIGHT:
						lr.SetPosition(i, new Vector3(x, 0, y));
						break;
					case Orientation.UP:
						lr.SetPosition(i, new Vector3(0, x, y));
						break;
					default:
						throw new ArgumentException();
				}

				angle += arcLength / quality;
			}
		}
		private const int CircleSegmentCount = 64;
		private const int CircleVertexCount = CircleSegmentCount + 2;
		private const int CircleIndexCount = CircleSegmentCount * 3;
		public static Mesh CircleMesh(this float radius, Orientation orientation)
		{
			Mesh circle = new Mesh();
			List<Vector3> vertices = new List<Vector3>(CircleVertexCount);
			int[] indices = new int[CircleIndexCount];
			const float segmentWidth = Mathf.PI * 2f / CircleSegmentCount;
			float angle = 0f;
			vertices.Add(Vector3.zero);
			for (int i = 1; i < CircleVertexCount; ++i)
			{
				switch (orientation)
				{
					case Orientation.FORWARD:
						vertices.Add(new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
						break;
					case Orientation.RIGHT:
						vertices.Add(new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
						break;
					case Orientation.UP:
						vertices.Add(new Vector3(0f, Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius));
						break;
					default:
						throw new ArgumentException();
				}
				angle -= segmentWidth;
				if (i <= 1) continue;
				int j = (i - 2) * 3;
				indices[j + 0] = 0;
				indices[j + 1] = i - 1;
				indices[j + 2] = i;
			}
			circle.SetVertices(vertices);
			circle.SetIndices(indices, MeshTopology.Triangles, 0);
			circle.RecalculateBounds();
			return circle;
		}
		public static Mesh QuadMesh()
		{
			Mesh quad = new Mesh();
			Vector3[] vertices = new Vector3[4];
			Vector2[] uv = new Vector2[4];
			int[] triangles = new int[6];
			
			vertices[0] = new Vector3(0,0,.1f);
			vertices[1] = new Vector3(.1f,0,.1f);
			vertices[2] = new Vector3(0,0,0);
			vertices[3] = new Vector3(.1f,0,0);
			
			uv[0] = new Vector2(0,1);
			uv[1] = new Vector2(1,1);
			uv[2] = new Vector2(0,0);
			uv[3] = new Vector2(1,0);

			triangles[0] = 0;
			triangles[1] = 1;
			triangles[2] = 2;
			triangles[3] = 2;
			triangles[4] = 1;
			triangles[5] = 3;

			quad.vertices = vertices;
			quad.uv = uv;
			quad.triangles = triangles;
			
			return quad;
		}
		/// <summary>
		/// Creates a mesh with the supplied width and height
		/// </summary>
		/// <returns></returns>
		public static Mesh CubeMesh(float width, float height, Vector3 origin)
		{
			Mesh mesh = new Mesh();

			float bottom = origin.y;
			float negative = -(width * .5f);
			float positive = width * .5f;
			
			Vector3[] vertices = 
			{
				new Vector3 (negative, bottom, negative),	// 	0	Front Bottom Left
				new Vector3 (positive, bottom, negative),	//	1	Front Bottom Right
				new Vector3 (positive, height, negative),	//	2	Front Top Right
				new Vector3 (negative, height, negative),	//	3	Front Top Left
				new Vector3 (negative, height, positive),	//	4	Back Top Left
				new Vector3 (positive, height, positive),	//	5	Back Top Right
				new Vector3 (positive, bottom, positive),	//	6	Back Bottom Right
				new Vector3 (negative, bottom, positive),	//	7	Back Bottom Left
			};
			int[] triangles = 
			{
				0, 2, 1, //face front
				0, 3, 2,
				2, 3, 4, //face top
				2, 4, 5,
				1, 2, 5, //face right
				1, 5, 6,
				0, 7, 4, //face left
				0, 4, 3,
				5, 4, 7, //face back
				5, 7, 6,
				0, 6, 7, //face bottom
				0, 1, 6
			};

			mesh.Clear ();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			
			mesh.Optimize ();
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();

			return mesh;
		}
		public static void DrawQuadMesh(this Mesh mesh, Vector3 start, Vector3 end)
		{
			Vector3[] vertices = new Vector3[4];
			Vector2[] uv = new Vector2[4];
			int[] triangles = new int[6];

			Vector3 a = start;
			Vector3 b = new Vector3(start.x, start.y, end.z);
			Vector3 c = new Vector3(end.x, end.y, start.z);
			Vector3 d = end;

			vertices[0] = a;
			vertices[1] = b;
			vertices[2] = c;
			vertices[3] = d;
			
			uv[0] = new Vector2(0,1);
			uv[1] = new Vector2(1,1);
			uv[2] = new Vector2(0,0);
			uv[3] = new Vector2(1,0);

			triangles[0] = 0;
			triangles[1] = 1;
			triangles[2] = 2;
			triangles[3] = 2;
			triangles[4] = 1;
			triangles[5] = 3;

			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.triangles = triangles;
			
			mesh.Optimize ();
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
		}

		public static Mesh CurvedQuad()
		{
			/*
			using UnityEngine;
			[RequireComponent(typeof(MeshFilter))]
			[RequireComponent(typeof(MeshRenderer))]
			public class CurvedPlane : MonoBehaviour
			{
			    private class MeshData
			    {
			        public Vector3[] Vertices { get; set; }
			        public int[] Triangles { get; set; }
			    }

			    [SerializeField] private float height = 1f;
			    [SerializeField] private float radius = 2f;

			    [SerializeField] [Range(1, 1024)] private int numSegments = 16;

			    [SerializeField] [Range(0f, 360f)] private float curvatureDegrees = 60f;

			    [SerializeField] private bool useArc = true;
			    
			    private MeshData plane;

			    void Start()
			    {
			        Generate();
			    }

				void OnValidate ()
				{
			        Generate();
				}

			    [ContextMenu("Generate")]
			    private void Generate()
			    {
			        GenerateScreen();
			        UpdateMeshFilter();
			    }

			    private void UpdateMeshFilter()
			    {
			        var filter = GetComponent<MeshFilter>();

			        var mesh = new Mesh
			        {
			            vertices = plane.Vertices,
			            triangles = plane.Triangles
			        };

			        filter.mesh = mesh;
			    }

			    private void GenerateScreen()
			    {
			        plane = new MeshData
			        {
			            Vertices = new Vector3[(numSegments + 2)*2],
			            Triangles = new int[numSegments*6]
			        };

			        int i,j;
			        for (i = j = 0; i < numSegments+1; i++)
			        {
			            GenerateVertexPair(ref i, ref j);

			            if (i < numSegments)
			            {
			                GenerateLeftTriangle(ref i, ref j);
			                GenerateRightTriangle(ref i, ref j);
			            }
			        }
			    }

			    private void GenerateVertexPair(ref int i, ref int j)
			    {
			        float amt = ((float)i) / numSegments;
			        float arcDegrees = curvatureDegrees * Mathf.Deg2Rad;
			        float theta = -0.5f + amt;

			        var x = useArc ? Mathf.Sin(theta * arcDegrees) * radius : (-0.5f * radius) + (amt * radius);
			        var z = Mathf.Cos(theta * arcDegrees) * radius;

			        plane.Vertices[i] = new Vector3(x, height / 2f, z);
			        plane.Vertices[i + numSegments + 1] = new Vector3(x, -height / 2f, z);
			    }

			    private void GenerateLeftTriangle(ref int i, ref int j)
			    {
			        plane.Triangles[j++] = i;
			        plane.Triangles[j++] = i + 1;
			        plane.Triangles[j++] = i + numSegments + 1;
			    }

			    private void GenerateRightTriangle(ref int i, ref int j)
			    {
			        plane.Triangles[j++] = i + 1;
			        plane.Triangles[j++] = i + numSegments + 2;
			        plane.Triangles[j++] = i + numSegments + 1;
			    }
			}
			 */
			return null;
		}
		public static class Noise 
		{
			/// <summary>
			/// Generates basic perlin noise
			/// </summary>
			/// <param name="noise"></param>
			/// <returns></returns>
			public static float[,] GeneratePerlinNoiseMap(EnvironmentController.Noise noise)
			{
				float[,] noiseMap = new float[noise.size, noise.size];

				for (int y = 0; y < noise.size; y++) 
				{
					for (int x = 0; x < noise.size; x++) 
					{
						float sampleX = x / noise.noiseScale;
						float sampleY = y / noise.noiseScale;

						float perlinValue = Mathf.PerlinNoise (sampleX, sampleY);
						noiseMap [x, y] = perlinValue;
					}
				}

				return noiseMap;
			}
			/// <summary>
			/// Generates fractional brownian noise
			/// </summary>
			/// <param name="noise"></param>
			/// <returns></returns>
			public static float[,] GenerateFractionalBrownianNoise(EnvironmentController.Noise noise) 
			{
				float[,] noiseMap = new float[noise.size, noise.size];

				System.Random prng = new System.Random(noise.seed);
				Vector2[] octaveOffsets = new Vector2[noise.octaves];
				for (int i = 0; i < noise.octaves; i++) 
				{
					float offsetX = prng.Next (-100000, 100000) + noise.offset.x;
					float offsetY = prng.Next (-100000, 100000) + noise.offset.y;
					octaveOffsets [i] = new Vector2 (offsetX, offsetY);
				}

				float maxNoiseHeight = float.MinValue;
				float minNoiseHeight = float.MaxValue;

				float halfWidth = noise.size / 2f;
				float halfHeight = noise.size / 2f;

				for (int y = 0; y < noise.size; y++) 
				{
					for (int x = 0; x < noise.size; x++) 
					{
						float amplitude = 1;
						float frequency = 1;
						float noiseHeight = 0;

						for (int i = 0; i < noise.octaves; i++) 
						{
							float sampleX = (x-halfWidth) / noise.noiseScale * frequency + octaveOffsets[i].x;
							float sampleY = (y-halfHeight) / noise.noiseScale * frequency + octaveOffsets[i].y;
							float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
							noiseHeight += perlinValue * amplitude;
							amplitude *= noise.persistence;
							frequency *= noise.lacunarity;
						}

						if (noiseHeight > maxNoiseHeight) 
						{
							maxNoiseHeight = noiseHeight;
						} else if (noiseHeight < minNoiseHeight) 
						{
							minNoiseHeight = noiseHeight;
						}
						noiseMap [x, y] = noiseHeight;
					}
				}

				for (int y = 0; y < noise.size; y++) 
				{
					for (int x = 0; x < noise.size; x++) 
					{
						noiseMap [x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap [x, y]);
					}
				}

				return noiseMap;
			}
			/// <summary>
			/// Creates a height map where each unit in the matrix has an associated land type 
			/// </summary>
			/// <param name="terrainNoise"></param>
			/// <param name="terrainNoiseMap"></param>
			/// <param name="regions"></param>
			/// <returns></returns>
			/// <exception cref="ArgumentOutOfRangeException"></exception>
			public static EnvironmentController.ResourceRegions[,] GenerateEnvironmentResources(EnvironmentController.Noise terrainNoise, float[,] terrainNoiseMap, EnvironmentController.EnvironmentRegions regions)
			{
				EnvironmentController.ResourceRegions[,] resourceRegions = new EnvironmentController.ResourceRegions[terrainNoise.size, terrainNoise.size];

				for (int y = 0; y < terrainNoise.size; y++) 
				{
					for (int x = 0; x < terrainNoise.size; x++) 
					{
						float currentHeight = terrainNoiseMap[x, y];
						resourceRegions[x, y].height = currentHeight;

						EnvironmentController.EnvironmentRegions.EnvironmentRegion regionType =
							currentHeight <= regions.seaLevel
								? EnvironmentController.EnvironmentRegions.EnvironmentRegion.WATER
								: EnvironmentController.EnvironmentRegions.EnvironmentRegion.LAND;

						switch (regionType)
						{
							case EnvironmentController.EnvironmentRegions.EnvironmentRegion.LAND:
								if (currentHeight <= (regions.seaLevel + regions.shoreDepth))
								{
									resourceRegions[x, y].resourceRegion = EnvironmentController.ResourceRegions.ResourceRegion.SHORE;
									break;
								}
								if (currentHeight >= regions.landHeight)
								{
									if (currentHeight <= regions.landHeight + regions.footHillHeight)
									{
										resourceRegions[x, y].resourceRegion = EnvironmentController.ResourceRegions.ResourceRegion.FOOTHILLS;
										break;
									}
									if (currentHeight >= 1 - regions.snowHeight)
									{
										resourceRegions[x, y].resourceRegion = EnvironmentController.ResourceRegions.ResourceRegion.SNOW;
										break;
									}
									resourceRegions[x, y].resourceRegion = EnvironmentController.ResourceRegions.ResourceRegion.MOUNTAIN;
									break;
								}
								resourceRegions[x, y].resourceRegion = EnvironmentController.ResourceRegions.ResourceRegion.LAND;
								break;
							case EnvironmentController.EnvironmentRegions.EnvironmentRegion.WATER:
								if (currentHeight <= (regions.seaLevel - regions.shallowsDepth))
								{
									resourceRegions[x, y].resourceRegion = EnvironmentController.ResourceRegions.ResourceRegion.SEA;
									break;
								}
								resourceRegions[x, y].resourceRegion = EnvironmentController.ResourceRegions.ResourceRegion.SHALLOWS;
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}
				}
				return resourceRegions;
			}
			/// <summary>
			/// Masks one noise map with another
			/// </summary>
			/// <param name="terrain"></param>
			/// <param name="resourceHeightMap"></param>
			/// <param name="upperLimit"></param>
			/// <param name="lowerLimit"></param>
			/// <returns></returns>
			public static float[,] MaskedNoise(float [,] terrain, float [,] resourceHeightMap, float upperLimit, float lowerLimit)
			{
				float[,] noiseMap = resourceHeightMap;
				for (int y = 0; y < terrain.GetLength(0); y++)
				{
					for (int x = 0; x < terrain.GetLength(1); x++)
					{
						float maskHeight = terrain[x, y];
						float resourceHeight = resourceHeightMap[x, y];
						noiseMap[x, y] = WithinLimits(maskHeight, upperLimit, lowerLimit) ? resourceHeight : 0;
					}
				}
				return noiseMap;
			}
			private static bool WithinLimits(float value, float upper, float lower, float border = 0f)
			{
				return value <= upper - border && value >= lower + border;
			}

			/// <summary>
			/// Takes in an indexed environment region and masks values based on the mask
			/// </summary>
			/// <param name="environmentResource"></param>
			/// <param name="indexedTerrainHeightMap"></param>
			/// <param name="regions"></param>
			/// <returns></returns>
			/// <exception cref="ArgumentOutOfRangeException"></exception>
			public static float[,] MaskedNoise(EnvironmentController.EnvironmentResources environmentResource, EnvironmentController.ResourceRegions[,] indexedTerrainHeightMap, EnvironmentController.EnvironmentRegions regions)
			{
				float[,] noiseMap = environmentResource.resourceNoiseMap;
				
				for (int y = 0; y < indexedTerrainHeightMap.GetLength(0); y++)
				{
					for (int x = 0; x < indexedTerrainHeightMap.GetLength(1); x++)
					{
						float maskHeight = indexedTerrainHeightMap[x, y].height;
						float resourceHeight = environmentResource.resourceNoiseMap[x, y];
						
						noiseMap[x, y] = indexedTerrainHeightMap[x,y].resourceRegion == environmentResource.resourceRegion ? resourceHeight : 0;
						
						switch (environmentResource.resourceRegion)
						{
							case EnvironmentController.ResourceRegions.ResourceRegion.SEA:
								noiseMap[x, y] = WithinLimits(maskHeight, regions.seaLevel - regions.shoreDepth, regions.seaLevel, environmentResource.borderThickness) ? resourceHeight : 0;
								break;
							case EnvironmentController.ResourceRegions.ResourceRegion.SHALLOWS:
								noiseMap[x, y] = WithinLimits(maskHeight, regions.seaLevel, regions.seaLevel - regions.shallowsDepth, environmentResource.borderThickness) ? resourceHeight : 0;
								break;
							case EnvironmentController.ResourceRegions.ResourceRegion.SHORE:
								noiseMap[x, y] = WithinLimits(maskHeight, regions.seaLevel + regions.shoreDepth, regions.seaLevel, environmentResource.borderThickness) ? resourceHeight : 0;
								break;
							case EnvironmentController.ResourceRegions.ResourceRegion.LAND:
								noiseMap[x, y] = WithinLimits(maskHeight, regions.landHeight, regions.seaLevel + regions.shoreDepth, environmentResource.borderThickness) ? resourceHeight : 0;
								break;
							case EnvironmentController.ResourceRegions.ResourceRegion.FOOTHILLS:
								noiseMap[x, y] = WithinLimits(maskHeight, regions.landHeight + regions.footHillHeight, regions.landHeight, environmentResource.borderThickness) ? resourceHeight : 0;
								break;
							case EnvironmentController.ResourceRegions.ResourceRegion.MOUNTAIN:
								noiseMap[x, y] = WithinLimits(maskHeight, 1 - regions.snowHeight, regions.landHeight + regions.footHillHeight, environmentResource.borderThickness) ? resourceHeight : 0;
								break;
							case EnvironmentController.ResourceRegions.ResourceRegion.SNOW:
								noiseMap[x, y] = WithinLimits(maskHeight, 1, 1 - regions.snowHeight, environmentResource.borderThickness) ? resourceHeight : 0;
								break;
							default:
								break;
						}
					}
				}
				return noiseMap;
			}
		}
		/// <summary>
		/// Generates colours for the noise based on the supplied environment and noise map
		/// </summary>
		/// <param name="noise"></param>
		/// <param name="regions"></param>
		/// <param name="noiseMap"></param>
		/// <returns></returns>
		public static Color[] ColourMap(EnvironmentController.Noise noise, EnvironmentController.EnvironmentRegions regions, float[,] noiseMap)
		{
			Color[] colourMap = new Color[noise.size * noise.size];
			
			for (int y = 0; y < noise.size; y++) 
			{
				for (int x = 0; x < noise.size; x++) 
				{
					float currentHeight = noiseMap[x, y];
					Color color;
					EnvironmentController.EnvironmentRegions.EnvironmentRegion regionType =
						currentHeight <= regions.seaLevel
							? EnvironmentController.EnvironmentRegions.EnvironmentRegion.WATER
							: EnvironmentController.EnvironmentRegions.EnvironmentRegion.LAND;

					switch (regionType)
					{
						case EnvironmentController.EnvironmentRegions.EnvironmentRegion.LAND:
							if (currentHeight <= (regions.seaLevel + regions.shoreDepth))
							{
								color = regions.shoreColour;
								break;
							}
							if (currentHeight >= regions.landHeight)
							{
								if (currentHeight <= regions.landHeight + regions.footHillHeight)
								{
									color = regions.footHillColour;
									break;
								}
								if (currentHeight >= 1 - regions.snowHeight)
								{
									color = regions.snowColour;
									break;
								}
								color = regions.mountainColour;
								break;
							}
							color = regions.landColour;
							break;
						case EnvironmentController.EnvironmentRegions.EnvironmentRegion.WATER:
							if (currentHeight <= (regions.seaLevel - regions.shallowsDepth))
							{
								color = regions.seaColour;
								break;
							}
							color = regions.shallowsColour;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					
					colourMap[y*noise.size+x] = color;
				}
			}
			return colourMap;
		}
		public static Color[] ColourMap(EnvironmentController.ResourceRegions[,] resourceRegions, EnvironmentController.EnvironmentRegions regions)
		{
			Color[] colourMap = new Color[resourceRegions.GetLength(0) * resourceRegions.GetLength(1)];
			
			for (int y = 0; y < resourceRegions.GetLength(0); y++) 
			{
				for (int x = 0; x < resourceRegions.GetLength(1); x++) 
				{
					Color color;
					switch (resourceRegions[x,y].resourceRegion)
					{
						case EnvironmentController.ResourceRegions.ResourceRegion.SEA:
							color = regions.seaColour;
							break;
						case EnvironmentController.ResourceRegions.ResourceRegion.SHALLOWS:
							color = regions.shallowsColour;
							break;
						case EnvironmentController.ResourceRegions.ResourceRegion.SHORE:
							color = regions.shoreColour;
							break;
						case EnvironmentController.ResourceRegions.ResourceRegion.LAND:
							color = regions.landColour;
							break;
						case EnvironmentController.ResourceRegions.ResourceRegion.FOOTHILLS:
							color = regions.footHillColour;
							break;
						case EnvironmentController.ResourceRegions.ResourceRegion.MOUNTAIN:
							color = regions.mountainColour;
							break;
						case EnvironmentController.ResourceRegions.ResourceRegion.SNOW:
							color = regions.snowColour;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					
					colourMap[y*resourceRegions.GetLength(0) + x] = color;
				}
			}
			return colourMap;
		}
		/// <summary>
		/// Generates colours for the noise based on the supplied environment and noise map
		/// </summary>
		/// <param name="resources"></param>
		/// <returns></returns>
		public static Color[] ColourMap(EnvironmentController.EnvironmentResources resources)
		{
			Color[] colourMap = new Color[resources.resourceNoise.size * resources.resourceNoise.size];

			for (int y = 0; y < resources.resourceNoise.size; y++) 
			{
				for (int x = 0; x < resources.resourceNoise.size; x++) 
				{
					colourMap[y * resources.resourceNoise.size + x] = resources.resourceNoiseMap[x, y] > 0 ? resources.resourceColour : new Color(0, 0, 0, 0);
				}
			}
			return colourMap;
		}
		/// <summary>
		/// Takes in two colour maps and outputs a new one with the overlaid colour map being masked using the mask colour
		/// </summary>
		/// <param name="noise"></param>
		/// <param name="baseColourMap"></param>
		/// <param name="overlayColourMap"></param>
		/// <param name="maskColour"></param>
		/// <returns></returns>
		public static Color[] OverlayColourMap(EnvironmentController.Noise noise, Color[] baseColourMap, Color[] overlayColourMap, Color maskColour)
		{
			Color[] colourMap = new Color[noise.size * noise.size];

			for (int y = 0; y < noise.size; y++) 
			{
				for (int x = 0; x < noise.size; x++)
				{
					colourMap[y * noise.size + x] = overlayColourMap[y * noise.size + x] == maskColour
						? baseColourMap[y * noise.size + x]
						: overlayColourMap[y * noise.size + x];;
				}
			}
			return colourMap;
		}
	}
}