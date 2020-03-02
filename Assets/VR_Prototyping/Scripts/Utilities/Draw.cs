using System;
using System.Collections.Generic;
using UnityEngine;

namespace VR_Prototyping.Scripts.Utilities
{
	public static class Draw
	{
		public static void BezierLineRenderer(this LineRenderer lr, Vector3 p0, Vector3 p1, Vector3 p2, int segments)
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
		public static Mesh GenerateCircleMesh(this float radius, Orientation orientation)
		{
			var circle = new Mesh();
			var vertices = new List<Vector3>(CircleVertexCount);
			var indices = new int[CircleIndexCount];
			const float segmentWidth = Mathf.PI * 2f / CircleSegmentCount;
			var angle = 0f;
			vertices.Add(Vector3.zero);
			for (var i = 1; i < CircleVertexCount; ++i)
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
				var j = (i - 2) * 3;
				indices[j + 0] = 0;
				indices[j + 1] = i - 1;
				indices[j + 2] = i;
			}
			circle.SetVertices(vertices);
			circle.SetIndices(indices, MeshTopology.Triangles, 0);
			circle.RecalculateBounds();
			return circle;
		}
		public static Mesh GenerateQuadMesh()
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
			//mesh.uv = uv;
			mesh.triangles = triangles;
		}
		/*
		public static void Circle (Vector3 center, float radius, Color color, float startAngle = 0f, float endAngle = 2*Mathf.PI) 
		{
			const int steps = 40;
			while (startAngle > endAngle) startAngle -= 2*Mathf.PI;
			Vector3 prev = new Vector3(Mathf.Cos(startAngle)*radius, 0, Mathf.Sin(startAngle)*radius);
			for (int i = 0; i <= steps; i++) {
				Vector3 c = new Vector3(Mathf.Cos(Mathf.Lerp(startAngle, endAngle, i/(float)steps))*radius, 0, Mathf.Sin(Mathf.Lerp(startAngle, endAngle, i/(float)steps))*radius);
				Line(center + prev, center + c, color);
				prev = c;
			}
		}
		*/
	}
}