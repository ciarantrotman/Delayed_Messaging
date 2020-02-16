using System;
using UnityEngine;

namespace VR_Prototyping.Scripts
{
    public static class Intersection
    {
        public static Vector3 Line(Vector3 point1, Vector3 vector1, Vector3 point2, Vector3 vector2, float tolerance)
        {
            var v3 = point2 - point1;
            var x1 = Vector3.Cross(vector1, vector2);
            var x2 = Vector3.Cross(v3, vector2);
 
            var planarFactor = Vector3.Dot(v3, x1);

            if (Mathf.Abs(planarFactor) < tolerance && x1.sqrMagnitude > tolerance)
            {
                var s = Vector3.Dot(x2, x1) / x1.sqrMagnitude;
                return point1 + vector1 * s;
            }
            return Vector3.zero;
        }
        
        public static Vector3 LineSegment(Vector3 point1, Vector3 vector1, Vector3 point2, Vector3 vector2, Vector3 point3, float tolerance)
        {
            var v3 = point2 - point1;
            var x1 = Vector3.Cross(vector1, vector2);
            var x2 = Vector3.Cross(v3, vector2);
 
            var x = Vector3.zero;
            
            var planarFactor = Vector3.Dot(v3, x1);

            if (Mathf.Abs(planarFactor) < tolerance && x1.sqrMagnitude > tolerance)
            {
                var s = Vector3.Dot(x2, x1) / x1.sqrMagnitude;
                x = point1 + vector1 * s;
            }
            else
            {
                return Vector3.zero;
            }

            return Math.Abs(Vector3.Distance(point1, x) +
                            Vector3.Distance(point3, x) -
                            Vector3.Distance(point1, point3)) < tolerance ? x : Vector3.zero;
        }
    }
}
