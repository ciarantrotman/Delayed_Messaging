using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spaces.Scripts.Utilities
{
    public static class Check
    {
        /// <summary>
        /// Maintains a list of booleans at a certain capacity
        /// </summary>
        /// <param name="list"></param>
        /// <param name="current"></param>
        /// <param name="sensitivity"></param>
        public static void BoolListCull(this List<bool> list, bool current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }
        /// <summary>
        /// Maintains a list of booleans at a certain capacity
        /// </summary>
        /// <param name="list"></param>
        /// <param name="current"></param>
        /// <param name="sensitivity"></param>
        public static void FloatListCull(this List<float> list, float current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }
        /// <summary>
        /// Maintains a list of Vector2 at a certain capacity
        /// </summary>
        /// <param name="list"></param>
        /// <param name="current"></param>
        /// <param name="sensitivity"></param>
        public static void Vector2ListCull(this List<Vector2> list, Vector2 current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }
        /// <summary>
        /// Maintains a list of Vector3 at a certain capacity
        /// </summary>
        /// <param name="list"></param>
        /// <param name="current"></param>
        /// <param name="sensitivity"></param>
        public static void Vector3ListCull(this List<Vector3> list, Vector3 current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }
        
        public static void RotationTracking(this List<Vector3> list, Vector3 current, float sensitivity)
        {
            list.Add(current);
            CullList(list, sensitivity);
        }

        /// <summary>
        /// Culls a list to maintain a stable volume of values
        /// </summary>
        /// <param name="list"></param>
        /// <param name="sensitivity"></param>
        private static void CullList(this IList list, float sensitivity)
        {
            if (list.Count > sensitivity)
            {
                list.RemoveAt(0);
            }
        }

        public static void Target(this GameObject visual, GameObject parent, Transform normal, Vector2 pos, GameObject target, bool advanced = true)
        {
            visual.transform.LookAt(RotationTarget(pos, target, advanced));
            
            parent.transform.forward = normal.forward;
        }
        
        private static Transform RotationTarget(this Vector2 pos, GameObject target, bool advanced)
        {
            target.transform.localPosition = advanced? Vector3.Lerp(target.transform.localPosition, new Vector3(pos.x, 0, pos.y), .1f) : Vector3.forward;
            return target.transform;
        }
        
        public static float ControllerAngle(this GameObject follow, GameObject proxy, GameObject normal, Transform controller, Transform head, bool debug)
        {
            proxy.transform.Position(controller);
            proxy.transform.ForwardVector(controller);
            proxy.transform.SplitRotation(normal.transform, true);
            head.SplitPosition(controller, follow.transform);
            follow.transform.LookAt(proxy.transform);
            
            Vector3 normalDown = -normal.transform.up;
            Vector3 proxyForward = proxy.transform.forward;
            Vector3 position = proxy.transform.position;
            
            if (!debug) return Vector3.Angle(normalDown, proxyForward);
            
            Debug.DrawLine(follow.transform.position, position, Color.red);
            Debug.DrawRay(normal.transform.position, normalDown, Color.blue);
            Debug.DrawRay(position, proxyForward, Color.blue);

            return Vector3.Angle(normalDown, proxyForward);
        }
        
        public static float CalculateDepth(this float angle, float maxAngle, float minAngle, float max, float min, Transform proxy)
        {
            float a = angle;

            a = a > maxAngle ? maxAngle : a;
            a = a < minAngle ? minAngle : a;
            
            float proportion = Mathf.InverseLerp(maxAngle, minAngle, a);
            return Mathf.SmoothStep(max, min, proportion);
        }

        /// <summary>
        /// Calculate the target location
        /// </summary>
        /// <param name="target"></param>
        /// <param name="hitPoint"></param>
        /// <param name="lastValidPosition"></param>
        /// <param name="layerIndex"></param>
        /// <param name="normal"></param>
        public static void TargetLocation(this GameObject target, GameObject hitPoint, Vector3 lastValidPosition, int layerIndex, bool normal = false)
        {
            Transform t = target.transform;
            Vector3 position = t.position;
            Vector3 up = t.up;
            hitPoint.transform.position = Vector3.Lerp(hitPoint.transform.position, Physics.Raycast(position, -up, out RaycastHit hit, 100, 1 << layerIndex) ? hit.point : lastValidPosition, .25f);
            hitPoint.transform.up = Physics.Raycast(position, -up, out RaycastHit h) && normal ? h.normal : Vector3.up;
        }

        public static GameObject RayCastFindFocusObject(this List<GameObject> objects, GameObject current, GameObject target, GameObject inactive, Transform controller, float distance, bool disable)
        {
            if (disable) return current == null ? null : current;
            
            var position = controller.position;
            var forward = controller.forward;

            if (Physics.Raycast(position, forward, out var hit, distance) && objects.Contains(hit.transform.gameObject))
            {
                target.transform.SetParent(hit.transform);
                target.transform.VectorLerpPosition(hit.point, .15f);
                return hit.transform.gameObject;
            }

            target.transform.SetParent(null);
            target.transform.TransformLerpPosition(inactive.transform, .1f);
            return null;
        }
        
        public static GameObject FuzzyFindFocusObject(this List<GameObject> objects, GameObject current, GameObject target, GameObject inactive, bool disable)
        {
            if (disable)
            {
                target.transform.TransformLerpPosition(inactive.transform, .05f);
                return current == null ? null : current;
            }
            
            target.transform.TransformLerpPosition(objects.Count > 0 ? objects[0].gameObject.transform: inactive.transform, .1f);
            return objects.Count > 0 ? objects[0].gameObject : null;
        }
        /// <summary>
        /// Finds an active object, it prioritises ray-cast, then uses the fuzzy method to find a center of an object
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="current"></param>
        /// <param name="target"></param>
        /// <param name="inactive"></param>
        /// <param name="controller"></param>
        /// <param name="forward"></param>
        /// <param name="distance"></param>
        /// <param name="disable"></param>
        /// <returns></returns>
        public static GameObject FusionFindFocusObject(this List<GameObject> objects, GameObject current, GameObject target, GameObject inactive, Transform controller, Vector3 forward, float distance, bool disable)
        {
            if (disable && current == null)
            {
                target.transform.TransformLerpPosition(inactive.transform, .2f);
                return null;
            }

            if (disable) return current == null ? null : current;

            Vector3 position = controller.position;

            if (Physics.SphereCast(position, .1f, forward, out RaycastHit hit, distance) && objects.Contains(hit.transform.gameObject))
            {
                target.transform.SetParent(hit.transform);
                target.transform.VectorLerpPosition(hit.point, .25f);
                target.transform.forward = hit.normal;
                /*
                switch (hit.transform.gameObject.GetComponent<BaseObject>().selectionType == Scripts.Selection.SelectionType.FUSION)
                {
                    
                }
                */
                return hit.transform.gameObject;
            }

            if (objects.Count > 0)
            {
                target.transform.SetParent(objects[0].gameObject.transform);
                target.transform.TransformLerpPosition(objects[0].gameObject.transform, .25f);
                return objects[0];
            }
            
            target.transform.SetParent(null);
            target.transform.TransformLerpPosition(inactive.transform, .2f);
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="castObjects"></param>
        /// <returns></returns>
        public static GameObject CastFindObject(this List<GameObject> castObjects)
        {
            return castObjects.Count > 0 ? castObjects[0] : null;
        }

        public static void GrabStart(this GameObject f, GameObject p, GameObject target, GameObject o, Transform con)
        {
            f.transform.LookAt(con);
            p.transform.position = con.position;
            p.transform.LookAt(target.transform);
            target.transform.SetParent(con);
            o.transform.position = con.position;
        }
        
        public static void FocusObjectFollow(this Transform focus, Transform con, Transform tar, Transform tarS, Transform objO, Transform conO, Transform objP, bool d)
        {
            if (focus.transform.gameObject == null || d) return;
			
            tar.Transforms(focus);
            tarS.Transforms(focus);
            objO.Transforms(focus);
            conO.Position(con);
            objP.Position(con);
        }
        public static bool IsCollinear(this Vector3 a, Vector3 b, Vector3 x, float tolerance)
        {
            return Math.Abs(Vector3.Distance(a, x) + Vector3.Distance(b, x) - Vector3.Distance(a, b)) < tolerance;
        }
        public static void CheckGaze(this GameObject o, float a, float c, ICollection<GameObject> g, ICollection<GameObject> l, ICollection<GameObject> r, ICollection<GameObject> global)
        {
            if (!global.Contains(o))
            {
                g.Remove(o);
                l.Remove(o);
                r.Remove(o);
            }
				
            if (a < c/2 && !g.Contains(o))
            {
                g.Add(o);
            }
			
            else if (a > c/2)
            {
                g.Remove(o);
                l.Remove(o);
                r.Remove(o);
            }
        }
        /// <summary>
        /// Checks the current manual angle against the allowed angle
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="gazeList"></param>
        /// <param name="manualAngle"></param>
        /// <param name="currentAngle"></param>
        /// <returns></returns>
        public static bool WithinHandCone(this GameObject gameObject, ICollection<GameObject> gazeList, float manualAngle, float currentAngle)
        {
            if (!gazeList.Contains(gameObject)) return false;
            return manualAngle > currentAngle / 2;
        }
        /// <summary>
        /// Compares selection distance with the cast distance
        /// </summary>
        /// <param name="baseObject"></param>
        /// <param name="globalList"></param>
        /// <param name="castDistance"></param>
        /// <param name="currentDistance"></param>
        /// <returns></returns>
        public static bool WithinCastDistance(this GameObject baseObject, ICollection<GameObject> globalList, float castDistance, float currentDistance)
        {
            if (!globalList.Contains(baseObject))
            {
                return false;
            }
            
            return currentDistance <= castDistance;
        }
        /// <summary>
        /// Controls the appearance of game objects based on selection criteria
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="list"></param>
        /// <param name="selectionConditionsMet"></param>
        /// <param name="disabled"></param>
        /// <param name="selectionRangeEnabled"></param>
        public static void ManageList(this GameObject gameObject, ICollection<GameObject> list, bool selectionConditionsMet, bool disabled = false, bool selectionRangeEnabled = true)
        {
            if (disabled || !selectionRangeEnabled) return;
			
            if (selectionConditionsMet && !list.Contains(gameObject))
            {
                list.Add(gameObject);
            }
            else if (!selectionConditionsMet && list.Contains(gameObject))
            {
                list.Remove(gameObject);
            }
        }
        public static float TransformDistance(this Transform a, Transform b)
        {
            if (a == null || b == null)
            {
                return 0f;
            }
            return Vector3.Distance(a.position, b.position);
        }
        
        public static bool DistanceCheck(this Transform a, Transform b, float distance)
        {
            return Vector3.Distance(a.position, b.position) < distance;
        }
        public static bool DistanceCheck(this Vector3 a, Vector3 b, float distance)
        {
            return Vector3.Distance(a, b) < distance;
        }
        /// <summary>
        /// Checks if an object is within range
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="self"></param>
        /// <param name="user"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool WithinRange(this Transform self, bool enabled, Transform user, float range)
        {
            if (!enabled) return true;
            return Vector3.Distance(self.position, user.position) <= range;
        }

        /// <summary>
        /// Placeholder to determine if an object has reached its destination, will not work for large objects so needs to be fixed
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="target"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool Arrived(this Transform unit, Transform target, float distance)
        {
            return unit.DistanceCheck(target, distance);
        }
        public static bool Arrived(this Transform unit, Vector3 target, float distance)
        {
            return unit.position.DistanceCheck(target, distance);
        }
        /// <summary>
        /// Returns the largest value based on a Vector3
        /// </summary>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static float LargestValue(this Vector3 vector3)
        {
            float x = vector3.x;
            float y = vector3.y;
            float z = vector3.z;
            
            float largestValue = x > y ? x : y;
            largestValue = z > largestValue ? z : largestValue;

            return largestValue;
        }
        /// <summary>
        /// Utility which checks whether a given point lies within a cone
        /// </summary>
        /// <returns></returns>
        public static bool IsPointInCone(this Transform point, Vector3 coneAxis, Vector3 coneTipPosition, float coneAngle, float coneHeight = Single.PositiveInfinity)
        {
            // Normalise the two vectors
            Vector3 axis = coneAxis.normalized;
            Vector3 pointVector = (point.position - coneTipPosition).normalized;
            
            // Use the dot product to calculate the angle between them
            float dotProduct = Vector3.Dot(pointVector, axis);
            float angle = Mathf.Acos(dotProduct);
            float coneRadians = Mathf.Lerp(0, coneAngle, .5f) * Mathf.Deg2Rad;
            
            // Check against the height of the cone
            bool heightCheck = Vector3.Distance(point.position, coneTipPosition) <= coneHeight;
            
            // If the angle is smaller than the cone radians, then it is within the cone
            return angle < coneRadians && heightCheck;
        }
    }
}
