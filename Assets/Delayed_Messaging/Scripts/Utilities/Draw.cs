using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Delayed_Messaging.Scripts.Environment;
using Pathfinding;
using UnityEngine;

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
		public enum Orientation
		{
			Forward,
			Right,
			Up
		}
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
					case Orientation.Forward:
						lr.SetPosition(i, new Vector3(x, y, 0));
						break;
					case Orientation.Right:
						lr.SetPosition(i, new Vector3(x, 0, y));
						break;
					case Orientation.Up:
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
			lr.SetPosition(0, start.position);
			lr.SetPosition(1, end.position);
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
					case Orientation.Forward:
						lr.SetPosition(i, new Vector3(x, y, 0));
						break;
					case Orientation.Right:
						lr.SetPosition(i, new Vector3(x, 0, y));
						break;
					case Orientation.Up:
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

		public static class GenerateMesh
		{
			
		}
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
					case Orientation.Forward:
						vertices.Add(new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0));
						break;
					case Orientation.Right:
						vertices.Add(new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
						break;
					case Orientation.Up:
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
			
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();
		}
		public static class Noise 
		{
			/// <summary>
			/// Generates basic perlin noise
			/// </summary>
			/// <param name="environmentDefinition"></param>
			/// <returns></returns>
			public static float[,] GeneratePerlinNoiseMap(EnvironmentController.Environment.EnvironmentDefinition environmentDefinition)
			{
				float[,] noiseMap = new float[environmentDefinition.size, environmentDefinition.size];

				for (int y = 0; y < environmentDefinition.size; y++) 
				{
					for (int x = 0; x < environmentDefinition.size; x++) 
					{
						float sampleX = x / environmentDefinition.noiseScale;
						float sampleY = y / environmentDefinition.noiseScale;

						float perlinValue = Mathf.PerlinNoise (sampleX, sampleY);
						noiseMap [x, y] = perlinValue;
					}
				}

				return noiseMap;
			}
			/// <summary>
			/// Generates fractional brownian noise
			/// </summary>
			/// <param name="environmentDefinition"></param>
			/// <returns></returns>
			public static float[,] GenerateFractionalBrownianNoise(EnvironmentController.Environment.EnvironmentDefinition environmentDefinition) 
			{
				float[,] noiseMap = new float[environmentDefinition.size, environmentDefinition.size];

				System.Random prng = new System.Random(environmentDefinition.seed);
				Vector2[] octaveOffsets = new Vector2[environmentDefinition.octaves];
				for (int i = 0; i < environmentDefinition.octaves; i++) 
				{
					float offsetX = prng.Next (-100000, 100000) + environmentDefinition.offset.x;
					float offsetY = prng.Next (-100000, 100000) + environmentDefinition.offset.y;
					octaveOffsets [i] = new Vector2 (offsetX, offsetY);
				}

				float maxNoiseHeight = float.MinValue;
				float minNoiseHeight = float.MaxValue;

				float halfWidth = environmentDefinition.size / 2f;
				float halfHeight = environmentDefinition.size / 2f;

				for (int y = 0; y < environmentDefinition.size; y++) 
				{
					for (int x = 0; x < environmentDefinition.size; x++) 
					{
						float amplitude = 1;
						float frequency = 1;
						float noiseHeight = 0;

						for (int i = 0; i < environmentDefinition.octaves; i++) 
						{
							float sampleX = (x-halfWidth) / environmentDefinition.noiseScale * frequency + octaveOffsets[i].x;
							float sampleY = (y-halfHeight) / environmentDefinition.noiseScale * frequency + octaveOffsets[i].y;
							float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
							noiseHeight += perlinValue * amplitude;
							amplitude *= environmentDefinition.persistence;
							frequency *= environmentDefinition.lacunarity;
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

				for (int y = 0; y < environmentDefinition.size; y++) 
				{
					for (int x = 0; x < environmentDefinition.size; x++) 
					{
						noiseMap [x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap [x, y]);
					}
				}

				return noiseMap;
			}
		}
		/// <summary>
		/// Generates colours for the noise based on the supplied environment and noise map
		/// </summary>
		/// <param name="environmentDefinition"></param>
		/// <param name="noiseMap"></param>
		/// <returns></returns>
		public static Color[] ColourMap(EnvironmentController.Environment.EnvironmentDefinition environmentDefinition, float[,] noiseMap)
		{
			Color[] colourMap = new Color[environmentDefinition.size * environmentDefinition.size];

			for (int y = 0; y < environmentDefinition.size; y++) 
			{
				for (int x = 0; x < environmentDefinition.size; x++) 
				{
					float currentHeight = noiseMap[x, y];
					Color color;
					EnvironmentController.EnvironmentRegions.EnvironmentRegion regionType =
						currentHeight <= environmentDefinition.environmentRegions.seaLevel
							? EnvironmentController.EnvironmentRegions.EnvironmentRegion.WATER
							: EnvironmentController.EnvironmentRegions.EnvironmentRegion.LAND;

					switch (regionType)
					{
						case EnvironmentController.EnvironmentRegions.EnvironmentRegion.LAND:
							if (currentHeight <= (environmentDefinition.environmentRegions.seaLevel + environmentDefinition.environmentRegions.shoreDepth))
							{
								color = environmentDefinition.environmentRegions.shoreColour;
								break;
							}
							if (currentHeight >= environmentDefinition.environmentRegions.landHeight)
							{
								if (currentHeight <= environmentDefinition.environmentRegions.landHeight + environmentDefinition.environmentRegions.footHillHeight)
								{
									color = environmentDefinition.environmentRegions.footHillColour;
									break;
								}
								if (currentHeight >= 1 - environmentDefinition.environmentRegions.snowHeight)
								{
									color = environmentDefinition.environmentRegions.snowColour;
									break;
								}
								color = environmentDefinition.environmentRegions.mountainColour;
								break;
							}
							color = environmentDefinition.environmentRegions.landColour;
							break;
						case EnvironmentController.EnvironmentRegions.EnvironmentRegion.WATER:
							if (currentHeight <= (environmentDefinition.environmentRegions.seaLevel - environmentDefinition.environmentRegions.shallowsDepth))
							{
								color = environmentDefinition.environmentRegions.seaColour;
								break;
							}
							color = environmentDefinition.environmentRegions.shallowsColour;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					
					colourMap[y*environmentDefinition.size+x] = color;
				}
			}
			return colourMap;
		}
	}
}