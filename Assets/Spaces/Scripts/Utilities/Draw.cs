using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace Spaces.Scripts.Utilities
{
	public static class Draw
	{
		/// <summary>
		/// Draws a bezier line renderer
		/// </summary>
		/// <param name="lr"></param>
		/// <param name="p0"></param>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="segments"></param>
		public static void Bezier(this LineRenderer lr, Vector3 p0, Vector3 p1, Vector3 p2, int segments = 40)
		{
			lr.positionCount = segments;
			lr.SetPosition(0, p0);
			lr.SetPosition(segments - 1, p2);

			for (int i = 1; i < segments; i++)
			{
				Vector3 point = GetPoint(p0, p1, p2, i / (float) segments);
				lr.SetPosition(i, point);
			}
		}
		private static float GetTimeStep(float flight, float step)
		{
			return Mathf.Lerp(0, flight, step);
		}
		public static void DrawLineRenderer(this LineRenderer lr, GameObject focus, GameObject midpoint, Transform controller, GameObject target, int quality)
		{
			midpoint.transform.localPosition = new Vector3(0, 0, controller.Midpoint(target.transform));
            
			lr.LineRenderWidth(.001f, focus != null ? .01f : 0f);
            
			lr.Bezier(controller.position,
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
		/// <summary>
		/// Draws a line renderer between two transforms
		/// </summary>
		/// <param name="lr"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public static void DrawStraightLineRender(this LineRenderer lr, Transform start, Transform end)
		{
			lr.positionCount = 2;
			lr.SetPosition(0, start.position);
			lr.SetPosition(1, end.position);
		}
		/// <summary>
		/// Draws a line renderer between two points
		/// </summary>
		/// <param name="lr"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public static void DrawStraightLineRender(this LineRenderer lr, Vector3 start, Vector3 end)
		{
			lr.positionCount = 2;
			lr.SetPosition(0, start);
			lr.SetPosition(1, end);
		}
		/// <summary>
		/// Draws a line renderer in an arc
		/// </summary>
		/// <param name="lr"></param>
		/// <param name="radius"></param>
		/// <param name="startAngle"></param>
		/// <param name="endAngle"></param>
		/// <param name="orientation"></param>
		/// <param name="quality"></param>
		/// <exception cref="ArgumentException"></exception>
		public static void ArcLineRenderer(this LineRenderer lr, float radius, float startAngle, float endAngle, Orientation orientation, int quality)
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
		private const int CircleSegmentCount = 64, CircleVertexCount = CircleSegmentCount + 2, CircleIndexCount = CircleSegmentCount * 3;
		/// <summary>
		/// Creates a circular mesh
		/// </summary>
		/// <param name="radius"></param>
		/// <param name="orientation"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
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
		/// <summary>
		/// Creates a mesh quad
		/// </summary>
		/// <returns></returns>
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
		/// <summary>
		/// Configures an extant quad mesh
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
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
	}
}